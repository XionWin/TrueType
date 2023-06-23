using App.Objects;
using Common;
using Extension;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Linq;

namespace App.Objects
{
    internal class RectangleObject : RenderObject
    {
        private IVertex2[]? _vertices = null;
        public override IVertex2[] Vertices => this._vertices ?? throw new ArgumentException();

        private uint[]? _indices = null;
        public override uint[] Indices => this._indices ?? throw new ArgumentException();

        public Rectangle Rectangle { get; set; }
        public Vector4 Color { get; set; }
        public RectangleF TexCoord { get; set; }
        public Matrix3 Matrix { get; set; } = Matrix3.Identity;
        public Point Offset { get; set; }


        public override Point Center => new Point(this.Rectangle.X + this.Rectangle.Width / 2, this.Rectangle.Y + this.Rectangle.Height / 2);

        public RectangleObject(Rectangle rectangle, Vector4 color, RectangleF texCoord, Point offset)
        {
            this.Rectangle = rectangle;
            this.Color = color;
            this.TexCoord = texCoord;
            this.Offset = offset;
        }

        public override void OnLoad(Shader shader)
        {
            var offsetX = this.Offset.X; 
            var offsetY = this.Offset.Y;

            // Change vertices data
            _vertices = new IVertex2[]
            {
                new ColorTextureVertex2(new Vector2(this.Rectangle.X + offsetX, this.Rectangle.Y + offsetY), this.Color, new Vector2(this.TexCoord.Left, this.TexCoord.Top)),
                new ColorTextureVertex2(new Vector2(this.Rectangle.X + offsetX + this.Rectangle.Width, this.Rectangle.Y + offsetY), this.Color, new Vector2(this.TexCoord.Right, this.TexCoord.Top)),
                new ColorTextureVertex2(new Vector2(this.Rectangle.X + offsetX + this.Rectangle.Width, this.Rectangle.Y + offsetY + this.Rectangle.Height), this.Color, new Vector2(this.TexCoord.Right, this.TexCoord.Bottom)),
                new ColorTextureVertex2(new Vector2(this.Rectangle.X + offsetX, this.Rectangle.Y + offsetY + this.Rectangle.Height), this.Color, new Vector2(this.TexCoord.Left, this.TexCoord.Bottom)),
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