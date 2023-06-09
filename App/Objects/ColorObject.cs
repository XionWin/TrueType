using Common;
using OpenTK.Mathematics;
using System.Drawing;

namespace App.Objects
{
    internal abstract class ColorObject : RenderObject
    {
        public Vector4 Color { get; set; }


        public ColorObject(Point location, Vector4 color)
        {
            this.Location = location;
            this.Color = color;
        }

        public override void OnLoad(Shader shader) => base.OnLoad(shader);

        public override void SetParameters(Shader shader)
        {
            base.SetParameters(shader);
        }

        public override void OnRenderFrame(Shader shader) => base.OnRenderFrame(shader);

    }
}