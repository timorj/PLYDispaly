using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace DataLoad
{
    class Mesh
    {
        /// <summary>
        /// 结构体：顶点
        /// </summary>
        public struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TexCoords;
        }
        /// <summary>
        /// 结构体：纹理
        /// </summary>
        public struct Texture
        {
            public int id;
            public string type;
            public string path;
        }

        public Vertex[] vertices;
        public int[] indices;
        public List<Texture> textures;

        public Mesh(Vertex[] vertices, int[] indices, List<Texture> textures)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.textures = textures;

            setupMesh();
        }

        private int VAO, VBO, EBO;
        private void setupMesh()
        {
            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);
            GL.GenBuffers(1, out EBO);

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * (2 * Vector3.SizeInBytes + Vector2.SizeInBytes), ref vertices[0], BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, EBO);

            GL.BufferData(BufferTarget.ArrayBuffer, indices.Length * sizeof(int), ref indices[0], BufferUsageHint.StaticDraw);

            //顶点位置
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (2 * Vector3.SizeInBytes + Vector2.SizeInBytes), 0);

            //顶点法线
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (2 * Vector3.SizeInBytes + Vector2.SizeInBytes), Vector3.SizeInBytes);

            //纹理坐标 
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (2 * Vector3.SizeInBytes + Vector2.SizeInBytes), 2 * Vector3.SizeInBytes);

            GL.BindVertexArray(0);

        }
        public void Draw(Shader shader)
        {
            int diffuseNr = 1;
            int specularNr = 1;
            for (int i = 0; i < textures.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                string number = "";
                string name = textures[i].type;
                if (name == "diffuse")
                {
                    number = (diffuseNr++).ToString();
                }
                else if (name == "specular")
                {
                    number = (specularNr++).ToString();
                }
                shader.SetFloat("material." + name, i);
                GL.BindTexture(TextureTarget.Texture2D, textures[i].id);
            }
            GL.ActiveTexture(0);

            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }



    }
}
