using App.Objects;
using Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Objects
{
    internal abstract class RenderObject : IRenderObject
    {
        public int VAO { get; set; }

        public int VBO { get; set; }

        public int EBO { get; set; }

        public Point Location { get; set; }

        public Matrix3 Matrix { get; set; } = Matrix3.Identity;
        public abstract IVertex2[] Vertices { get; }
        public abstract uint[] Indices { get; }

        public abstract Point Center { get; }


        public virtual void OnLoad(Shader shader)
        {
            this.VAO = GL.GenVertexArray();
            this.VBO = GL.GenBuffer();
            this.EBO = GL.GenBuffer();

            GL.BindVertexArray(this.VAO);
            SetVBO();
        }

        public void SetVBO()
        {
            // bind vbo and set data for vbo
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.VBO);
            var vertices = this.Vertices.GetRaw();
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // bind ebo and set data for ebo
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);
        }

        public virtual void SetParameters(Shader shader)
        {
            //shader.UniformMatrix3("aTransform", this.Matrix);
            //shader.Uniform2("aCenter", new Vector2(this.Center.X, this.Center.Y));

            // Active texture
            shader.Uniform1("aTexture", 0);
            //shader.Uniform2("aTexOffset", new Vector2(0, 0));
            //shader.Uniform1("aMode", 0);
            shader.Uniform1("aPointSize", 0);
        }

        public virtual void OnRenderFrame(Shader shader)
        {
            // Bind the VAO
            GL.BindVertexArray(this.VAO);

            this.SetParameters(shader);

            // Enable Alpha
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.DrawElements(PrimitiveType.Triangles, this.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.VBO);
            GL.BindVertexArray(this.VAO);

            // Delete all the resources.
            GL.DeleteBuffer(this.VBO);
            GL.DeleteVertexArray(this.VAO);
        }
    }
}