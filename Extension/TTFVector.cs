using TrueType.Domain;
using TrueType.Mode;
using TrueType.Support;

namespace TrueType.Extension
{
    public static class TTTFVectorExtension
    {
        const int FIXSHIFT = 10;
        const int FIX = (1 << FIXSHIFT);
        const int FIXMASK = (FIX - 1);

        internal static TTFBitmap Rasterize(this TTFVector vector, TTFIndex index, Size renderSize, PointF scale, Point offset)
        {
            var flatness_in_pixels = 0.35f;
            int vsubsample = renderSize.Height < 8 ? 15 : 5;
            var windings = vector.FlattenCurves(flatness_in_pixels / Math.Min(scale.X, scale.Y));

            var data = System.Text.Json.JsonSerializer.Serialize(windings);

            var edges = windings!.stbtt__rasterize(vsubsample, scale, true);
            var pixels = edges!.stbtt__rasterize_sorted_edges(renderSize, vsubsample, offset);

            var bitmap = MonoCanvas.Instance.LocateCharacter(index, pixels, renderSize, index.Size);
            return bitmap;
        }
        private static PointF[][]? FlattenCurves(this TTFVector vector, float objspace_flatness)
        {
            var vertices = vector.Vertices;
            var objspace_flatness_pow_2 = (float)Math.Pow(objspace_flatness, 2);

            var pointsList = new List<PointF[]>();
            List<PointF>? currentPoints = null;

            float x = 0, y = 0;
            for (var i = 0; i < vertices.Length; ++i)
            {
                switch (vertices[i].Type)
                {
                    case VertexType.MoveTo:
                        // start the next contour
                        if (currentPoints is not null)
                        {
                            pointsList.Add(currentPoints.ToArray());
                        }
                        currentPoints = new List<PointF>();

                        x = vertices[i].X;
                        y = vertices[i].Y;
                        currentPoints.Add(new PointF(x, y));
                        break;
                    case VertexType.LineTo:
                        x = vertices[i].X;
                        y = vertices[i].Y;
                        currentPoints!.Add(new PointF(x, y));
                        break;
                    case VertexType.CurveTo:
                        currentPoints!.stbtt__tesselate_curve(x, y,
                            vertices[i].CenterX, vertices[i].CenterY,
                            vertices[i].X, vertices[i].Y, objspace_flatness_pow_2);
                        x = vertices[i].X;
                        y = vertices[i].Y;
                        break;
                }
            }
            if (currentPoints is not null)
            {
                pointsList.Add(currentPoints.ToArray());
            }
            return pointsList.ToArray();
        }
        private static TTFEdge[]? stbtt__rasterize(this PointF[][] windings, int vsubsample, PointF scale, bool isInvented)
        {
            float y_scale_inv = isInvented ? -scale.Y : scale.Y;
            // vsubsample should divide 255 evenly; otherwise we won't reach full opacity

            //e = (stbtt__edge) STBTT_malloc(sizeof(*e) * (n+1), userdata); // add an extra one as a sentinel
            var edges = new List<TTFEdge>();

            for (int i = 0; i < windings.Length; i++)
            {
                var points = windings[i];

                for (int j = 1; j < points.Length; j++)
                {
                    var p0 = points[j];
                    var p1 = points[j - 1];

                    // skip the edge if horizontal
                    if (p0.Y == p1.Y)
                        continue;

                    var inventedFlag = false;

                    if (isInvented)
                    {
                        if (p1.Y > p0.Y)
                        {
                            inventedFlag = true;
                            var temp = p0;
                            p0 = p1;
                            p1 = temp;
                        }
                    }
                    else
                    {
                        if (p0.Y < p1.Y)
                        {
                            inventedFlag = true;
                            var temp = p0;
                            p0 = p1;
                            p1 = temp;
                        }
                    }

                    var edge = new TTFEdge();
                    edge.P0 = new PointF(p0.X * scale.X,
                                    p0.Y * y_scale_inv * vsubsample);
                    edge.P1 = new PointF(p1.X * scale.X,
                                    p1.Y * y_scale_inv * vsubsample);
                    edge.IsInvented = inventedFlag;
                    edges.Add(edge);
                }
            }
            edges.Sort();

            return edges.ToArray();
        }

        static byte[] stbtt__rasterize_sorted_edges(this TTFEdge[] edges, Size renderSize, int vsubsample, Point offset)
        {
            var result = new byte[renderSize.Width * renderSize.Height];

            var activeIsNext = new TTFActiveEdge();

            // int y, j = 0, eIndex = 0;
            int max_weight = (255 / vsubsample);        // weight per vertical scanline
            var scanline = Scanline.Instance.Request(renderSize.Width);


            var y = offset.Y * vsubsample;

            edges = edges.Append(new TTFEdge(new PointF(0, (offset.Y + renderSize.Height) * (float)vsubsample + 1), new PointF(), false)).ToArray();

            var lineIndex = 0;
            var eIndex = 0;

            byte[,] scanlines = new byte[renderSize.Height, renderSize.Width];

            while (lineIndex < renderSize.Height)
            {
                Array.Clear(scanline, 0, scanline.Length);
                // vertical subsample index
                for (var s = 0; s < vsubsample; ++s)
                {
                    // find center of pixel for this scanline
                    float scan_y = y + 0.5f;
                    TTFActiveEdge stepIsNext = activeIsNext;

                    // update all active edges;
                    // remove all active edges that terminate before the center of this scanline
                    while (stepIsNext.Next is not null)
                    {
                        var z = stepIsNext.Next;
                        if (z.EY <= scan_y)
                        {
                            stepIsNext.Next = z.Next; // delete from list
                            z.Valid = 0;
                        }
                        else
                        {
                            z.X += z.DX; // advance to position for current scanline
                            stepIsNext = stepIsNext.Next; // advance through list
                        }
                    }

                    for (; ; )
                    {
                        bool changed = false;
                        stepIsNext = activeIsNext;
                        while (stepIsNext.Next != null && stepIsNext.Next.Next != null)
                        {
                            if (stepIsNext.Next.X > stepIsNext.Next.Next.X)
                            {
                                TTFActiveEdge t = stepIsNext.Next;
                                TTFActiveEdge q = t.Next;

                                t.Next = q.Next;
                                q.Next = t;
                                stepIsNext.Next = q;
                                changed = true;
                            }
                            stepIsNext = stepIsNext.Next;
                        }
                        if (!changed)
                            break;
                    }

                    // insert all edges that start before the center of this scanline -- omit ones that also end on this scanline
                    while (edges[eIndex].P0.Y <= scan_y)
                    {
                        if (edges[eIndex].P1.Y > scan_y)
                        {
                            TTFActiveEdge z = edges[eIndex].new_active(offset.X, scan_y);
                            // find insertion point
                            if (activeIsNext.Next == null)
                                activeIsNext.Next = z;
                            else if (z.X < activeIsNext.Next.X)
                            {
                                // insert at front
                                z.Next = activeIsNext.Next;
                                activeIsNext.Next = z;
                            }
                            else
                            {
                                // find thing to insert AFTER
                                var p = activeIsNext.Next;
                                while (p.Next != null && p.Next.X < z.X)
                                    p = p.Next;
                                // at this point, p->next->x is NOT < z->x
                                z.Next = p.Next;
                                p.Next = z;
                            }
                        }
                        ++eIndex;
                    }

                    // now process all active edges in XOR fashion
                    if (activeIsNext.Next != null)
                        stbtt__fill_active_edges(scanline, renderSize.Width, activeIsNext.Next, max_weight);

                    ++y;
                }

                Array.Copy(scanline, 0, result, lineIndex * renderSize.Width, renderSize.Width);

                ++lineIndex;
            }

            return result;
        }

        private static int stbtt__tesselate_curve(this List<PointF> points, float x0, float y0, float x1, float y1, float x2, float y2, float objspace_flatness_squared, int n = 0)
        {
            // midpoint
            float mx = (x0 + 2 * x1 + x2) / 4;
            float my = (y0 + 2 * y1 + y2) / 4;
            // versus directly drawn line
            float dx = (x0 + x2) / 2 - mx;
            float dy = (y0 + y2) / 2 - my;
            if (n > 16) // 65536 segments on one curve better be enough!
                return 1;
            if (dx * dx + dy * dy > objspace_flatness_squared)
            {
                // half-pixel error allowed... need to be smaller if AA
                stbtt__tesselate_curve(points, x0, y0, (x0 + x1) / 2.0f, (y0 + y1) / 2.0f, mx, my, objspace_flatness_squared, n + 1);
                stbtt__tesselate_curve(points, mx, my, (x1 + x2) / 2.0f, (y1 + y2) / 2.0f, x2, y2, objspace_flatness_squared, n + 1);
            }
            else
            {
                points.Add(new PointF(x2, y2));
            }
            return 1;
        }

        private static TTFActiveEdge new_active(this TTFEdge edge, int off_x, float start_point)
        {
            //stbtt__active_edge z = (stbtt__active_edge *) STBTT_malloc(sizeof(*z), userdata); // @TODO: make a pool of these!!!
            var z = new TTFActiveEdge();
            float dxdy = (edge.P1.X - edge.P0.X) / (edge.P1.Y - edge.P0.Y);
            //STBTT_assert(e->y0 <= start_point);
            if (z == null)
                throw new ArgumentException();
            // round dx down to avoid going too far
            if (dxdy < 0)
                z.DX = -(int)Math.Floor(FIX * -dxdy);
            else
                z.DX = (int)Math.Floor(FIX * dxdy);
            z.X = (int)Math.Floor(FIX * (edge.P0.X + dxdy * (start_point - edge.P0.Y)));
            z.X -= off_x * FIX;
            z.EY = edge.P1.Y;
            z.Next = null;
            z.Valid = edge.IsInvented ? 1 : -1;
            return z;
        }

        private static void stbtt__fill_active_edges(byte[] scanline, int len, TTFActiveEdge? edge, int max_weight)
        {
            byte ab = 0;
            // non-zero winding fill
            int x0 = 0, w = 0;

            while (edge != null)
            {
                if (w == 0)
                {
                    // if we're currently at zero, we need to record the edge start point
                    x0 = edge.X;
                    w += edge.Valid;
                }
                else
                {
                    int x1 = edge.X;
                    w += edge.Valid;
                    // if we went to zero, we need to draw
                    if (w == 0)
                    {
                        int i = x0 >> FIXSHIFT;
                        int j = x1 >> FIXSHIFT;

                        if (i < len && j >= 0)
                        {
                            if (i == j)
                            {
                                // x0,x1 are the same pixel, so compute combined coverage
                                ab = (byte)(scanline[i] + (byte)((x1 - x0) * max_weight >> FIXSHIFT));
                                scanline[i] = ab;
                            }
                            else
                            {
                                if (i >= 0) // add antialiasing for x0
                                {
                                    ab = (byte)(scanline[i] + (byte)(((FIX - (x0 & FIXMASK)) * max_weight) >> FIXSHIFT));
                                    scanline[i] = ab;
                                }
                                else
                                    i = -1; // clip

                                if (j < len) // add antialiasing for x1
                                {
                                    ab = (byte)(scanline[j] + (byte)(((x1 & FIXMASK) * max_weight) >> FIXSHIFT));
                                    scanline[j] = ab;
                                }
                                else
                                    j = len; // clip

                                for (++i; i < j; ++i) // fill pixels between x0 and x1
                                {
                                    ab = (byte)(scanline[i] + (byte)max_weight);
                                    scanline[i] = ab;
                                }
                            }
                        }
                    }
                }
                edge = edge.Next;
            }
        }
    }
}
