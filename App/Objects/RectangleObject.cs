using App.Objects;
using Common;
using Extension;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Linq;

namespace App.Objects
{
    internal class RectangleObject : ColorObject
    {
        private IVertex2[]? _vertices = null;
        public override IVertex2[] Vertices => this._vertices ?? throw new ArgumentException();

        private uint[]? _indices = null;
        public override uint[] Indices => this._indices ?? throw new ArgumentException();


        public Size Size { get; set; }

        public Point Offset { get; set; }

        public override Point Center => new Point(this.Location.X + this.Size.Width / 2, this.Location.Y + this.Size.Height / 2);

        public RectangleObject(Rectangle rect, Vector4 color, Point offset) : base(rect.Location, color)
        {
            this.Size = rect.Size;
            this.Offset = offset;
        }

        Random random = new Random();
        public override void OnLoad(Shader shader)
        {
            var left = (float)this.Location.X / 512;
            var top = (float)this.Location.Y / 512;
            var right = (float)(this.Location.X + this.Size.Width) / 512;
            var bottom = (float)(this.Location.Y + this.Size.Height) / 512;
            // Change vertices data
            _vertices = new IVertex2[]
            {
                //new ColorVertex2(new Vector2(this.Location.X, this.Location.Y), this.Color),
                //new ColorVertex2(new Vector2(this.Location.X + this.Size.Width, this.Location.Y), this.Color),
                //new ColorVertex2(new Vector2(this.Location.X + this.Size.Width, this.Location.Y + this.Size.Height), this.Color),
                //new ColorVertex2(new Vector2(this.Location.X, this.Location.Y + this.Size.Height), this.Color),

                new ColorTextureVertex2(new Vector2(this.Location.X + this.Offset.X, this.Location.Y + this.Offset.Y + 100), new Vector4(random.Next(2), random.Next(2), random.Next(2), 1), new Vector2(left, top)),
                new ColorTextureVertex2(new Vector2(this.Location.X + this.Offset.X + this.Size.Width, this.Location.Y + this.Offset.Y + 100), new Vector4(random.Next(2), random.Next(2), random.Next(2), 1), new Vector2(right, top)),
                new ColorTextureVertex2(new Vector2(this.Location.X + this.Offset.X + this.Size.Width, this.Location.Y + this.Offset.Y + this.Size.Height + 100), new Vector4(random.Next(2), random.Next(2), random.Next(2), 1), new Vector2(right, bottom)),
                new ColorTextureVertex2(new Vector2(this.Location.X + this.Offset.X, this.Location.Y + this.Offset.Y + this.Size.Height + 100), new Vector4(random.Next(2), random.Next(2), random.Next(2), 1), new Vector2(left, bottom)),
            };

            _indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            base.OnLoad(shader);

            shader.EnableAttribs(ColorTextureVertex2.AttribLocations);
        }
    }
}