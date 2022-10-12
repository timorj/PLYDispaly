using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using GlmNet;
// base class of all models
namespace DataLoad
{
    public enum ShapeEnum
    {
        Undefine = 0,
        Box = 1,
        Triangles = 2,
        Shphere = 3,
        Cylinder = 4,
        Line = 5,
        Polygon = 6,
        Points = 7,
        Grid3D = 10,
        Mesh = 11,
        Borehole = 12,
        Slicer = 13,    //grid slicer
        ISOSurface = 14,//derived from MC
        ISOSurfaceEX = 15,//derived from Improved MC
        LineMesh = 16,
        Polygon2D = 17,
        PolygonSlicer = 18,//slicer as polygon
        GeoProfile = 19,
        CylinderExt = 50,        
    };
    
    public class C3DObjectBase
    {
        public string name = "untitled";
        public ShapeEnum type = ShapeEnum.Undefine;
        public bool visible = true;
        public vec3 offset = new vec3(0,0,0);
        public vec3 rotate = new vec3(0, 0, 0);
        public vec3 scale = new vec3(1, 1, 1);
        public bool blend = false;
        public float alpha = 1.0f; // 0 - 1
        public bool enbaleTexture { get; set;} = true;
        public string textureImgFile = "";
        public double minx = 0.0;
        public double miny = 0.0;
        public double minz = 0.0;
        public double minv = 0.0;
        public double maxx = 0.0;
        public double maxy = 0.0;
        public double maxz = 0.0;
        public double maxv = 0.0;    
        public virtual double Minx { get { return minx; } }
        public virtual double Miny { get { return miny; } }
        public virtual double Minz { get { return minz; } }
        public virtual double Minv { get { return minv; } }
        public virtual double Maxx { get { return maxx; } }
        public virtual double Maxy { get { return maxy; } }
        public virtual double Maxz { get { return maxz; } }
        public virtual double Maxv { get { return maxv; } }
        public double Longitude1 { get; set; } = 0;
        public double Longitude2 { get; set; } = 0;
        public double Latitude1 { get; set; } = 0;
        public double Latitude2 { get; set; } = 0;
        public double Elevation1 { get; set; } = 0;
        public double Elevation2 { get; set; } = 0;
        public virtual EarthVector toEarthCoordinate(Vector64 p)
        {
            if (Longitude1 == Longitude2 || Latitude1 == Latitude2) return new EarthVector(0, 0, p.z, p.v);
            double longitude = Longitude1;
            double latitude = Latitude1;
            double elevation = Elevation1;
            if (Longitude2 != Longitude1 && Maxx != Minx)
                longitude = Longitude1 + (p.x - Minx) / (Maxx - Minx) * (Longitude2 - Longitude1);
            if (Latitude2 != Latitude1 && Maxy != Miny)
                latitude = Latitude1 + (p.y - Miny) / (Maxy - Miny) * (Latitude2 - Latitude1);
            if (Elevation2 != Elevation1 && Maxz != Minz)
                elevation = Elevation1 + (p.z - Minz) / (Maxz - Minz) * (Elevation2 - Elevation1);
            return new EarthVector(longitude, latitude, elevation, p.v);
        }
        public bool IsEarthMapped
        {
            get
            {
                if (Latitude1 != Latitude2 && Longitude1 != Longitude2)
                    return true;
                else return false;
            }
        }
        /// <summary>
        /// 球面系统坐标中根据配准的经纬度来生成纹理坐标
        /// </summary>
        /// <param name="p"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void GetEarthTextureCoord(Vector32 p, out float x, out float y)
        {
            x = y = 0;
            if (maxx != minx) x = (float)((p.x - minx) / (maxx - minx));
            if (maxy != miny) y = (float)((p.y - miny) / (maxy - miny));
        }
        public virtual void GetTextureCoord(Vector32 p, out float x, out float y)
        {
            if (IsEarthMapped) { GetEarthTextureCoord(p, out x, out y); return; }
            x = y = -1;
            int ret1 = 0;
            int ret2 = 1;
            double xx = maxx - minx;
            double yy = maxy - miny;
            double zz = maxz - minz;
            if (xx >= yy && xx >= zz)
            {
                ret1 = 0;
                ret2 = 1;
                if (zz >= yy) ret2 = 2;
            }
            else if (yy >= xx && yy >= zz)
            {
                ret1 = 1;
                ret2 = 0;
                if (zz >= xx) ret2 = 2;
            }
            else if (zz >= xx && zz >= yy)
            {
                ret1 = 2;
                ret2 = 0;
                if (yy >= xx) ret2 = 1;
            }

            if (ret1 == 0)
            {
                x = (float)((p.x - minx) / xx);
                if (ret2 == 1) //x,y
                    y = (float)((p.y - miny) / yy);
                else            //x,z
                    y = (float)((p.z - minz) / zz);
            }
            else if (ret1 == 1)
            {
                x = (float)((p.y - miny) / yy);
                if (ret2 == 0) //y,x
                    y = (float)((p.x - minx) / xx);
                else            //x,z
                    y = (float)((p.z - minz) / zz);
            }
            else if (ret1 == 2)
            {
                x = (float)((p.z - minz) / zz);
                if (ret2 == 0) //y,x
                    y = (float)((p.x - minx) / xx);
                else            //x,z
                    y = (float)((p.y - miny) / yy);
            }
        }


        public virtual void Clear() { }
        public virtual void Draw() { }
        public virtual void UpdateRange() {
        }

        //convert points according to tranform,scale,offset
        public virtual bool IsInRange(double x,double y,double z)
        {
            if (x < minx || x > maxx) return false;
            if (y < miny || y > maxy) return false;
            if (z < minz || z > maxz) return false;
            return true;
        }
        public virtual void Normalize() { }
        public virtual void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2) { }
        
        public virtual bool ExportData(string path) { return true; }
        public virtual bool SaveAs(string path) { return true; }
        public virtual bool SaveAs(ref BinaryWriter br)
        {
            br.Write((int)type);
            br.Write((Int32)name.Length);
            br.Write(name.ToCharArray());

            br.Write(visible);
            br.Write(blend);
            br.Write(alpha);
            br.Write(IsWireFrameMode);

            br.Write(minx);
            br.Write(maxx);
            br.Write(miny);
            br.Write(maxy);
            br.Write(minz);
            br.Write(maxz);
            br.Write(minv);
            br.Write(maxv);
            br.Write(offset.x);
            br.Write(offset.y);
            br.Write(offset.z);
            br.Write(scale.x);
            br.Write(scale.y);
            br.Write(scale.z);
            br.Write(rotate.x);
            br.Write(rotate.y);
            br.Write(rotate.z);

            return true;
        }
        public void CopyHeader(C3DObjectBase obj1)
        {
            obj1.name = name;
          //  obj1.type = type;
          //  obj1.visible = visible;
            obj1.offset = offset;
            obj1.rotate = rotate;
            obj1.scale = scale;
            obj1.blend = blend;
            obj1.alpha = alpha;
            obj1.enbaleTexture = enbaleTexture;
            obj1.textureImgFile = textureImgFile;
            obj1.minx = minx;
            obj1.miny = miny;
            obj1.minz = minz;
            obj1.minv = minv;
            obj1.maxx = maxx;
            obj1.maxy = maxy;
            obj1.maxz = maxz;
            obj1.maxv = maxv;
        }
       
        public virtual bool ImportData(string path) { return true; }
        public virtual bool LoadFrom(ref BinaryReader br)
        {
            type = (ShapeEnum)br.ReadInt32();
            int n = br.ReadInt32();
            char[] header = new char[n];
            header = br.ReadChars(n);
            name = new string(header);

            visible = br.ReadBoolean();
            blend = br.ReadBoolean();
            alpha = br.ReadSingle();
            IsWireFrameMode = br.ReadBoolean();            

            minx = br.ReadDouble();
            maxx = br.ReadDouble();
            miny = br.ReadDouble();
            maxy = br.ReadDouble();
            minz = br.ReadDouble();
            maxz = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();
            
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            offset = new vec3(x, y, z);

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            scale = new vec3(x, y, z);

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            rotate = new vec3(x, y, z);

            return true;
        }
        public virtual bool LoadFrom(string path) { return true; }

        public virtual bool ExportVRML(ref StreamWriter wr) { return true;}
        public virtual Vector64 GetCenter64()
        {
            double x = (Minx + Maxx) / 2;
            double y = (Miny + Maxy) / 2;
            double z = (Minz + Maxz) / 2;
            return new Vector64(x, y, z);
        }
        public virtual Vector32 GetCenter32()
        {
            float x = (float)(Minx + Maxx) / 2;
            float y = (float)(Miny + Maxy) / 2;
            float z = (float)(Minz + Maxz) / 2;

            return new Vector32(x, y, z);
        }
        public virtual Vector32 GetOffset32()
        {
            //Vector32 p0 = GetCenter32();
            //return new Vector32(p0.X + offset.x, p0.Y + offset.y, p0.Z + offset.z);
            return new Vector32(offset.x, offset.y, offset.z);
        }
        public virtual Vector64 GetOffset64()
        {
            Vector64 p0 = GetCenter64();
            return new Vector64(p0.X + offset.x, p0.Y + offset.y, p0.Z + offset.z);
        }
        public virtual vec3 toPoint(Vector32 p)
        {
            return new vec3(p.x, p.y, p.z);
        }
        public virtual Vector32 toPoint(vec3 p)
        {
            return new Vector32(p.x, p.y, p.z);
        }
        public virtual void CutWith(C3DObjectBase obj,int method,int methodPara)
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
        }
        public virtual Vector32 TransformedPoint(Vector32 p)
        {
            Vector32 p0 = GetCenter32();
            Vector32 p1 = p - p0;

            if (scale.x != 1.0) p1.X = p1.X * scale.x;
            if (scale.y != 1.0) p1.Y = p1.Y * scale.y;
            if (scale.z != 1.0) p1.Z = p1.Z * scale.z;

            if (rotate.x != 0.0) p1.RotateOnAngle(rotate.x, 0);
            if (rotate.y != 0.0) p1.RotateOnAngle(rotate.y, 1);
            if (rotate.z != 0.0) p1.RotateOnAngle(rotate.z, 2);

            p1 = p1 + p0;

            p1.x += offset.x;
            p1.y += offset.y;
            p1.z += offset.z;

            return p1;
        }
        public virtual Vector32 UnTransformedPoint(Vector32 p)
        {
            Vector32 p0 = GetCenter32();

            Vector32 p1 = new Vector32(p.x,p.y,p.z);

            p1.z -= (p0.z + offset.z);
            p1.y -= (p0.y + offset.y);
            p1.x -= (p0.x + offset.x);           

            if (rotate.z != 0.0) p1.RotateOnAngle(-rotate.z, 2);
            if (rotate.y != 0.0) p1.RotateOnAngle(-rotate.y, 1);
            if (rotate.x != 0.0) p1.RotateOnAngle(-rotate.x, 0);

            if (scale.z != 1.0 && scale.z != 0.0) p1.Z = p1.Z / scale.z;
            if (scale.y != 1.0 && scale.y != 0.0) p1.Y = p1.Y / scale.y;
            if (scale.x != 1.0 && scale.x != 0.0) p1.X = p1.X / scale.x;
            
            p1 = p1 + p0;

            return p1;
        }
        public virtual Vector64 TransformedPoint(Vector64 p)
        {
            Vector64 p0 = GetCenter64();
            Vector64 p1 = p - p0;

            if (scale.x != 1.0) p1.X = p1.X * scale.x;
            if (scale.y != 1.0) p1.Y = p1.Y * scale.y;
            if (scale.z != 1.0) p1.Z = p1.Z * scale.z;

            if (rotate.x != 0.0) p1.RotateOnAngle(rotate.x, 0);
            if (rotate.y != 0.0) p1.RotateOnAngle(rotate.y, 1);
            if (rotate.z != 0.0) p1.RotateOnAngle(rotate.z, 2);

            p1 = p1 + p0;

            p1.x += offset.x;
            p1.y += offset.y;
            p1.z += offset.z;

            return p1;
        }
        public virtual Vector64 UnTransformedPoint(Vector64 p)
        {
            Vector64 p0 = GetCenter64();
            Vector64 p1 = new Vector64(p.x, p.y, p.z);

            p1.z -= offset.z;
            p1.y -= offset.y;
            p1.x -= offset.x;
            p1 = p1 - p0;

            if (rotate.z != 0.0) p1.RotateOnAngle(-rotate.z, 2);
            if (rotate.y != 0.0) p1.RotateOnAngle(-rotate.y, 1);
            if (rotate.x != 0.0) p1.RotateOnAngle(-rotate.x, 0);

            if (scale.z != 1.0 && scale.z != 0.0) p1.Z = p1.Z / scale.z;
            if (scale.y != 1.0 && scale.y != 0.0) p1.Y = p1.Y / scale.y;
            if (scale.x != 1.0 && scale.x != 0.0) p1.X = p1.X / scale.x;

            p1 = p1 + p0;

            return p1;
        }

        //模型坐标转换到世界坐标
        public virtual Vector64 toWorldVector(Vector64 p)
        {
            if (IsEarthMapped) return toEarthCoordinate(p).toXYZVector();
            else return p;
        }
        public virtual Vector64 toWorldVector(Vector32 p)
        {
            return toWorldVector(p.toVector64());
        }

        [CategoryAttribute("Display"), DisplayNameAttribute("name")]
        public string _name
        {
            get { return name; }
            set { name = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Information")]
        public virtual string _info
        {
            get
            {
                string info = "x:" + minx + " to " + maxx+"\r\n";
                info += "y:" + miny + " to " + maxy + "\r\n";
                info += "z:" + minz + " to " + maxz;
                return info;
            }
            //set { name = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("visible")]
        public bool _visible
        {
            get { return visible; }
            set { visible = value; }
        }
        
        [CategoryAttribute("Display"), DisplayNameAttribute("Translation X")]
        public float _OffsetX
        {
            get { return offset.x; }
            set { offset.x = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Translation Y")]
        public float _OffsetY
        {
            get { return offset.y; }
            set { offset.y = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Translation Z")]
        public float _OffsetZ
        {
            get { return offset.z; }
            set { offset.z = value; }
        }

        [CategoryAttribute("Display"), DisplayNameAttribute("Angle X")]
        public float _RotateX
        {
            get { return rotate.x; }
            set { rotate.x = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Angle Y")]
        public float _RotateY
        {
            get { return rotate.y; }
            set { rotate.y = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Angle Z")]
        public float _RotateZ
        {
            get { return rotate.z; }
            set { rotate.z = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Scale X")]
        public float _ScaleX
        {
            get { return scale.x; }
            set { scale.x = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Scale Y")]
        public float _ScaleY
        {
            get { return scale.y; }
            set { scale.y = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Scale Z")]
        public float _ScaleZ
        {
            get { return scale.z; }
            set { scale.z = value; }
        }
        //blending         
        [CategoryAttribute("Display"), DisplayNameAttribute("Blend")]
        public bool _blend
        {
            get { return blend; }
            set { blend = value; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Blend Alpha(0-1)")]
        public float _alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }
        private bool bIsWireFrameMode = false;
        [CategoryAttribute("Display"), DisplayNameAttribute("Wireframe")]
        public bool IsWireFrameMode
        {
            get { return bIsWireFrameMode; }
            set { bIsWireFrameMode = value; }
        }
        public virtual bool ReadHeader(BinaryReader br)
        {
            try
            {
                char[] header = br.ReadChars(5);
                string tag = new string(header);
                //DSGD version 1.0 ,DSGE version 1.1
                if ( tag != "3DOBJ" ) return false;

                float x, y, z;

                name = br.ReadString();
                int itype = br.ReadByte();
                type = (ShapeEnum)itype;
                visible = br.ReadBoolean();
                
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                offset = new vec3(x, y, z);

                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                rotate = new vec3(x, y, z);

                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                scale = new vec3(x, y, z);

                blend = br.ReadBoolean();
                alpha = br.ReadSingle();

                minx = br.ReadDouble();
                miny = br.ReadDouble();
                minz = br.ReadDouble();
                minv = br.ReadDouble();
                maxx = br.ReadDouble();
                maxy = br.ReadDouble();
                maxz = br.ReadDouble();
                maxv = br.ReadDouble();
            }
            catch (IOException e)
            {              
                return false;
            }
            return true;
        }
        public bool WriteHeader(BinaryWriter br)
        {
            try
            {
                char[] header = new char[] { '3', 'D', 'O', 'B','J' };
                br.Write(header);
                br.Write(name);
                byte itype = (byte)type;
                br.Write(itype);
                br.Write(visible);
                br.Write(offset.x);
                br.Write(offset.y);
                br.Write(offset.z);
                br.Write(rotate.x);
                br.Write(rotate.y);
                br.Write(rotate.z);
                br.Write(scale.x);
                br.Write(scale.y);
                br.Write(scale.z);
                br.Write(blend);
                br.Write(alpha);
                br.Write(minx);
                br.Write(miny);
                br.Write(minz);
                br.Write(minv);
                br.Write(maxx);
                br.Write(maxy);
                br.Write(maxz);
                br.Write(maxv);
            }
            catch (IOException e)
            {
                return false;
            }
            return true;
        }
        public C3DObjectBase()
        {
           
        }      
    }    
}
