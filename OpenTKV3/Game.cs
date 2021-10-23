using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTKV3
{
    class Game:GameWindow
    {
        string path = @"E:\Vstudio_Projects\OpenTKV3\OpenTKV3\";


        Camera cam = new Camera();

        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";


        Vector2[] texcoorddata;


        int ibo_elements;

        Vector3[] vertdata;
        Vector3[] coldata;
        List<Volume> objects = new List<Volume>();

        int[] indicedata;

        float time = 0.0f;

        void initProgram()
        {
            GL.GenBuffers(1, out ibo_elements);
            shaders.Add("default", new ShaderProgram(path+"vs.glsl", path+"fs.glsl", true));
            ObjVolume obj1 = ObjVolume.LoadFromFile();
            objects.Add(obj1);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initProgram();

            Title = "Hello OpenTK!";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            shaders[activeShader].EnableVertexAttribArrays();


            int indiceat = 0;

            foreach (Volume v in objects)
            {
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);
                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }

            shaders[activeShader].DisableVertexAttribArrays();


            GL.Flush();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();

            int vertcount = 0;

            foreach (Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                vertcount += v.VertCount;
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();

            //time += 0.005f;
            base.OnUpdateFrame(e);

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }


            objects[0].Position = new Vector3(0.3f, -0.05f + (float)Math.Sin(time), -10.0f);
            objects[0].Rotation = new Vector3(0f, 0f, 0f);
            objects[0].Scale = new Vector3(1f, 1f, 1f);

            //Random rand = new Random();

            //for (int i = 0; i < 100; i++)
            //{
            //    ColorCube c = new ColorCube(new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
            //    c.Position = new Vector3((float)rand.Next(-4, 4), (float)rand.Next(-4, 4), (float)rand.Next(-8, 8));
            //    c.Rotation = new Vector3((float)rand.Next(0, 6), (float)rand.Next(0, 6), (float)rand.Next(0, 6));
            //    c.Scale = Vector3.One * ((float)rand.NextDouble() + 0.2f);
            //    objects.Add(c);
            //}

            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                //v.ViewProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
                v.ViewProjectionMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(shaders[activeShader].ProgramID);


            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
            
            
            ProcessInput();

        }

        private void ProcessInput()
        {

            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                cam.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                cam.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                cam.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                cam.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                cam.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                cam.Move(0f, 0f, -0.1f);
            }
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }
    }
}
