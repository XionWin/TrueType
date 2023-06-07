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
        public override IVertex2[]? Vertices => this._vertices;


        public Size Size { get; set; }

        public override Point Center => new Point(this.Location.X + this.Size.Width / 2, this.Location.Y + this.Size.Height / 2);

        public RectangleObject(Rectangle rect, Vector3 color) : base(rect.Location, color) => this.Size = rect.Size;

        public override void OnLoad(Shader shader)
        {
            var step = this.Size * 8;
            // Change vertices data
            _vertices = new IVertex2[]
            {
                new ColorVertex2(new Vector2(this.Location.X, this.Location.Y), this.Color),
                new ColorVertex2(new Vector2(this.Location.X + this.Size.Width, this.Location.Y), this.Color),
                new ColorVertex2(new Vector2(this.Location.X, this.Location.Y + this.Size.Height), this.Color),
                new ColorVertex2(new Vector2(this.Location.X + this.Size.Width, this.Location.Y), this.Color),
                new ColorVertex2(new Vector2(this.Location.X + this.Size.Width, this.Location.Y + this.Size.Height), this.Color),
                new ColorVertex2(new Vector2(this.Location.X, this.Location.Y + this.Size.Height), this.Color),
            };

            base.OnLoad(shader);

            shader.EnableAttribs(ColorVertex2.AttribLocations);
        }

        public override void SetParameters(Shader shader)
        {
            base.SetParameters(shader);
        }


        public override void OnRenderFrame(Shader shader)
        {
            //var step = this.Size * 8;
            //var points = Enumerable.Range(0, step).Select(x => new PointF((float)Math.Cos(Math.PI * 2 / step * x) * Size / 2 + Location.X, (float)Math.Sin(Math.PI * 2 / step * x) * Size / 2 + Location.Y));
            //this._vertices = points.Select(x => new ColorVertex2(new Vector2(x.X, x.Y), this.Color)).Cast<IVertex2>().ToArray();

            this.SetVBO();

            base.OnRenderFrame(shader);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, this.Vertices!.Length);

        }

    }
}