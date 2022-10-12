using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GlmNet;
using System.Drawing;
using System.IO;

namespace DataLoad
{
    //intersection type of point and triangle 
    public enum IntersectionType
    {
        none = 0,
        triangle = 1,
        line = 2,
        corner = 3
    };
    public enum AxisEnum
    {
        xAxis = 0,
        yAxis = 1,
        zAxis = 2
    };
    //box in a 3DGrid
    public class GridBox : C3DObjectBase
    {
        public bool[] faces = null;    //6 faces
        public vec3[] points = null;   //x,y,z
        public vec4[] colors = null;
        public GridBox()
        {
            points = null;
            faces = null;
            colors = null;
            name = "untitled";
            type = ShapeEnum.Box;
        }
        public virtual void Destroy()
        {
            points = null;
            faces = null;
            colors = null;
        }
        public void SetColor(vec4 _colors)
        {
            if (colors == null) colors = new vec4[8];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = _colors;
        }
        public void SetColor(vec4[] _colors)
        {
            colors = _colors;
        }
        public void Create(double x, double y, double z, double xlen, double ylen, double zlen, vec4[] _colors = null)
        {
            //      p3--------p2 
            //      |         |
            //   p7 |     p6  |
            //   |  /p0---|---p1
            //   | /      | / 
            // p4|/-------p5--->east 
            points = new vec3[8];
            points[0] = new vec3((float)(x - xlen / 2), (float)(y - ylen / 2), (float)(z - zlen / 2));
            points[1] = new vec3((float)(x + xlen / 2), (float)(y - ylen / 2), (float)(z - zlen / 2));
            points[2] = new vec3((float)(x + xlen / 2), (float)(y + ylen / 2), (float)(z - zlen / 2));
            points[3] = new vec3((float)(x - xlen / 2), (float)(y + ylen / 2), (float)(z - zlen / 2));
            points[4] = new vec3((float)(x - xlen / 2), (float)(y - ylen / 2), (float)(z + zlen / 2));
            points[5] = new vec3((float)(x + xlen / 2), (float)(y - ylen / 2), (float)(z + zlen / 2));
            points[6] = new vec3((float)(x + xlen / 2), (float)(y + ylen / 2), (float)(z + zlen / 2));
            points[7] = new vec3((float)(x - xlen / 2), (float)(y + ylen / 2), (float)(z + zlen / 2));

            faces = new bool[6];
            for (int i = 0; i < 6; i++) faces[i] = true;

            colors = _colors;

            UpdateRange();
        }
        public TriangleObj toTriangleObj()
        {
            //      |(y)
            //      p3--------p2 
            //      |         |
            //   p7 |     p6  |
            //   |  /p0---|---p1--->(x)
            //   | /      | / 
            // p4|/-------p5--->east 
            //   / (z)
            TriangleObj obj = new TriangleObj();
            obj.name = name;
            obj.IsUniformColor = true;

            if (points == null || faces == null) return obj;
            int[] up = new int[] { 2, 3, 7, 7, 6, 2 };
            int[] down = new int[] { 0, 1, 5, 5, 4, 0 };
            int[] left = new int[] { 3, 0, 4, 3, 4, 7 };
            int[] right = new int[] { 1, 2, 6, 1, 6, 5 };
            int[] front = new int[] { 4, 5, 6, 4, 6, 7 };
            int[] back = new int[] { 3, 2, 1, 3, 1, 0 };

            int i;
            for (i = 0; i < points.Length; i++)
            {
                obj.AddPoint(points[i].x, points[i].y, points[i].z);
            }

            obj.color = new vec4(0, 0, 0, 1);

            if (colors != null)
            {
                for (i = 0; i < colors.Length && i < 8; i++)
                    obj.AddPointColor(colors[i]);
            }
            int start = 0;
            List<int> indices = new List<int>();
            if (faces[0])    //up
            {
                for (i = 0; i < up.Length; i++) indices.Add(up[i] + start);
            }
            if (faces[1])    //down
            {
                for (i = 0; i < down.Length; i++) indices.Add(down[i] + start);
            }
            if (faces[2])    //left
            {
                for (i = 0; i < left.Length; i++) indices.Add(left[i] + start);
            }
            if (faces[3])    //right
            {
                for (i = 0; i < right.Length; i++) indices.Add(right[i] + start);
            }
            if (faces[4])    //front
            {
                for (i = 0; i < front.Length; i++) indices.Add(front[i] + start);

            }
            if (faces[5])    //back
            {
                for (i = 0; i < back.Length; i++) indices.Add(back[i] + start);
            }

            for (i = 0; i < indices.Count / 3; i++)
            {
                obj.AddTriangleIndex(indices[3 * i], indices[3 * i + 1], indices[3 * i + 2]);
            }
            indices.Clear();
            return obj;
        }
        public override void Normalize()
        {
            UpdateRange();
            Vector32 p;
            for (int i = 0; i < points.Length; i++)
            {
                p = TransformedPoint(toPoint(points[i]));
                points[i] = toPoint(p);
            }
            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            rotate = new vec3(0, 0, 0);
            UpdateRange();
        }
        public override void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;           
            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                {
                    minx = maxx = points[i].x;
                    miny = maxy = points[i].y;
                    minz = maxz = points[i].z;
                }
                else
                {
                    if (points[i].x < minx) minx = points[i].x;
                    if (points[i].y < miny) miny = points[i].y;
                    if (points[i].z < minz) minz = points[i].z;
                    if (points[i].x > maxx) maxx = points[i].x;
                    if (points[i].y > maxy) maxy = points[i].y;
                    if (points[i].z > maxz) maxz = points[i].z;
                }
            }
        }
        public void EnableFace(DirectionEnum face, bool show)
        {
            int c = 0;
            for (int i = 0; i < 6; i++)
            {
                c = (int)face & (1 << i);
                if (c > 0) faces[i] = show;
            }
        }
    };
    //object based on triangles
    public class TriangleObj : C3DObjectBase
    {
        public List<Vector32> points = new List<Vector32>();
        public List<vec4> colors = new List<vec4>();
        public List<Int32XYZ> triangles = new List<Int32XYZ>();
        public List<vec2> texCoords = new List<vec2>();
        public vec4 color = new vec4(1.0f, 1.0f, 1.0f, 1.0f);
        private bool _IsUniformColor = false;
        public string textureFile = "";
        [CategoryAttribute("Display"), DisplayNameAttribute("texture")]
        public string texture
        {
            get { return textureFile; }
            set { textureFile = value; }
        }
        [CategoryAttribute("Uniform Color"), DisplayNameAttribute("UniformColor")]
        public bool IsUniformColor
        {
            get { return _IsUniformColor; }
            set { _IsUniformColor = value; }
        }
        [CategoryAttribute("Uniform Color"), DisplayNameAttribute("Color")]
        public Color uniformColor
        {
            get { return Color.FromArgb((int)(color.w * 255), (int)(color.x * 255), (int)(color.y * 255), (int)(color.z * 255)); }
            set
            {
                Color c = value;
                color = new vec4(c.R / 255, c.G / 255, c.B / 255, c.A / 255);
            }
        }

        public double minSquare;   //minimum triangle 
        public double maxSquare;    //minimum angle
        public double minAngle;    //minimum angle        
        public double minEdge;     //minimum edge length
        public TriangleObj()
        {
            type = ShapeEnum.Triangles;
        }
        /// <summary>
        /// 复制对象
        /// </summary>
        /// <returns></returns>
        public TriangleObj Copy()
        {
            TriangleObj obj = new TriangleObj();
            CopyHeader(obj);
            foreach (Vector32 p in points)
            {
                obj.points.Add(p);
            }
            foreach (vec4 c in colors)
            {
                obj.colors.Add(c);
            }
            foreach (Int32XYZ id in triangles)
            {
                obj.triangles.Add(id);
            }
            foreach (vec2 tex in texCoords)
            {
                obj.texCoords.Add(tex);
            }

            obj.color = color;
            obj._IsUniformColor = _IsUniformColor;
            obj.textureFile = textureFile;
            return obj;
        }
        public virtual void Destroy()
        {
            points.Clear();
            triangles.Clear();
            texCoords.Clear();
        }
        //Save as PLY 
        public bool SaveAsPLY(string path)
        {
            if (points.Count < 3 || triangles.Count < 1)
            {
                errMessage = "no valid vertics and faces.";
                return false;
            }
            try
            {
                StringBuilder builder = new StringBuilder();
                File.WriteAllText(path, builder.ToString());
                builder.AppendLine("ply");
                builder.AppendLine("format ascii 1.0");

                builder.AppendLine("comment created by 3D Surfer v3.0.");
                builder.AppendLine("comment name: " + name);
                builder.AppendLine("comment  " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());


                builder.AppendLine("element vertex " + points.Count);

                builder.AppendLine("property float32 x");
                builder.AppendLine("property float32 y");
                builder.AppendLine("property float32 z");

                //if colors.Count == points.Count
                //if colors.Count < points.Count
                if (colors.Count > 0)
                {
                    builder.AppendLine("property float32 Red");
                    builder.AppendLine("property float32 Green");
                    builder.AppendLine("property float32 Blue");
                }
                if (texCoords.Count > 0)
                {
                    builder.AppendLine("property float32 u");
                    builder.AppendLine("property float32 v");
                }
                builder.AppendLine("element face " + triangles.Count);
                builder.AppendLine("property list uint8 int32 vertex_indices");
                builder.AppendLine("end_header");
                string line;
                float r = 0, g = 0, b = 0;
                int ic = 0;
                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    //xyz
                    p = TransformedPoint(points[i]);
                    //line = points[i].x + " " + points[i].y + " " + points[i].z;
                    line = p.x + " " + p.y + " " + p.z;
                    //r,g,b
                    if (colors.Count > 0)
                    {
                        if (colors.Count >= points.Count)
                        {
                            r = colors[i].x;
                            g = colors[i].y;
                            b = colors[i].z;
                        }
                        else
                        {
                            r = colors[ic].x;
                            g = colors[ic].y;
                            b = colors[ic].z;
                            ic++;
                            if (ic >= colors.Count) ic = 0;
                        }
                        line += " " + r + " " + g + " " + b;
                    }

                    //u,v
                    if (texCoords.Count > 0)
                        line += " " + texCoords[i].x + " " + texCoords[i].y;

                    builder.AppendLine(line);
                }
                for (int i = 0; i < triangles.Count; i++)
                {
                    builder.AppendLine("3 " + triangles[i].x + " " + triangles[i].y + " " + triangles[i].z);
                }
                File.WriteAllText(path, builder.ToString());
                builder.Clear();
                builder = null;
            }
            catch (Exception e)
            {
                errMessage = "writing to file faild!\n" + e.Message;
                return false;
            }

            return true;
        }
        public override bool SaveAs(ref BinaryWriter br)
        {
            bool ret = base.SaveAs(ref br);
            if (!ret) return false;
            br.Write(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                br.Write(points[i].x);
                br.Write(points[i].y);
                br.Write(points[i].z);
                br.Write(points[i].v);
            }
            br.Write(colors.Count);
            for (int i = 0; i < colors.Count; i++)
            {
                br.Write(colors[i].x);
                br.Write(colors[i].y);
                br.Write(colors[i].z);
                br.Write(colors[i].w);
            }
            br.Write(triangles.Count);
            for (int i = 0; i < triangles.Count; i++)
            {
                br.Write(triangles[i].x);
                br.Write(triangles[i].y);
                br.Write(triangles[i].z);
            }
            br.Write(texCoords.Count);
            for (int i = 0; i < texCoords.Count; i++)
            {
                br.Write(texCoords[i].x);
                br.Write(texCoords[i].y);
            }
            br.Write(color.x);
            br.Write(color.y);
            br.Write(color.z);
            br.Write(color.w);
            short n = (short)textureFile.Length;
            br.Write(n);
            br.Write(textureFile.ToCharArray());
            return true;
        }
        public override bool LoadFrom(ref BinaryReader br)
        {
            bool ret = base.LoadFrom(ref br);
            if (!ret) return false;

            float x, y, z, v, w;
            int ix, iy, iz;
            points.Clear();
            colors.Clear();
            triangles.Clear();
            texCoords.Clear();
            int n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                v = br.ReadSingle();
                points.Add(new Vector32(x, y, z, v));
            }
            n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                w = br.ReadSingle();
                colors.Add(new vec4(x, y, z, w));
            }
            n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                ix = br.ReadInt32();
                iy = br.ReadInt32();
                iz = br.ReadInt32();
                triangles.Add(new Int32XYZ(ix, iy, iz));
            }
            n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                texCoords.Add(new vec2(x, y));
            }

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            color = new vec4(x, y, z, w);

            textureFile = "";
            n = br.ReadInt16();
            if (n > 0) textureFile = br.ReadChars(n).ToString();

            return true;
        }
        public vec4 GetPointColor(int index)
        {
            if (colors.Count > 0 && colors.Count < index)
            {
                return colors[index];
            }
            return color;
        }
        public void AddPoint(Vector32 p)
        {
            points.Add(p);
        }
        public void AddPoint(double x, double y, double z)
        {
            points.Add(new Vector32((float)x, (float)y, (float)z));
        }
        public void AddPointColor(vec4 cc)
        {
            colors.Add(cc);
        }
        public void AddPointColor(double r, double g, double b, double a = 1)
        {
            colors.Add(new vec4((float)r, (float)g, (float)b, 1));
        }
        public void AddTriangleIndex(int i1, int i2, int i3)
        {
            triangles.Add(new Int32XYZ(i1, i2, i3));
        }
        public CTriangle3f GetTriangle(int index)
        {
            CTriangle3f tri = new CTriangle3f();
            if (index >= 0 && index < triangles.Count)
            {
                tri.p1 = points[triangles[index].x];
                tri.p2 = points[triangles[index].y];
                tri.p3 = points[triangles[index].z];
            }
            return tri;
        }
        public CTriangle3f GetNearestTriangle(Vector32 p)
        {
            double len, lx, ly, lz, dist = 1.0E10;
            int i1, i2, i3;
            Vector32 p1, p2, p3;
            int id = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                p1 = points[i1];
                p2 = points[i2];
                p3 = points[i3];

                lx = p1.x - p.x;
                ly = p1.y - p.y;
                lz = p1.z - p.z;
                if (lx < 0) lx = -lx;
                if (ly < 0) ly = -ly;
                if (lz < 0) lz = -lz;
                len = lx + ly + lz;

                lx = p2.x - p.x;
                ly = p2.y - p.y;
                lz = p2.z - p.z;
                if (lx < 0) lx = -lx;
                if (ly < 0) ly = -ly;
                if (lz < 0) lz = -lz;
                len += (lx + ly + lz);

                lx = p3.x - p.x;
                ly = p3.y - p.y;
                lz = p3.z - p.z;
                if (lx < 0) lx = -lx;
                if (ly < 0) ly = -ly;
                if (lz < 0) lz = -lz;
                len += (lx + ly + lz);

                if (len < dist)
                {
                    id = i;
                    dist = len;
                }
            }
            return GetTriangle(id);
        }
        public virtual Vector32 GetMiddlePoint(Vector32 p1, Vector32 p2)
        {
            Vector32 p = (p1 + p2) / 2;
            return p;
        }
        private double GetSquare(Vector32 p1, Vector32 p2, Vector32 p3)
        {
            Vector32 p12 = new Vector32(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector32 p23 = new Vector32(p3.X - p2.X, p3.Y - p2.Y, p3.Z - p2.Z);
            Vector32 p = p12.Cross(p23);
            return p.Length;
        }
        private void DoSmooth()
        {
            Int32XYZ[] indexes = new Int32XYZ[triangles.Count];

            for (int i = 0; i < triangles.Count; i++)
                indexes[i] = triangles[i];

            for (int i = 0; i < indexes.Length; i++)
                DividTriangle(i);

            indexes = null;
        }
        public void SmoothTriangle(double _minSquare = 0.01)
        {
            if (_minSquare >= 1) return;
            /*
            minSquare = maxSquare = 0;
            double square = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                square = GetSquare(points[triangles[i].x], points[triangles[i].y], points[triangles[i].z]);
                if (i == 0) minSquare = maxSquare = square;
                else
                {
                    if (square < minSquare) minSquare = square;
                    if (square > maxSquare) maxSquare = square;
                }
            }

            minSquare = _minSquare * maxSquare;
            */
            minSquare = _minSquare;
            DoSmooth();
        }
        public bool IsPointInRange(double x, double y, double z, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            if (x < x1 || x > x2) return false;
            if (y < y1 || y > y2) return false;
            if (z < z1 || z > z2) return false;
            return true;
        }
        /// <summary>
        /// 对三角形进行裁剪，保留指定范围内的三角形
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="z1"></param>
        /// <param name="z2"></param>
        /// <returns></returns>
        public TriangleObj TrimObject(double x1, double x2, double y1, double y2, double z1, double z2)
        {
            TriangleObj obj = new TriangleObj();
            bool[] trim = new bool[points.Count];
            int[] indices = new int[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                trim[i] = false;
                indices[i] = i;
            }
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (!IsPointInRange(p.x, p.y, p.z, x1, x2, y1, y2, z1, z2))
                    trim[i] = true;
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (trim[i]) continue;
                indices[i] = obj.points.Count;
                obj.AddPoint(points[i]);
            }
            int i1, i2, i3;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                if (trim[i1] || trim[i2] || trim[i3]) continue;
                obj.AddTriangleIndex(indices[i1], indices[i2], indices[i3]);
            }
            obj.UpdateRange();

            trim = null;
            indices = null;

            obj.colors = new List<vec4>();
            obj.texCoords = texCoords;
            obj.color = color;
            obj.textureFile = textureFile;

            return obj;
        }
        public void SmoothTriangle(int dividNum = 2)
        {
            minSquare = maxSquare = 0;
            double square = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                square = GetSquare(points[triangles[i].x], points[triangles[i].y], points[triangles[i].z]);
                if (i == 0) minSquare = maxSquare = square;
                else
                {
                    if (square < minSquare) minSquare = square;
                    if (square > maxSquare) maxSquare = square;
                }
            }

            square = maxSquare / dividNum;
            minSquare = maxSquare / dividNum;

            DoSmooth();
        }
        virtual public void DividTriangle(int index)
        {
            //       p1
            //     /   \
            // i12/     \i13
            //   /       \
            // p2 --i23-- p3
            int i1 = triangles[index].x;
            int i2 = triangles[index].y;
            int i3 = triangles[index].z;
            Vector32 p1 = points[i1];
            Vector32 p2 = points[i2];
            Vector32 p3 = points[i3];

            //triangle meet minimum square requirment
            if (GetSquare(p1, p2, p3) <= minSquare) return;

            Vector32 p12 = GetMiddlePoint(p1, p2);
            Vector32 p23 = GetMiddlePoint(p2, p3);
            Vector32 p13 = GetMiddlePoint(p1, p3);

            int i12 = points.Count;
            int i23 = i12 + 1;
            int i13 = i23 + 1;

            //add new points
            points.Add(p12);
            points.Add(p23);
            points.Add(p13);

            Int32XYZ d0 = new Int32XYZ(i12, i23, i13);
            Int32XYZ d1 = new Int32XYZ(i1, i12, i13);
            Int32XYZ d2 = new Int32XYZ(i12, i2, i23);
            Int32XYZ d3 = new Int32XYZ(i13, i23, i3);

            int t0 = triangles.Count;
            //new triangles
            //replace original one
            triangles[index] = d0;
            //add to end triangles
            triangles.Add(d1);
            triangles.Add(d2);
            triangles.Add(d3);

            DividTriangle(index);
            DividTriangle(t0);
            DividTriangle(t0 + 1);
            DividTriangle(t0 + 2);
        }

        public override void Normalize()
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = TransformedPoint(points[i]);
            }
            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            rotate = new vec3(0, 0, 0);

            UpdateRange();
        }
        /// <summary>
        /// 将物体缩放到指定范围
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>
        public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            UpdateRange();
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (maxx == minx) p.x = (float)((x1 + x2) / 2.0);
                else p.x = (float)(x1 + (x2 - x1) * (p.x - minx) / (maxx - minx));
                if (maxy == miny) p.y = (float)((y1 + y2) / 2.0);
                else p.y = (float)(y1 + (y2 - y1) * (p.y - miny) / (maxy - miny));
                if (maxz == minz) p.z = (float)((z1 + z2) / 2.0);
                else p.z = (float)(z1 + (z2 - z1) * (p.z - minz) / (maxz - minz));
                points[i] = p;
            }
            minx = x1;
            miny = y1;
            minz = z1;
            maxx = x2;
            maxy = y2;
            maxz = z2;
        }
        public override void CutWith(C3DObjectBase obj, int method, int methodPara)
        {
            //method = 0 Polygon
            //"keep outside"
            //"keep inside"
            //"create intersection"
            //method = 1 mesh
            //keep up(y+) //0 
            //keep down(y-)//1
            //keep left(-x)//2
            //keep right(x+)//3
            //keep front(z+)//4
            //keep back(z-)//5
            //create intersection//6
            if (method == 0)
            {
                if (methodPara == 0)
                    CutWithPolygon(new Polygon3D((TriangleObj)obj), true);
                else if (methodPara == 1)
                    CutWithPolygon(new Polygon3D((TriangleObj)obj), false);
            }
        }
        //p1 - p2 is on axis = 0 x 1 y 2 z
        //return 
        public bool GetAxisIntersection(Vector32 v1, Vector32 v2, int axis, out List<Vector32> sects, out List<int> triIndices)
        {
            sects = new List<Vector32>();
            triIndices = new List<int>();

            //not in the range
            if (!IsInRange(v1.x, v1.y, v1.z) &&
                 !IsInRange(v2.x, v2.y, v2.z)) return false;

            int i1, i2, i3;

            CTriangle3f tri = new CTriangle3f();
            Vector32 p1, p2, p3;
            Vector32 p = new Vector32();
            double r1, r2;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                p1 = points[i1];
                p2 = points[i2];
                p3 = points[i3];
                tri.p1 = p1;
                tri.p2 = p2;
                tri.p3 = p3;
                if (axis == 0) // x axis
                {
                    r1 = r2 = p1.x;
                    if (r1 > p2.x) r1 = p2.x;
                    if (r1 > p3.x) r1 = p3.x;
                    if (r2 < p2.x) r2 = p2.x;
                    if (r2 < p3.x) r2 = p3.x;

                    //outof range
                    if ((v1.x < r1 && v2.x < r1) ||
                         (v1.x > r2 && v2.x > r2)) continue;

                    //axis triangle relation
                    p = v1; p.x = 0;
                    p1.x = 0; p2.x = 0; p3.x = 0;
                    if (!CTriangle2f.IsPointInTriangle(p, p1, p2, p3)) continue;
                    //calculation intersection
                    if (tri.GetIntersectionOnTriangle(v1, v2, out p))
                    {
                        sects.Add(p);
                        triIndices.Add(i);
                    }
                }
                else if (axis == 1) // y axis
                {
                    r1 = r2 = p1.y;
                    if (r1 > p2.y) r1 = p2.y;
                    if (r1 > p3.y) r1 = p3.y;
                    if (r2 < p2.y) r2 = p2.y;
                    if (r2 < p3.y) r2 = p3.y;
                    //outof range
                    if ((v1.y < r1 && v2.y < r1) ||
                         (v1.y > r2 && v2.y > r2)) continue;
                    //axis triangle relation
                    p = v1; p.y = 0;
                    p1.y = 0; p2.y = 0; p3.y = 0;
                    if (!CTriangle2f.IsPointInTriangle(p, p1, p2, p3)) continue;
                    //calculation intersection
                    if (tri.GetIntersectionOnTriangle(v1, v2, out p))
                    {
                        sects.Add(p);
                        triIndices.Add(i);
                    }
                }
                else if (axis == 2) // z axis
                {
                    r1 = r2 = p1.z;
                    if (r1 > p2.z) r1 = p2.z;
                    if (r1 > p3.z) r1 = p3.z;
                    if (r2 < p2.z) r2 = p2.z;
                    if (r2 < p3.z) r2 = p3.z;
                    //outof range
                    if ((v1.z < r1 && v2.z < r1) ||
                         (v1.z > r2 && v2.z > r2)) continue;
                    //axis triangle relation
                    p = v1; p.z = 0;
                    p1.z = 0; p2.z = 0; p3.z = 0;
                    if (!CTriangle2f.IsPointInTriangle(p, p1, p2, p3)) continue;
                    //calculation intersection
                    if (tri.GetIntersectionOnTriangle(v1, v2, out p))
                    {
                        sects.Add(p);
                        triIndices.Add(i);
                    }
                }

            }//for (int i = 0; i < triangles.Count; i++)

            if (sects.Count > 0)
                return true;
            else return false;
        } //end of function

        public bool GetLineIntersection(Vector32 p1, Vector32 p2, out Vector32 p, out CTriangle3f ret)
        {
            int i1, i2, i3;
            p = new Vector32(0, 0, 0);
            ret = new CTriangle3f();
            if (!IsInRange(p1.x, p1.y, p1.z) && !IsInRange(p2.x, p2.y, p2.z))
                return false;

            CTriangle3f tri = new CTriangle3f();
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                tri.p1 = points[i1];
                tri.p2 = points[i2];
                tri.p3 = points[i3];
                if (tri.GetIntersectionOnTriangle(p1, p2, out p))
                {
                    ret = tri;
                    return true;
                }
            }
            return false;
        }

        public virtual void CutWithPolygon(Polygon3D poly, bool keep_outer = true)
        {
            if (points.Count < 1) return;

            bool[] cuted = new bool[points.Count];
            int[] indices = new int[points.Count];
            for (int i = 0; i < cuted.Length; i++)
            {
                cuted[i] = false;
                indices[i] = 0;
            }

            Vector32 p1;
            for (int i = 0; i < points.Count; i++)
            {
                p1 = TransformedPoint(points[i]);
                if (poly.IsPointInPolygon(p1))
                {
                    if (keep_outer) cuted[i] = true;
                }
                else
                {
                    if (!keep_outer) cuted[i] = true;
                }
            }
            int index = 0;
            for (int i = 0; i < cuted.Length; i++)
            {
                if (cuted[i]) index++;
                indices[i] = index;
            }

            List<Int32XYZ> new_tries = new List<Int32XYZ>();
            int i1, i2, i3;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                if (cuted[i1]) continue;
                if (cuted[i2]) continue;
                if (cuted[i3]) continue;
                i1 = i1 - indices[i1];
                i2 = i2 - indices[i2];
                i3 = i3 - indices[i3];
                new_tries.Add(new Int32XYZ(i1, i2, i3));
            }

            triangles.Clear();
            for (int i = 0; i < new_tries.Count; i++)
            {
                triangles.Add(new_tries[i]);
            }
            new_tries.Clear();

            for (int i = cuted.Length - 1; i >= 0; i--)
            {
                if (cuted[i])
                {
                    points.RemoveAt(i);
                }
            }
            cuted = null;
            indices = null;
        }

        public override void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;         
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    minx = maxx = points[i].X;
                    miny = maxy = points[i].Y;
                    minz = maxz = points[i].Z;
                }
                else
                {
                    if (points[i].X < minx) minx = points[i].X;
                    if (points[i].Y < miny) miny = points[i].Y;
                    if (points[i].Z < minz) minz = points[i].Z;
                    if (points[i].X > maxx) maxx = points[i].X;
                    if (points[i].Y > maxy) maxy = points[i].Y;
                    if (points[i].Z > maxz) maxz = points[i].Z;
                }
            }
        }
        public override bool LoadFrom(string path)
        {
            Clear();

            PlyFile ply = new PlyFile();
            if (!ply.LoadFrom(path)) return false;

            TriangleObj obj = ply.toTriangleObj();
            points = obj.points;
            colors = obj.colors;
            triangles = obj.triangles;
            texCoords = obj.texCoords;
            color = obj.color;
            textureFile = obj.textureFile;

            return true;
        }
        public string errMessage = "";
        public override bool SaveAs(string path)
        {
            return ExportData(path);

            Vector32 p;
            Int32XYZ tri;
            if (points.Count < 3 || triangles.Count < 1)
            {
                errMessage = "vertics and faces are not enough";
                return false;
            }
            try
            {
                StringBuilder builder = new StringBuilder();
                File.WriteAllText(path, builder.ToString());
                builder.AppendLine("ply");
                builder.AppendLine("format ascii 1.0");
                builder.AppendLine("element vertex " + points.Count);

                builder.AppendLine("property float32 x");
                builder.AppendLine("property float32 y");
                builder.AppendLine("property float32 z");
                builder.AppendLine("element face " + triangles.Count);
                builder.AppendLine("property list uint8 int32 vertex_indices");
                builder.AppendLine("end_header");

                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    builder.AppendLine(p.x + " " + p.y + " " + p.z);
                }
                for (int i = 0; i < triangles.Count; i++)
                {
                    tri = triangles[i];
                    builder.AppendLine("3 " + tri.x + " " + tri.y + " " + tri.z);
                }
                File.WriteAllText(path, builder.ToString());
                builder.Clear();
                builder = null;
            }
            catch (Exception e)
            {
                errMessage = "writing to file faild!\n" + e.Message;
                return false;
            }
            return true;
        }
        public override bool ExportData(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);
            string str = "ply";
            wr.WriteLine(str);
            str = "format ascii 1.0"; wr.WriteLine(str);

            str = "comment TextureFile " + Path.GetFileName(textureFile); wr.WriteLine(str);
            str = "comment author:  jiansir@163.com"; wr.WriteLine(str);
            str = "comment object:  " + name; wr.WriteLine(str);
            str = "element vertex   " + points.Count; wr.WriteLine(str);

            str = "property float x"; wr.WriteLine(str);
            str = "property float y"; wr.WriteLine(str);
            str = "property float z"; wr.WriteLine(str);
            if (texCoords.Count > 0)
            {
                str = "property float u"; wr.WriteLine(str);
                str = "property float v"; wr.WriteLine(str);
            }
            str = "element face " + triangles.Count; wr.WriteLine(str);
            str = "property list uchar int vertex_indices"; wr.WriteLine(str);
            if (texCoords.Count > 0)
            {
                str = "property list uchar float texcoord"; wr.WriteLine(str);
            }
            str = "end_header"; wr.WriteLine(str);

            for (int i = 0; i < points.Count; i++)
            {
                str = points[i].x + " ";
                str += points[i].y + " ";
                str += points[i].z + " ";
                if (texCoords.Count > 0)
                {
                    str += texCoords[i].x + " ";
                    str += texCoords[i].y + " ";
                }
                wr.WriteLine(str);
            }
            int i1, i2, i3;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                str = "3 " + i1 + " " + i2 + " " + i3;
                wr.WriteLine(str);
                if (texCoords.Count > 0)
                {
                    str = "6 " + texCoords[i1].x;
                    str += " ";
                    str += texCoords[i1].y;
                    str += " ";

                    str += texCoords[i2].x;
                    str += " ";
                    str += texCoords[i2].y;
                    str += " ";

                    str += texCoords[i3].x;
                    str += " ";
                    str += texCoords[i3].y;
                    str += " ";

                    wr.WriteLine(str);
                }
            }
            wr.Close();
            fs.Close();

            if (points.Count > 1)
                return true;
            else return false;
        }

        /*
        public override bool ExportVRML(ref StreamWriter wr)
        {
            if (!visible) return false;            
            int n = points.Count;
            if (n < 1) return false;

            Vector32 p;
            string line;            
            line = "#---Triangles " + name; wr.WriteLine(line);

            //export transform
            line = "DEF " + "MESH_"+ name + " Transform"; wr.WriteLine(line);
            line = "{"; wr.WriteLine(line);

            // translation
            p = GetOffset32();
            p = CDataModel.CovertCoordGeoToGL(p);
            p = CDataModel.ToModelVector32(p);
            line = "  translation 0 0 0";
            wr.WriteLine(line);

            //scale
            line = "  scale 1 1 1";
            wr.WriteLine(line);

            //rotation, degress 0,90,180,360
            //line = "  rotation 1 0 0 " + Vector32.toRad(rotate.x); wr.WriteLine(line);
            //line = "  rotation 0 1 0 " + Vector32.toRad(rotate.y); wr.WriteLine(line);
            //line = "  rotation 0 0 1 " + Vector32.toRad(rotate.z); wr.WriteLine(line);
            
            line = "  children"; wr.WriteLine(line);
            line = "  ["; wr.WriteLine(line);
            line = "   Shape"; wr.WriteLine(line);
            line = "   {"; wr.WriteLine(line);
            if(textureFile.Length > 1 && texCoords.Count > 1 )
            {
                line = "    appearance Appearance"; wr.WriteLine(line);
                line = "    {"; wr.WriteLine(line);

                line = "      texture ImageTexture "; wr.WriteLine(line);
                line = "      {"; wr.WriteLine(line);

                line = @"         url "" ";
                string txtfile = Path.GetFileName( textureFile );
                line += (txtfile + @" "" ");
                wr.WriteLine(line);

                line = "      repeatS TRUE"; wr.WriteLine(line);
                line = "      repeatT TRUE"; wr.WriteLine(line);

                line = "      }"; wr.WriteLine(line);
                line = "    }"; wr.WriteLine(line);
            }
            
            line = "      geometry IndexedFaceSet"; wr.WriteLine(line);
            line = "      { "; wr.WriteLine(line);
            line = "       coord Coordinate"; wr.WriteLine(line);
            line = "       { point["; wr.WriteLine(line);
            line = "         ";
            for (int i = 0; i < n; i++)
            {
                p = TransformedPoint(points[i]);
                
                p = CDataModel.ToModelVector32(p);                
                if (i % 50 == 0)
                {
                    wr.WriteLine(line);
                    line = "          ";
                }
                line += (p.x + " ");
                line += (p.y + " ");
                line += p.z;
                if (i < n - 1) line += ",";
            }
            //write the last row of points
            wr.WriteLine(line);

            line = "               ]#end of points"; wr.WriteLine(line);
            line = "        }#end of Coordinate"; wr.WriteLine(line);
            line = " #-------------------------"; wr.WriteLine(line);
            line = "        coordIndex["; wr.WriteLine(line);
            line = "          ";
            int nt = triangles.Count;
            int i1, i2, i3;
            for (int i = 0; i < nt; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                if (i % 50 == 0)
                {
                    wr.WriteLine(line);
                    line = "          ";
                }
                line += i1 + "," + i2 + ","+ i3 + "," + "-1";                    
                if (i < nt - 1) line += ",";
             }

             //the last row of coordIndex
             wr.WriteLine(line);
             line = "                  ]#end coordIndex"; wr.WriteLine(line);

            // if no texture then write colormap            
            if ( textureFile.Length < 1 && colors.Count > 0 )
            {
                vec4 cc;
                line = " #-------------------------"; wr.WriteLine(line);
                line = "       color Color"; wr.WriteLine(line);
                line = "       {"; wr.WriteLine(line);
                line = "          color["; wr.WriteLine(line);
                line = "                 ";
                for (int i = 0; i < colors.Count; i++)
                {
                    if (i % 50 == 0)
                    {
                        wr.WriteLine(line);
                        line = "                 ";
                    }
                    cc = GetPointColor(i);                   
                    line += ( cc.x + " "+ cc.y + " "+ cc.z );                    
                    if (i < colors.Count - 1) line += ",";
                }
            
                //write the last line of colors
                wr.WriteLine(line);
                line = "               ]#end of color[]"; wr.WriteLine(line);
                line = "       }#end of color"; wr.WriteLine(line);
            }//if ( textureFile.Length < 1 && colors.Count > 0 )

            if ( textureFile.Length > 1 && texCoords.Count > 1 )
            {
                line = " #-------------------------"; wr.WriteLine(line);
                line = "         texCoord TextureCoordinate "; wr.WriteLine(line);
                line = "         { point["; wr.WriteLine(line);
                line = "                 ";
                for (int i = 0; i < texCoords.Count; i++)
                {
                    if (i % 100 == 0)
                    {
                        wr.WriteLine(line);
                        line = "                 ";
                    }
                    
                    line += (texCoords[i].x + " " + texCoords[i].y + " ");
                    if (i < texCoords.Count - 1) line += ",";
                }
                //write the last line of texCoords
                wr.WriteLine(line);
                line = "           ]#end of point"; wr.WriteLine(line);
                line = "         }#end of TextureCoordinate"; wr.WriteLine(line);
            }

            line = "       normalPerVertex TRUE "; wr.WriteLine(line);
            line = "       colorPerVertex TRUE "; wr.WriteLine(line);
            line = "       solid TRUE "; wr.WriteLine(line);
            line = "     }#end of geometry"; wr.WriteLine(line);          
            line = "    }#end Shape "; wr.WriteLine(line);
            line = "   ]#end children "; wr.WriteLine(line);
            line = "}#end of transform"; wr.WriteLine(line);
            return true;
        }
    }
    */

        public class GeometryHelper
        {
            const double EquityTolerance = 0.000000001d;

            public static bool IsEqual(double d1, double d2)
            {
                return Math.Abs(d1 - d2) <= EquityTolerance;
            }
            //math logic from http://www.wyrmtale.com/blog/2013/115/2d-line-intersection-in-c
            public static bool GetIntersectionPoint(Vector32 l1p1, Vector32 l1p2, Vector32 l2p1, Vector32 l2p2, out Vector32 p)
            {
                p = new Vector32(0, 0, 0);

                double A1 = l1p2.Y - l1p1.Y;
                double B1 = l1p1.X - l1p2.X;
                double C1 = A1 * l1p1.X + B1 * l1p1.Y;

                double A2 = l2p2.Y - l2p1.Y;
                double B2 = l2p1.X - l2p2.X;
                double C2 = A2 * l2p1.X + B2 * l2p1.Y;

                //lines are parallel
                double det = A1 * B2 - A2 * B1;
                if (IsEqual(det, 0d))
                {
                    return false; //parallel lines
                }
                else
                {
                    double x = (B2 * C1 - B1 * C2) / det;
                    double y = (A1 * C2 - A2 * C1) / det;
                    bool online1 = ((Math.Min(l1p1.X, l1p2.X) < x || IsEqual(Math.Min(l1p1.X, l1p2.X), x))
                        && (Math.Max(l1p1.X, l1p2.X) > x || IsEqual(Math.Max(l1p1.X, l1p2.X), x))
                        && (Math.Min(l1p1.Y, l1p2.Y) < y || IsEqual(Math.Min(l1p1.Y, l1p2.Y), y))
                        && (Math.Max(l1p1.Y, l1p2.Y) > y || IsEqual(Math.Max(l1p1.Y, l1p2.Y), y))
                        );
                    bool online2 = ((Math.Min(l2p1.X, l2p2.X) < x || IsEqual(Math.Min(l2p1.X, l2p2.X), x))
                        && (Math.Max(l2p1.X, l2p2.X) > x || IsEqual(Math.Max(l2p1.X, l2p2.X), x))
                        && (Math.Min(l2p1.Y, l2p2.Y) < y || IsEqual(Math.Min(l2p1.Y, l2p2.Y), y))
                        && (Math.Max(l2p1.Y, l2p2.Y) > y || IsEqual(Math.Max(l2p1.Y, l2p2.Y), y))
                        );

                    if (online1 && online2)
                    {
                        p = new Vector32((float)x, (float)y, 0);
                        return true;
                    }
                }
                return false; //intersection is at out of at least one segment.
            }
        }

        //include polygons and lines
        public class PolygonSlicer : C3DObjectBase
        {
            //空间定位
            public AxisEnum axis = AxisEnum.zAxis;
            public List<Vector64> Locations2D = new List<Vector64>(); //Slicer平面定位点
            public List<Vector64> Locations3D = new List<Vector64>(); //空间定位点
            public Vector64 LocationCorner1 = new Vector64();   //空间坐标最低点
            public Vector64 LocationCorner2 = new Vector64();   //空间坐标最高点
            public double minxLocated = 0;  //空间定位后的坐标范围
            public double minyLocated = 0;
            public double minzLocated = 0;
            public double maxxLocated = 0;
            public double maxyLocated = 0;
            public double maxzLocated = 0;
            public List<Vector64> sampledGrids = new List<Vector64>();//网格采样后的空间散乱点
                                                                      //----------------------------------------------------------
                                                                      //辅助图形对象,多边形，线，etc
            public List<Polygon2D> polygons = new List<Polygon2D>();
            //地层对象，通过边界追踪提取的geo对象，或者绘制的geo对象，
            //包括layer和section line
            public List<Polygon2D> tracedGeoObjects = new List<Polygon2D>();
            public double backgroundPropertyValue { get; set; } = 0;
            public override double Minx
            {
                get
                {
                    if (IsLocated) return minxLocated;
                    else return minx;
                }
            }
            public override double Miny
            {
                get
                {
                    if (IsLocated) return minyLocated;
                    else return miny;
                }
            }
            public override double Minz
            {
                get
                {
                    if (IsLocated) return minzLocated;
                    else return minz;
                }
            }
            public override double Maxx
            {
                get
                {
                    if (IsLocated) return maxxLocated;
                    else return maxx;
                }
            }
            public override double Maxy
            {
                get
                {
                    if (IsLocated) return maxyLocated;
                    else return maxy;
                }
            }
            public override double Maxz
            {
                get
                {
                    if (IsLocated) return maxzLocated;
                    else return maxz;
                }
            }
            public bool ShowOutlines { get; set; } = false;
            public bool ShowLayers { get; set; } = true;
            public PolygonSlicer()
            {
                type = ShapeEnum.PolygonSlicer;
            }

            public PolygonSlicer Copy()
            {
                PolygonSlicer poly = new PolygonSlicer();

                foreach (Polygon2D p in polygons)
                    poly.AddPolygon(p.Copy());

                CopyHeader(poly);
                poly.axis = axis;
                poly.minxLocated = minxLocated;
                poly.minyLocated = minyLocated;
                poly.minzLocated = minzLocated;
                poly.maxxLocated = maxxLocated;
                poly.maxyLocated = maxyLocated;
                poly.maxzLocated = maxzLocated;
                poly.backgroundPropertyValue = backgroundPropertyValue;

                foreach (Polygon2D obj in tracedGeoObjects)
                    poly.tracedGeoObjects.Add(obj.Copy());

                Vector64 p1, p2;
                poly.Locations2D.Clear();
                poly.Locations3D.Clear();
                for (int i = 0; i < Locations2D.Count; i++)
                {
                    p1 = Locations2D[i];
                    p2 = Locations3D[i];
                    poly.AddLocationPoint(p1, p2);
                }

                poly.LocationCorner1 = LocationCorner1;
                poly.LocationCorner2 = LocationCorner2;

                for (int i = 0; i < sampledGrids.Count; i++)
                    poly.sampledGrids.Add(sampledGrids[i]);

                return poly;
            }
            public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
            {
                if (IsLocated)
                {
                    Vector64 p;

                    double zmax = maxz - minz; //MaxZ
                    double zlen = Math.Abs(LocationCorner2.z - LocationCorner1.z);//tracedZ
                    double zscale = zlen / zmax;

                    for (int i = 0; i < Locations3D.Count; i++)
                    {
                        p = Locations3D[i];
                        if (Maxx > Minx)
                            p.x = x1 + (x2 - x1) * (p.x - Minx) / (Maxx - Minx);
                        else p.x = (x1 + x2) / 2;
                        if (Maxy > Miny)
                            p.y = y1 + (y2 - y1) * (p.y - Miny) / (Maxy - Miny);
                        else p.y = (y1 + y2) / 2;
                        if (Maxz > Minz)
                            p.z = z1 + (z2 - z1) * (p.z - Minz) / (Maxz - Minz);
                        else p.z = (z1 + z2) / 2;
                        Locations3D[i] = p;
                    }
                    UpdateLocationRange();
                }
                else
                {
                    UpdateRange();
                    for (int i = 0; i < polygons.Count; i++)
                    {
                        Polygon2D poly = polygons[i];
                        for (int j = 0; j < poly.points.Count; j++)
                        {
                            Vector64 p = Locations3D[j];
                            if (Maxx > Minx)
                                p.x = (x1 + (x2 - x1) * (p.x - Minx) / (Maxx - Minx));
                            else p.x = ((x1 + x2) / 2);
                            if (Maxy > Miny)
                                p.y = (y1 + (y2 - y1) * (p.y - Miny) / (Maxy - Miny));
                            else p.y = (y1 + y2) / 2;
                            if (Maxz > Minz)
                                p.z = (z1 + (z2 - z1) * (p.z - Minz) / (Maxz - Minz));
                            else p.z = (z1 + z2) / 2;
                            poly.points[j] = p;
                        }
                        polygons[i] = poly;
                    }
                    for (int i = 0; i < tracedGeoObjects.Count; i++)
                    {
                        Polygon2D poly = tracedGeoObjects[i];
                        for (int j = 0; j < poly.points.Count; j++)
                        {
                            Vector64 p = Locations3D[j];
                            if (Maxx > Minx)
                                p.x = (x1 + (x2 - x1) * (p.x - Minx) / (Maxx - Minx));
                            else p.x = ((x1 + x2) / 2);
                            if (Maxy > Miny)
                                p.y = (y1 + (y2 - y1) * (p.y - Miny) / (Maxy - Miny));
                            else p.y = (y1 + y2) / 2;
                            if (Maxz > Minz)
                                p.z = (z1 + (z2 - z1) * (p.z - Minz) / (Maxz - Minz));
                            else p.z = (z1 + z2) / 2;
                            poly.points[j] = p;
                        }
                        tracedGeoObjects[i] = poly;
                    }
                    minx = x1;
                    maxx = x2;
                    miny = y1;
                    maxy = y2;
                    minz = z1;
                    maxz = z2;
                }
            }
            public override bool SaveAs(string path)
            {
                BinaryWriter br;
                try
                {
                    br = new BinaryWriter(new FileStream(path, FileMode.Create));
                    return SaveAs(ref br);
                }
                catch (IOException e)
                {
                    return false;
                }
            }
            public override bool LoadFrom(string path)
            {
                BinaryReader br;
                try
                {
                    br = new BinaryReader(new FileStream(path, FileMode.Open));
                    return LoadFrom(ref br);
                }
                catch (IOException e)
                {
                    return false;
                }
            }
            public override bool SaveAs(ref BinaryWriter br)
            {
                base.SaveAs(ref br);
                br.Write((int)axis);
                br.Write(minxLocated);
                br.Write(minyLocated);
                br.Write(minzLocated);
                br.Write(maxxLocated);
                br.Write(maxyLocated);
                br.Write(maxzLocated);
                br.Write(backgroundPropertyValue);

                br.Write(polygons.Count);
                foreach (Polygon2D obj in polygons)
                {
                    if (!obj.SaveAs(ref br)) return false;
                }
                br.Write(tracedGeoObjects.Count);
                foreach (Polygon2D obj in tracedGeoObjects)
                {
                    if (!obj.SaveAs(ref br)) return false;
                }
                Vector64 p1, p2;
                br.Write(Locations2D.Count);
                for (int i = 0; i < Locations2D.Count; i++)
                {
                    p1 = Locations2D[i];
                    p2 = Locations3D[i];
                    br.Write(p1.X);
                    br.Write(p1.Y);
                    br.Write(p1.Z);
                    br.Write(p2.X);
                    br.Write(p2.Y);
                    br.Write(p2.Z);
                }
                p1 = LocationCorner1;
                p2 = LocationCorner2;
                br.Write(p1.X);
                br.Write(p1.Y);
                br.Write(p1.Z);
                br.Write(p2.X);
                br.Write(p2.Y);
                br.Write(p2.Z);

                return true;
            }
            public override bool LoadFrom(ref BinaryReader br)
            {
                base.LoadFrom(ref br);
                axis = (AxisEnum)br.ReadInt32();
                minxLocated = br.ReadDouble();
                minyLocated = br.ReadDouble();
                minzLocated = br.ReadDouble();
                maxxLocated = br.ReadDouble();
                maxyLocated = br.ReadDouble();
                maxzLocated = br.ReadDouble();
                backgroundPropertyValue = br.ReadDouble();

                int n = br.ReadInt32();
                polygons.Clear();
                for (int i = 0; i < n; i++)
                {
                    Polygon2D obj = new Polygon2D();
                    if (!obj.LoadFrom(ref br)) return false;
                    polygons.Add(obj);
                }

                n = br.ReadInt32();
                tracedGeoObjects.Clear();
                for (int i = 0; i < n; i++)
                {
                    Polygon2D obj = new Polygon2D();
                    if (!obj.LoadFrom(ref br)) return false;
                    tracedGeoObjects.Add(obj);
                }

                n = br.ReadInt32();
                Locations2D.Clear();
                Locations3D.Clear();

                Vector64 p1, p2;
                double x, y, z;
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    p1 = new Vector64(x, y, z);
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    p2 = new Vector64(x, y, z);
                    AddLocationPoint(p1, p2);
                }

                x = br.ReadDouble();
                y = br.ReadDouble();
                z = br.ReadDouble();
                LocationCorner1 = new Vector64(x, y, z);
                x = br.ReadDouble();
                y = br.ReadDouble();
                z = br.ReadDouble();
                LocationCorner2 = new Vector64(x, y, z);

                return true;
            }
            public void AddTracedGeoObject(Polygon2D obj)
            {
                tracedGeoObjects.Add(obj);
            }
            public void AddPolygon(Polygon2D poly)
            {
                polygons.Add(poly);
            }
            public bool IsLocated
            {
                get
                {
                    if (Locations2D.Count > 1) return true;
                    else return false;
                }
            }
            public void AddLocationPoint(Vector64 p2d, Vector64 p3d)
            {
                Locations2D.Add(p2d);
                Locations3D.Add(p3d);
            }
            private void UpdateLocationCorner()
            {
                Vector64 p1, p2;
                for (int i = 0; i < Locations2D.Count; i++)
                {
                    p1 = Locations2D[i];
                    p2 = Locations3D[i];
                    if (i == 0)
                    {
                        LocationCorner1.Y = LocationCorner2.Y = p1.Y;
                        LocationCorner1.Z = LocationCorner2.Z = p2.Z;
                    }
                    else
                    {
                        if (LocationCorner1.Z > p2.Z)
                        {
                            LocationCorner1.Z = p2.Z;
                            LocationCorner1.Y = p1.Y;
                        }
                        if (LocationCorner2.Z < p2.Z)
                        {
                            LocationCorner2.Z = p2.Z;
                            LocationCorner2.Y = p1.Y;
                        }
                    }
                }
            }
            //更新空间定位数据
            public void UpdateTraced()
            {
                if (!IsLocated) return;
                SortLocation();
                UpdateLocationCorner();
                UpdateLocationRange();
            }
            private void SortLocation()
            {
                int n = Locations2D.Count;
                Vector64 p1, p2;
                for (int i = 0; i < n; i++)
                    for (int j = i + 1; j < n; j++)
                    {
                        p1 = Locations2D[i];
                        p2 = Locations2D[j];
                        if (p1.X > p2.X)
                        {
                            Locations2D[i] = p2;
                            Locations2D[j] = p1;
                            p1 = Locations3D[i];
                            p2 = Locations3D[j];
                            Locations3D[i] = p2;
                            Locations3D[j] = p1;
                        }
                    }

            }

            //根据屏幕坐标y，计算空间高程Z
            private double GetTracedElevation(double y)
            {
                double zz = LocationCorner2.Z - LocationCorner1.Z;
                return LocationCorner1.Z + zz * (y - LocationCorner1.y) / (LocationCorner2.y - LocationCorner1.y);
            }

            private Vector64 toTracedPoint(Vector64 p, int id1, int id2)
            {
                Vector64 p1 = Locations2D[id1];
                Vector64 p2 = Locations2D[id2];
                Vector64 v1 = Locations3D[id1];
                Vector64 v2 = Locations3D[id2];
                double scale = (p.X - p1.X) / (p2.X - p1.X);
                Vector64 v = v1 + scale * (v2 - v1);
                v.Z = GetTracedElevation(p.Y);
                v.v = p.v;
                return v;
            }
            public Vector64 toTracedPoint(double x, double y, double z, double v)
            {
                return toTracedPoint(new Vector64(x, y, z, v));
            }
            public Vector64 toTracedPoint(Vector64 p)
            {
                if (Locations2D.Count < 2) return p;
                //left side            
                if (p.X <= Locations2D[0].X)
                {
                    return toTracedPoint(p, 0, 1);
                }
                //right side            
                if (p.X >= Locations2D[Locations2D.Count - 1].X)
                {
                    return toTracedPoint(p, Locations2D.Count - 2, Locations2D.Count - 1);
                }

                for (int i = 1; i < Locations2D.Count; i++)
                {
                    if (p.X <= Locations2D[i].X) return toTracedPoint(p, i - 1, i);
                }
                return p;
            }

            //更新空间定位后的坐标范围
            private void UpdateLocationRange()
            {
                if (!IsLocated) return;

                Vector32 p;
                long i = 0;
                foreach (Polygon2D obj in polygons)
                {
                    foreach (Vector32 p0 in obj.points)
                    {
                        p = toTracedPoint(p0.toVector64());
                        if (i == 0)
                        {
                            minxLocated = maxxLocated = p.X;
                            minyLocated = maxyLocated = p.Y;
                            minzLocated = maxzLocated = p.Z;
                        }
                        else
                        {
                            if (p.X < minxLocated) minxLocated = p.X;
                            if (p.Y < minyLocated) minyLocated = p.Y;
                            if (p.Z < minzLocated) minzLocated = p.Z;
                            if (p.X > maxxLocated) maxxLocated = p.X;
                            if (p.Y > maxyLocated) maxyLocated = p.Y;
                            if (p.Z > maxzLocated) maxzLocated = p.Z;
                        }
                        i++;
                    }
                }
                foreach (Polygon2D obj in tracedGeoObjects)
                {
                    foreach (Vector32 p0 in obj.points)
                    {
                        p = toTracedPoint(p0.toVector64());
                        if (i == 0)
                        {
                            minxLocated = maxxLocated = p.X;
                            minyLocated = maxyLocated = p.Y;
                            minzLocated = maxzLocated = p.Z;
                        }
                        else
                        {
                            if (p.X < minxLocated) minxLocated = p.X;
                            if (p.Y < minyLocated) minyLocated = p.Y;
                            if (p.Z < minzLocated) minzLocated = p.Z;
                            if (p.X > maxxLocated) maxxLocated = p.X;
                            if (p.Y > maxyLocated) maxyLocated = p.Y;
                            if (p.Z > maxzLocated) maxzLocated = p.Z;
                        }
                        i++;
                    }
                }
                UpdateLocationCorner();
            }

            public override void UpdateRange()
            {
                Polygon2D poly;
                bool init = false;
                for (int i = 0; i < polygons.Count; i++)
                {
                    poly = polygons[i];
                    poly.UpdateRange();
                    if (i == 0)
                    {
                        minx = poly.minx;
                        maxx = poly.maxx;
                        miny = poly.miny;
                        maxy = poly.maxy;
                        minz = poly.minz;
                        maxz = poly.maxz;
                    }
                    else
                    {
                        if (poly.minx < minx) minx = poly.minx;
                        if (poly.maxx > maxx) maxx = poly.maxx;
                        if (poly.miny < miny) miny = poly.miny;
                        if (poly.maxy > maxy) maxy = poly.maxy;
                        if (poly.minz < minz) minz = poly.minz;
                        if (poly.maxz > maxz) maxz = poly.maxz;
                    }
                    init = true;
                }
                foreach (Polygon2D obj in tracedGeoObjects)
                {
                    obj.UpdateRange();
                    if (!init)
                    {
                        minx = obj.minx;
                        maxx = obj.maxx;
                        miny = obj.miny;
                        maxy = obj.maxy;
                        minz = obj.minz;
                        maxz = obj.maxz;
                    }
                    else
                    {
                        if (obj.minx < minx) minx = obj.minx;
                        if (obj.maxx > maxx) maxx = obj.maxx;
                        if (obj.miny < miny) miny = obj.miny;
                        if (obj.maxy > maxy) maxy = obj.maxy;
                        if (obj.minz < minz) minz = obj.minz;
                        if (obj.maxz > maxz) maxz = obj.maxz;
                    }
                }
            }
            //获取指定点地层属性值
            public double GetPropertyValue(double x, double y)
            {
                //intColor = 0;
                Vector32 p = new Vector32(x, y, 0);
                foreach (Polygon2D obj in tracedGeoObjects)
                {
                    if (obj.IsPointInsidePoly(p))
                    {
                        //intColor = ColorRGBA.ParseRGB(obj.fillColor);
                        return obj.PropertyValue;
                    }
                }
                return backgroundPropertyValue;
            }
            //--按照网格剖分采样，对地层属性值进行采样,xgrid横向,ygrid纵向
            public float[] SampleToGrid(int xgrid = 100, int ygrid = 100)
            {
                double x, y;
                double xstep = (maxx - minx) / (xgrid - 1);
                double ystep = (maxy - miny) / (ygrid - 1);

                float[] grids = new float[xgrid * ygrid];
                //int intColor = 0;
                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + ystep * iy;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + xstep * ix;
                        grids[ix + iy * xgrid] = (float)GetPropertyValue(x, y);
                    }
                }
                return grids;
            }
            //--按照网格剖分采样，网格节点转换到空间坐标对
            //地层属性值进行采样,xgrid横向,ygrid纵向
            // IsUniforGrid = true，全局网格，否则每个Polygon采用一个网格
            List<int> sampledPointsColor = new List<int>();
            public List<Vector32> SampleToPoints(int xgrid = 100, int ygrid = 100, bool IsUniformGrid = false)
            {
                sampledPointsColor.Clear();
                if (IsUniformGrid) return SampleToPointsUniform(xgrid, ygrid);
                else return SampleToPointsNotUniform(xgrid, ygrid);
            }
            private List<Vector32> SampleToPointsNotUniform(int xgrid = 40, int ygrid = 40)
            {
                Vector32 p;
                double x, y, xstep, ystep, val;
                List<Vector32> points = new List<Vector32>();

                foreach (Polygon2D obj in tracedGeoObjects)
                {
                    float[] grids = obj.SampleToGrid(xgrid, ygrid);

                    xstep = (obj.maxx - obj.minx) / (xgrid - 1);
                    ystep = (obj.maxy - obj.miny) / (ygrid - 1);

                    for (int iy = 0; iy < ygrid; iy++)
                    {
                        y = obj.miny + ystep * iy;
                        for (int ix = 0; ix < xgrid; ix++)
                        {
                            val = grids[ix + xgrid * iy];
                            if (val != 0)
                            {
                                x = obj.minx + xstep * ix;
                                p = toTracedPoint(x, y, 0, 0);
                                p.v = (float)val;
                                points.Add(p);
                                //sampledPointsColor.Add(ColorRGBA.ParseRGB(obj.fillColor));
                            }
                        }//for (int ix = 0; ix < xgrid; ix++)
                    }//for (int iy = 0; iy < ygrid; iy++)
                    grids = null;
                }//foreach (Polygon2D obj in tracedGeoObjects)
                return points;
            }

            private List<Vector32> SampleToPointsUniform(int xgrid = 100, int ygrid = 100)
            {
                double x, y;
                double xstep = (maxx - minx) / (xgrid - 1);
                double ystep = (maxy - miny) / (ygrid - 1);
                Vector32 p;
                //int intColor =0;
                List<Vector32> points = new List<Vector32>();
                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + ystep * iy;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + xstep * ix;
                        p = toTracedPoint(x, y, 0, 0);
                        p.v = (float)GetPropertyValue(x, y);
                        points.Add(p);
                        // sampledPointsColor.Add(intColor);
                    }
                }
                return points;
            }
            /// <summary>
            /// 当前点是否是其他多边形内点
            /// </summary>
            /// <param name="propertyval"></param>        
            /// <param name="ignorelayer">自身多边形不检查</param>
            /// <returns></returns>
            bool IsInsideLayer(double x, double y, double propertyval, int ignorelayer)
            {
                Polygon2D obj;
                for (int i = 0; i != ignorelayer && i < tracedGeoObjects.Count; i++)
                {
                    obj = tracedGeoObjects[i];
                    if (obj.PropertyValue == propertyval)
                    {
                        if (obj.IsPointInsidePoly(x, y))
                            return true;
                    }
                }
                return false;
            }
            //对指定的地层属性进行散乱点坐标采样
            public int SampleLayerCoords(ref List<Vector64> points, double propertyValue,
                bool sampleBoudary = true, bool sampleBoudaryInter = true, bool sampleBoudaryOuter = true,
                int xGridOuter = 101, int yGridOuter = 101,
                int xGridInter = 41, int yGridInter = 41,
                int outerExt = 1)
            {

                int sampled = 0;
                Polygon2D obj;
                List<Vector64> lists = new List<Vector64>();
                bool ignore;
                double xlen = maxx - minx;
                double ylen = maxy - miny;
                int xgrid, ygrid;
                for (int i = 0; i < tracedGeoObjects.Count; i++)
                {
                    obj = tracedGeoObjects[i];
                    if (obj.PropertyValue != propertyValue) continue;

                    xgrid = (int)(xGridOuter * (obj.maxx - obj.minx) / xlen) + 2;
                    ygrid = (int)(yGridOuter * (obj.maxy - obj.miny) / ylen) + 2;
                    //z = 0 边界点，1边界外点，2边界内点，
                    lists.Clear();
                    if (sampleBoudary) sampled += obj.SampleBoudary(ref lists);
                    if (sampleBoudaryOuter) sampled += obj.SampleOuterBoudary(ref lists, xgrid, ygrid, outerExt);

                    xgrid = (int)(xGridInter * (obj.maxx - obj.minx) / xlen) + 2;
                    ygrid = (int)(yGridInter * (obj.maxy - obj.miny) / ylen) + 2;

                    if (sampleBoudaryInter) sampled += obj.SampleInterBoudary(ref lists, xgrid, ygrid);

                    //剔除边界外点有可能和其他多边形内点重复点
                    foreach (Vector64 p in lists)
                    {
                        ignore = false;
                        //如果是边界或边界外点，还需要判别是否是其他多边形内点
                        if (p.z < 2) //边界点及外点
                        {
                            if (IsInsideLayer(p.x, p.y, p.v, i))
                                ignore = true;
                        }
                        if (!ignore)
                        {
                            //边界外点
                            if (p.z == 1) points.Add(new Vector64(p.x, p.y, 0, 0));
                            //边界点或者边界内点
                            else points.Add(new Vector64(p.x, p.y, 0, obj.PropertyValue));
                        }
                    }//foreach (Vector64 p1 in lists) 
                    lists.Clear();
                }//for ( int i = 0; i < tracedGeoObjects.Count; i++ )

                Vector64 p1, p2;
                for (int i = 0; i < points.Count; i++)
                {
                    p1 = points[i];
                    p2 = toTracedPoint(p1);
                    p2.v = p1.v;
                    points[i] = p2;
                }
                RemoveDuplicatedSampled(ref points);
                return points.Count;
            }
            //sample all 
            public List<Vector64> SampleLayerCoords(
                                         int xgrid = 101,//边界采样网格 
                                         int ygrid = 101,
                                         int xgridInside = 40,//内部采样网格
                                         int ygridInside = 40)
            {
                double x, y, z, v;
                double xx = (maxx - minx) / (xgrid - 1);
                double yy = (maxy - miny) / (ygrid - 1);

                int all = xgrid * ygrid;
                double[] grids = new double[all];

                sampledGrids.Clear();
                //采样到grids 中，grids[i] 保存tracedGeoObjects索引
                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + iy * yy + yy / 2;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + ix * xx + xx / 2;
                        for (int i = 0; i < tracedGeoObjects.Count; i++)
                        {
                            if (tracedGeoObjects[i].IsPointInsidePoly(x, y))
                            {
                                z = (i + 1);
                                grids[ix + iy * xgrid] = z;
                                break;
                            }
                        }
                    }//for (int ix = 0; ix < xgrid; ix++)
                }//for (int iy = 0; iy < ygrid; iy++)

                //搜索边界点，保存到boders中
                int id;
                double val = 0, val1 = 0;
                int[] Nears = new int[8];
                bool[] boders = new bool[all];//是否边界点
                for (int i = 0; i < all; i++) boders[i] = false;

                bool[] keep = new bool[all];//是否保存点
                for (int i = 0; i < all; i++) keep[i] = false;

                for (id = 0; id < all; id++)
                {
                    if (grids[id] < 1) continue;
                    val = tracedGeoObjects[(int)grids[id] - 1].PropertyValue;
                    //过滤为0的值
                    if (val == 0) continue;

                    Nears[0] = id - xgrid;
                    Nears[1] = id + xgrid;
                    Nears[2] = id - 1;
                    Nears[3] = id + 1;
                    Nears[4] = id - xgrid - 1;
                    Nears[5] = id + xgrid - 1;
                    Nears[6] = id - xgrid + 1;
                    Nears[7] = id + xgrid + 1;
                    foreach (int idnear in Nears)
                    {
                        if (idnear < 0 || idnear >= all)
                        {
                            boders[id] = true;
                            keep[id] = true;
                            break;
                        }

                        if (grids[idnear] < 1)//边界无值
                        {
                            boders[id] = true;
                            keep[id] = true;
                            break;
                        }
                        else
                        {
                            val1 = tracedGeoObjects[(int)grids[idnear] - 1].PropertyValue;
                            if (val != val1)
                            {
                                boders[id] = true;
                                keep[id] = true;
                                break;
                            }
                        }
                    }
                }//for (int id = 0; id < all; id++)

                //网格搜索半径
                int rx = (int)(xgrid / xgridInside / 2.0) + 1;
                int ry = (int)(ygrid / ygridInside / 2.0) + 1;

                double xx1 = (maxx - minx) / (xgridInside - 1);
                double yy1 = (maxy - miny) / (ygridInside - 1);
                int ix0, iy0, ix1, iy1;
                int id1;
                bool iskeep = true;
                Queue<int> searchList = new Queue<int>();
                //搜索内部点，按照剖分网格决定是否保存
                for (id = 0; id < all; id++)
                {
                    if (keep[id]) continue; //已经保存点
                    if (boders[id]) continue;//边界点
                    if (grids[id] < 1) continue;//无值点

                    iy0 = id / xgrid;
                    ix0 = id % xgrid;

                    //搜索周围点                
                    for (int iy = iy0 - ry; iy <= iy0 + ry; iy++)
                    {
                        for (int ix = ix0 - rx; ix <= ix0 + rx; ix++)
                        {
                            id1 = ix + iy * xgrid;
                            if (id1 < 0 || id1 >= all) continue;
                            if (id == id1) continue;
                            searchList.Enqueue(id1);
                        }
                    }
                    iskeep = true;
                    while (searchList.Count > 0)
                    {
                        id1 = searchList.Dequeue();
                        iy1 = id1 / xgrid;
                        ix1 = id1 % xgrid;
                        if (keep[id1] &&
                             Math.Abs(ix0 - ix1) * xx < xx1 &&
                             Math.Abs(iy0 - iy1) * yy < yy1)
                        {
                            iskeep = false;
                            break;
                        }
                    }
                    keep[id] = iskeep;
                }//for(int id =0;

                //输出
                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + iy * yy + yy / 2;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + ix * xx + xx / 2;
                        id = ix + iy * xgrid;
                        if (keep[id])
                        {
                            z = grids[id] - 1;
                            v = tracedGeoObjects[(int)z].PropertyValue;
                            sampledGrids.Add(new Vector64(x, y, z, v));
                        }
                    }
                }
                grids = null;
                boders = null;
                keep = null;

                return sampledGrids;
            }

            //对某个值进行采样 
            public List<Vector64> SampleLayerCoords(double layerValue,
                                                    int xgrid = 101,//边界采样网格 
                                                    int ygrid = 101,
                                                    int xgridInside = 40,//内部采样网格
                                                    int ygridInside = 40)
            {
                double x, y, z, v;
                double xx = (maxx - minx) / (xgrid - 1);
                double yy = (maxy - miny) / (ygrid - 1);

                int all = xgrid * ygrid;
                double[] grids = new double[all];

                sampledGrids.Clear();
                //采样到grids 中，grids[i] 保存tracedGeoObjects索引
                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + iy * yy + yy / 2;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + ix * xx + xx / 2;
                        foreach (Polygon2D poly in tracedGeoObjects)
                        {
                            if (poly.PropertyValue == layerValue &&
                                poly.IsPointInsidePoly(x, y))
                            {
                                grids[ix + iy * xgrid] = layerValue;
                                break;
                            }
                        }
                    }//for (int ix = 0; ix < xgrid; ix++)
                }//for (int iy = 0; iy < ygrid; iy++)

                //搜索边界点，保存到boders中
                int id;
                double val = 0, val1 = 0;
                int[] Nears = new int[8];
                bool[] boders = new bool[all];//是否边界点
                for (int i = 0; i < all; i++) boders[i] = false;

                bool[] keep = new bool[all];//是否保存点
                for (int i = 0; i < all; i++) keep[i] = false;

                for (id = 0; id < all; id++)
                {
                    if (grids[id] == 0) continue;

                    Nears[0] = id - xgrid;
                    Nears[1] = id + xgrid;
                    Nears[2] = id - 1;
                    Nears[3] = id + 1;
                    Nears[4] = id - xgrid - 1;
                    Nears[5] = id + xgrid - 1;
                    Nears[6] = id - xgrid + 1;
                    Nears[7] = id + xgrid + 1;
                    foreach (int idnear in Nears)
                    {
                        if (idnear < 0 || idnear >= all)
                        {
                            boders[id] = true;
                            keep[id] = true;
                            break;
                        }
                        if (grids[idnear] != grids[id])
                        {
                            boders[id] = true;
                            keep[id] = true;
                            break;
                        }
                    }
                }//for (int id = 0; id < all; id++)

                //边界内和边界外一起搜索
                //网格搜索半径
                int rx = (int)(xgrid / xgridInside / 2.0) + 1;
                int ry = (int)(ygrid / ygridInside / 2.0) + 1;

                double xx1 = (maxx - minx) / (xgridInside - 1);
                double yy1 = (maxy - miny) / (ygridInside - 1);
                int ix0, iy0, ix1, iy1;
                int id1;
                bool iskeep = true;
                Queue<int> searchList = new Queue<int>();
                //搜索内部点，按照剖分网格决定是否保存
                for (id = 0; id < all; id++)
                {
                    if (keep[id]) continue; //已经保存点
                    if (boders[id]) continue;//边界点

                    iy0 = id / xgrid;
                    ix0 = id % xgrid;

                    //搜索周围点                
                    for (int iy = iy0 - ry; iy <= iy0 + ry; iy++)
                    {
                        for (int ix = ix0 - rx; ix <= ix0 + rx; ix++)
                        {
                            id1 = ix + iy * xgrid;
                            if (id1 < 0 || id1 >= all) continue;
                            if (id == id1) continue;
                            searchList.Enqueue(id1);
                        }
                    }

                    iskeep = true;
                    while (searchList.Count > 0)
                    {
                        id1 = searchList.Dequeue();
                        iy1 = id1 / xgrid;
                        ix1 = id1 % xgrid;

                        if (keep[id1] && grids[id] == grids[id1] &&
                             Math.Abs(ix0 - ix1) * xx < xx1 &&
                             Math.Abs(iy0 - iy1) * yy < yy1)
                        {
                            iskeep = false;
                            break;
                        }
                    }
                    keep[id] = iskeep;
                }//for(int id =0;

                //输出
                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + iy * yy + yy / 2;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + ix * xx + xx / 2;
                        id = ix + iy * xgrid;
                        if (keep[id])
                        {
                            z = 0;
                            v = grids[id];
                            sampledGrids.Add(new Vector64(x, y, z, v));
                        }
                    }
                }

                grids = null;
                boders = null;
                keep = null;

                return sampledGrids;
            }
            /// <summary>
            /// 将可能重复采样的点过滤，优先删除边界外点
            /// </summary>
            /// <param name="points"></param>
            /// <param name="zerobase"></param>
            /// <returns></returns>
            int RemoveDuplicatedSampled(ref List<Vector64> points, double zerobase = 0.0001)
            {
                if (points.Count < 2) return 0;

                double dist;
                Vector64 p, p1, p2;
                double maxlen = 0;
                double x1 = 0, y1 = 0, z1 = 0;
                double x2 = 0, y2 = 0, z2 = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    if (i == 0)
                    {
                        x1 = x2 = p.x;
                        y1 = y2 = p.y;
                        z1 = z2 = p.z;
                    }
                    else
                    {
                        if (p.x < x1) x1 = p.x;
                        if (p.y < y1) y1 = p.y;
                        if (p.z < z1) z1 = p.z;
                        if (p.x > x2) x2 = p.x;
                        if (p.y > y2) y2 = p.y;
                        if (p.z > z2) z2 = p.z;
                    }
                }
                maxlen = x2 - x1;
                if (y2 - y1 > maxlen) maxlen = y2 - y1;
                if (z2 - z1 > maxlen) maxlen = z2 - z1;

                double err = maxlen * zerobase;

                bool[] del = new bool[points.Count];
                for (int i = 0; i < points.Count; i++) del[i] = false;

                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = i + 1; j < points.Count; j++)
                    {
                        p1 = points[i];
                        p2 = points[j];
                        dist = Math.Abs(p1.x - p2.x) +
                               Math.Abs(p1.y - p2.y) +
                               Math.Abs(p1.z - p2.z);
                        if (dist <= err)
                        {
                            if (p2.v == 0) del[j] = true;
                            else del[i] = true;
                        }
                    }
                }
                int num = 0;
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    if (del[i]) { points.RemoveAt(i); num++; }
                }
                del = null;
                return num;
            }
            //将地层属性数据输出为散乱点XYZ格式
            public bool ExportLayerPropertyToXYZ(string filename)
            {
                try
                {
                    bool appended = false;
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Exists && fi.Length > 0) appended = true;

                    FileStream fs;
                    if (!fi.Exists) fs = new FileStream(filename, FileMode.CreateNew);
                    else fs = new FileStream(filename, FileMode.Append);

                    StreamWriter wr = new StreamWriter(fs);

                    string line = "x,   y,  z,  value";
                    if (!appended) wr.WriteLine(line);

                    Vector64 p1;
                    foreach (Vector64 p in sampledGrids)
                    {
                        p1 = toTracedPoint(p);
                        line = p1.x + "," + p1.y + "," + p1.z + "," + p1.v;
                        wr.WriteLine(line);
                    }

                    wr.Close();
                    fs.Close();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }//public bool ExportLayerPropertyToXYZ       
        }
        public enum ClockDirection
        {
            /// <summary>
            /// 无.可能是不可计算的图形，比如多点共线
            /// </summary>
            None = 0,
            /// <summary>
            /// 顺时针方向
            /// </summary>
            Clockwise = 1,
            /// <summary>
            /// 逆时针方向
            /// </summary>
            Counterclockwise = 2
        }
        public enum PolygonType
        {
            /// <summary>
            /// 无.不可计算的多边形(比如多点共线)
            /// </summary>
            None = 0,
            /// <summary>
            /// 凸多边形
            /// </summary>
            Convex = 1,
            /// <summary>
            /// 凹多边形
            /// </summary>
            Concave = 2
        }

        public class Polygon2D : C3DObjectBase
        {
            [CategoryAttribute("Object Properties"), DisplayNameAttribute("Is Closed")]
            public bool IsClosed { get; set; } = true;
            [CategoryAttribute("Object Properties"), DisplayNameAttribute("Fill Color")]
            public Color fillColor { get; set; } = Color.White;
            [CategoryAttribute("Object Properties"), DisplayNameAttribute("Line Color")]
            public Color lineColor { get; set; } = Color.Black;
            [CategoryAttribute("Object Properties"), DisplayNameAttribute("Property Value")]
            public double PropertyValue { get; set; } = 0; //property value

            //z == 0        
            public List<Vector64> points = new List<Vector64>();
            //切面轴，默认垂向切面z
            public AxisEnum axis = AxisEnum.zAxis;

            [CategoryAttribute("Display"), DisplayNameAttribute("Line Width")]
            public float lineWidth { get; set; } = 1.0f;
            [CategoryAttribute("Display"), DisplayNameAttribute("Is Filled")]
            public bool IsFill { get; set; } = false;

            /*
            [CategoryAttribute("DOT"), DisplayNameAttribute("Show Dot")]
            public bool IsShowDot { get; set; } = false;

            [CategoryAttribute("DOT"), DisplayNameAttribute("Dot Size")]
            public float dotSize { get; set; } = 8;
            [CategoryAttribute("DOT"), DisplayNameAttribute("Dot Color")]
            public Color dotColor { get; set; } = Color.Red;
            */
            public Vector64 this[int index]
            {
                get
                {
                    return points[index];
                }
                set
                {
                    points[index] = value;
                }
            }
            public Polygon2D()
            {
                type = ShapeEnum.Polygon2D;
            }
            public Polygon2D(Vector64[] array)
            {
                type = ShapeEnum.Polygon2D;
                for (int i = 0; i < array.Length; i++)
                    points.Add(array[i]);
                UpdateRange();
            }
            public Polygon2D(Vector32[] array)
            {
                type = ShapeEnum.Polygon2D;
                for (int i = 0; i < array.Length; i++)
                    points.Add(array[i].toVector64());
                UpdateRange();
            }
            public Polygon2D(List<Vector64> lists)
            {
                type = ShapeEnum.Polygon2D;
                for (int i = 0; i < lists.Count; i++)
                    points.Add(lists[i]);
                UpdateRange();
            }
            public Polygon2D(List<Vector32> lists)
            {
                type = ShapeEnum.Polygon2D;
                for (int i = 0; i < lists.Count; i++)
                    points.Add(lists[i].toVector64());
                UpdateRange();
            }
            public int Count { get { return points.Count; } }
            public Polygon2D Copy()
            {
                Polygon2D poly = new Polygon2D(points);
                CopyHeader(poly);
                poly.IsClosed = IsClosed;
                poly.IsFill = IsFill;
                poly.fillColor = fillColor;
                poly.lineColor = lineColor;
                poly.PropertyValue = PropertyValue;
                poly.axis = axis;
                poly.lineWidth = lineWidth;

                return poly;
            }

            /// <summary>
            /// 判断多边形是顺时针还是逆时针.
            /// </summary>
            /// <param name="points">所有的点</param>
            /// <param name="isYAxixToDown">true:Y轴向下为正(屏幕坐标系),false:Y轴向上为正(一般的坐标系)</param>
            /// <returns></returns>
            public ClockDirection CalculateClockDirection(bool isYAxixToDown = false)
            {
                int i, j, k;
                int count = 0;
                double z;
                int yTrans = isYAxixToDown ? (-1) : (1);
                if (points.Count < 3)
                {
                    return ClockDirection.None;
                }

                int n = points.Count;
                for (i = 0; i < n; i++)
                {
                    j = (i + 1) % n;
                    k = (i + 2) % n;
                    z = (points[j].X - points[i].X) * (points[k].Y * yTrans - points[j].Y * yTrans);
                    z -= (points[j].Y * yTrans - points[i].Y * yTrans) * (points[k].X - points[j].X);
                    if (z < 0)
                    {
                        count--;
                    }
                    else if (z > 0)
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    return (ClockDirection.Counterclockwise);
                }
                else if (count < 0)
                {
                    return (ClockDirection.Clockwise);
                }
                else
                {
                    return (ClockDirection.None);
                }
            }
            public override bool SaveAs(ref BinaryWriter br)
            {
                base.SaveAs(ref br);
                br.Write(points.Count);
                foreach (Vector32 p in points)
                {
                    br.Write(p.x);
                    br.Write(p.y);
                    br.Write(p.z);
                }
                br.Write(IsClosed);
                br.Write(IsFill);
                br.Write(ColorRGBA.ParseRGB(fillColor));
                br.Write(ColorRGBA.ParseRGB(lineColor));
                br.Write(PropertyValue);
                br.Write((int)axis);
                br.Write(lineWidth);
                return true;
            }
            public override bool LoadFrom(ref BinaryReader br)
            {
                points.Clear();
                base.LoadFrom(ref br);
                int n = br.ReadInt32();
                float x, y, z;
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    Add(x, y, z);
                }

                IsClosed = br.ReadBoolean();
                IsFill = br.ReadBoolean();

                fillColor = ColorRGBA.RGB(br.ReadInt32());
                lineColor = ColorRGBA.RGB(br.ReadInt32());

                PropertyValue = br.ReadDouble();
                axis = (AxisEnum)br.ReadInt32();
                lineWidth = br.ReadSingle();

                return true;
            }

            public override bool SaveAs(string path)
            {
                BinaryWriter br;
                try
                {
                    br = new BinaryWriter(new FileStream(path, FileMode.Create));
                    return SaveAs(ref br);
                }
                catch (IOException e)
                {
                    return false;
                }
            }
            public override bool LoadFrom(string path)
            {
                BinaryReader br;
                try
                {
                    br = new BinaryReader(new FileStream(path, FileMode.Open));
                    return LoadFrom(ref br);
                }
                catch (IOException e)
                {
                    return false;
                }
            }
            public void Offset(double offx, double offy, double offz)
            {
                Vector64 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    p.x += (float)offx;
                    p.y += (float)offy;
                    p.z += (float)offz;
                    points[i] = p;
                }
                minx += offx;
                miny += offy;
                minz += offz;
                maxx += offx;
                maxy += offy;
                maxz += offz;
            }
            public void Add(Vector64 p)
            {
                points.Add(p);
            }
            public void Add(double x, double y, double z = 0)
            {
                points.Add(new Vector64(x, y, z));
            }
            /// <summary>
            /// 简化点，将直线上冗余点去掉
            /// 过滤掉距离小于平均点距的1/filter的点
            /// 删除距离很近的点
            /// </summary>
            public void Simplify(bool distFilter = true, bool lineFilter = true, double zero = 0.001)
            {
                //去重点,过滤两点点距小于总长1/1000的点 zero = 0.001
                if (distFilter) Vector64.RemoveLineDuplicated(ref points, zero);
                if (lineFilter) Vector64.RemoveLineRedundant(ref points);
                UpdateRange();
            }
            public override void UpdateRange()
            {
                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    if (i == 0)
                    {
                        minx = maxx = p.X;
                        miny = maxy = p.Y;
                        minz = maxz = p.Z;
                    }
                    else
                    {
                        if (p.x < minx) minx = p.x;
                        if (p.x > maxx) maxx = p.x;

                        if (p.y < miny) miny = p.y;
                        if (p.y > maxy) maxy = p.y;

                        if (p.z < minz) minz = p.z;
                        if (p.z > maxz) maxz = p.z;
                    }
                }
            }
            //多边形剖分成网格，返回网格的值
            public float[] SampleToGrid(int xgrid = 100, int ygrid = 100)
            {
                double x, y;
                double xstep = (maxx - minx) / (xgrid - 1);
                double ystep = (maxy - miny) / (ygrid - 1);

                float[] grids = new float[xgrid * ygrid];

                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = miny + ystep * iy;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + xstep * ix;
                        if (IsPointInsidePoly(x, y))
                            grids[ix + iy * xgrid] = (float)PropertyValue;
                        else grids[ix + iy * xgrid] = 0;
                    }
                }
                return grids;
            }
            public int SampleBoudary(ref List<Vector64> lists)
            {
                double x, y, z, v = PropertyValue;
                foreach (Vector32 p in points)
                {
                    x = p.x;
                    y = p.y;
                    z = 0; //边界点
                    lists.Add(new Vector64(x, y, z, v));
                }
                return points.Count;
            }
            //对边界及边界内坐标进行采样
            public int SampleInterBoudary(ref List<Vector64> lists, int xgrid = 11, int ygrid = 11)
            {
                double xstep = (maxx - minx) / (xgrid - 1);
                double ystep = (maxy - miny) / (ygrid - 1);
                double x, y;
                int sampled = 0;
                for (int iy = 0; iy < ygrid; iy++)
                {
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        x = minx + ix * xstep + xstep / 2;
                        y = miny + iy * ystep + ystep / 2;
                        if (IsPointInsidePoly(x, y))
                        {
                            lists.Add(new Vector64(x, y, 2, PropertyValue));//z = 2 边界内点
                            sampled++;
                        }
                    }
                }
                return sampled;
            }
            //对外边界坐标进行采样,采用边界坐标和边界外的坐标
            //保存坐标点：lists, x,y,0,v = 0
            //采样网格，xgrid,ygrid
            //网格扩展数：ext
            //返回：采样点数
            public int SampleOuterBoudary(ref List<Vector64> lists, int xgrid = 21, int ygrid = 21, int ext = 1)
            {
                double xstep = (maxx - minx) / (xgrid - 1);
                double ystep = (maxy - miny) / (ygrid - 1);
                double x, y;

                //grid width and height
                int width = xgrid + 2 * ext;
                int height = ygrid + 2 * ext;

                bool[] boders = new bool[width * height];

                for (int iy = 0; iy < height; iy++)
                {
                    for (int ix = 0; ix < width; ix++)
                    {
                        x = minx + (ix - ext) * xstep + xstep / 2;
                        y = miny + (iy - ext) * ystep + ystep / 2;
                        if (IsPointInsidePoly(x, y))
                            boders[iy * width + ix] = true;
                        else
                            boders[iy * width + ix] = false;
                    }
                }


                int id, ix1, iy1;
                int sampled = 0;
                bool keep = true;
                int[] nears = new int[8];
                for (int iy = 0; iy < height; iy++)
                {
                    for (int ix = 0; ix < width; ix++)
                    {
                        id = iy * width + ix;
                        if (boders[id]) continue;//只处理边界外点

                        nears[0] = id - 1;
                        nears[1] = id + 1;
                        nears[2] = id - width;
                        nears[3] = id + width;
                        nears[4] = id - width - 1;
                        nears[5] = id - width + 1;
                        nears[6] = id + width - 1;
                        nears[7] = id + width + 1;

                        keep = false;
                        for (int j = 0; j < 8; j++)
                        {
                            if (nears[j] >= 0 && nears[j] < boders.Length &&
                                 boders[nears[j]]) //相邻点内点，则为边界点
                            {
                                iy1 = nears[j] / width;
                                ix1 = nears[j] - iy1 * width;
                                x = minx + (ix1 - ext) * xstep + xstep / 2;
                                y = miny + (iy1 - ext) * ystep + ystep / 2;
                                lists.Add(new Vector64(x, y, 2, PropertyValue));//z =2 边界内点
                                sampled++;
                                keep = true;
                                break;
                            }
                        }
                        if (keep)//边界外点
                        {
                            x = minx + (ix - ext) * xstep + xstep / 2;
                            y = miny + (iy - ext) * ystep + ystep / 2;
                            lists.Add(new Vector64(x, y, 1, PropertyValue));//z =1 边界外点
                            sampled++;
                        }
                    }
                }
                boders = null;
                nears = null;
                return sampled;
            }

            public void Clear()
            {
                points.Clear();
            }
            //点在多边形内判断，2D版本
            // taken from https://wrf.ecse.rpi.edu//Research/Short_Notes/pnpoly.html
            public bool IsPointInsidePoly(Vector32 test)
            {
                return IsPointInsidePoly(test.x, test.y);
            }
            public bool IsPointInsidePoly(double x, double y)
            {
                int i;
                int j;
                bool result = false;
                double x1, y1, x2, y2;
                for (i = 0, j = points.Count - 1; i < points.Count; j = i++)
                {
                    x1 = points[i].x;
                    y1 = points[i].y;
                    x2 = points[j].x;
                    y2 = points[j].y;

                    if ((y1 > y) != (y2 > y) && (x < (x2 - x1) * (y - y1) / (y2 - y1) + x1))
                    {
                        result = !result;
                    }
                }
                return result;
            }

            //get intersections with line
            public virtual Vector64[] GetIntersectPoints(CLine line)
            {
                List<Vector64> intersets = new List<Vector64>();
                Vector64 p1, p2, p;
                for (int i = 0; i < points.Count - 1; i++)
                {
                    p1 = points[i];
                    p2 = points[i + 1];
                    CLine s1 = new CLine(p1, p2);
                    if (line.GetIntersection(s1, out p))
                    {
                        intersets.Add(p);
                        break;
                    }
                }
                return intersets.ToArray();
            }


            //Finding Intersection Points of a line segment and given convex polygon
            public virtual Vector32[] GetIntersectionPoints(Vector32 l1p1, Vector32 l1p2)
            {
                List<Vector32> intersectionPoints = new List<Vector32>();
                Vector32 ip;
                for (int i = 0; i < points.Count; i++)
                {
                    int next = (i + 1 == points.Count) ? 0 : i + 1;
                    if (GeometryHelper.GetIntersectionPoint(l1p1, l1p2, points[i], points[next], out ip))
                        intersectionPoints.Add(ip);
                }
                return intersectionPoints.ToArray();
            }

            //One Important Tip
            //Some edge cases, such as two overlapping corners or intersection on a corner can cause 
            //some duplicates corner added to the polygon.
            //We can easily get rid of these with such small utility function:
            public static void Add(List<Vector32> pool, Vector32[] newpoints)
            {
                foreach (Vector32 np in newpoints)
                {
                    bool found = false;
                    foreach (Vector32 p in pool)
                    {
                        if (GeometryHelper.IsEqual(p.X, np.X) && GeometryHelper.IsEqual(p.Y, np.Y))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) pool.Add(np);
                }
            }
            //Ordering the corners of a polygon clockwise
            public static Vector32[] OrderClockwise(Vector32[] points)
            {
                double mX = 0;
                double my = 0;
                foreach (Vector32 p in points)
                {
                    mX += p.X;
                    my += p.Y;
                }
                mX /= points.Length;
                my /= points.Length;

                return points.OrderBy(v => Math.Atan2(v.Y - my, v.X - mX)).ToArray();
            }
            //main algorithm
            public static Polygon2D GetIntersectionOfPolygons(Polygon2D poly1, Polygon2D poly2)
            {
                List<Vector32> clippedCorners = new List<Vector32>();

                //Add  the corners of poly1 which are inside poly2       
                for (int i = 0; i < poly1.points.Count; i++)
                {
                    if (poly2.IsPointInsidePoly(poly1.points[i]))
                        Add(clippedCorners, new Vector32[] { poly1.points[i] });
                }

                //Add the corners of poly2 which are inside poly1
                for (int i = 0; i < poly2.points.Count; i++)
                {
                    if (poly1.IsPointInsidePoly(poly2.points[i]))
                        Add(clippedCorners, new Vector32[] { poly2.points[i] });
                }

                //Add  the intersection points
                for (int i = 0, next = 1; i < poly1.points.Count;
                     i++, next = (i + 1 == poly1.points.Count) ? 0 : i + 1)
                {
                    Add(clippedCorners, poly2.GetIntersectionPoints(poly1.points[i], poly1.points[next]));
                }

                return new Polygon2D(OrderClockwise(clippedCorners.ToArray()));
            }
        }
        public class Box3D : Polygon3D
        {
            public Box3D(Vector32 p0, double xl, double yl, double zl)
            {
                double x = p0.x;
                double y = p0.y;
                double z = p0.z;
                double xs = xl / 2;
                double ys = yl / 2;
                double zs = zl / 2;
                AddPoint(x - xs / 2, y - ys / 2, z - zs / 2);
                AddPoint(x + xs / 2, y - ys / 2, z - zs / 2);
                AddPoint(x + xs / 2, y + ys / 2, z - zs / 2);
                AddPoint(x - xs / 2, y + ys / 2, z - zs / 2);
                AddPoint(x - xs / 2, y - ys / 2, z + zs / 2);
                AddPoint(x + xs / 2, y - ys / 2, z + zs / 2);
                AddPoint(x + xs / 2, y + ys / 2, z + zs / 2);
                AddPoint(x - xs / 2, y + ys / 2, z + zs / 2);
                int[] indices = new int[] { 0, 1, 5, 0, 5, 4, 1, 2, 5, 2, 6, 5, 2, 3, 6, 6, 3, 7, 4, 7, 3, 4, 3, 0, 6, 4, 5, 6, 7, 4, 3, 2, 1, 3, 1, 0 };
                for (int i = 0; i < indices.Length / 3; i++)
                {
                    AddTriangleIndex(indices[3 * i], indices[3 * i + 1], indices[3 * i + 2]);
                }
                indices = null;
                UpdateRange();
            }
            /*
            public override bool IsPointInPolygon(Vector32 point)
            {
                //移动到中心，            
                Vector32 p = UnTransformedPoint(point);
                if( p.x > minx && p.x < maxx &&
                    p.y > miny && p.y < maxy &&
                    p.z > minz && p.z < maxz )
                    return true;
                return false;
            }*/
        }
        public enum PointLineRelation
        {
            None = -1,
            Left = 0,
            Right = 1,
            OnLine = 2,
            OnLineExtend = 10
        }
        public class CLine
        {
            //x = x0 + at
            //y = y0 + bt
            //z = z0 + ct
            public Vector64 p1;
            public Vector64 p2;
            public CLine()
            {

            }
            public CLine(Vector64 _p1, Vector64 _p2)
            {
                p1 = _p1;
                p2 = _p2;
            }
            public CLine(Vector32 _p1, Vector32 _p2)
            {
                p1 = new Vector64(_p1.x, _p1.y, _p1.z);
                p2 = new Vector64(_p2.x, _p2.y, _p2.z);
            }
            /// <summary>
            /// 线方向，单位矢量
            /// </summary>
            public Vector64 Direction
            {
                get
                {
                    Vector64 v = (p2 - p1);
                    return v.Normalize();
                }
            }
            public double a
            {
                get { return p2.x - p1.x; }
            }
            public double b
            {
                get { return p2.y - p1.y; }
            }
            public double c
            {
                get { return p2.z - p1.z; }
            }
            public double x0
            {
                get { return p1.x; }
            }
            public double y0
            {
                get { return p1.y; }
            }
            public double z0
            {
                get { return p1.z; }
            }
            static public bool IsZero(double val, double zero = 1.0E-20)
            {
                double v = val;
                if (v < 0) v = -v;
                if (v <= zero) return true;
                else return false;
            }
            /// <summary>
            /// p点是否在线段p1p2上，通过距离来判断
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            //——p---p1 -- p --- p2——p
            public bool IsOnLine(Vector32 p)
            {
                double dist1 = p.Distance(p1);
                double dist2 = p.Distance(p2);
                double dist = p1.Distance(p2);
                if (IsEquals(dist1 + dist2, dist)) return true;
                else return false;
            }
            /// <summary>
            /// 点是否在直线及延长线上
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            /// //——p---p1 -- p --- p2——p
            public bool IsOnLineExt(Vector32 p)
            {
                double dist1 = p.Distance(p1);
                double dist2 = p.Distance(p2);
                double dist12 = p1.Distance(p2);
                if (IsEquals(dist1 + dist2, dist12)) return true;
                if (IsEquals(dist1 + dist12, dist2)) return true;
                if (IsEquals(dist2 + dist12, dist1)) return true;
                return false;
            }
            public static bool IsEquals(double v1, double v2, double err = 1.0E-20)
            {
                if (v1 >= v2 - err && v2 <= v2 + err) return true;
                else return false;
            }
            //p点到直线距离p1,p2
            //二维版本
            double Distance(Vector32 p)
            {
                double a = p2.y - p1.y;
                double b = p1.x - p2.x;
                double c = p2.x * p1.y - p1.x * p2.y;
                if (a == 0 && b == 0)
                    return Vector32.Distance(p1, p);
                else return Math.Abs(a * p.x + b * p.y + c) / Math.Sqrt(a * a + b * b);
            }
            //点与直线的关系2D version
            public PointLineRelation GetPointReletion(Vector32 p)
            {
                double x0 = p.x;
                double y0 = p.y;
                double z0 = p.z;
                double val = (p2.x - p1.x) * (y0 - p1.y) - (x0 - p1.x) * (p2.y - p1.y);
                if (IsZero(val))
                {
                    if (IsOnLine(p)) //在直线上
                        return PointLineRelation.OnLine;
                    else return PointLineRelation.OnLineExtend;
                }
                else
                {
                    if (val > 0) return PointLineRelation.Left;
                    else return PointLineRelation.Right;
                }
            }
            //点到直线投影2D
            public Vector32 GetPointProjection(Vector32 p)
            {
                Vector32 p0 = new Vector32();

                double dx = p1.x - p2.x;
                double dy = p1.y - p2.y;

                double sq2 = (dx * dx) + (dy * dy);

                if (sq2 == 0) return p1;

                double u = (p.x - p1.x) * (p1.x - p2.x) +
                           (p.y - p1.y) * (p1.y - p2.y);

                u = u / sq2;

                p0.x = (float)(p1.x + u * dx);
                p0.y = (float)(p1.y + u * dy);

                return p0;
            }
            //直线line是否在当前直线矩形范围内
            public bool IsRectIntersect(CLine line)
            {
                if (Math.Min(p1.x, p2.x) <= Math.Max(line.p1.x, line.p2.x) &&
                     Math.Min(p1.y, p2.y) <= Math.Max(line.p1.y, line.p2.y) &&
                     Math.Min(p1.z, p2.z) <= Math.Max(line.p1.z, line.p2.z) &&
                     Math.Min(line.p1.x, line.p2.x) <= Math.Max(p1.x, p2.x) &&
                     Math.Min(line.p1.y, line.p2.y) <= Math.Max(p1.y, p1.y) &&
                     Math.Min(line.p1.z, line.p2.z) <= Math.Max(p1.z, p2.z))
                    return true;
                else return false;
            }

            /// <summary>
            /// 判断线与线之间的相交
            /// </summary>
            /// <param name="intersection">交点</param>
            /// <param name="p1">直线1上一点</param>
            /// <param name="v1">直线1方向,单位矢量</param>
            /// <param name="p2">直线2上一点</param>
            /// <param name="v2">直线2方向,单位矢量</param>
            /// <returns>是否相交</returns>
            public static bool GetIntersection(CLine line1, CLine line2, out Vector64 intersection)
            {
                intersection = new Vector64();

                Vector64 P1 = line1.p1;
                Vector64 P2 = line2.p1;
                Vector64 V1 = line1.Direction;
                Vector64 V2 = line2.Direction;

                // 两线是否平行
                if (IsZero(Vector64.Dot(V1, V2))) return false;
                Vector64 startPointSeg = P2 - P1;
                Vector64 vecS1 = Vector64.Cross(V1, V2);            // 有向面积1
                Vector64 vecS2 = Vector64.Cross(startPointSeg, V2); // 有向面积2
                double num = Vector64.Dot(startPointSeg, vecS1);
                // 判断两这直线是否共面
                if (Math.Abs(num) >= 1E-05)
                {
                    return false;
                }

                // 有向面积比值，利用点乘是因为结果可能是正数或者负数
                double num2 = Vector64.Dot(vecS2, vecS1) / vecS1.sqrMagnitude;
                intersection = P1 + V1 * num2;
                return true;
            }
            //---线段相交并求交点：2D版本-------------
            // this is only 2D version, improvement needed
            //作者：Away - Far
            //来源：CSDN
            //原文：https://blog.csdn.net/wcl0617/article/details/78654944 
            //版权声明：本文为博主原创文章，转载请附上博文链接！
            //int get_line_intersection(float p0_x, float p0_y, float p1_x, float p1_y,
            //float p2_x, float p2_y, float p3_x, float p3_y, float* i_x, float* i_y)
            public bool GetIntersection(CLine line, out Vector64 p)
            {
                double p0_x = p1.x;
                double p0_y = p1.y;
                double p1_x = p2.x;
                double p1_y = p2.y;
                double p2_x = line.p1.x;
                double p2_y = line.p1.y;
                double p3_x = line.p2.x;
                double p3_y = line.p2.y;

                double s02_x, s02_y, s10_x, s10_y, s32_x, s32_y, s_numer, t_numer, denom, t;
                s10_x = p1_x - p0_x;
                s10_y = p1_y - p0_y;
                s32_x = p3_x - p2_x;
                s32_y = p3_y - p2_y;
                p = new Vector64(0, 0, 0);
                denom = s10_x * s32_y - s32_x * s10_y;
                if (denom == 0)//平行或共线
                    return false;
                bool denomPositive = denom > 0;

                s02_x = p0_x - p2_x;
                s02_y = p0_y - p2_y;
                s_numer = s10_x * s02_y - s10_y * s02_x;

                //参数是大于等于0且小于等于1的，分子分母必须同号且分子小于等于分母
                if ((s_numer < 0) == denomPositive)
                    return false; // No collision

                t_numer = s32_x * s02_y - s32_y * s02_x;
                if ((t_numer < 0) == denomPositive)
                    return false; // No collision

                if (Math.Abs(s_numer) > Math.Abs(denom) ||
                     Math.Abs(t_numer) > Math.Abs(denom))
                    return false; // No collision,Collision detected

                t = t_numer / denom;

                p.x = p0_x + (t * s10_x);
                p.y = p0_y + (t * s10_y);

                return true;
            }
            //直线与直线相交于延长线
            //2D版本
            public bool GetIntersectionExt(CLine line, out Vector64 p)
            {
                double p0_x = p1.x;
                double p0_y = p1.y;
                double p1_x = p2.x;
                double p1_y = p2.y;
                double p2_x = line.p1.x;
                double p2_y = line.p1.y;
                double p3_x = line.p2.x;
                double p3_y = line.p2.y;

                double s02_x, s02_y, s10_x, s10_y, s32_x, s32_y, s_numer, t_numer, denom, t;
                s10_x = p1_x - p0_x;
                s10_y = p1_y - p0_y;
                s32_x = p3_x - p2_x;
                s32_y = p3_y - p2_y;
                p = new Vector64(0, 0, 0);
                denom = s10_x * s32_y - s32_x * s10_y;
                if (denom == 0)//平行或共线
                    return false;
                bool denomPositive = denom > 0;

                s02_x = p0_x - p2_x;
                s02_y = p0_y - p2_y;
                s_numer = s10_x * s02_y - s10_y * s02_x;
                t_numer = s32_x * s02_y - s32_y * s02_x;
                t = t_numer / denom;
                p.x = p0_x + (t * s10_x);
                p.y = p0_y + (t * s10_y);

                return true;
            }
            // two extended lines intersection
            /*
            public bool GetIntersectionExt(CLine line,out Vector32 p)
            {
                p = new Vector32(0, 0, 0);
                //if ( !IsRectIntersect(line) ) return false;
                double a1 = line.a;
                double b1 = line.b;
                double c1 = line.c;
                double b0 = a1 * b - a * b1;
                if (b0 == 0) return false;
                double t = (b1 * (x0 - line.x0) + (y0 - line.y0)) / b0;
                p.x = (float)(x0 + a * t);
                p.y = (float)(y0 + b * t);
                p.z = (float)(z0 + c * t);
                return true;
            }
            */
        }
        public class Polygon3D : TriangleObj
        {
            public ShapeEnum shape = ShapeEnum.Polygon;
            public Polygon3D()
            {
                type = ShapeEnum.Polygon;
            }

            public Polygon3D(TriangleObj obj)
            {
                points = obj.points;
                triangles = obj.triangles;
                minx = obj.minx;
                miny = obj.miny;
                minz = obj.minz;
                maxx = obj.maxx;
                maxy = obj.maxy;
                maxz = obj.maxz;
                type = ShapeEnum.Polygon;
            }

            private void CreateCircle(double x0, double y0, double z0, double B, double rad, int slice)
            {
                double angleStep = 2.0 * Math.PI / slice;
                double a, b = B;
                double x, y, z;
                Vector32 p = new Vector32();
                for (int i = 0; i < slice; i++)
                {
                    a = i * angleStep;

                    x = Math.Round(rad * Math.Cos(a), 5);
                    z = Math.Round(rad * Math.Sin(a), 5);
                    y = 0;
                    p = new Vector32((float)x, (float)y, (float)z);
                    p.Rotate((float)b, 2);

                    p.X += (float)x0;
                    p.Y += (float)y0;
                    p.Z += (float)z0;
                    points.Add(p);
                }
            }
            public Polygon3D(C3DLine obj, double rad, int slice = 20)
            {
                type = ShapeEnum.Polygon;
                int np = obj.points.Count;
                if (np < 2) return;
                name = obj.name + "_poly";

                Vector32 p1, p2;
                Vector32 v1, v2, vp1, vp2, v0 = new Vector32(0, 1, 0);
                double a;
                for (int i = 0; i < np; i++)
                {
                    if (i == 0)
                    {
                        p1 = obj.points[i];
                        p2 = obj.points[i + 1];
                        v1 = new Vector32(p1.x, p1.y, p1.z);
                        v2 = new Vector32(p2.x, p2.y, p2.z);
                        a = Vector32.VectorAngle(v0, v2 - v1);
                    }
                    else if (i == np - 1)
                    {
                        p1 = obj.points[i - 1];
                        p2 = obj.points[i];
                        v1 = new Vector32(p1.x, p1.y, p1.z);
                        v2 = new Vector32(p2.x, p2.y, p2.z);
                        a = Vector32.VectorAngle(v0, v2 - v1);
                    }
                    else
                    {
                        p1 = obj.points[i - 1];
                        p2 = obj.points[i];
                        v1 = new Vector32(p1.x, p1.y, p1.z);
                        v2 = new Vector32(p2.x, p2.y, p2.z);
                        vp1 = v2 - v1;

                        p1 = obj.points[i];
                        p2 = obj.points[i + 1];
                        v1 = new Vector32(p1.x, p1.y, p1.z);
                        v2 = new Vector32(p2.x, p2.y, p2.z);
                        vp2 = v2 - v1;

                        //mid vector,angle 0-90
                        a = Vector32.VectorAngle(v0, (vp1 + vp2) / 2);
                    }

                    p1 = obj.points[i];

                    a = Vector32.toRad(a);

                    CreateCircle(p1.x, p1.y, p1.z, a, rad, slice);
                }
                //create triangle indices
                int i1, i2, i3, i4;
                for (int i = 0; i < np - 1; i++)
                {
                    for (int j = 0; j < slice; j++)
                    {
                        i1 = i * slice + j;
                        i2 = (i + 1) * slice + j;

                        i3 = i1 + 1;
                        i4 = i2 + 1;
                        if (j == slice - 1)
                        {
                            i3 = i * slice;
                            i4 = (i + 1) * slice;
                        }
                        triangles.Add(new Int32XYZ(i1, i2, i3));
                        triangles.Add(new Int32XYZ(i2, i4, i3));
                    }
                }

                //create head and tail triangle indices            
                p1 = obj.points[0];
                v1 = new Vector32(p1.x, p1.y, p1.z);
                int id = points.Count;
                points.Add(v1);
                for (int j = 0; j < slice; j++)
                {
                    i1 = id;
                    i2 = j;
                    i3 = j + 1;
                    if (j == slice - 1) i3 = 0;
                    triangles.Add(new Int32XYZ(i1, i2, i3));
                }
                p1 = obj.points[np - 1];
                v1 = new Vector32(p1.x, p1.y, p1.z);
                id = points.Count;
                points.Add(v1);
                int start = (np - 1) * slice;
                for (int j = 0; j < slice; j++)
                {
                    i1 = id;
                    i2 = start + j;
                    i3 = start + j + 1;
                    if (j == slice - 1) i3 = start;
                    triangles.Add(new Int32XYZ(i1, i3, i2));
                }
                UpdateRange();
            }

#if DEBUG
            private static int errno = 0;
#endif
            //p1,p2 edge points, b1 b2 is point status if point is blanked
            public bool GetIntersectionOnAxis(Vector32 p1, Vector32 p2, bool b1, bool b2, out Vector32 sect, int axis)
            {
                // p1--->p2
                Vector32 p;

                if (b1) sect = new Vector32(p2.X, p2.Y, p2.Z);
                else sect = new Vector32(p1.X, p1.Y, p1.Z);

                int nsec = 0;
                CTriangle3f tri = new CTriangle3f();
                for (int i = 0; i < triangles.Count; i++)
                {
                    tri.p1 = points[triangles[i].x];
                    tri.p2 = points[triangles[i].y];
                    tri.p3 = points[triangles[i].z];
                    if (tri.GetIntersectionOnTriangle(p1, p2, out p))
                    {
                        if (b1)
                        {
                            if (axis == 0)
                            {
                                if (p.X < sect.X) sect = p;
                            }
                            else if (axis == 1)
                            {
                                if (p.Y < sect.Y) sect = p;
                            }
                            else if (axis == 2)
                            {
                                if (p.Z < sect.Z) sect = p;
                            }
                        }
                        else if (b2)
                        {
                            if (axis == 0)
                            {
                                if (p.X > sect.X) sect = p;
                            }
                            else if (axis == 1)
                            {
                                if (p.Y > sect.Y) sect = p;
                            }
                            else if (axis == 2)
                            {
                                if (p.Z > sect.Z) sect = p;
                            }
                        }
                        nsec++;
                    }
                }

                if (nsec < 1)
                {
#if DEBUG
                    errno++;
#endif
                    return false;
                }
                else return true;
            }

            //direct =0 x,1 y 2 z
            private IntersectionType CheckCrossTriangle(double x0, double y0, double z0,
                                                        double h1, double h2,
                                                        int tri, int direct)
            {
                Vector32 p1 = points[triangles[tri].x];
                Vector32 p2 = points[triangles[tri].y];
                Vector32 p3 = points[triangles[tri].z];

                //get triangle range
                double _minx, _maxx, _miny, _maxy, _minz, _maxz;
                _minx = _maxx = p1.X;
                _miny = _maxy = p1.Y;
                _minz = _maxz = p1.Z;
                if (_minx > p2.X) _minx = p2.X;
                if (_miny > p2.Y) _miny = p2.Y;
                if (_minz > p2.Z) _minz = p2.Z;
                if (_maxx < p2.X) _maxx = p2.X;
                if (_maxy < p2.Y) _maxy = p2.Y;
                if (_maxz < p2.Z) _maxz = p2.Z;
                if (_minx > p3.X) _minx = p3.X;
                if (_miny > p3.Y) _miny = p3.Y;
                if (_minz > p3.Z) _minz = p3.Z;
                if (_maxx < p3.X) _maxx = p3.X;
                if (_maxy < p3.Y) _maxy = p3.Y;
                if (_maxz < p3.Z) _maxz = p3.Z;

                //triangle out of the range of line h1---h2 
                switch (direct)
                {
                    case 0://x direction
                        if (h2 < _minx || h1 > _maxx) return IntersectionType.none;
                        if (y0 < _miny || y0 > _maxy) return IntersectionType.none;
                        if (z0 < _minz || z0 > _maxz) return IntersectionType.none;
                        //triangle project to yz plane
                        p1.X = p2.X = p3.X = 0;
                        if (p1.Y == p2.Y || p1.Y == p3.Y || p2.Y == p3.Y) return IntersectionType.line;
                        if (p1.Z == p2.Z || p1.Z == p3.Z || p2.Z == p3.Z) return IntersectionType.line;
                        return Vector32.IsPointInTriangle(p1, p2, p3, new Vector32(0, (float)y0, (float)z0));
                    case 1://y direction
                        if (h2 < _miny || h1 > _maxy) return IntersectionType.none;
                        if (x0 < _minx || x0 > _maxx) return IntersectionType.none;
                        if (z0 < _minz || z0 > _maxz) return IntersectionType.none;
                        //triangle project to xz plane
                        p1.Y = p2.Y = p3.Y = 0;
                        if (p1.X == p2.X || p1.X == p3.X || p2.X == p3.X) return IntersectionType.line;
                        if (p1.Z == p2.Z || p1.Z == p3.Z || p2.Z == p3.Z) return IntersectionType.line;
                        //if (!CTriangle3f.ValidTriangle(p1, p2, p3)) return IntersectionType.triangle;
                        return Vector32.IsPointInTriangle(p1, p2, p3, new Vector32((float)x0, 0, (float)z0));
                    case 2://z direction
                        if (h2 < _minz || h1 > _maxz) return IntersectionType.none;
                        if (x0 < _minx || x0 > _maxx) return IntersectionType.none;
                        if (y0 < _miny || y0 > _maxy) return IntersectionType.none;
                        //triangle project to xy plane
                        p1.Z = p2.Z = p3.Z = 0;
                        if (p1.X == p2.X || p1.X == p3.X || p2.X == p3.X) return IntersectionType.line;
                        if (p1.Y == p2.Y || p1.Y == p3.Y || p2.Y == p3.Y) return IntersectionType.line;
                        //if (!CTriangle3f.ValidTriangle(p1, p2, p3)) return IntersectionType.triangle;
                        return Vector32.IsPointInTriangle(p1, p2, p3, new Vector32((float)x0, (float)y0, 0));
                }
                return IntersectionType.none;
            }

            // Copyright 2001 softSurfer, 2012 Dan Sunday
            // This code may be freely used and modified for any purpose
            // providing that this copyright notice is included with it.
            // SoftSurfer makes no warranty for this code, and cannot be held
            // liable for any real or imagined damage resulting from its use.
            // Users of this code must verify correctness for their application.


            // Assume that classes are already given for the objects:
            //    Point and Vector with
            //        coordinates {float x, y, z;}
            //        operators for:
            //            == to test  equality
            //            != to test  inequality
            //            (Vector)0 =  (0,0,0)         (null vector)
            //            Point   = Point ± Vector
            //            Vector =  Point - Point
            //            Vector =  Scalar * Vector    (scalar product)
            //            Vector =  Vector * Vector    (cross product)
            //    Line and Ray and Segment with defining  points {Point P0, P1;}
            //        (a Line is infinite, Rays and  Segments start at P0)
            //        (a Ray extends beyond P1, but a  Segment ends at P1)
            //    Plane with a point and a normal {Point V0; Vector  n;}
            //    Triangle with defining vertices {Point V0, V1, V2;}
            //    Polyline and Polygon with n vertices {int n;  Point *V;}
            //        (a Polygon has V[n]=V[0])
            //===================================================================


            //#define SMALL_NUM   0.00000001 // anything that avoids division overflow
            // dot product (3D) which allows vector operations in arguments
            //#define dot(u,v)   ((u).x * (v).x + (u).y * (v).y + (u).z * (v).z)



            // intersect3D_RayTriangle(): find the 3D intersection of a ray with a triangle
            //    Input:  a ray R, and a triangle T
            //    Output: *I = intersection point (when it exists)
            //    Return: -1 = triangle is degenerate (a segment or point)
            //             0 =  disjoint (no intersect)
            //             1 =  intersect in unique point I1
            //             2 =  are in the same plane
            public int intersect3D_RayTriangle(Vector32 p1, Vector32 p2, Vector32 v0, Vector32 v1, Vector32 v2, out Vector32 I)
            {
                Vector32 u, v, n;              // triangle vectors
                Vector32 dir, w0, w;           // ray vectors
                double r, a, b;              // params to calc ray-plane intersect
                double SMALL_NUM = 0.00000001f;

                I = new Vector32(0, 0, 0);
                // get triangle edge vectors and plane normal
                u = v1 - v0;
                v = v2 - v0;
                n = Vector32.Cross(u, v);              // cross product
                if (n.Length == 0) return -1;
                //if (n == (Vector32)0)             // triangle is degenerate
                //    return -1;                  // do not deal with this case

                dir = p2 - p1;              // ray direction vector
                w0 = p1 - v0;
                a = -Vector32.Dot(n, w0);
                b = Vector32.Dot(n, dir);
                if (Math.Abs(b) < SMALL_NUM)
                {     // ray is  parallel to triangle plane
                    if (a == 0)                 // ray lies in triangle plane
                        return 2;
                    else return 0;              // ray disjoint from plane
                }

                // get intersect point of ray with triangle plane
                r = a / b;
                if (r < 0.0)                    // ray goes away from triangle
                    return 0;                   // => no intersect
                                                // for a segment, also test if (r > 1.0) => no intersect

                I = p1 + r * dir;            // intersect point of ray and plane

                // is I inside T?
                double uu, uv, vv, wu, wv, D;
                uu = Vector32.Dot(u, u);
                uv = Vector32.Dot(u, v);
                vv = Vector32.Dot(v, v);
                w = I - v0;
                wu = Vector32.Dot(w, u);
                wv = Vector32.Dot(w, v);
                D = uv * uv - uu * vv;

                // get and test parametric coords
                double s, t;
                s = (uv * wv - vv * wu) / D;
                if (s < 0.0 || s > 1.0)         // I is outside T
                    return 0;
                t = (uv * wu - uu * wv) / D;
                if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                    return 0;

                return 1;                       // I is in T
            }

            public bool IsPointInPolygon3(Vector32 point)
            {
                double x = point.X;
                double y = point.Y;
                double z = point.Z;
                //point may on the edge of the polygon
                if (x < minx || x > maxx) return false;
                if (y < miny || y > maxy) return false;
                if (z < minz || z > maxz) return false;
                //collect all the triangles on the direction of p1p2;
                Vector32 v0, v1, v2;
                Vector32 p1 = point;
                Vector32 p2 = new Vector32(p1.X, p1.Y, p1.Z);
                Vector32 sect;
                int ret;
                p2.Y = (float)maxx + 100;
                int nsec = 0;
                for (int i = 0; i < triangles.Count; i++)
                {
                    // return 0-no intersection,1-intersected,
                    // 2 -intersected on line or on cornerpoint
                    v0 = points[triangles[i].x];
                    v1 = points[triangles[i].y];
                    v2 = points[triangles[i].z];
                    ret = intersect3D_RayTriangle(p1, p2, v0, v1, v2, out sect);
                    if (ret == 1) nsec++;
                }
                if (nsec > 0)
                {
                    int d = nsec;
                    d = d >> 1;
                    d = d << 1;
                    //odd
                    if (d != nsec) return true;
                }
                return false;
            }
            public virtual bool IsPointInPolygon(Vector32 point)
            {
                float x = point.X;
                float y = point.Y;
                float z = point.Z;

                //point may on the edge of the polygon
                if (x < minx || x > maxx) return false;
                if (y < miny || y > maxy) return false;
                if (z < minz || z > maxz) return false;

                //check point in which part of the polygon
                double x0 = (minx + maxx) / 2;
                double y0 = (miny + maxy) / 2;
                double z0 = (minz + maxz) / 2;

                //ret ==0, outside, ret == 1 inside, ret < 0 faild
                int ret = 0;

                if (x < x0)
                    ret = CheckPointInPolygon(new Vector32((float)minx - 1, y, z), new Vector32(x, y, z), 0);
                else ret = CheckPointInPolygon(new Vector32(x, y, z), new Vector32((float)maxx + 1, y, z), 0);

                //try y direction
                if (ret < 0)
                {
                    if (y < y0) ret = CheckPointInPolygon(new Vector32(x, (float)miny - 1, z), new Vector32(x, y, z), 1);
                    else ret = CheckPointInPolygon(new Vector32(x, y, z), new Vector32(x, (float)maxy + 1, z), 1);
                }
                //try z direction
                if (ret < 0)
                {
                    if (z < z0) ret = CheckPointInPolygon(new Vector32(x, y, (float)minz - 1), new Vector32(x, y, z), 2);
                    else ret = CheckPointInPolygon(point, new Vector32(x, y, (float)maxz + 1), 2);
                }

                /////try another direction------------------------- 
                if (ret == 0) return false;
                else if (ret == 1) return true;
                else
                {
                    throw new Exception("Ambigous of point status.");
                    return true;
                }

            }
            /// <summary>
            /// check point x,y,z is inside polygon
            /// point on left part of polygon
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <returns>
            /// ret == 0, outside
            /// ret == 1 inside
            /// ret < 0 faild
            /// </returns>        
            int CheckPointInPolygon(Vector32 p1, Vector32 p2, int direct)
            {
                //draw a line cross through the polygon            
                int nsec = 0;
                IntersectionType ret = 0;
                CTriangle3f tri = new CTriangle3f();
                //collect all the triangles on the direction of p1p2;
                for (int i = 0; i < triangles.Count; i++)
                {
                    // return 0-no intersection,1-intersected,
                    // 2 -intersected on line or on cornerpoint

                    tri.p1 = points[triangles[i].x];
                    tri.p2 = points[triangles[i].y];
                    tri.p3 = points[triangles[i].z];

                    ret = tri.CheckLineCrossTriangle(p1, p2, direct);
                    if (ret == IntersectionType.none) continue;
                    else if (ret == IntersectionType.triangle) nsec++;
                    else //intersection on lines or on coner,can't determine the result
                    {
                        return -1;
                    }
                }
                // intersection points num is odd, 
                // even is outside polygon
                if (nsec > 0 && (nsec % 2) != 0) return 1;
                else return 0;
            }
        }
        public enum SphereSurface
        {
            UpLeftBack = 1,
            UpLeftFront = 2,
            UpRightBack = 4,
            UpRightFront = 8,
            DownLeftBack = 16,
            DownLeftFront = 32,
            DownRightBack = 64,
            DownRightFront = 128
        };
        public class C3DLine : C3DObjectBase
        {
            public List<Vector64> points = new List<Vector64>();
            public ColorRGBA color = new ColorRGBA(128, 128, 128);
            public vec4[] colors = null;

            public bool Closed { get; set; } = false; // is closed obj-polygon
            public double Value { get; set; } = 0;    // property value;
            public Color FillColor { get; set; } = Color.Black; // polygon filled color

            private double[] distances = null;

            [CategoryAttribute("Display"), DisplayNameAttribute("Color")]
            public Color Color
            {
                get { return Color.FromArgb(color.A, color.R, color.G, color.B); }
                set
                {
                    Color c = value;
                    color = new ColorRGBA(c.R, c.G, c.B);
                }
            }
            [CategoryAttribute("Display"), DisplayNameAttribute("lineWidth")]
            public double lineWidth { get; set; } = 1.0f;
            /*
            [CategoryAttribute("DOT"), DisplayNameAttribute("Show Dot")]
            public bool IsShowDot { get; set; } = false;

            [CategoryAttribute("DOT"), DisplayNameAttribute("Dot Size")]
            public float dotSize { get; set; } = 5;
            [CategoryAttribute("DOT"), DisplayNameAttribute("Dot Color")]
            public Color dotColor { get; set; } = Color.Red;
            */

            public C3DLine()
            {
                color = new ColorRGBA(128, 128, 128);
                type = ShapeEnum.Line;
            }
            public C3DLine(List<Vector32> _points)
            {
                color = new ColorRGBA(128, 128, 128);
                type = ShapeEnum.Line;
                for (int i = 0; i < _points.Count; i++)
                {
                    points.Add(_points[i].toVector64());
                }
            }
            public C3DLine(List<Vector64> _points)
            {
                color = new ColorRGBA(128, 128, 128);
                type = ShapeEnum.Line;
                for (int i = 0; i < _points.Count; i++)
                {
                    points.Add(_points[i]);
                }
            }
            public int Count { get { return points.Count; } }

            public void AddPoint(List<Vector64> points_array)
            {
                for (int i = 0; i < points_array.Count; i++)
                    AddPoint(points_array[i]);
            }
            public void AddPoint(List<Vector32> points_array)
            {
                for (int i = 0; i < points_array.Count; i++)
                    AddPoint(points_array[i]);
            }
            public void AddPoint(Vector64 p)
            {
                AddPoint(p.x, p.y, p.z, p.v);
            }
            public void Offset(double offx, double offy, double offz)
            {
                Vector64 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    p.x += offx;
                    p.y += offy;
                    p.z += offz;
                    points[i] = p;
                }
                minx += offx;
                miny += offy;
                minz += offz;
                maxx += offx;
                maxy += offy;
                maxz += offz;
            }
            /// <summary>
            /// 简化点，将直线上冗余点去掉
            /// </summary>
            public void Simplify()
            {
                //去重点
                Vector64.RemoveLineDuplicated(ref points);
                Stack<Vector64> lists = new Stack<Vector64>();

                for (int i = Count - 1; i >= 0; i--)
                    lists.Push(points[i]);

                points.Clear();

                Vector64 p1 = lists.Pop();
                Vector64 p2 = lists.Pop();
                Vector64 p;

                points.Add(p1);
                while (lists.Count > 0)
                {
                    p = lists.Pop();
                    if (Vector64.IsPointOnLine(p, p1, p2)) p2 = p;
                    else
                    {
                        points.Add(p2);
                        p1 = p2;
                        p2 = p;
                    }
                }
                //last one
                points.Add(p2);
                UpdateRange();
            }
            public void AddPoint(double x, double y, double z, double v = 0)
            {
                if (Count == 0)
                {
                    minx = maxx = x;
                    miny = maxy = y;
                    minz = maxz = z;
                    minv = maxv = v;
                }
                else
                {
                    if (x < minx) minx = x;
                    if (y < miny) miny = y;
                    if (z < minz) minz = z;
                    if (v < minv) minv = v;
                    if (x > maxx) maxx = x;
                    if (y > maxy) maxy = y;
                    if (z > maxz) maxz = z;
                    if (v > maxv) maxv = v;
                }
                points.Add(new Vector64(x, y, z));
            }
            public void AddPoint(Vector32 p)
            {
                AddPoint(p.x, p.y, p.z, p.v);
            }

            public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
            {
                UpdateRange();
                Vector64 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    if (maxx > minx)
                        p.x = x1 + (x2 - x1) * (p.x - minx) / (maxx - minx);
                    else p.x = (x1 + x2) / 2;
                    if (maxy > miny)
                        p.y = y1 + (y2 - y1) * (p.y - miny) / (maxy - miny);
                    else p.y = (y1 + y2) / 2;
                    if (maxz > minz)
                        p.z = z1 + (z2 - z1) * (p.z - minz) / (maxz - minz);
                    else p.z = (z1 + z2) / 2;
                    points[i] = p;
                }
                minx = x1;
                miny = y1;
                minz = z1;
                maxx = x2;
                maxy = y2;
                maxz = z2;
            }
            public override void Normalize()
            {
                if (points.Count < 2) return;

                UpdateRange();

                Vector64 p0 = GetCenter64();
                Vector64 p = new Vector64(0, 0, 0);

                double x, y, z;
                for (int i = 0; i < points.Count; i++)
                {
                    p.x = scale.x * (points[i].x - p0.x);
                    p.y = scale.y * (points[i].y - p0.y);
                    p.z = scale.z * (points[i].z - p0.z);

                    //GL is clockwise,otherwise Rotate is counter clockwise
                    //if (rotate.x != 0) p.RotateOnAngle(-rotate.x, 0,0);
                    //if (rotate.y != 0) p.RotateOnAngle(0,rotate.y, 0);
                    //if (rotate.z != 0) p.RotateOnAngle(0,0,-rotate.z);

                    if (rotate.x != 0 || rotate.y != 0 || rotate.z != 0)
                        p.RotateOnAngle(-rotate.x, rotate.y, -rotate.z);

                    x = (p.x + p0.x + offset.x);
                    y = (p.y + p0.y + offset.y);
                    z = (p.z + p0.z + offset.z);

                    points[i] = new Vector64(x, y, z);
                }

                scale = new vec3(1, 1, 1);
                offset = new vec3(0, 0, 0);
                rotate = new vec3(0, 0, 0);

                UpdateRange();
            }
            public override void UpdateRange()
            {
                minx = maxx = 0;
                miny = maxy = 0;
                minz = maxz = 0;
                if (points.Count < 1) return;
                minx = maxx = points[0].x;
                miny = maxy = points[0].y;
                minz = maxz = points[0].z;
                for (int i = 1; i < points.Count; i++)
                {
                    if (points[i].x < minx) minx = points[i].x;
                    if (points[i].x > maxx) maxx = points[i].x;
                    if (points[i].y < miny) miny = points[i].y;
                    if (points[i].y > maxy) maxy = points[i].y;
                    if (points[i].z < minz) minz = points[i].z;
                    if (points[i].z > maxz) maxz = points[i].z;
                }
            }


            public override bool ExportData(string filename)
            {
                FileStream fs = new FileStream(filename, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);
                string str;
                string header = "3D Line";
                wr.WriteLine(header);
                str = "Name = " + name;
                wr.WriteLine(str);
                str = "[BASE LINE]";
                wr.WriteLine(str);
                str = "Count = " + points.Count;
                wr.WriteLine(str);

                for (int i = 0; i < points.Count; i++)
                {
                    str = points[i].x + "," + points[i].y + "," + points[i].z;
                    wr.WriteLine(str);
                }

                wr.Close();
                fs.Close();

                if (points.Count > 1)
                    return true;
                else return false;
            }
            /*
            public override bool ImportData(string filename)
            {
                Clear();

                AsciiReader asc = new AsciiReader(filename);
                asc.SeekHeader("Grid Slicer Line");
                string[] ss;

                if ( !asc.SeekSection("[HEADER]") )
                {
                    asc.Close();
                    return false;
                }

                name = asc.GetItemValue(asc.GetLine(), "name");

                if (!asc.SeekSection("[BASE LINE]"))
                {
                    asc.Close();
                    return false;
                }

                int count = Convert.ToInt32(asc.GetItemValue(asc.GetLine(), "Count"));

                List<double>values = asc.GetBlockValues(3*count);

                for(int i=0;i<count; i++)
                {
                    AddPoint(values[3 * i], values[3 * i+1], values[3 * i+2]);
                }

                asc.Close();
                UpdateRange();

                if (points.Count > 1) return true;
                else return false;            
            }
            */

            public override bool ImportData(string filename)
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string ss;
                int i = 0;
                double x, y, z;
                while ((ss = sr.ReadLine()) != null)
                {
                    if (ss.Length < 3) continue;
                    if (i > 0)
                    {
                        string[] str = ss.Split(new Char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        x = double.Parse(str[0]);    //x
                        y = double.Parse(str[1]);    //y
                        z = double.Parse(str[2]);    //z                
                        AddPoint(x, y, z);
                    }
                    i++;
                }
                sr.Close();
                fs.Close();

                UpdateRange();

                if (points.Count > 1) return true;
                else return false;
            }

            public override void Clear()
            {
                base.Clear();
                points.Clear();
            }
            public C3DLine Copy()
            {
                C3DLine line = new C3DLine(points);
                CopyHeader(line);
                line.name = name;
                line.color = color;
                line.lineWidth = lineWidth;
                line.distances = distances;
                line.colors = colors;
                line.Closed = Closed;
                line.Value = Value;
                line.FillColor = FillColor;
                return line;
            }

            //sect < = p1p2
            private Vector64 GetInterpolate(double sect, Vector64 p1, Vector64 p2)
            {
                if (sect == 0) return p1;

                double len = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                        (p1.y - p2.y) * (p1.y - p2.y) +
                                        (p1.z - p2.z) * (p1.z - p2.z));

                if (sect >= len) return p2;
                else return p1 + (p2 - p1) * sect / len;
            }

            //曲线重新分段，divNum 分段数目,节点数目divNum+1
            public void LineDivid(int divNum = 100)
            {
                C3DLine line = new C3DLine();
                line.name = name + "_smoothed";

                double len = GetLength();
                double step = len / divNum;

                //分段插值点数值
                List<Vector64> temp_points = new List<Vector64>();
                temp_points.Add(points[0]); //first

                int n1, n2;
                Vector64 p, p1, p2;
                double l1, l2, sum = 0, sum1 = 0, sum2 = 0, len1, len2;

                double[] lens = new double[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    lens[i] = GetLength(i);
                }

                p = points[0];
                int start = 1;
                for (int i = 0; i < divNum; i++)
                {
                    sum += step;

                    //search 
                    for (int j = start; j < points.Count; j++)
                    {
                        if (sum == lens[j])
                        {
                            p = points[j];
                            temp_points.Add(p);
                            start = j;
                            break;
                        }
                        else if (sum < lens[j])
                        {
                            p1 = points[j - 1];
                            p2 = points[j];
                            p = GetInterpolate(sum - lens[j - 1], p1, p2);
                            temp_points.Add(p);
                            start = j;
                            break;
                        }
                        else start = j;
                    }//for(int j = start; j<points.Count;j++)

                }//for (int i = 0; i < divNum;i++)

                points.Clear();

                for (int i = 0; i < temp_points.Count; i++)
                    points.Add(temp_points[i]);

                lens = null;
                temp_points.Clear();
            }//public void LineDivid(int divNum = 100)

            //debug-jian 2019-5-5 此处是2D的平滑，需要修改为3D版本
            //public C3DLine Smooth() { return new C3DLine(); }
            // 1 2 3
            //   2 3 4
            //     3 4 5
            // 分段光滑似乎不可行，只能将曲线投影到一个平面上
            /*
            public C3DLine Smooth()
            {
                int n = points.Count;
                if (n < 3) return this;

                C3DLine line = new C3DLine();
                line.name = name + "_smoothed";

                int n1, n2;
                Vector64 p,p1,p2;
                Vector64[] pt = new Vector64[points.Count];

                double xlen = maxx - minx;
                double ylen = maxy - miny;
                double zlen = maxz - minz;
                double len, len1;
                if ( xlen <= ylen && xlen <= zlen) //project to YOZ
                {
                    for(int i=0;i<points.Count;i++)
                    {
                        pt[i] = new Vector64(points[i].y, points[i].z,0);
                    }

                    Spline sp = new Spline(pt);
                    List<Vector64> pps = sp.CreateSpline();
                    for (int i = 0; i < pps.Count; i++)
                    {
                        p = new Vector64( 0, pps[i].x, pps[i].y );
                        p.x = points[0].x;
                        n1 = n2 = 0;
                        for (int j = 0; j < sp.gridIndics.Length; j++)
                        {
                            if (i == sp.gridIndics[j])
                            {
                                n1 = n2 = j;
                                break;
                            }
                            else if (i < sp.gridIndics[j])
                            {
                                n2 = j;
                                break;
                            }
                            else n1 = j;//if (i > sp.gridIndics[j])                        
                        }

                        if (n1 == n2) p.x = points[n1].x;
                        else
                        {
                            p1 = points[n1];
                            p2 = points[n2];
                            len = Math.Sqrt((p.y - p1.y) * (p.y - p1.y) + (p.z - p1.z) * (p.z - p1.z));
                            len1 = Math.Sqrt((p2.y - p1.y) * (p2.y - p1.y) + (p2.z - p1.z) * (p2.z - p1.z));
                            if (len1 > 0) p.x = p1.x + (p2.x - p1.x) * len / len1;
                            else p.x = p1.x;
                        }
                        line.points.Add(p);
                    }// for (int i = 0; i < pps.Count; i++)

                    pps.Clear();
                    sp.Clear();

                }
                else if (ylen <= xlen && ylen <= zlen) //project to XOZ
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        pt[i] = new Vector64(points[i].x, points[i].z, 0);
                    }
                    Spline sp = new Spline(pt);
                    List<Vector64> pps = sp.CreateSpline();
                    for (int i = 0; i < pps.Count; i++)
                    {
                        p = new Vector64(pps[i].x, 0, pps[i].y);
                        p.y = points[0].y;
                        n1 = n2 = 0;
                        for (int j = 0; j < sp.gridIndics.Length; j++)
                        {
                            if (i == sp.gridIndics[j])
                            {
                                n1 = n2 = j;
                                break;
                            }
                            else if (i < sp.gridIndics[j])
                            {
                                n2 = j;
                                break;
                            }
                            else n1 = j;//if (i > sp.gridIndics[j])                        
                        }

                        if (n1 == n2) p.y = points[n1].y;
                        else
                        {
                            p1 = points[n1];
                            p2 = points[n2];
                            len = Math.Sqrt((p.x - p1.x) * (p.x - p1.x) + (p.z - p1.z) * (p.z - p1.z));
                            len1 = Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.z - p1.z) * (p2.z - p1.z));
                            if (len1 > 0) p.y = p1.y + (p2.y - p1.y) * len / len1;
                            else p.y = p1.y;
                        }
                        line.points.Add(p);
                    }// for (int i = 0; i < pps.Count; i++)

                    pps.Clear();
                    sp.Clear();
                }
                else    //project to XOY
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        pt[i] = new Vector64(points[i].x, points[i].y, 0);
                    }
                    Spline sp = new Spline(pt);
                    List<Vector64> pps = sp.CreateSpline();               

                    for (int i = 0; i < pps.Count; i++)
                    {
                        p = pps[i];
                        p.z = points[0].z;
                        n1 = n2 = 0;
                        for (int j = 0; j < sp.gridIndics.Length; j++)
                        {
                            if (i == sp.gridIndics[j])
                            {
                                n1 = n2 = j;
                                break;
                            }
                            else if (i < sp.gridIndics[j])
                            {
                                n2 = j;
                                break;
                            }
                            else n1 = j;//if (i > sp.gridIndics[j])                        
                        }

                        if( n1 == n2 ) p.z = points[n1].z;                    
                        else 
                        {
                            p1 = points[n1];
                            p2 = points[n2];
                            len =  Math.Sqrt( (p.x - p1.x) * (p.x - p1.x) + (p.y - p1.y) * (p.y - p1.y) );
                            len1 = Math.Sqrt( (p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y) );
                            if (len1 > 0) p.z = p1.z + (p2.z - p1.z) * len / len1;
                            else p.z = p1.z;
                        }                    
                        line.points.Add(p);
                    }// for (int i = 0; i < pps.Count; i++)

                    pps.Clear();
                    sp.Clear();

                }//else  //project to XOY

                pt = null;
                line.UpdateRange();
                return line;
            }//end of smooth()
            */

            /// <summary>
            /// GetLength Functions
            /// </summary>
            public void CreateDistances()
            {
                if (distances != null) return;
                if (points.Count < 2) return;

                distances = new double[points.Count];

                double len = 0;
                Vector32 p, p0 = points[0];

                distances[0] = 0;
                for (int i = 1; i < points.Count; i++)
                {
                    p = points[i];
                    len += Math.Sqrt((p.x - p0.x) * (p.x - p0.x) +
                                      (p.y - p0.y) * (p.y - p0.y) +
                                      (p.z - p0.z) * (p.z - p0.z));
                    p0 = p;
                    distances[i] = len;
                }
            }

            public double GetLength(int index = -1)
            {
                if (points.Count < 1) return 0;

                int id = index;
                if (id < 0) id = Count - 1;

                CreateDistances();

                return distances[id];
            }

            public double Length
            {
                get { return GetLength(); }
            }

            public double GetLength(List<Vector64> pp, int index = -1)
            {
                if (index == 0) return 0;
                int end = pp.Count - 1;
                if (index >= 0) end = index;

                Vector32 p1, p2;
                double sum = 0, len;
                for (int i = 1; i <= end; i++)
                {
                    p1 = pp[i - 1];
                    p2 = pp[i];
                    len = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                (p1.y - p2.y) * (p1.y - p2.y) +
                                (p1.z - p2.z) * (p1.z - p2.z));
                    sum += len;
                }
                return sum;
            }

            /*
             public C3DLine Smooth()
             {
                 int n = points.Count;
                 if (n < 3) return this;

                 C3DLine line = Copy();
                 line.name = name + "_smoothed";
                 line.Clear();

                 double x1, x2, y1, y2, z1, z2,xl,yl,zl;
                 double px1, px2, py1, py2;
                 Vector32 p1, p2, p3;            
                 Vector32[] pt = new Vector32[3];

                 for (int k=0;k<n-2;k++)
                 {
                     p1 = points[k];
                     p2 = points[k+1];
                     p3 = points[k+2];

                     x1 = x2 = p1.x;
                     y1 = y2 = p1.y;
                     z1 = z2 = p1.z;
                     if (p2.x < x1) x1 = p2.x;
                     if (p2.x > x2) x2 = p2.x;
                     if (p2.y < y1) y1 = p2.y;
                     if (p2.y > y2) y2 = p2.y;
                     if (p2.z < z1) z1 = p2.z;
                     if (p2.z > z2) z2 = p2.z;
                     if (p3.x < x1) x1 = p3.x;
                     if (p3.x > x2) x2 = p3.x;
                     if (p3.y < y1) y1 = p3.y;
                     if (p3.y > y2) y2 = p3.y;
                     if (p3.z < z1) z1 = p3.z;
                     if (p3.z > z2) z2 = p3.z;
                     xl = x2 - x1;
                     yl = y2 - y1;
                     zl = z2 - z1;

                     if (x1 <= yl && xl <= z1) //project to YOZ
                     {
                         pt[0] = new Vector32(p1.y, p1.z, 0);
                         pt[1] = new Vector32(p2.y, p2.z, 0);
                         pt[2] = new Vector32(p3.y, p3.z, 0);
                         Spline sp = new Spline(pt);
                         List<Vector32> pps = sp.CreateSpline();

                         double yy1 = Math.Abs(p2.y - p1.y);
                         double yy2 = Math.Abs(p3.y - p2.y);
                         double zz1 = Math.Abs(p2.z - p1.z);
                         double zz2 = Math.Abs(p3.z - p2.z);

                         px1 = p1.y; px2 = p2.y;
                         if (px2 < px1) { px1 = p2.y; px2 = p1.y; }
                         py1 = p1.z; py2 = p2.z;
                         if (py2 < py1) { py1 = p2.z; py2 = p1.z; }

                         double dx = 0;
                         for (int i = 0; i < pps.Count; i++)
                         {
                             //p1--p2
                             if ((pps[i].x >= px1 && pps[i].x < px2) &&
                                 (pps[i].y >= py1 && pps[i].y < py2))
                             {
                                 if (k == 0)//first fragment
                                 {
                                     if (yy1 >= zz1)//y                             
                                         dx = p1.x + (p2.x - p1.x) * (pps[i].x - p1.y) / (p2.y - p1.y);
                                     else //z                            
                                         dx = p1.x + (p2.x - p1.x) * (pps[i].y - p1.z) / (p2.z - p1.z);
                                     line.points.Add(new Vector32((float)dx, pps[i].x, pps[i].y));
                                 }
                             }
                             else//p2--p3
                             {
                                 if ((i == pps.Count - 1 && k == n - 3) || i < pps.Count - 1)
                                 {
                                     if (yy2 >= zz2)//y                             
                                         dx = p2.x + (p3.x - p2.x) * (pps[i].x - p2.y) / (p3.y - p2.y);
                                     else //z                            
                                         dx = p2.x + (p3.x - p2.x) * (pps[i].y - p2.z) / (p3.z - p2.z);
                                     line.points.Add(new Vector32((float)dx, pps[i].x, pps[i].y));
                                 }
                             }


                         }
                         pps.Clear();
                     }
                     else if (y1 <= xl && yl <= z1) //project to XOZ
                     {
                         pt[0] = new Vector32(p1.x, p1.z, 0);
                         pt[1] = new Vector32(p2.x, p2.z, 0);
                         pt[2] = new Vector32(p3.x, p3.z, 0);
                         Spline sp = new Spline(pt);
                         List<Vector32> pps = sp.CreateSpline();

                         double xx1 = Math.Abs(p2.x - p1.x);
                         double xx2 = Math.Abs(p3.x - p2.x);
                         double zz1 = Math.Abs(p2.z - p1.z);
                         double zz2 = Math.Abs(p3.z - p2.z);

                         px1 = p1.x; px2 = p2.x;
                         if (px2 < px1) { px1 = p2.x; px2 = p1.x; }
                         py1 = p1.z; py2 = p2.z;
                         if (py2 < py1) { py1 = p2.z; py2 = p1.z; }

                         double dy = 0;
                         for (int i = 0; i < pps.Count; i++)
                         {
                             //p1--p2
                             if ((pps[i].x >= px1 && pps[i].x < px2) &&
                                 (pps[i].y >= py1 && pps[i].y < py2))
                             {
                                 if (k == 0)
                                 {
                                     if (xx1 >= zz1)//x                             
                                         dy = p1.y + (p2.y - p1.y) * (pps[i].x - p1.x) / (p2.x - p1.x);
                                     else //z                            
                                         dy = p1.y + (p2.y - p1.y) * (pps[i].z - p1.z) / (p2.z - p1.z);
                                     line.points.Add(new Vector32(pps[i].x, (float)dy, pps[i].y));
                                 }
                             }
                             else//p2--p3
                             {
                                 if ((i == pps.Count - 1 && k == n - 3) || i < pps.Count - 1)
                                 {
                                     if (xx2 >= zz2)//x                             
                                         dy = p2.y + (p3.y - p2.y) * (pps[i].x - p2.x) / (p3.x - p2.x);
                                     else //z                            
                                         dy = p2.y + (p3.y - p2.y) * (pps[i].z - p2.z) / (p3.z - p2.z);
                                     line.points.Add(new Vector32(pps[i].x, (float)dy, pps[i].y));
                                 }
                             }

                         }
                         pps.Clear();
                     }
                     else //if (z1 <= xl && zl <= y1) //project to XOY
                     {
                         pt[0] = new Vector32(p1.x, p1.y, 0);
                         pt[1] = new Vector32(p2.x, p2.y, 0);
                         pt[2] = new Vector32(p3.x, p3.y, 0);
                         Spline sp = new Spline(pt);
                         List<Vector32> pps = sp.CreateSpline();

                         double xx1 = Math.Abs(p2.x - p1.x);
                         double xx2 = Math.Abs(p3.x - p2.x);
                         double yy1 = Math.Abs(p2.y - p1.y);
                         double yy2 = Math.Abs(p3.y - p2.y);

                         px1 = p1.x; px2 = p2.x;
                         if (px2 < px1) { px1 = p2.x; px2 = p1.x; }
                         py1 = p1.y; py2 = p2.y;
                         if (py2 < py1) { py1 = p2.y; py2 = p1.y; }

                         double dz = 0;
                         for (int i = 0; i < pps.Count; i++)
                         {
                             //p1--p2
                             if ((pps[i].x >= px1 && pps[i].x < px2) &&
                                 (pps[i].y >= py1 && pps[i].y < py2))
                             {
                              //   if (k == 0)
                                 {
                                     if (xx1 >= yy1)//x                             
                                         dz = p1.z + (p2.z - p1.z) * (pps[i].x - p1.x) / (p2.x - p1.x);
                                     else //y                            
                                         dz = p1.z + (p2.z - p1.z) * (pps[i].y - p1.y) / (p2.y - p1.y);
                                     line.points.Add(new Vector32(pps[i].x, pps[i].y, (float)dz));
                                 }
                             }
                             else //p2--p3
                             {
                                // if ((i == pps.Count - 1 && k == n - 3) || i < pps.Count - 1)
                                 {
                                     if (xx2 >= yy2)//x                             
                                         dz = p2.z + (p3.z - p2.z) * (pps[i].x - p2.x) / (p3.x - p2.x);
                                     else //y                            
                                         dz = p2.z + (p3.z - p2.z) * (pps[i].y - p2.y) / (p3.y - p2.y);
                                //     line.points.Add(new Vector32(pps[i].x, pps[i].y, (float)dz));
                                 }
                             }   

                         }
                         pps.Clear();
                     }
                 }

                 return line;
             }//end of smooth()
             */
        }//end of class C3DLine

        public class CSphere : TriangleObj
        {
            public double rad = 1.0;

            public Vector32 top = new Vector32(0, 1, 0);
            public Vector32 bottom = new Vector32(0, -1, 0);
            public Vector32 left = new Vector32(-1, 0, 0);
            public Vector32 right = new Vector32(1, 0, 0);
            public Vector32 front = new Vector32(0, 0, 1);
            public Vector32 back = new Vector32(0, 0, -1);
            public CSphere()
            {
                points.Add(top);
                points.Add(bottom);
                points.Add(left);
                points.Add(right);
                points.Add(front);
                points.Add(back);
                type = ShapeEnum.Shphere;
                minSquare = 0.001;
            }
            //generate Texcoords
            public void GenTexcoords()
            {
                int n = points.Count;
                if (n < 1) return;
                float x, y, z, u, v;

                for (int i = 0; i < n; i++)
                {
                    x = points[i].X;
                    y = points[i].Y;
                    z = points[i].Z;
                    v = (float)(Math.Asin(z / rad) / Math.PI + 0.5);
                    u = (float)(Math.Atan(y / x) / 2 / Math.PI);
                }
            }
            public void CreateHalf(double _rad, double _minSquare, bool top = true)
            {
                if (_rad <= 0) return;
                if (minSquare >= 0.1) return;
                rad = _rad;
                minSquare = _minSquare;
                if (top)
                {
                    CreateHalf8(1);
                    CreateHalf8(2);
                    CreateHalf8(4);
                    CreateHalf8(8);
                }
                else
                {
                    CreateHalf8(16);
                    CreateHalf8(32);
                    CreateHalf8(64);
                    CreateHalf8(128);
                }
            }
            public void Create(double _rad, double _minSquare)
            {
                CreateHalf(_rad, _minSquare, true);
                CreateHalf(_rad, _minSquare, false);
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = (float)rad * points[i];
                }
            }
            public void CreateHalf8(int surface = 1)
            {
                int start = points.Count;
                if ((surface & (int)SphereSurface.UpLeftBack) > 0)
                {
                    start = points.Count;
                    //points.Add(top);  0
                    //points.Add(back); 5
                    //points.Add(left); 2
                    triangles.Add(new Int32XYZ(0, 5, 2));
                    DividTriangle(triangles.Count - 1);
                }
                if ((surface & (int)SphereSurface.UpLeftFront) > 0)
                {
                    start = points.Count;
                    //points.Add(top);  0 2 4
                    //points.Add(left);
                    //points.Add(front);                
                    triangles.Add(new Int32XYZ(0, 2, 4));
                    DividTriangle(triangles.Count - 1);
                }
                if ((surface & (int)SphereSurface.UpRightBack) > 0)
                {
                    start = points.Count;
                    //points.Add(top);
                    //points.Add(right);
                    //points.Add(back);
                    triangles.Add(new Int32XYZ(0, 3, 5));
                    DividTriangle(triangles.Count - 1);
                }
                if ((surface & (int)SphereSurface.UpRightFront) > 0)
                {
                    start = points.Count;
                    //points.Add(top);
                    //points.Add(front);
                    //points.Add(right);
                    triangles.Add(new Int32XYZ(0, 4, 3));
                    DividTriangle(triangles.Count - 1);
                }
                /////////////////////////down////////////////////////
                if ((surface & (int)SphereSurface.DownLeftBack) > 0)
                {
                    start = points.Count;
                    //points.Add(bottom);
                    //points.Add(left);
                    //points.Add(back);                
                    triangles.Add(new Int32XYZ(1, 2, 5));
                    DividTriangle(triangles.Count - 1);
                }
                if ((surface & (int)SphereSurface.DownLeftFront) > 0)
                {
                    start = points.Count;
                    //points.Add(bottom);                
                    //points.Add(front);
                    //points.Add(left);
                    triangles.Add(new Int32XYZ(1, 4, 2));
                    DividTriangle(triangles.Count - 1);
                }
                if ((surface & (int)SphereSurface.DownRightBack) > 0)
                {
                    start = points.Count;
                    //points.Add(bottom);
                    //points.Add(back);
                    //points.Add(right);                
                    triangles.Add(new Int32XYZ(1, 5, 3));
                    DividTriangle(triangles.Count - 1);
                }
                if ((surface & (int)SphereSurface.DownRightFront) > 0)
                {
                    start = points.Count;
                    //points.Add(bottom);
                    //points.Add(right);
                    //points.Add(front);                
                    triangles.Add(new Int32XYZ(1, 3, 4));
                    DividTriangle(triangles.Count - 1);
                }
            }
            public override void DividTriangle(int index)
            {
                //      p1
                //    / |  \
                // p2 - p0- p3
                int i1 = triangles[index].x;
                int i2 = triangles[index].y;
                int i3 = triangles[index].z;
                Vector32 p1 = points[i1];
                Vector32 p2 = points[i2];
                Vector32 p3 = points[i3];
                Vector32 p0 = (p2 + p3) / 2;
                double r01 = Vector32.Distance(p0, p1);
                double r23 = Vector32.Distance(p2, p3);

                //triangle meet minimum square requirment
                if (0.5 * r01 * r23 <= minSquare) return;

                Vector32 p12 = GetMiddlePoint(p1, p2);
                Vector32 p23 = GetMiddlePoint(p2, p3);
                Vector32 p13 = GetMiddlePoint(p1, p3);

                int i12 = points.Count;
                int i23 = i12 + 1;
                int i13 = i23 + 1;
                //new points
                points.Add(p12);
                points.Add(p23);
                points.Add(p13);

                Int32XYZ d0 = new Int32XYZ(i12, i23, i13);
                Int32XYZ d1 = new Int32XYZ(i1, i12, i13);
                Int32XYZ d2 = new Int32XYZ(i12, i2, i23);
                Int32XYZ d3 = new Int32XYZ(i13, i23, i3);
                triangles[index] = d0;
                int i0 = triangles.Count;

                triangles.Add(d1);
                triangles.Add(d2);
                triangles.Add(d3);

                DividTriangle(index);
                DividTriangle(i0);
                DividTriangle(i0 + 1);
                DividTriangle(i0 + 2);
            }
            public override Vector32 GetMiddlePoint(Vector32 p1, Vector32 p2)
            {
                Vector32 p = (p1 + p2) / 2;
                p = p.Normalize();
                return p;
            }
        }
        public class CCylinder : TriangleObj
        {
            public double rad = 1.0;
            public double height = 5.0;
            public int vertSlices = 20;
            public int horSlices = 20;
            public CCylinder()
            {
                type = ShapeEnum.Cylinder;
            }
            public override void Destroy()
            {
                base.Destroy();
            }
            public void Create(double _rad, double _height, int _verSlices = 20, int _horSlices = 20)
            {
                if (_verSlices < 2 || _horSlices < 2) return;

                rad = _rad;
                height = _height;

                vertSlices = _verSlices;
                horSlices = _horSlices;

                double ystep = height / (vertSlices - 1);
                double x, y, z, a;

                double angleStep = 2.0 * Math.PI / (horSlices - 1);

                for (int i = 0; i < vertSlices; i++)
                {
                    y = -height / 2.0 + i * ystep;
                    for (int j = 0; j < horSlices; j++)
                    {
                        //if (j == horSlices-1) a = 0;
                        //else 
                        a = j * angleStep;

                        x = Math.Round(rad * Math.Cos(a), 5);
                        z = -Math.Round(rad * Math.Sin(a), 5);

                        Vector32 p = new Vector32((float)x, (float)y, (float)z);
                        vec2 tex = new vec2((float)j / (horSlices - 1), (float)i / (vertSlices - 1));
                        if (j == horSlices - 1) tex.x = 1;
                        texCoords.Add(tex);
                        points.Add(p);
                    }
                }
            }//void Create()
        }//CCylinder
        public class CCylinderExt : C3DObjectBase
        {
            //first point connect the last point
            public List<Vector32> points = new List<Vector32>();
            public double bottom = 0;
            public double top = 1; //z is the height
            public vec4 color = new vec4(0.5f, 0.5f, 0.5f, 1.0f);
            [CategoryAttribute("Display"), DisplayNameAttribute("Color")]
            public Color Color
            {
                get
                {
                    double r = color.x * 255;
                    double g = color.y * 255;
                    double b = color.z * 255;
                    double a = color.w * 255;
                    return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
                }
                set
                {
                    Color c = value;
                    float r = c.R / 255;
                    float g = c.G / 255;
                    float b = c.B / 255;
                    float w = c.A / 255;
                    color = new vec4(r, g, b, w);
                }
            }
            [CategoryAttribute("Display"), DisplayNameAttribute("top")]
            public double Top
            {
                get
                {
                    return top;
                }
                set
                {
                    top = value;
                }
            }
            [CategoryAttribute("Display"), DisplayNameAttribute("bottom")]
            public double Bottom
            {
                get
                {
                    return bottom;
                }
                set
                {
                    bottom = value;
                }
            }
            public CCylinderExt()
            {
                type = ShapeEnum.CylinderExt;
            }
            public CCylinderExt(Vector32[] array)
            {
                type = ShapeEnum.CylinderExt;
                for (int i = 0; i < array.Length; i++)
                    points.Add(array[i]);
            }
            public CCylinderExt(Vector64[] array)
            {
                type = ShapeEnum.CylinderExt;
                for (int i = 0; i < array.Length; i++)
                    points.Add(array[i]);
            }
            public CCylinderExt(List<Vector32> lists)
            {
                type = ShapeEnum.CylinderExt;
                for (int i = 0; i < lists.Count; i++)
                    points.Add(lists[i]);
            }
            public CCylinderExt(List<Vector64> lists)
            {
                type = ShapeEnum.CylinderExt;
                for (int i = 0; i < lists.Count; i++)
                    points.Add(lists[i]);
            }
            //two cylinder intersect
            public static CCylinderExt Intersect(CCylinderExt obj1, CCylinderExt obj2)
            {
                int n1 = obj1.points.Count;
                int n2 = obj2.points.Count;
                if (n1 < 3 || n2 < 3) return null;
                if (obj2.bottom >= obj1.top || obj2.top <= obj1.bottom) return null;

                Polygon2D poly1 = new Polygon2D(obj1.points);
                Polygon2D poly2 = new Polygon2D(obj2.points);
                Polygon2D poly = Polygon2D.GetIntersectionOfPolygons(poly1, poly2);

                CCylinderExt cy = new CCylinderExt(poly.points);
                cy.top = obj1.top;
                cy.bottom = obj1.bottom;
                if (obj2.bottom > obj1.bottom) cy.bottom = obj2.bottom;
                if (obj2.top < obj1.top) cy.top = obj2.top;

                return cy;
            }
            public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
            {
                UpdateRange();
                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    if (maxx > minx)
                        p.x = (float)(x1 + (x2 - x1) * (p.x - minx) / (maxx - minx));
                    else p.x = (float)(x1 + x2) / 2;
                    if (maxy > miny)
                        p.y = (float)(y1 + (y2 - y1) * (p.y - miny) / (maxy - miny));
                    else p.y = (float)(y1 + y2) / 2;
                    points[i] = p;
                }
                minx = x1;
                miny = y1;
                minz = z1;
                maxx = x2;
                maxy = y2;
                maxz = z2;
                bottom = z1;
                top = z2;
                minz = bottom;
                maxz = top;
            }
            public override void Normalize()
            {
                if (points.Count < 2) return;

                UpdateRange();

                Vector64 p0 = GetCenter64();
                Vector64 p = new Vector64(0, 0, 0);

                double x, y, z;
                for (int i = 0; i < points.Count; i++)
                {
                    p.x = scale.x * (points[i].x - p0.x);
                    p.y = scale.y * (points[i].y - p0.y);
                    p.z = scale.z * (points[i].z - p0.z);

                    //GL is clockwise,otherwise Rotate is counter clockwise
                    //if (rotate.x != 0) p.RotateOnAngle(-rotate.x, 0,0);
                    //if (rotate.y != 0) p.RotateOnAngle(0,rotate.y, 0);
                    //if (rotate.z != 0) p.RotateOnAngle(0,0,-rotate.z);

                    if (rotate.x != 0 || rotate.y != 0 || rotate.z != 0)
                        p.RotateOnAngle(-rotate.x, rotate.y, -rotate.z);

                    x = (p.x + p0.x + offset.x);
                    y = (p.y + p0.y + offset.y);
                    z = (p.z + p0.z + offset.z);

                    points[i] = new Vector32((float)x, (float)y, (float)z);
                }
                double z0 = (top - bottom) / 2;
                double h = scale.z * (top - bottom);

                bottom = z0 - h / 2;
                top = z0 + h / 2;
                bottom += offset.z;
                top += offset.z;

                scale = new vec3(1, 1, 1);
                offset = new vec3(0, 0, 0);
                rotate = new vec3(0, 0, 0);

                UpdateRange();
            }
            public override void UpdateRange()
            {
                minx = maxx = 0;
                miny = maxy = 0;
                minz = maxz = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    if (i == 0)
                    {
                        minx = maxx = points[i].X;
                        miny = maxy = points[i].Y;
                        minz = maxz = points[i].Z;
                    }
                    else
                    {
                        if (points[i].X < minx) minx = points[i].X;
                        if (points[i].Y < miny) miny = points[i].Y;
                        if (points[i].Z < minz) minz = points[i].Z;
                        if (points[i].X > maxx) maxx = points[i].X;
                        if (points[i].Y > maxy) maxy = points[i].Y;
                        if (points[i].Z > maxz) maxz = points[i].Z;
                    }
                }
                minz = bottom;
                maxz = top;
            }
            private bool SeekSection(string section, ref StreamReader sr)
            {
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    str = str.Trim(' ');
                    if (str.Length < 1) continue;
                    if (str == section) return true;
                }
                return false;
            }

            private string GetLineValue(string str, string name)
            {
                string[] ss = str.Split(new Char[] { '=', '=' }, 2);
                if (ss.Length < 2) return "";

                string s1 = ss[0].Trim(' ');
                if (s1.ToLower() != name.ToLower()) return "";

                return ss[1].Trim(' ');
            }
            public override bool ExportData(string filename)
            {
                FileStream fs = new FileStream(filename, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);

                string str = "[CylinderExt]";
                wr.WriteLine(str);

                str = "name = " + name;
                wr.WriteLine(str);

                str = "bottom = " + bottom;
                wr.WriteLine(str);

                str = "top = " + top;
                wr.WriteLine(str);

                str = "[POINTS]";
                wr.WriteLine(str);
                for (int i = 0; i < points.Count; i++)
                {
                    str = points[i].x + "," + points[i].y + "," + points[i].z;
                    wr.WriteLine(str);
                }
                wr.Close();
                fs.Close();

                return true;
            }
            public string errMsg = "";
            public override bool ImportData(string filename)
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);

                Clear();

                SeekSection("[CylinderExt]", ref sr);
                //name = s1 
                string str = sr.ReadLine();
                name = GetLineValue(str, "name");
                //bottom = 10
                str = sr.ReadLine();
                bottom = Convert.ToDouble(GetLineValue(str, "bottom"));
                //top = 20
                str = sr.ReadLine();
                top = Convert.ToDouble(GetLineValue(str, "top"));

                SeekSection("[POINTS]", ref sr);
                string[] ss;
                float x, y, z;
                for (int i = 0; (str = sr.ReadLine()) != null; i++)
                {
                    str = str.Replace((char)9, ' ');
                    ss = str.Split(new Char[] { ',', ',' }, 3);
                    x = (float)Convert.ToDouble(ss[0]);
                    y = (float)Convert.ToDouble(ss[1]);
                    z = (float)Convert.ToDouble(ss[2]);
                    points.Add(new Vector32(x, y, z));
                }
                sr.Close();
                fs.Close();

                UpdateRange();

                if (points.Count > 1)
                    return true;
                else return false;
            }
        }
    }
}



