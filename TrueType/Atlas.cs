using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueType
{
    public class Atlas
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public List<Skyline> Skylines { get; } = new List<Skyline>();

        public Atlas(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Skylines.Add(new Skyline(0, 0, this.Width));
        }

        private static Atlas _Instance = new Atlas(512, 512);
        public static Atlas Instance => _Instance;
    }
    public class Skyline
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }

        public Skyline(int x, int y, int width)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
        }
    }

    internal static class AtlasExtension
    {
        public static void FitAtlas(this Atlas atlas, Size glyphSize)
        {
            var atlasSize = new Size(atlas.Width, atlas.Height);
            var points = atlas.Skylines.Select(x => FitSkyline(x, atlasSize, glyphSize)).Where(x => x is not null).ToArray();
            var bestPoint = points.Min(x => x!.Value.Y);
        }

        public static System.Drawing.Point? FitSkyline(this Skyline skyline, Size atlasSize, Size glyphSize)
        {
            if (skyline.X + glyphSize.Width <= atlasSize.Width)
            {
                if (skyline.Y + glyphSize.Height <= atlasSize.Height)
                {
                    return new System.Drawing.Point(skyline.X + glyphSize.Width, skyline.Y);
                }
                else
                    return null;
            }
            else
                return null;
        }




        public static void FitSkylines(this Atlas atlas)
        {
            foreach (var skyline in atlas.Skylines)
            {
                var range = (skyline.X, skyline.X + skyline.Width);
                var overlapSkyLines = atlas.Skylines.Where(x => range.ContainsRange((x.X, x.X + x.Width)) is not null);


            }
        }

        public static (int start, int end)? ContainsRange(this (int start, int end) r1, (int start, int end) r2)
        {
            if (r1.Contains(r2.start) || r1.Contains(r2.end))
            {
                return (Math.Max(r1.start, r2.start), Math.Min(r1.end, r2.end));
            }
            else
            {
                return null;
            }
        }

        public static bool Contains(this (int start, int end) range, int value) => value > range.start && value < range.end;
    }
}
