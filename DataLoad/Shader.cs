using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace DataLoad
{
    class Shader
    {
        private readonly int Handle;
        private readonly Dictionary<string, int> _uniformlocation;
        public Shader(string vertPath, string fragPath)
        {
            //创建顶点着色器
            var shaderSource = LoadSource(vertPath);
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            //绑定着色器代码
            GL.ShaderSource(vertexShader, shaderSource);
            //检测是否编译错误
            CompileShader(vertexShader);

            //创建片段着色器
            shaderSource = LoadSource(fragPath);
            var fragShader = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(fragShader, shaderSource);
            CompileShader(fragShader);
            //创建程序
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragShader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int NumberOfUniforms);
            _uniformlocation = new Dictionary<string, int>();
            for (int i = 0; i < NumberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                _uniformlocation.Add(key, location);
            }


        }
        private static string LoadSource(string path)
        {
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
        private static void CompileShader(int Shader)
        {
            GL.CompileShader(Shader);

            GL.GetShader(Shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(Shader);
                throw new Exception($"Error occurred whilst compiling Shader({Shader}).\n\n{infoLog}");
            }
        }
        private static void LinkProgram(int Program)
        {
            GL.LinkProgram(Program);
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out int code);
            if (code != (int)All.True)
            {
                throw new Exception($"Error occurred whilst linking Program({Program})");
            }
        }
        public void Use()
        {
            GL.UseProgram(Handle);
        }
        public int GetAtrribLocation(string atrribName)
        {
            return GL.GetAttribLocation(Handle, atrribName);
        }
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformlocation[name], data);
        }
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformlocation[name], data);
        }
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformlocation[name], true, ref data);
        }
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformlocation[name], data);
        }

    }
}
