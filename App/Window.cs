using App.Objects;
using Common;
using Extension;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrueType2.Domain.Cache.Pixel;

namespace App
{
    public class Window : GLWindow
    {
        public Window(int width, int height) : base("TrueType", width, height)
        { }

        private readonly IVertex2[] _vertices = new IVertex2[]
        {
            //new ColorTextureVertex2(new Vector2(16f, 16f), new Vector4(1, 0, 1, 1), new Vector2(0.0f, 0.0f)),
            //new ColorTextureVertex2(new Vector2(496f, 16f), new Vector4(1, 0, 0, 1), new Vector2(1f, 0.0f)),
            //new ColorTextureVertex2(new Vector2(496f, 496f), new Vector4(0, 1, 0, 1), new Vector2(1f, 1f)),
            //new ColorTextureVertex2(new Vector2(16f, 496f), new Vector4(0, 0, 1, 1), new Vector2(0.0f, 1f)),
            new ColorTextureVertex2(new Vector2(0, 512f), new Vector4(1, 1, 1, 1), new Vector2(0.0f, 0.0f)),
            new ColorTextureVertex2(new Vector2(512f, 512f), new Vector4(1, 1, 1, 1), new Vector2(1f, 0.0f)),
            new ColorTextureVertex2(new Vector2(512f, 1024f), new Vector4(1, 1, 1, 1), new Vector2(1f, 1f)),
            new ColorTextureVertex2(new Vector2(0, 1024f), new Vector4(1, 1, 1, 1), new Vector2(0.0f, 1f)),
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



        private List<IRenderObject> _renderObjects = new List<IRenderObject>();


        protected override void OnLoad()
        {

            var path = @"Resources/Fonts/Zpix.ttf";

            if (File.Exists(path))
            {
                var ttf = new TrueType2.Domain.TTF("Zpix", path);

                Random random = new Random();
                var fontSize = 12;
                foreach (var c in "，看看现在的效果怎么样了？还可以吧。，看看现在的效果怎么样了？还可以吧。，看看现在的效果怎么样了？还可以吧。，看看现在的效果怎么样了？还可以吧。")
                {
                    var bitmap = ttf.GetGlyph(c, fontSize, 0);

                    _renderObjects.Add(new RectangleObject(new Rectangle(bitmap.Rectangle.X, bitmap.Rectangle.Y, bitmap.Rectangle.Width, bitmap.Rectangle.Height), new Vector4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1), new Point(bitmap.Offset.X, bitmap.Offset.Y)));
                }
            }


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



            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            //_texture = new Texture(TextureUnit.Texture0, TextureMinFilter.Nearest).With(x => x.LoadRaw(TrueType.Cache.Instance.Pixels, w, h, PixelFormat.Alpha, PixelInternalFormat.Rgba));

            var fontBitmapCache = BitmapCache.Instance.First().Value;

            var canvases = BitmapCache.Instance.Values.SelectMany(x => x.Values).ToArray();

            var canvas = canvases[DateTime.Now.Second % canvases.Count()];

            _texture = new Texture(TextureUnit.Texture0, TextureMinFilter.Nearest).With(x => x.LoadRaw(canvas.Pixels, canvas.Size.Width, canvas.Size.Height, PixelFormat.Alpha, PixelInternalFormat.Rgba));


            //var subData = new byte[] {
            //    0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
            //    0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF,
            //    0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
            //    0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF,
            //    0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
            //    0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF,
            //    0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00,
            //    0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF,
            //};

            //var x = 0;
            //var y = 0;
            //var w = 8;
            //var h = 8;

            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            ////GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, x);
            ////GL.PixelStore(PixelStoreParameter.UnpackSkipRows, y);
            //GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, PixelFormat.Alpha, PixelType.UnsignedByte, subData);

            foreach (var renderObject in _renderObjects)
            {
                renderObject.OnLoad(this.Shader);
            }


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


            Random random = new Random();
            foreach (var renderObject in _renderObjects)
            {
                //if (renderObject is PointObject pointObject)
                //{
                //    pointObject.Location = new Point(random.Next(this.Size.X), random.Next(this.Size.Y));
                //}
                renderObject.OnRenderFrame(this.Shader);
            }

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
