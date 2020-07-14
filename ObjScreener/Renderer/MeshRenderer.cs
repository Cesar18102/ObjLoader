using System;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Drawing.Imaging;

using ObjScreener.Data;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ObjScreener.Renderer
{
    public class MeshRenderer : GameWindow
    {
        private int DrawProgram { get; set; }
        private int Texture { get; set; }

        private int VertexBufferName { get; set; }
        private int VertexIndexBufferName { get; set; }

        private Mesh Mesh { get; }
        private bool Saved { get; set; }

        public string SavePath { get; set; }
        public bool CloseOnSaved { get; set; } = true;

        public float AngleX { get; set; } = 0;
        public float AngleY { get; set; } = 0;
        public float AngleZ { get; set; } = 0;

        public float CameraDistance { get; set; } = 40;
        public Color4 BackColor { get; set; } = Color4.White;

        public MeshRenderer(int width, int height, Mesh mesh) : 
            base(width, height, GraphicsMode.Default, "Some window title", GameWindowFlags.Fullscreen)
        {
            Mesh = mesh;
            VSync = VSyncMode.On;
        }

        private void Save()
        {
            if (SavePath == null)
                throw new InvalidOperationException("No save path specified");

            byte[] buffer = new byte[Width * Height * 3 * sizeof(byte)];
            GL.ReadPixels(0, 0, Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, buffer);

            using (Bitmap bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                Rectangle rect = new Rectangle(0, 0, Width, Height);
                BitmapData data = bitmap.LockBits(
                    rect, ImageLockMode.ReadWrite, bitmap.PixelFormat
                );
                Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
                bitmap.UnlockBits(data);

                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                bitmap.Save(SavePath);
            }

            Saved = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(BackColor);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);

            InitBuffers();
            InitTexture();

            DrawProgram = PreloadShaders(null, null);
                /*"varying float z; void main() { gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex; z = gl_Vertex.z; }",
                "varying float z; void main() { gl_FragColor = vec4(cos(z), 0.0, sin(z), 1.0); }"
            );*/

            GL.UseProgram(DrawProgram);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(
                ClientRectangle.X, ClientRectangle.Y, 
                ClientRectangle.Width, ClientRectangle.Height
            );

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4, Width / (float)Height, 1.0f, 512.0f
            );

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(BackColor);

            Matrix4 modelview = Matrix4.LookAt(
                new Vector3(CameraDistance, 0, 0), 
                new Vector3(0, 0, 0), Vector3.UnitY
            );

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.PushMatrix();

            GL.Rotate(AngleX, Vector3.UnitX);
            GL.Rotate(AngleY, Vector3.UnitY);
            GL.Rotate(AngleZ, Vector3.UnitZ);

            GL.Translate(-Mesh.Geometry.BoundingBox.Center);

            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VertexIndexBufferName);
            GL.DrawElements(BeginMode.Triangles, Mesh.Geometry.Indexes.Length, DrawElementsType.UnsignedInt, 0);

            GL.Translate(Mesh.Geometry.BoundingBox.Center);

            GL.Rotate(-AngleX, Vector3.UnitX);
            GL.Rotate(-AngleY, Vector3.UnitY);
            GL.Rotate(-AngleZ, Vector3.UnitZ);

            GL.PopMatrix();

            if (!Saved)
            {
                Save();

                if (CloseOnSaved)
                    this.Close();
            }

            SwapBuffers();
        }

        private void InitTexture()
        {
            Texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            BitmapData data = Mesh.Texture.LockBits(
                new Rectangle(0, 0, Mesh.Texture.Width, Mesh.Texture.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            GL.TexImage2D(
                TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0
            );

            Mesh.Texture.UnlockBits(data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        private void InitBuffers()
        {
            int structSize = Marshal.SizeOf(typeof(ReusablePoint));
            int vector3Size = Marshal.SizeOf(typeof(Vector3));

            VertexBufferName = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                structSize * Mesh.Geometry.Points.Length,
                Mesh.Geometry.Points, BufferUsageHint.StaticDraw
            );

            VertexIndexBufferName = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VertexIndexBufferName);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                sizeof(uint) * Mesh.Geometry.Indexes.Length,
                Mesh.Geometry.Indexes, BufferUsageHint.StaticDraw
            );

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, structSize, 0);

            GL.EnableClientState(ArrayCap.NormalArray);
            GL.NormalPointer(NormalPointerType.Float, structSize, vector3Size);

            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, structSize, vector3Size * 2);
        }

        private int MakeShader(ShaderType type, string code)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            return shader;
        }

        private int PreloadShaders(string vertexShaderCode, string fragmentShaderCode)
        {
            int program = GL.CreateProgram();

            if (vertexShaderCode != null)
            {
                int vertexShader = MakeShader(ShaderType.VertexShader, vertexShaderCode);
                GL.AttachShader(program, vertexShader);
            }

            if (fragmentShaderCode != null)
            {
                int fragmentShader = MakeShader(ShaderType.FragmentShader, fragmentShaderCode);
                GL.AttachShader(program, fragmentShader);
            }

            GL.LinkProgram(program);
            return program;
        }
    }
}
