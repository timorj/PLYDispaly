  
using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Assimp.Configs;

namespace DataLoad
{

    public class Window : GameWindow
    {
        #region 变量
  
        #region 基本变量
     
        //模型
        private int _objectVBO;

        private int _objectVAO;

        //灯光
        private int _lightVBO;

        private int _lightVAO;

        private Shader objectshader;

        private Shader lightshader;

        private Texture _diffuseMap;

        private Texture _specularMap;

        private Matrix4 model;

        private Matrix4 lightmodel;

        private Matrix4 view;

        private Matrix4 projection;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private readonly Vector3[] lightPositions =
        {
            new Vector3(0.7f, 0.2f, 2.0f),
            new Vector3(2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f, 2.0f, -12.0f),
            new Vector3(0.0f, 0.0f, -3.0f)
        };
        #region 光源顶点
        private readonly float[] _lightvertices = {
       	    // positions         
        -0.5f, -0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,

        -0.5f, -0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f, -0.5f,  0.5f,

        -0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,

         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,

        -0.5f, -0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
         0.5f, -0.5f,  0.5f,
         0.5f, -0.5f,  0.5f,
        -0.5f, -0.5f,  0.5f,
        -0.5f, -0.5f, -0.5f,

        -0.5f,  0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
         0.5f,  0.5f,  0.5f,
         0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f,
        -0.5f,  0.5f, -0.5f,

        };
        #endregion
        #region 物体顶点
        private readonly float[] _objectvertices = {

         -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  0.0f,

        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  1.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
        -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,
        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  0.0f,

         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  1.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  1.0f
        };
        #endregion
        #endregion
        private struct Object{
            public Matrix4 model;
            public Matrix4 view;
            public Matrix4 projection;
            public Vector3 ambient;
            public Vector3 diffuse;
            public Vector3 specular;
            public float shininess;

            public Object(Matrix4 model,Matrix4 view,Matrix4 projection,Vector3 ambient,Vector3 diffuse,Vector3 specular,float shininess)
            {
                this.model = model;
                this.view = view;
                this.projection = projection;
                this.ambient = ambient;
                this.diffuse = diffuse;
                this.specular = specular;
                this.shininess = shininess;
            }
            }
       

        #endregion

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }



        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.CullFace);
            //DataLoad();
           
            //模型
            GL.GenBuffers(1, out _objectVBO);           
            GL.BindBuffer(BufferTarget.ArrayBuffer, _objectVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _objectvertices.Length * sizeof(float), _objectvertices, BufferUsageHint.StaticDraw);
                     
            objectshader = new Shader("../../Shaders/objectshader.vert", "../../Shaders/objectshader.frag");
            objectshader.Use();

            //纹理
            _diffuseMap = new Texture("../../Pictures/container2.png");
         

            _specularMap = new Texture("../../Pictures/container2_specular.png");
             
            GL.GenVertexArrays(1,out _objectVAO);
            GL.BindVertexArray(_objectVAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _objectVBO);
            var vertexLocation = objectshader.GetAtrribLocation("aPos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _objectVBO);
            var normalLocation = objectshader.GetAtrribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, _objectVBO);
            var textureLocation = objectshader.GetAtrribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textureLocation);
            GL.VertexAttribPointer(textureLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float),6 * sizeof(float));

  
            //灯光
            _lightVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lightVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _lightvertices.Length * sizeof(float), _lightvertices, BufferUsageHint.StaticDraw);

            lightshader = new Shader("../../Shaders/objectshader.vert", "../../Shaders/lightshader.frag");
    
            _lightVAO = GL.GenVertexArray();
            GL.BindVertexArray(_lightVAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _lightVBO) ;
            vertexLocation = lightshader.GetAtrribLocation("aPos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);




            _camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);
            CursorVisible = false;
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            objectshader.Use();
            _diffuseMap.Use();
            _specularMap.Use(TextureUnit.Texture1);
            

            model = Matrix4.CreateRotationX(30f);
            view = _camera.GetViewMatrix();
            projection = _camera.GetProjectionMatrix();
            objectshader.SetMatrix4("model", model);
            objectshader.SetMatrix4("view", view);
            objectshader.SetMatrix4("projection", projection);
            
            //摄像机位置
            objectshader.SetVector3("viewPos", _camera.Position);
            
            //物体材质
            
            objectshader.SetInt("material.diffuse", 0);
            objectshader.SetInt("material.specular",1);
            objectshader.SetVector3("material.specular", new Vector3(0.5f));
            objectshader.SetFloat("material.shininess", 32.0f);

            //光源设置
            //平行光
            objectshader.SetVector3("dirlight.direction", new Vector3(-0.2f, -1.0f, -0.3f));
            objectshader.SetVector3("dirlight.ambient", new Vector3(0.5f));
            objectshader.SetVector3("dirlight.diffuse", new Vector3(0.4f));
            objectshader.SetVector3("dirlight.specular", new Vector3(0.5f));
            
            //点光源
            for (int i = 0; i < lightPositions.Length; i++)
            {
                objectshader.SetVector3($"pointlights[{i}].position", lightPositions[i]);
                objectshader.SetVector3($"pointlights[{i}].ambient", new Vector3(0.05f, 0.05f, 0.05f));
                objectshader.SetVector3($"pointlights[{i}].diffuse", new Vector3(0.8f, 0.8f, 0.8f));
                objectshader.SetVector3($"pointlights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
                objectshader.SetFloat($"pointlights[{i}].constant", 1.0f);
                objectshader.SetFloat($"pointlights[{i}].linear", 0.09f);
                objectshader.SetFloat($"pointlights[{i}].quadratic", 0.032f);
            }
            
            //手电筒
            objectshader.SetVector3("spotlight.position", _camera.Position);
            objectshader.SetVector3("spotlight.direction", _camera.Front);
            objectshader.SetVector3("spotlight.ambient", new Vector3(0.05f));
            objectshader.SetVector3("spotlight.diffuse", new Vector3(0.5f));
            objectshader.SetVector3("spotlight.specular", new Vector3(0.5f));
            objectshader.SetFloat("spotlight.cutoff", 20.5f);
            objectshader.SetFloat("spotlight.outerCutoff", 25.0f);
            objectshader.SetFloat("spotlight.constant", 1.0f);
            objectshader.SetFloat("spotlight.linear", 0.09f);
            objectshader.SetFloat("spotlight.quadratic", 0.0032f);
            
            GL.BindVertexArray(_objectVAO);


            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            //光源
            GL.BindVertexArray(_lightVAO);

            lightshader.Use();

            lightshader.SetMatrix4("view", view);
            lightshader.SetMatrix4("projection", projection);
            for (int i = 0; i < lightPositions.Length; i++)
            {
                lightmodel = new Matrix4();
                lightmodel = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(lightPositions[i]);


                lightshader.SetMatrix4("model", lightmodel);


                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            }

            SwapBuffers();

            base.OnRenderFrame(e);
        }
        /*
        private void DataLoad()
        {
            //加载模型文件
            data = new PlyFile();
            data.LoadFrom("../../Data/layers-2.ply");
            data.Normalize();

            _vertices = data.vertices;//顶点数组           
            var IntIndices = new int[data.faceno];//转换
            _indices = new uint[data.faceno];
            IntIndices = data.faces;//顶点序列         
            char[] aColors = data.colors;//顶点颜色
            _colors =new float[data.colors.Length];
            for(int i = 0; i<aColors.Length;i++){
                _colors[i]=((float)aColors[i])/255f;
            }
            _normals = data.normals;//法线
            for (int i = 0; i < data.faceno; i++)
            {
                _indices[i] = (uint)IntIndices[i];
            }

        }
        */
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;
            

            if (input.IsKeyDown(Key.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Key.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Key.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Key.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Key.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Key.LShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }
            if (input.IsKeyDown(Key.R))
            {
                _camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);
            }
            var mouse = Mouse.GetState();

            if (_firstMove) 
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; 
            }

            base.OnUpdateFrame(e);
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseMove(e);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.Fov -= e.DeltaPrecise;
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_lightVAO);
            //GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_objectVAO);
 

            base.OnUnload(e);
        }
    }
}

