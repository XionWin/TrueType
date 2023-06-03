using TrueType2.Domain;
using TrueType2.Mode;

namespace TrueType2.Extension
{
    public static class TTTFVectorExtension
    {
        public static TTFVector GetVector(this TTFRaw raw, int index) =>
            new TTFVector(raw.GetShape(index));

        public static Vertex[] GetShape(this TTFRaw raw, int index)
        {
            var offset = raw.GetGlyphOffset(index);
            short numberOfContours = raw.GetNumber<short>(offset);

            return numberOfContours switch
            {
                0 => throw new ArgumentException(),    // Do nothing.
                > 0 => raw.GetSimpleShape(offset, numberOfContours, index),
                < 0 => raw.GetCompositeShape(offset, numberOfContours, index),     // Composite Glyph == -1
            };
        }

        private static Vertex[] GetSimpleShape(this TTFRaw raw, int offset, int numberOfContours, int index)
        {
            int iData = offset + 10;

            var ins = raw.GetNumber<short>(iData + numberOfContours * 2);

            int iPoints = iData + numberOfContours * 2 + 2 + ins;

            var n = 1 + raw.GetNumber<ushort>(numberOfContours * 2 - 2 + iData);
            var off = 2 * numberOfContours;
            var m = n + off;
            var vertices = new Vertex[m];
            // first load flags
            var flagcount = 0;
            var points = raw.Data;
            byte flags = 0;
            for (var i = 0; i < n; ++i)
            {
                if (flagcount == 0)
                {
                    flags = points[iPoints++];
                    if ((flags & 8) != 0)
                        flagcount = points[iPoints++];
                }
                else
                    --flagcount;
                vertices[off + i].Type = (VertexType)flags;
            }

            // now load x coordinates
            var x = 0;
            for (var i = 0; i < n; ++i)
            {
                flags = (byte)vertices[off + i].Type;
                if ((flags & 2) != 0)
                {
                    short dx = points[iPoints++];
                    x += (flags & 16) != 0 ? dx : -dx; // ???
                }
                else
                {
                    if (!((flags & 16) != 0))
                    {
                        x = x + (short)((points[0 + iPoints] * 256) + points[1 + iPoints]);
                        iPoints += 2;
                    }
                }
                vertices[off + i].X = (short)x;
            }


            // now load y coordinates
            var y = 0;
            for (var i = 0; i < n; ++i)
            {
                flags = (byte)vertices[off + i].Type;
                if ((flags & 4) != 0)
                {
                    short dy = points[iPoints++];
                    y += (flags & 32) != 0 ? dy : -dy; // ???
                }
                else
                {
                    if (!((flags & 32) != 0))
                    {
                        y = y + (short)((points[0 + iPoints] * 256) + points[1 + iPoints]);
                        iPoints += 2;
                    }
                }
                vertices[off + i].Y = (short)y;
            }


            // now convert them to our format
            var num_vertices = 0;

            int cx, cy, sx, sy, scx, scy;
            sx = sy = cx = cy = scx = scy = 0;
            var next_move = 0;
            var was_off = 0;
            var start_off = 0;
            var j = 0;

            for (var i = 0; i < n; ++i)
            {
                flags = (byte)vertices[off + i].Type;
                x = (short)vertices[off + i].X;
                y = (short)vertices[off + i].Y;

                if (next_move == i)
                {
                    if (i != 0)
                        num_vertices = VertexsCloseShape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);

                    // now start the new one               
                    start_off = 0 == (flags & 1) ? 1 : 0;
                    if (start_off != 0)
                    {
                        // if we start off with an off-curve point, then when we need to find a point on the curve
                        // where we can start, and we need to save some state for when we wraparound.
                        scx = x;
                        scy = y;
                        if (!(((int)vertices[off + i + 1].Type & 1) != 0))
                        {
                            // next point is also a curve point, so interpolate an on-point curve
                            sx = (x + (int)vertices[off + i + 1].X) >> 1;
                            sy = (y + (int)vertices[off + i + 1].Y) >> 1;
                        }
                        else
                        {
                            // otherwise just use the next point as our start point
                            sx = (int)vertices[off + i + 1].X;
                            sy = (int)vertices[off + i + 1].Y;
                            ++i; // we're using point i+1 as the starting point, so skip it
                        }
                    }
                    else
                    {
                        sx = x;
                        sy = y;
                    }
                    SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.MoveTo, sx, sy, 0, 0);
                    was_off = 0;
                    next_move = 1 + raw.GetNumber<ushort>(iData + j * 2);
                    ++j;
                }
                else
                {
                    if (!((flags & 1) != 0))
                    { // if it's a curve
                        if (was_off != 0) // two off-curve control points in a row means interpolate an on-curve midpoint
                            SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.CurveTo, (cx + x) >> 1, (cy + y) >> 1, cx, cy);
                        cx = x;
                        cy = y;
                        was_off = 1;
                    }
                    else
                    {
                        if (was_off != 0)
                            SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.CurveTo, x, y, cx, cy);
                        else
                            SetVertex(ref vertices[num_vertices++], num_vertices, VertexType.LineTo, x, y, 0, 0);
                        was_off = 0;
                    }
                }
            }

            num_vertices = VertexsCloseShape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);
            return vertices.Take(num_vertices).ToArray();
        }

        private static Vertex[] GetCompositeShape(this TTFRaw raw, int offset, int numberOfContours, int index)
        {
            int more = 1;
            byte[] compositeData = raw.Data;
            int iCompositeData = offset + 10;
            var num_vertices = 0;
            Vertex[]? vertices = null;

            while (more != 0)
            {
                ushort flags, gidx;
                int comp_num_verts = 0, i;
                Vertex[]? comp_verts = null, tmp = null;
                float[] mtx = { 1, 0, 0, 1, 0, 0 };
                float m, n;

                flags = raw.GetNumber<ushort>(iCompositeData);
                iCompositeData += 2;
                gidx = raw.GetNumber<ushort>(iCompositeData);
                iCompositeData += 2;

                if ((flags & 2) != 0)
                { // XY values
                    if ((flags & 1) != 0)
                    { // shorts
                        mtx[4] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 2;
                        mtx[5] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 2;
                    }
                    else
                    {
                        mtx[4] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 1;
                        mtx[5] = raw.GetNumber<short>(iCompositeData);
                        iCompositeData += 1;
                    }
                }
                else
                {
                    // @TODO handle matching point
                    throw new NotImplementedException();
                }
                if ((flags & (1 << 3)) != 0)
                { // WE_HAVE_A_SCALE
                    mtx[0] = mtx[3] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[1] = mtx[2] = 0;
                }
                else if ((flags & (1 << 6)) != 0)
                { // WE_HAVE_AN_X_AND_YSCALE
                    mtx[0] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[1] = mtx[2] = 0;
                    mtx[3] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                }
                else if ((flags & (1 << 7)) != 0)
                { // WE_HAVE_A_TWO_BY_TWO
                    mtx[0] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[1] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[2] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                    mtx[3] = raw.GetNumber<short>(iCompositeData) / 16384.0f;
                    iCompositeData += 2;
                }

                // Find transformation scales.
                m = (float)Math.Sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
                n = (float)Math.Sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);

                // Get indexed glyph.
                comp_verts = raw.GetShape(gidx);
                comp_num_verts = comp_verts?.Length ?? 0;
                if (comp_num_verts > 0)
                {
                    // Transform vertices.
                    for (i = 0; i < comp_num_verts; ++i)
                    {
                        Vertex v = comp_verts![i];
                        short x, y; // stbtt_vertex_type = short;
                        x = v.X;
                        y = v.Y;
                        v.X = (short)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                        v.Y = (short)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                        x = v.CenterX;
                        y = v.CenterY;
                        v.CenterX = (short)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                        v.CenterY = (short)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                    }

                    // Append vertices.
                    //tmp = (stbtt_vertex*)STBTT_malloc((num_vertices+comp_num_verts)*sizeof(stbtt_vertex), info->userdata);
                    tmp = new Vertex[num_vertices + comp_num_verts];
                    if (tmp == null)
                    {
                        //if (vertices) STBTT_free(vertices, info->userdata);
                        //if (comp_verts) STBTT_free(comp_verts, info->userdata);
                        throw new ArgumentException();
                    }
                    if (num_vertices > 0)
                        //memcpy(tmp, vertices, num_vertices*sizeof(stbtt_vertex));
                        Array.Copy(vertices!, tmp, num_vertices);
                    //memcpy(tmp+num_vertices, comp_verts, comp_num_verts*sizeof(stbtt_vertex));
                    Array.Copy(comp_verts!, 0, tmp, num_vertices, comp_num_verts);
                    //if (vertices) 
                    //	STBTT_free(vertices, info->userdata);
                    vertices = tmp;
                    //STBTT_free(comp_verts, info->userdata);
                    num_vertices += comp_num_verts;
                }
                // More components ?
                more = flags & (1 << 5);
            }
            return vertices!;
        }


        private static int VertexsCloseShape(Vertex[] vertices, int num_vertices, int was_off, int start_off,
                                    int sx, int sy, int scx, int scy, int cx, int cy)
        {
            if (start_off != 0)
            {
                if (was_off != 0)
                    SetVertex(ref vertices[num_vertices++], num_vertices,
                        VertexType.CurveTo, (cx + scx) >> 1, (cy + scy) >> 1, cx, cy);
                SetVertex(ref vertices[num_vertices++], num_vertices,
                    VertexType.CurveTo, sx, sy, scx, scy);
            }
            else
            {
                if (was_off != 0)
                    SetVertex(ref vertices[num_vertices++], num_vertices,
                        VertexType.CurveTo, sx, sy, cx, cy);
                else
                    SetVertex(ref vertices[num_vertices++], num_vertices,
                        VertexType.LineTo, sx, sy, 0, 0);
            }
            return num_vertices;
        }

        private static void SetVertex(ref Vertex v, int numVert, VertexType type, int x, int y, int cx, int cy)
        {
            if (numVert == 15)
            {
                numVert = 15;
            }

            v.Type = type;
            v.X = (short)x;
            v.Y = (short)y;
            v.CenterX = (short)cx;
            v.CenterY = (short)cy;
        }

    }
}
