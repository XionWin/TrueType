

using OpenTK.Mathematics;

namespace Common
{
    public struct AttribLocation
    {
        public string Name { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        public AttribLocation(string name, int start, int len)
        {
            this.Name = name;
            this.Start = start;
            this.Length = len;
        }
    }

    public interface IVertex2
    {
        public Vector2 Position { get; init; }
        public float[] Raw { get; }
    }

    public struct ColorVertex2 : IVertex2
    {
        public readonly static IEnumerable<AttribLocation> AttribLocations = new[]
        {
            new AttribLocation("aPos", 0, 2),
            new AttribLocation("aColor", 2, 4),
        };
        public Vector2 Position { get; init; }
        public Vector4 Color { get; init; }

        private float[]? raw = null;
        public float[] Raw
        {
            get
            {
                if (raw is null)
                {
                    raw = this.GetRaw();
                }
                return raw;
            }
        }

        public ColorVertex2(Vector2 position, Vector4 color)
        {
            this.Position = position;
            this.Color = color;
        }
    }

    public struct TextureVertex2: IVertex2
    {
        public readonly static IEnumerable<AttribLocation> AttribLocations = new[]
        {
            new AttribLocation("aPos", 0, 2),
            new AttribLocation("aTexCoord", 2, 2),
        };

        public Vector2 Position { get; init; }
        public Vector2 Coordinate { get; init; }

        private float[]? raw = null;
        public float[] Raw
        {
            get
            {
                if (raw is null)
                {
                    raw = this.GetRaw();
                }
                return raw;
            }
        }

        public TextureVertex2(Vector2 position, Vector2 coordinate)
        {
            this.Position = position;
            this.Coordinate = coordinate;
        }
    }


    public struct ColorTextureVertex2 : IVertex2
    {
        public readonly static IEnumerable<AttribLocation> AttribLocations = new[]
        {
            new AttribLocation("aPos", 0, 2),
            new AttribLocation("aColor", 2, 4),
            new AttribLocation("aTexCoord", 6, 2),
        };

        public Vector2 Position { get; init; }
        public Vector4 Color { get; init; }
        public Vector2 Coordinate { get; init; }

        private float[]? raw = null;
        public float[] Raw
        {
            get
            {
                if (raw is null)
                {
                    raw = this.GetRaw();
                }
                return raw;
            }
        }

        public ColorTextureVertex2(Vector2 position, Vector4 color, Vector2 coordinate)
        {
            this.Position = position;
            this.Color = color;
            this.Coordinate = coordinate;
        }
    }

    public static class Vertex2Extension
    {
        public static float[] GetRaw(this ColorVertex2 vertex) => new[] { vertex.Position.X, vertex.Position.Y, vertex.Color.X, vertex.Color.Y, vertex.Color.Z, vertex.Color.W };
        public static float[] GetRaw(this TextureVertex2 vertex) => new[] { vertex.Position.X, vertex.Position.Y, vertex.Coordinate.X, vertex.Coordinate.Y };
        public static float[] GetRaw(this ColorTextureVertex2 vertex) => new[] { vertex.Position.X, vertex.Position.Y, vertex.Color.X, vertex.Color.Y, vertex.Color.Z, vertex.Color.W, vertex.Coordinate.X, vertex.Coordinate.Y };
        public static float[] GetRaw(this IEnumerable<IVertex2> vertices) => vertices.SelectMany(x => x.Raw).ToArray();
    }
}
