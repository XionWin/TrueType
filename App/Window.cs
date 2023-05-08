using Common;
using Extension;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Window : GLWindow
    {
        public Window(int width, int height) : base("TrueType", width, height)
        { }

        private readonly IVertex2[] _vertices = new IVertex2[]
        {
            new ColorTextureVertex2(new Vector2(100f, 100f), new Vector4(1, 0, 1, 1), new Vector2(0.0f, 0.0f)),
            new ColorTextureVertex2(new Vector2(612f, 100f), new Vector4(1, 0, 0, 1), new Vector2(1f, 0.0f)),
            new ColorTextureVertex2(new Vector2(612f, 612f), new Vector4(0, 1, 0, 1), new Vector2(1f, 1f)),
            new ColorTextureVertex2(new Vector2(100f, 612f), new Vector4(0, 0, 1, 1), new Vector2(0.0f, 1f)),
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _vbo;

        private int _vao;

        private int _ebo;

        private Texture? _texture;

        private int _uniformViewPort;

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(Color.MidnightBlue);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            // bind vbo and set data for vbo
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            var vertices = _vertices.GetRaw();
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // bind ebo and set data for ebo
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            this.Shader.EnableAttribs(ColorTextureVertex2.AttribLocations);


            var data = new byte[] {
                0xFF, 0x00, 0x00, 0xFF
            };

            var w = 2;
            var h = 2;
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            _texture = new Texture(TextureUnit.Texture0, TextureMinFilter.Nearest).With(x => x.LoadRaw(data, w, h, PixelFormat.Alpha, PixelInternalFormat.Alpha));


            //var subData = new byte[] { 
            //    0xFF, 0x00, 0x00, 0xFF
            //};

            //x = 0;
            //y = 0;
            //w = 2;
            //h = 2;

            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, x);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipRows, y);
            //GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, PixelFormat.Alpha, PixelType.UnsignedByte, subData);


            this._uniformViewPort = GL.GetUniformLocation(this.Shader.ProgramHandle, "aViewport");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, this.Size.X, this.Size.Y);
            GL.Disable(EnableCap.DepthTest);
            // Bind the VAO
            GL.BindVertexArray(_vao);

            // Enable Alpha
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.FrontAndBack);

            // Active texture
            this.Shader.Uniform1("aTexture", 0);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.Enable(EnableCap.DepthTest);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (this.KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.Uniform3(this._uniformViewPort, this.Size.X, this.Size.Y, 1.0f);
        }

        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);

            GL.DeleteProgram(Shader.ProgramHandle);

            base.OnUnload();
        }
    }
}
