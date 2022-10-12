using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
/************************************
* Common Data Structure defination
* 
* Vector32,Vector64 
* 
* ******************************/
namespace DataLoad
{
    public enum DirectionEnum
    {
        undefine = 0,
        up = 1,
        down = 2,
        left = 4,
        right = 8,
        front = 16,
        back = 32,
    }
    static public class DirectionCheck
    {
        static public int toInt(DirectionEnum direction)
        {
            return (int)(direction);
        }
        static public DirectionEnum toDirection(int no)
        {
            if (no > 0 && no <= 63) return (DirectionEnum)(no);
            else return DirectionEnum.undefine;
        }
        static public bool Check(int no, DirectionEnum direction)
        {
            int id = (int)direction;
            if ((id & no) > 0) return true;
            else return false;
        }
    }


    //平面投影方法
    public enum ProjectionMethod
    {

    }
    /// <summary>
    /// 球面坐标（经纬度坐标，以度为单位）
    /// 经度坐标格式，以小数表示 0 - 360
    /// 维度坐标格式，以小数表示 0 - 180
    /// 采用右手螺旋，从南到北为正方向，从西向东为正
    /// </summary>
    public struct EarthVector
    {
        public double Longitude;    //经度
        public double Latitude;     //维度
        public double Elevation;    //高程
        public double Value;        //点属性值

        public EarthVector(double longitude, double latitude, double elevation = 0, double value = 0)
        {
            Longitude = longitude;
            Latitude = latitude;
            Elevation = elevation;
            Value = value;
        }
        /// <summary>
        /// 将经纬度坐标转换成球面平面坐标，便于绘制到球面上
        /// 以球心为原点，北极为z方向，东经为x轴
        /// 坐标单位：以1米的圆球面投影半径参考面
        ///     z(N)
        ///     | 
        ///     |_______y(N)
        ///    /
        ///   /x(E)
        ///   长轴半径：6378137m
        ///   扁率：298.257222101
        /// </summary>
        /// <returns></returns>
        public Vector64 toXYZVector(double rad = 6378137)
        {
            double x = 0, y = 0, z = 0;
            //此处计算
            double r = rad + Elevation;
            double a = toSNVector(Latitude);
            double b = toWEVector(Longitude);

            x = r * Math.Cos(Vector64.toRad(a)) * Math.Cos(Vector64.toRad(b));
            y = r * Math.Cos(Vector64.toRad(a)) * Math.Sin(Vector64.toRad(b));
            z = r * Math.Sin(Vector64.toRad(a));
            return new Vector64(x, y, z, Value);
        }
        public double toSNVector(double latitude)
        {
            if (latitude > 90) return latitude - 90;
            else
            {
                return latitude - 90;
            }
        }
        public double toWEVector(double longitude)
        {
            if (longitude < 180) return longitude;
            else
            {
                return longitude - 360;
            }
        }
        public int Degree(double ddd)
        {
            return (int)ddd;
        }
        public int Minute(double ddd)
        {
            return (int)(60 * (ddd - Degree(ddd)));
        }
        public double Second(double ddd)
        {
            double ms = 60 * (ddd - Degree(ddd));
            return (ms - (int)ms) * 60;
        }
        public double TotalSecond(double ddd)
        {
            return ddd * 3600;
        }
        /// <summary>
        /// 经度转换成度分秒格式
        /// </summary>
        /// <returns></returns>
        public string toDMSFormatLongitude()
        {
            double ddd = Longitude;
            string c = "E";
            string ss = "";
            if (Longitude > 180)
            {
                ddd = 360 - Longitude;
                c = "W";
            }
            ss += Degree(ddd);
            ss += "°";
            ss += Minute(ddd);
            ss += "′";
            ss += Second(ddd);
            ss += "″";
            ss += c;
            return ss;
        }
        public string toDMSFormatLatitude()
        {
            double ddd = Latitude;
            string c = "S";
            string ss = "";
            if (Latitude > 90)
            {
                ddd = Latitude - 90;
                c = "W";
            }
            else //0-90
            {
                ddd = 90 - Latitude;
                c = "S";
            }
            ss += Degree(ddd);
            ss += "°";
            ss += Minute(ddd);
            ss += "′";
            ss += Second(ddd);
            ss += "″";
            ss += c;
            return ss;
        }
        /// <summary>
        /// 将经纬度格式字符串转换成度为单位的小数格式
        /// 如维度：39°52′48″N
        /// 经度：116°24′20″E
        /// </summary>
        /// <returns></returns>
        public EarthVector Parse(string latitude, string longtitude)
        {
            EarthVector p = new EarthVector(0, 0, 0, 0);
            p.Latitude = ParseLatitude(latitude);
            p.Longitude = ParseLongitude(longtitude);
            return p;
        }
        public double ParseLatitude(string latitude)
        {
            try
            {
                string[] ss1 = latitude.ToUpper().Split(new char[] { '°', '′', '″', '度', '分', '秒', 'N', 'S' }, StringSplitOptions.RemoveEmptyEntries);
                double d = 0, m = 0, s = 0;
                if (ss1.Length > 0) d = (int)(double.Parse(ss1[0]));
                if (ss1.Length > 1) m = (int)(double.Parse(ss1[1]));
                if (ss1.Length > 2) s = double.Parse(ss1[2]);

                char c1 = latitude[0];
                char c2 = latitude[latitude.Length - 1];
                if (c1 == 'N' || c2 == 'N')
                {
                    return d + 90 + m / 60 + s / 3600;
                }
                else if (c1 == 'S' || c2 == 'S')
                {
                    return 90 - (d + m / 60 + s / 3600);
                }
                else return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        public double ParseLongitude(string longitude)
        {
            try
            {
                string[] ss1 = longitude.ToUpper().Split(new char[] { '°', '′', '″', '度', '分', '秒', 'W', 'E' }, StringSplitOptions.RemoveEmptyEntries);
                double d = 0, m = 0, s = 0;
                if (ss1.Length > 0) d = (int)(double.Parse(ss1[0]));
                if (ss1.Length > 1) m = (int)(double.Parse(ss1[1]));
                if (ss1.Length > 2) s = double.Parse(ss1[2]);

                char c1 = longitude[0];
                char c2 = longitude[longitude.Length - 1];
                if (c1 == 'E' || c2 == 'E')
                {
                    return d + m / 60 + s / 3600;
                }
                else if (c1 == 'W' || c2 == 'W')
                {
                    return 360 - (d + m / 60 + s / 3600);
                }
                else return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public override int GetHashCode()
        {
            return Longitude.GetHashCode() + Latitude.GetHashCode() +
                   Elevation.GetHashCode() + Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is EarthVector))
            {
                return false;
            }
            EarthVector p = (EarthVector)obj;
            return Longitude.Equals(p.Longitude) &&
                   Latitude.Equals(p.Latitude) &&
                   Elevation.Equals(p.Elevation);
        }

        public static bool operator ==(EarthVector p1, EarthVector p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return (p1.Longitude == p2.Longitude) &&
                   (p1.Latitude == p2.Latitude) &&
                   (p1.Elevation == p2.Elevation);
        }
        public static bool operator !=(EarthVector p1, EarthVector p2)
        {
            return (!(p1 == p2));
        }
    }


    public struct ColorRGBA
    {
        public byte R, G, B, A;        
        public ColorRGBA(int r, int g, int b, int a = 255)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)a;
        }
        public ColorRGBA(double r, double g, double b, double a = 255)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)a;
        }
        public static ColorRGBA operator +(ColorRGBA p1, ColorRGBA p2)
        {
            return new ColorRGBA(p1.R + p2.R, p1.G + p2.G, p1.B + p2.B, p1.A + p2.A);
        }
        public static ColorRGBA operator -(ColorRGBA p1, ColorRGBA p2)
        {
            return new ColorRGBA(p1.R - p2.R, p1.G - p2.G, p1.B - p2.B, p1.A - p2.A);
        }
        public static ColorRGBA operator *(double s, ColorRGBA p1)
        {
            return new ColorRGBA(p1.R * s, p1.G * s, p1.B * s, p1.A * s);
        }
        public static ColorRGBA operator *(ColorRGBA p1, double s)
        {
            return new ColorRGBA(p1.R * s, p1.G * s, p1.B * s, p1.A * s);
        }
        public static ColorRGBA operator /(ColorRGBA p1, double s)
        {
            if (s == 0) return new ColorRGBA(0, 0, 0, 0);
            else return new ColorRGBA(p1.R / s, p1.G / s, p1.B / s, p1.A / s);
        }
        public static Int32 ParseRGB(Color color)
        {
            return (Int32)(((uint)color.B << 16) | (ushort)(((ushort)color.G << 8) | color.R));
        }
        public static Color RGB(Int32 color)
        {
            Int32 r = 0xFF & color;
            Int32 g = 0xFF00 & color;
            g >>= 8;
            Int32 b = 0xFF0000 & color;
            b >>= 16;
            return Color.FromArgb(r, g, b);
        }
    }
    public struct ByteXYZ
    {
        public byte x, y, z;
        public ByteXYZ(byte _x, byte _y, byte _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() +
                   x.GetHashCode() ^ z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is byte))
            {
                return false;
            }
            ByteXYZ p = (ByteXYZ)obj;
            return x.Equals(p.x) &&
                   y.Equals(p.y) &&
                   z.Equals(p.z);
        }
        public static bool operator ==(ByteXYZ p1, ByteXYZ p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return (p1.x == p2.x) &&
                   (p1.y == p2.y) &&
                   (p1.z == p2.z);
        }
        public static bool operator !=(ByteXYZ p1, ByteXYZ p2)
        {
            return (!(p1 == p2));
        }
    }
    public struct DoubleRect
    {
        public double x1, x2, y1, y2;
        public double Width
        {
            get { return x2 - x1; }
        }
        public double Height
        {
            get { return y2 - y1; }
        }
        public void Offset(double offx, double offy)
        {
            x1 += offx;
            x2 += offx;
            y1 += offy;
            y2 += offy;
        }
        //if contain rect
        public bool Contains(DoubleRect rect)
        {
            if (x1 <= rect.x1 && x2 >= rect.x2 &&
                y1 <= rect.y1 && y2 >= rect.y2) return true;
            else return false;
        }
        public bool Contains(double x,double y)
        {
            if ( x >= x1 && x <= x2 && 
                 y >= y1 && y <= y2 ) return true;
            else return false;
        }
        public bool IsIntersectWith(DoubleRect rect)
        {
            if (rect.x1 > x2 || rect.x2 < x1 ||
                rect.y1 > y2 || rect.y2 < y1 ) return false;
            else return true;
        }
        public bool IsIntersectWith(double minx,double miny,double maxx,double maxy)
        {
            if (minx > x2 || maxx < x1 ||
                miny > y2 || maxy < y1) return false;
            else return true;
        }
        public void Scale(double scale)
        {
            double x0 = (x1 + x2) / 2.0;
            double y0 = (y1 + y2) / 2.0;
            double ww = Width * scale;
            double hh = Height * scale;
            x1 = x0 - ww / 2.0;
            x2 = x0 + ww / 2.0;
            y1 = y0 - hh / 2.0;
            y2 = y0 + hh / 2.0;
        }
        public DoubleRect(double _x1, double _y1, double _x2, double _y2)
        {
            x1 = _x1;
            y1 = _y1;
            x2 = _x2;
            y2 = _y2;
        }
    }
    public enum CPUCoding
    {
        BigEndian = 0,
        LittleEndian = 1,
    }
    static public class ConvertData
    {
        static public int checkCPU()
        {
            return 1;
        }
        static public byte[] ReverseBytes(byte[] buf)
        {
            int len = buf.Length;
            if (len < 2) return buf;

            byte[] buf1 = new byte[len];

            for (int i = 0; i < len; i++)
                buf1[i] = buf[len - 1 - i];

            return buf1;
        }
        static public UInt32 BitToUInt(byte[] buf, bool reverseBit = true)
        {
            int len = buf.Length;
            UInt32 ret = 0;
            if (reverseBit)
            {
                byte[] buf1 = ReverseBytes(buf);
                if (len <= 2) ret = BitConverter.ToUInt16(buf1, 0);
                else ret = BitConverter.ToUInt32(buf1, 0);
                buf1 = null;
            }
            else
            {
                if (len <= 2) ret = BitConverter.ToUInt16(buf, 0);
                else ret = BitConverter.ToUInt32(buf, 0);
            }
            return ret;
        }
        static public int BitToInt(byte[] buf, bool reverseBit = true)
        {
            int len = buf.Length;
            int ret = 0;
            if (reverseBit)
            {
                byte[] buf1 = ReverseBytes(buf);
                if (len <= 2) ret = BitConverter.ToInt16(buf1, 0);
                else ret = BitConverter.ToInt32(buf1, 0);
                buf1 = null;
            }
            else
            {
                if (len <= 2) ret = BitConverter.ToInt16(buf, 0);
                else ret = BitConverter.ToInt32(buf, 0);
            }
            return ret;
        }
        static public double BitToDouble(byte[] buf, bool reverseBit = true)
        {
            int len = buf.Length;
            double ret = 0;
            if (reverseBit)
            {
                byte[] buf1 = ReverseBytes(buf);
                if (len <= 4) ret = BitConverter.ToSingle(buf1, 0);
                else ret = BitConverter.ToDouble(buf1, 0);
                buf1 = null;
            }
            else
            {
                if (len <= 4) ret = BitConverter.ToSingle(buf, 0);
                else ret = BitConverter.ToDouble(buf, 0);
            }
            return ret;
        }
        static public long MakeLongFromBytes(byte c1, byte c2, byte c3, byte c4, bool reverseBit = true)
        {
            byte[] cc = new byte[4];
            if (reverseBit)
            {
                cc[0] = c4;
                cc[1] = c3;
                cc[2] = c2;
                cc[3] = c1;
            }
            else
            {
                cc[0] = c1;
                cc[1] = c2;
                cc[2] = c3;
                cc[3] = c4;
            }
            return BitConverter.ToInt32(cc, 0);
        }
        static public ulong MakeULongFromBytes(byte c1, byte c2, byte c3, byte c4, bool reverseBit = true)
        {
            byte[] cc = new byte[4];
            if (reverseBit)
            {
                cc[0] = c4;
                cc[1] = c3;
                cc[2] = c2;
                cc[3] = c1;
            }
            else
            {
                cc[0] = c1;
                cc[1] = c2;
                cc[2] = c3;
                cc[3] = c4;
            }
            return BitConverter.ToUInt32(cc, 0);
        }
        static public int MakeIntFromBytes(byte c1, byte c2, bool reverseBit = true)
        {
            byte[] cc = new byte[2];
            cc[0] = c1;
            cc[1] = c2;
            return BitToInt(cc, reverseBit);
        }
        // http://en.wikipedia.org/wiki/IBM_Floating_Point_Architecture
        // float2ibm(-118.625F) == 0xC276A000
        // 1 100 0010    0111 0110 1010 0000 0000 0000
        // IBM/370 single precision, 4 bytes
        // xxxx.xxxx xxxx.xxxx xxxx.xxxx xxxx.xxxx
        // s|-exp--| |--------fraction-----------|
        //    (7)          (24)
        // value = (-1)**s * 16**(e - 64) * .f   range = 5E-79 ... 7E+75
        static public int float2ibm(float from)
        {
            byte[] bytes = BitConverter.GetBytes(from);
            int fconv = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];

            if (fconv == 0) return 0;
            int fmant = (0x007fffff & fconv) | 0x00800000;
            int t = (int)((0x7f800000 & fconv) >> 23) - 126;
            while (0 != (t & 0x3)) { ++t; fmant >>= 1; }
            fconv = (int)(0x80000000 & fconv) | (((t >> 2) + 64) << 24) | fmant;
            return fconv; // big endian order
        }
        /*
        public void ibmFloat2float(byte[] bufData, float* traceData, int iSamples)
        {
            //  d24 = 1.0f/16777216.0f;  
            int byte1, byte2, byte3, byte4, bsign;
            int exp, basep;
            for (int i = 0; i < iSamples; i++)
            {
                int index = i * 4;
                byte1 = bufData[index++];
                byte2 = bufData[index++];
                byte3 = bufData[index++];
                byte4 = bufData[index];
                bsign = byte1 & 0x80;
                exp = (byte1 & 0x7F) - 64;

                basep = byte2;
                basep = (basep << 8) + byte3;
                basep = (basep << 8) + byte4;
                if (0 == bsign)
                    traceData[i] = pow(16.0, exp) * basep * d24;
                else
                    traceData[i] = -1 * pow(16.0, exp) * basep * d24;
            }
        }
        */
        public static float toIEEEfloat(byte[]cc)
        {
            //S1 E8        F23
            //00000000011111111111111111111111
            //10000000000000000000000000000000
            //0 00000000 00000000000000000000000
            uint val = BitConverter.ToUInt32(ReverseBytes(cc), 0);
            
            uint a = val >> 31;
            int s = 1;
            if (a > 0) s = -1;

            uint E = val << 1;
            E = E >> 24;
            uint F = val & 0x7fffff;

            //IEEE
            uint A = 2;
            uint B = 127;
            uint C = 1;
            uint M = C + F;
            //A=16;B=64;C=0;ibm

             return (float)(s* M * Math.Pow(A, E - B));
        }
        public static float IBMtoIEEE(byte[] bb)
        {
            uint fraction;
            int exponent;
            int sign;
            uint ui;

            ////根据情况看是否进行字节转换  
            System.Array.Reverse(bb);  

            // @ 标识符号位     
            // # 标识阶数位     
            // * 标识尾数位  
            //IBM浮点数： SEEEEEEE MMMMMMMM MMMMMMMM MMMMMMMM        Value = (-1)^s * M * 16^(E-64)  
            //IEEE浮点数：SEEEEEEE EMMMMMMM MMMMMMMM MMMMMMMM        Value = (-1)^s * (1 +  M) * 2^(E-127)  

            fraction = System.BitConverter.ToUInt32(bb, 0);

            sign = (int)(fraction >> 31);           // 获取符号位;  
            fraction <<= 1;                         // 左移移出符号位，右侧填0;  
            exponent = (int)(fraction >> 25);       // 获取阶数;  
            fraction <<= 7;                         //移出符号位 和 阶数 剩余的部分：尾数部分;  

            /* 
             * 如果尾数部分为0,则说明该数是特定值：0或者无穷。 
             * 当指数=127，说明当前数是无穷大; 对应的IEEE无穷大时，指数为255 
             * 当指数=0,说明当前数为0;对应的IEEE为0时，指数为255 
             * 当0<指数<127，根据公式 (-1)^s * M * 16^(E-64) ，M为0,则最后结果是0；对应的IEEE为0时，指数为255 
             */
            if (fraction == 0)
            {
                if (exponent != 127)
                    exponent = 0;
                else
                    exponent = 255;

                goto done;
            }

            // 将IBM 浮点数的阶码转化成 IEEE 的阶码：(exp - 64) * 4 + 127 - 1 == exp * 4 - 256 + 126 == (exp << 2) - 130  
            //IEEE阶码= IBM阶码 * 4 -130  
            exponent = (exponent << 2) - 130;

            // 将尾数规格化，因为浮点数能表示的最小数是1/16，所以规格化过程最多左移三次。  
            while (fraction < 0x80000000)
            {
                --exponent;
                fraction <<= 1;
            }


            if (exponent <= 0)  //下限溢出，指数不能小于零  0<=E（ieee）<=254  
            {
                //if (exponent < -24)  
                //{  
                //    // complete underflow - return properly signed zero  
                //    fraction = 0;  
                //}  
                //else  
                //{  
                //    // partial underflow - return denormalized number  
                //    fraction >>= -exponent;  
                //}  

                exponent = 0;
                fraction = 0;
            }
            else if (exponent >= 255)   //上限溢出:指数不能大于255;表示无穷大；  
            {
                fraction = 0;
                exponent = 255;
            }
            else //IEEE尾码 = IBM尾码 * 2 -1;相当于左移一位  
            {
                fraction <<= 1;
            }

            done:
            ui = (uint)((exponent << 23) | (sign << 31));
            ui = ui | (fraction >> 9);

            bb = System.BitConverter.GetBytes(ui);

            return System.BitConverter.ToSingle(bb, 0);
        }
       
        public static float IEEEtoIBM(float from)
       {  
          uint fraction;  
          int exponent;  
          int sign;  
          uint ui;  
  
          ////根据情况看是否进行字节转换  
          //System.Array.Reverse(bb);  
  
          // @ 标识符号位     
          // # 标识阶数位     
          // * 标识尾数位  
          //IBM浮点数： SEEEEEEE MMMMMMMM MMMMMMMM MMMMMMMM        Value = (-1)^s * M * 16^(E-64)  
          //IEEE浮点数：SEEEEEEE EMMMMMMM MMMMMMMM MMMMMMMM        Value = (-1)^s * (1 +  M) * 2^(E-127)  
          byte[] bb = BitConverter.GetBytes(from);  
          fraction = System.BitConverter.ToUInt32(bb, 0);  
  
          sign = (int) (fraction >> 31);           // 获取符号位;  
          fraction <<= 1;                         // 左移移出符号位，右侧填0;  
          exponent = (int) (fraction >> 24);       // 获取阶数;  
          fraction <<= 8;                         //移出符号位 和 阶数 剩余的部分：尾数部分;  
  
  
          /* 
           * 特定概念值处理 
           *  
           * 如果尾数部分为0,则说明该数是特定值：0或者无穷。 
           * 当指数=255，说明当前数是无穷大; 对应的IBM无穷大时，指数为127。 
           * 当指数=0,说明当前数为0; 对应的IBM为0时，指数为0. 
           *  
           * IEEE非数字：指数为255，小数部分不为零。 
           * IBM非数字：指数为127，小数部分最高位为1，其他位为0. 
          */  
          if (fraction == 0) //如果尾数为零 判断是否是 无穷大 或 0  
          {  
              if (exponent == 0) //0  
                  goto done;  
              else if (exponent == 255) //无穷大  
              {  
                  exponent = 127;  
                  goto done;  
              }  
          }  
          else if (exponent == 255)  //判断是否是数字  
          {  
              fraction = 0x80000000;  
              goto done;  
          }  
  
          //执行（M+1）/2;  
          fraction = (fraction >> 1) | 0x80000000;  
  
          //因为IBM 和 IEEE 的指数都是整数  
          //但是（IEEE阶码 +130）/4= IBM阶码。为了保证IBM 阶码是整数。必须对IBM 尾数进行移位处理。  
          int remainder = (exponent + 130) % 4; //余数  
          exponent = (exponent + 130) >> 2;  //商  
          if (remainder > 0)  
          {  
              exponent++;  
              fraction = fraction >> (4-remainder);  
          }  
  
        done:  
          ui = (uint) ((exponent << 24) | (sign << 31));  
          ui = ui | (fraction >> 8);  
  
          bb = System.BitConverter.GetBytes(ui);  
  
          return System.BitConverter.ToSingle(bb, 0);  
        }

       //---------------------------------------------------------------
        public static bool StringToInt(string ss, out int value)
        {
            value = 0;
            try
            {
                value = int.Parse(ss);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        public static bool StringToFloat(string ss, out float value)
        {
            value = 0;
            try
            {
                value = float.Parse(ss);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        public static bool StringToDouble(string ss, out double value)
        {
            value = 0;
            try
            {
                value = double.Parse(ss);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        //---------------------------------------------------
        public static double StringToDouble(string ss)
        {
            return toDouble(ss);
        }
        public static int StringToInt(string ss)
        {
            return toInt(ss);
        }
        public static float StringToFloat(string ss)
        {
            return toFloat(ss);
        }
        //-----------------------------------------------------
        public static double toDouble(string ss)
        {
            double ret = 0;
            try
            {
                ret = double.Parse(ss);
            }
            catch (Exception e) { }
            return ret;           
        }
        public static int toInt(string ss)
        {
            int ret = 0;
            try
            {
                ret = int.Parse(ss);
            }
            catch (Exception e) { }
            return ret;
        }
        public static float toFloat(string ss)
        {
            float ret = 0;
            try
            {
                ret = float.Parse(ss);
            }
            catch (Exception e) { }
            return ret;
        }
        //-----------------------------------------------------
    }
    public struct Int16XYZ
    {
        public Int16 x, y, z;
        public Int16XYZ(Int16 _x, Int16 _y, Int16 _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() +
                   x.GetHashCode() ^ z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Int16XYZ))
            {
                return false;
            }
            Int16XYZ p = (Int16XYZ)obj;
            return x.Equals(p.x) &&
                   y.Equals(p.y) &&
                   z.Equals(p.z);
        }
        public static bool operator ==(Int16XYZ p1, Int16XYZ p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return (p1.x == p2.x) &&
                   (p1.y == p2.y) &&
                   (p1.z == p2.z);
        }
        public static bool operator !=(Int16XYZ p1, Int16XYZ p2)
        {
            return (!(p1 == p2));
        }
    }
    public struct UInt16XYZ
    {
        public UInt16 x, y, z;
        public UInt16XYZ(UInt16 _x, UInt16 _y, UInt16 _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() +
                   x.GetHashCode() ^ z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is UInt16XYZ))
            {
                return false;
            }
            UInt16XYZ p = (UInt16XYZ)obj;
            return x.Equals(p.x) &&
                   y.Equals(p.y) &&
                   z.Equals(p.z);
        }
        public static bool operator ==(UInt16XYZ p1, UInt16XYZ p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return (p1.x == p2.x) &&
                   (p1.y == p2.y) &&
                   (p1.z == p2.z);
        }
        public static bool operator !=(UInt16XYZ p1, UInt16XYZ p2)
        {
            return (!(p1 == p2));
        }
    }
    public struct Int32XYZ
    {
        public Int32 x, y, z;
        public Int32XYZ(Int32 _x, Int32 _y, Int32 _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() +
                   x.GetHashCode() ^ z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Int32XYZ))
            {
                return false;
            }
            Int32XYZ p = (Int32XYZ)obj;
            return x.Equals(p.x) &&
                   y.Equals(p.y) &&
                   z.Equals(p.z);
        }
        public static bool operator ==(Int32XYZ p1, Int32XYZ p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return (p1.x == p2.x) &&
                   (p1.y == p2.y) &&
                   (p1.z == p2.z);
        }
        public static bool operator !=(Int32XYZ p1, Int32XYZ p2)
        {
            return (!(p1 == p2));
        }
    }
    public struct UInt32XYZ
    {
        public UInt32 x, y, z;
        public UInt32XYZ(UInt32 _x, UInt32 _y, UInt32 _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() +
                   x.GetHashCode() ^ z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is UInt32XYZ))
            {
                return false;
            }
            UInt32XYZ p = (UInt32XYZ)obj;
            return x.Equals(p.x) &&
                   y.Equals(p.y) &&
                   z.Equals(p.z);
        }
        public static bool operator ==(UInt32XYZ p1, UInt32XYZ p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return p1.Equals(p2);
        }
        public static bool operator !=(UInt32XYZ p1, UInt32XYZ p2)
        {
            return (!(p1 == p2));
        }
    }
    public struct CubeModel32
    {
        public float X1, Y1, Z1, X2, Y2, Z2;
        public CubeModel32(float _x1, float _y1, float _z1, float _x2, float _y2, float _z2)
        {
            X1 = _x1;
            Y1 = _y1;
            Z1 = _z1;
            X2 = _x2;
            Y2 = _y2;
            Z2 = _z2;
        }       
        public bool IsPointIn(double x, double y, double z)
        {
            if (x < X1 || y < Y1 || z < Z1 ||
                    x > X2 || y > Y2 || z > Z2)
                return false;
            else return true;
        }
        public bool IsPointIn(Vector32 p)
        {
            return IsPointIn(p.x, p.y, p.z);
        }
        public override int GetHashCode()
        {
            return X1.GetHashCode() ^ Y1.GetHashCode() +
                   Z1.GetHashCode() ^ X2.GetHashCode() +
                   Y2.GetHashCode() ^ Z2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is CubeModel32))
            {
                return false;
            }
            CubeModel32 p = (CubeModel32)obj;
            return X1.Equals(p.X1) &&
                   Y1.Equals(p.Y1) &&
                   Z1.Equals(p.Z1) &&
                   X2.Equals(p.X2) &&
                   Y2.Equals(p.Y2) &&
                   Z2.Equals(p.Z2);
        }
        public static bool operator ==(CubeModel32 p1, CubeModel32 p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }

            return p1.Equals(p2);
        }
        public static bool operator !=(CubeModel32 p1, CubeModel32 p2)
        {
            return (!(p1 == p2));
        }
        public Vector32 GetCenterPoint()
        {
            return new Vector32((X1 + X2) / 2, (Y1 + Y2) / 2, (Z1 + Z2) / 2);
        }
        public float GetMaxLength()
        {
            float len = X2 - X1;
            if (Y2 - Y1 > len) len = Y2 - Y1;
            if (Z2 - Z1 > len) len = Z2 - Z1;
            return len;
        }
    }
    public struct CubeModel64
    {
        public double X1, Y1, Z1, X2, Y2, Z2;
        public CubeModel64(double _x1, double _y1, double _z1, double _x2, double _y2, double _z2)
        {
            X1 = _x1;
            Y1 = _y1;
            Z1 = _z1;
            X2 = _x2;
            Y2 = _y2;
            Z2 = _z2;
        }
        public CubeModel64 Copy()
        {
            return new CubeModel64(X1,Y1,Z1,X2,Y2,Z2);            
        }
        public bool IsIdentityCube
        {
            get 
            {
                if (X1 == -1 && X2 == 1 &&
                    Y1 == -1 && Y2 == 1 &&
                    Z1 == -1 && Z2 == 1) return true;
                else return false;
            }
        }
        public Vector64 GetCenter()
        {
            return new Vector64((X1 + X2) / 2.0, (Y1 + Y2) / 2.0, (Z1 + Z2) / 2.0);
        }
        //合并两个区域范围
        static public CubeModel64 MergeCubes(CubeModel64 cube1,CubeModel64 cube2)
        {
            CubeModel64 cube = cube1.Copy();
            if ( cube1.X1 > cube2.X1 ) cube.X1 = cube2.X1;
            if ( cube1.Y1 > cube2.Y1 ) cube.Y1 = cube2.Y1;
            if ( cube1.Z1 > cube2.Z1 ) cube.Z1 = cube2.Z1;
            if ( cube1.X2 < cube2.X2 ) cube.X2 = cube2.X2;
            if ( cube1.Y2 < cube2.Y2 ) cube.Y2 = cube2.Y2;
            if ( cube1.Z2 < cube2.Z2 ) cube.Z2 = cube2.Z2;
            return cube;
        }
        //将区域cube变换到长宽高等长，移动并居中        
        public CubeModel64 toCentricCube()
        {
            CubeModel64 cube = new CubeModel64(X1,Y1,Z1,X2,Y2,Z2);
            double maxlen = MaxLength;
            double xw = X2 - X1;
            double yw = Y2 - Y1;
            double zw = Z2 - Z1;
            cube.X1 = X1 - (maxlen - xw) / 2.0;
            cube.X2 = X2 + (maxlen - xw) / 2.0;
            cube.Y1 = Y1 - (maxlen - yw) / 2.0;
            cube.Y2 = Y2 + (maxlen - yw) / 2.0;
            cube.Z1 = Z1 - (maxlen - zw) / 2.0;
            cube.Z2 = Z2 + (maxlen - zw) / 2.0;

            return cube;
        }
        public bool IsPointIn(double x, double y,double z)
        {
            if ( x < X1 || y < Y1 || z < Z1 ||
                    x > X2 || y > Y2 || z > Z2)
                return false;
            else return true;
        }
        public bool IsPointIn(Vector64 p)
        {
            return IsPointIn(p.x, p.y, p.z);            
        }
        public bool IsPointIn(Vector32 p)
        {
            return IsPointIn(p.x, p.y, p.z);
        }
        public double XWidth { get { return X2 - X1; } }
        public double YWidth { get { return Y2 - Y1; } }
        public double ZWidth { get { return Z2 - Z1; } }
        public double GetWidth(int i)
        {
            if (i == 0) return X2 - X1;
            else if (i == 1) return Y2 - Y1;
            else if (i == 2) return Z2 - Z1;
            return 0;
        }
        public string toString()
        {
            string ss = "X(";
            ss += X1.ToString();
            ss += " - ";
            ss += X2.ToString();
            ss += ") ";

            ss += "Y(";
            ss += Y1.ToString();
            ss += " - ";
            ss += Y2.ToString();
            ss += ") ";

            ss += "Z(";
            ss += Z1.ToString();
            ss += " - ";
            ss += Z2.ToString();
            ss += ");";
            return ss;
        }
        public override int GetHashCode()
        {
            return X1.GetHashCode() ^ Y1.GetHashCode() +
                   Z1.GetHashCode() ^ X2.GetHashCode() +
                   Y2.GetHashCode() ^ Z2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is CubeModel64))
            {
                return false;
            }
            CubeModel64 p = (CubeModel64)obj;
            return X1.Equals(p.X1) &&
                   Y1.Equals(p.Y1) &&
                   Z1.Equals(p.Z1) &&
                   X2.Equals(p.X2) &&
                   Y2.Equals(p.Y2) &&
                   Z2.Equals(p.Z2);
        }
        public static bool operator ==(CubeModel64 p1, CubeModel64 p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return p1.Equals(p2);
        }

        public static bool operator !=(CubeModel64 p1, CubeModel64 p2)
        {
            return (!(p1 == p2));
        }
        public double MaxLength 
        {
            get 
            {
                double len = X2 - X1;
                if (Y2 - Y1 > len) len = Y2 - Y1;
                if (Z2 - Z1 > len) len = Z2 - Z1;
                return len;
            }
        }       
        public Vector64 GetCenterPoint()
        {
            return new Vector64((X1 + X2) / 2, (Y1 + Y2) / 2, (Z1 + Z2) / 2);
        }
    }
    public struct Vector32
    {
        public float X, Y, Z, V;        
        public Vector32(double _x, double _y, double _z, double _v = 0)
        {
            X = (float)_x;
            Y = (float)_y;
            Z = (float)_z;
            V = (float)_v;
        }        
        public Vector64 toVector64()
        {
            return new Vector64(x,y,z,v);
        }
        
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() + Z.GetHashCode() ^ V.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Vector32))
            {
                return false;
            }
            Vector32 p = (Vector32)obj;
            return X.Equals(p.X) &&
                   Y.Equals(p.Y) &&
                   Z.Equals(p.Z) &&
                   V.Equals(p.V);
        }
        static public List<Vector64> toVector64List(List<Vector32> points)
        {
            List<Vector64> points64 = new List<Vector64>();
            foreach (Vector32 p in points)
            {
                points64.Add(p.toVector64());
            }
            return points64;
        }
        /// <summary>
        /// 删除重复点，点距小于的最大值的 * zerobase
        /// </summary>
        /// <param name="points"></param>
        /// <param name="zerobase"></param>
        /// <returns></returns>
        static public int RemoveDuplicated(ref List<Vector32> points,double zerobase = 0.0001)
        {
            if (points.Count < 2) return 0;

            double dist;
            Vector32 p,p1, p2;
            double maxlen = 0;
            double x1=0, y1=0, z1=0;
            double x2=0, y2=0, z2=0;
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
                    if (dist <= err) del[j] = true;
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
        /// <summary>
        /// 删除线上相近点
        /// </summary>
        /// <param name="points"></param>
        /// <param name="ZeroBase">作为零值分母基数</param>
        /// <returns></returns>
        static public int RemoveLineDuplicated(ref List<Vector32> points, double ZeroBase = 0.001)
        {
            int n = points.Count;
            if (n < 2) return 0;
            
            //删除标记
            bool[] mark = new bool[n];
            for (int i = 0; i < n; i++) mark[i] = false;

            //计算总长,不计算实际长度，只计算|x1-x2|+|y2-y1|,避免开方节省时间
            Vector32 p1, p2;
            double sum = 0.0;
            double dist = 0.0;
            for (int i = 1; i < n; i++)
            {
                p1 = points[i - 1];
                p2 = points[i];
                dist = Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
                sum += dist;
            }

            //点距平均长度 
            double zero = ZeroBase * sum / (n - 1);
            for (int i = 1; i < n; i++)
            {
                p1 = points[i - 1];
                p2 = points[i];
                dist = Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
                if ( dist <= zero ) mark[i] = true;
            }

            int removed = 0;
            for ( int i = n - 1; i > 0; i--)
            {
                if (mark[i])
                {
                    points.RemoveAt(i);
                    removed++;
                }
            }

            mark = null;
            
            return removed;
        }
        /// <summary>
        /// 去直线上冗余点
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /////-------------------
        static public int RemoveLineRedundant(ref List<Vector32> points)
        {
            if (points.Count < 3) return 0;

            Stack<Vector32> lists = new Stack<Vector32>();
            for ( int i = points.Count - 1; i >= 0; i--)lists.Push(points[i]);

            int removed = 0;
            points.Clear();
            Vector32 p1 = lists.Pop();
            Vector32 p2 = lists.Pop();
            Vector32 p;

            points.Add(p1);

            while (lists.Count > 0)
            {
                p = lists.Pop();

                //p点是否在直线p1p2上
                if ( IsPointOnLine2D(p, p1, p2) )
                {
                    p2 = p; //忽略该点
                    removed++;
                }
                else
                {
                    points.Add(p2);
                    p1 = p2;
                    p2 = p;
                }
            }
            //last one
            points.Add(p2);
            return removed;
        }
        public static bool operator ==(Vector32 p1, Vector32 p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return p1.Equals(p2);
        }

        public static bool operator !=(Vector32 p1, Vector32 p2)
        {
            return !p1.Equals(p2);
        }
        public static Vector32 operator +(Vector32 p1, Vector32 p2)
        {
            return new Vector32(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z, p1.V + p2.V);
        }
        public static Vector32 operator -(Vector32 p1, Vector32 p2)
        {
            return new Vector32(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z, p1.V - p2.V);
        }
        public static Vector32 operator *(double s, Vector32 p1)
        {
            return new Vector32( p1.X * s, p1.Y * s, p1.Z * s, p1.V * s);
        }
        public static Vector32 operator *(Vector32 p1, double s)
        {
            return new Vector32(p1.X * s, p1.Y * s, p1.Z * s, p1.V * s);
        }
        public static Vector32 operator /(Vector32 p1, double s)
        {
            if (s == 0) return new Vector32(0, 0, 0, 0);
            else return new Vector32(p1.X / s, p1.Y / s, p1.Z / s, p1.V / s);
        }
        public static implicit operator Vector64(Vector32 v)
        {
            return new Vector64(v.X, v.Y, v.Z, v.V);
        }
        //to 0 - 360 degree
        public static double toAngle(double rad)
        {
            double angle = 180 * rad / Math.PI;
            int n = (int)(angle / 360);

            if (n > 0) angle = angle - n * 360;

            return angle;
        }
        //to 0 - 2*Pai
        public static double toRad(double angle)
        {
            double rad = angle * Math.PI / 180;

            int n = (int)(0.5 * rad / Math.PI);

            if (n > 0) rad = rad - n * 2 * Math.PI;

            return rad;
        }
        //return angle ,not rad
        public static double VectorAngle(Vector32 p1, Vector32 p2)
        {
            double m1 = p1.Length;
            double m2 = p2.Length;

            //no angle
            if (m1 == 0 || m2 == 0) return 0;

            double acos = Dot(p1, p2) / (m1 * m2);
            double angle = toAngle(Math.Acos(acos));

            Vector32 vp = Cross(p1, p2);
            if (p1.X == p2.X)//yz
            {
                //clock <0 , counter clockwise >0
                if (vp.X < 0) return -angle;
            }
            if (p1.Y == p2.Y)//yz
            {
                //clock <0 , counter clockwise >0
                if (vp.Y < 0) return -angle;
            }
            if (p1.Z == p2.Z)//yz
            {
                //clock <0 , counter clockwise >0
                if (vp.Z < 0) return -angle;
            }
            return angle;
        }
        //2d version
        public static double PointToLineDistance(Vector32 p,Vector32 p1,Vector32 p2)
        {
            double a = p2.y - p1.y;
            double b = p1.x - p2.x;
            double c = p2.x * p1.y - p1.x * p2.y;
            if (a == 0 && b == 0)
                return Vector32.Distance(p1, p);
            else return Math.Abs(a * p.x + b * p.y + c) / Math.Sqrt(a * a + b * b);
        }
        public static double PointToLineDistance(double x0,double y0,double x1,double y1,double x2,double y2)
        {
            double a = y2 - y1;
            double b = x1 - x2;
            double c = x2 * y1 - x1 * y2;
            if (a == 0 && b == 0)
                return Math.Sqrt((x0-x1)*(x0-x1)+(y0-y1)*(y0-y1));
            //else return Math.Abs(a * x0 + b * y0 + c) / Math.Sqrt(a * a + b * b);
            else return Math.Abs( (y2 - y1) * x0 + (x1 - x2) * y0 + (x2 * y1) - (x1 * y2) ) 
                                / Math.Sqrt( (y2 - y1)* (y2 - y1) + (x1 - x2)* (x1 - x2) );

        }
        public static double Distance(Vector32 p1, Vector32 p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) +
                                (p1.Y - p2.Y) * (p1.Y - p2.Y) +
                                (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }
        public double Distance(Vector32 p)
        {
            return Distance(this, p);
        }
        public static double GetLength(Vector32 p1)
        {
            return Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y + p1.Z * p1.Z);
        }
        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }
        public double sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }
        public double Magnitude
        {
            get { return Math.Sqrt(x * x + y * y + z * z); }
        }
        public static Vector32 Normalize(Vector32 p)
        {
            double r = GetLength(p);
            if (r == 0) return new Vector32(0, 0, 0);
            else return new Vector32((float)(p.X / r), (float)(p.Y / r), (float)(p.Z / r));
        }
        public Vector32 Normalize()
        {
            double r = Length;
            if (r == 0) return new Vector32(0, 0, 0);
            else return new Vector32((float)(X / r), (float)(Y / r), (float)(Z / r));
        }
        // Dot product
        public double Dot(Vector32 p)
        {
            return X * p.X + Y * p.Y + Z * p.Z;
        }
        // Cross product
        public Vector32 Cross(Vector32 v)
        {
            return new Vector32(
            Y * v.Z - Z * v.Y,
            Z * v.X - X * v.Z,
            X * v.Y - Y * v.X);
        }
        static public double Dot(Vector32 p1, Vector32 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }
        static public Vector32 Cross(Vector32 v1, Vector32 v2)
        {
            return new Vector32(
            v1.Y * v2.Z - v1.Z * v2.Y,
            v1.Z * v2.X - v1.X * v2.Z,
            v1.X * v2.Y - v1.Y * v2.X);
        }
        //rotate on Counter clockwise        
        public Vector32 RotateOnAngle(double degree_x, double degree_y, double degree_z)
        {
            return Rotate(toRad(degree_x), toRad(degree_y), toRad(degree_z));
        }
        public Vector32 Rotate(double xa, double ya, double za)
        {
            double Sinx = Math.Sin(xa);
            double Siny = Math.Sin(ya);
            double Sinz = Math.Sin(za);
            double Cosx = Math.Cos(xa);
            double Cosy = Math.Cos(ya);
            double Cosz = Math.Cos(za);
            double x0 = X;
            double y0 = Y;
            double z0 = Z;
            double x1 = x0 * (Cosy * Cosz - Sinx * Siny * Sinz) - y0 * Cosx * Sinz + z0 * (Siny * Cosz + Sinx * Cosy * Sinz);
            double y1 = x0 * (Cosy * Sinz + Sinx * Siny * Cosz) + y0 * Cosx * Cosz + z0 * (Siny * Sinz - Sinx * Cosy * Cosz);
            double z1 = x0 * (-Cosx * Siny) + y0 * Sinx + z0 * Cosx * Cosy;

            X = (float)x1;
            Y = (float)y1;
            Z = (float)z1;
            return new Vector32((float)x1, (float)y1, (float)z1);
        }
        //axis == 0,1,2 ,x,y,z
        public Vector32 RotateOnAngle(double degress, int axis)
        {
            return Rotate(toRad(degress), axis);
        }
        public Vector32 Rotate(double angle, int axis)
        {
            float x0 = X;
            float y0 = Y;
            float z0 = Z;
            //rotate x
            if (axis == 0)
            {
                Y = (float)(y0 * Math.Cos(angle) - z0 * Math.Sin(angle));
                Z = (float)(y0 * Math.Sin(angle) + z0 * Math.Cos(angle));
            }
            //rotate y
            else if (axis == 1)
            {
                Z = (float)(z0 * Math.Cos(angle) - x0 * Math.Sin(angle));
                X = (float)(z0 * Math.Sin(angle) + x0 * Math.Cos(angle));
            }
            //rotate z
            else if (axis == 2)
            {
                X = (float)(x0 * Math.Cos(angle) - y0 * Math.Sin(angle));
                Y = (float)(x0 * Math.Sin(angle) + y0 * Math.Cos(angle));
            }
            return new Vector32(X, Y, Z);
        }

        /*! @param PIP Point-in-Plane */
        public static bool TestLineThruTriangle(Vector32 P1, Vector32 P2, Vector32 P3, Vector32 R1, Vector32 R2, out Vector32 PIP)
        {
            PIP = new Vector32(0, 0, 0);
            // Find Triangle Normal
            Vector32 Normal = Cross(P2 - P1, P3 - P1);
            Normal.Normalize(); // not really needed?  Vector3f does this with cross.

            // Find distance from LP1 and LP2 to the plane defined by the triangle
            double Dist1 = (R1 - P1).Dot(Normal);
            double Dist2 = (R2 - P1).Dot(Normal);

            if ((Dist1 * Dist2) >= 0.0f)
            {
                //SFLog(@"no cross"); 
                return false;
            } // line doesn't cross the triangle.

            if (Dist1 == Dist2)
            {
                //SFLog(@"parallel"); 
                return false;
            } // line and plane are parallel

            // Find point on the line that intersects with the plane
            Vector32 IntersectPos = R1 + (R2 - R1) * ( -Dist1 / (Dist2 - Dist1) );

            // Find if the interesection point lies inside the triangle by testing it against all edges            
            Vector32 vTest = Normal.Cross(P2 - P1);
            if (vTest.Dot(IntersectPos - P1) < 0.0f)
            {
                //SFLog(@"no intersect P2-P1"); 
                return false;
            }

            vTest = Normal.Cross(P3 - P2);
            if (vTest.Dot(IntersectPos - P2) < 0.0f)
            {
                //SFLog(@"no intersect P3-P2"); 
                return false;
            }

            vTest = Normal.Cross(P1 - P3);
            if (vTest.Dot(IntersectPos - P1) < 0.0f)
            {
                //SFLog(@"no intersect P1-P3"); 
                return false;
            }

            PIP = IntersectPos;

            return true;
        }
        //          p
        //  p1------------->p2
        //          p
        // return 0 ,on the line, otherwise on the side of the line
        public static int GetPointLineRelation(Vector32 p1, Vector32 p2, Vector32 p)
        {
            Vector32 p0 = Cross(p1 - p, p2 - p);
            double fz = p0.x + p.y + p0.z;
            if (fz >= -1e-6 && fz <= 1e-6)
            {
                return 0;      // on the line
            }
            else if (fz > 0)
            {
                return 1;      // on the left of the line
            }
            else
            {
                return -1;      // on the right of the line
            }
        }
        public static bool IsPointOnLine(Vector32 p, Vector32 p1, Vector32 p2, double err = 1.0E-6)
        {
            return Vector64.IsPointOnLine(p.toVector64(), p1.toVector64(), p2.toVector64(), err);
        }
        public static bool IsPointOnLine2D(Vector32 p, Vector32 p1, Vector32 p2)
        {
            double x = p.x;
            double y = p.y;
            double x1 = p1.x;
            double y1 = p1.y;
            double x2 = p2.x;
            double y2 = p2.y;
            if (x1 == x2 && x == x1) return true;
            else if (y1 == y2 && y == y1) return true;
            else
            {
                double k1 = (y2 - y1) / (x2 - x1);
                double k2 = (y - y1) / (x - x1);
                if (k1 == k2) return true;
                else return false;
            }
        }
        public static IntersectionType IsPointInTriangle(Vector32 p1, Vector32 p2, Vector32 p3, Vector32 p)
        {
            // on the corner points
            if (p == p1 || p == p2 || p == p3)
                return IntersectionType.corner;

            //if (!CTriangle3f.ValidTriangle(p1, p2, p3))
            //    return IntersectionType.triangle;

            Vector32 v0 = p3 - p1;
            Vector32 v1 = p2 - p1;
            Vector32 v2 = p - p1;

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            double u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1) // if u out of range, return directly
            {
                return IntersectionType.none;
            }

            double v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1) // if v out of range, return directly
            {
                return IntersectionType.none;
            }
            //intersected
            if (u + v <= 1)
            {
                if (Vector32.GetPointLineRelation(p1, p2, p) == 0)
                    return IntersectionType.line;
                if (Vector32.GetPointLineRelation(p2, p3, p) == 0)
                    return IntersectionType.line;
                if (Vector32.GetPointLineRelation(p1, p3, p) == 0)
                    return IntersectionType.line;

                return IntersectionType.triangle;
            }
            return IntersectionType.none;
        }
        public float R { get { return X; } set { X = value; } }
        public float G { get { return Y; } set { Y = value; } }
        public float B { get { return Z; } set { Z = value; } }
        public float A { get { return V; } set { V = value; } }
        public float W { get { return V; } set { V = value; } }

        public float x { get { return X; } set { X = value; } }
        public float y { get { return Y; } set { Y = value; } }
        public float z { get { return Z; } set { Z = value; } }
        public float v { get { return V; } set { V = value; } }
        public float w { get { return V; } set { V = value; } }

        public float left { get { return X; } set { X = value; } }
        public float top { get { return Y; } set { Y = value; } }
        public float right { get { return Z; } set { Z = value; } }
        public float bottom { get { return V; } set { V = value; } }
        public float minx { get { return X; } set { X = value; } }
        public float maxx { get { return Y; } set { Y = value; } }
        public float miny { get { return Z; } set { Z = value; } }
        public float maxy { get { return V; } set { V = value; } }
        public float[] XYZV
        {
            get
            { float[] xyz = new float[4];
                xyz[0] = X; xyz[1] = Y; xyz[2] = Z; xyz[3] = V;
                return xyz;
            } set { X = value[0]; Y = value[1]; Z = value[2]; V = value[3]; }
        }

    }

    /// <summary>
    /// vecter64
    /// </summary>
    public struct Vector64
    {
        public double X, Y, Z, V;
        public Vector64(double _x, double _y, double _z, double _v = 0)
        {
            X = _x;
            Y = _y;
            Z = _z;
            V = _v;
        }
        public Vector32 toVector32()
        {
            return new Vector32(x,y,z,v);
        }
        
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() +
                   Z.GetHashCode() ^ V.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Vector64))
            {
                return false;
            }
            Vector64 p = (Vector64)obj;
            return X.Equals(p.X) &&
                   Y.Equals(p.Y) &&
                   Z.Equals(p.Z) &&
                   V.Equals(p.V);
        }
        static public List<Vector32> toVector32List(List<Vector64> points)
        {
            List<Vector32> points32 = new List<Vector32>();
            foreach(Vector64 p in points)
            {
                points32.Add(p);
            }
            return points32;
        }
        /// <summary>
        /// 删除重复点，点距小于最大值的 * zerobase
        /// </summary>
        /// <param name="points"></param>
        /// <param name="zerobase">一个极小值，最大的点距的</param>
        /// <returns></returns>
        static public int RemoveDuplicated(ref List<Vector64> points, double zerobase = 1.0E-10)
        {
            if (points.Count < 2) return 0;

            double dist;
            Vector64 p,p1, p2;
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
                    if (dist <= err) del[j] = true;
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
        /// <summary>
        /// 删除线上相近点
        /// </summary>
        /// <param name="points"></param>
        /// <param name="ZeroBase">作为零值分母基数</param>
        /// <returns></returns>
        static public int RemoveLineDuplicated(ref List<Vector64> points, double ZeroBase = 0.001)
        {
            int n = points.Count;
            if (n < 2) return 0;

            //删除标记
            bool[] mark = new bool[n];
            for (int i = 0; i < n; i++) mark[i] = false;

            //计算总长,不计算实际长度，只计算|x1-x2|+|y2-y1|,避免开方
            Vector64 p1, p2;
            double sum = 0.0;
            double dist = 0.0;
            for (int i = 1; i < n; i++)
            {
                p1 = points[i - 1];
                p2 = points[i];
                dist = Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
                sum += dist;
            }

            //点距平均长度 
            double zero = ZeroBase * sum / (n - 1);
            for (int i = 1; i < n; i++)
            {
                p1 = points[i - 1];
                p2 = points[i];
                dist = Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
                if (dist <= zero) mark[i] = true;
            }

            int removed = 0;
            for (int i = n - 1; i > 0; i--)
            {
                if (mark[i])
                {
                    points.RemoveAt(i);
                    removed++;
                }
            }

            mark = null;

            return removed;
        }
        /// <summary>
        /// 去直线上冗余点
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /////-------------------
        static public int RemoveLineRedundant(ref List<Vector64> points)
        {
            if (points.Count < 3) return 0;

            Stack<Vector64> lists = new Stack<Vector64>();
            
            for (int i = points.Count - 1; i >= 0; i--) lists.Push(points[i]);

            int removed = 0;
            points.Clear();
            Vector64 p1 = lists.Pop();
            Vector64 p2 = lists.Pop();
            Vector64 p;

            points.Add(p1);

            while (lists.Count > 0)
            {
                p = lists.Pop();

                //p点是否在直线p1p2上
                if (IsPointOnLine2D(p, p1, p2))
                {
                    p2 = p; //忽略该点
                    removed++;
                }
                else
                {
                    points.Add(p2);
                    p1 = p2;
                    p2 = p;
                }
            }
            //last one
            points.Add(p2);
            return removed;
        }
        public static bool operator ==(Vector64 p1, Vector64 p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return (p1.X == p2.X) &&
                   (p1.Y == p2.Y) &&
                   (p1.Z == p2.Z) &&
                   (p1.V == p2.V);
        }
        public static bool operator !=(Vector64 p1, Vector64 p2)
        {
            return (!(p1 == p2));
        }
        public static Vector64 operator +(Vector64 p1, Vector64 p2)
        {
            return new Vector64(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z, p1.V + p2.V);
        }
        public static Vector64 operator -(Vector64 p1, Vector64 p2)
        {
            return new Vector64(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z, p1.V - p2.V);
        }
        public static Vector64 operator *(double s, Vector64 p1)
        {
            return new Vector64(p1.X * s, p1.Y * s, p1.Z * s, p1.V * s);
        }
        public static Vector64 operator *(Vector64 p1, double s)
        {
            return new Vector64(p1.X * s, p1.Y * s, p1.Z * s, p1.V * s);
        }
        public static Vector64 operator /(Vector64 p1, double s)
        {
            if (s == 0) return p1;
            else return new Vector64(p1.X / s, p1.Y / s, p1.Z / s, p1.V / s);
        }
        public static implicit operator Vector32(Vector64 v)
        {
            return new Vector32((float)v.X, (float)v.Y, (float)v.Z, (float)v.V);
        }
        public static implicit operator Vector64(Vector32 v)
        {
            return new Vector64(v.X, v.Y, v.Z, v.V);
        }
        //          p
        //  p1------------->p2
        //          p
        // return 0 ,on the line, otherwise on the side of the line
        public static int GetPointLineRelation(Vector64 p1, Vector64 p2, Vector64 p)
        {
            Vector64 p0 = Cross(p1 - p, p2 - p);
            double fz = p0.X + p.Y + p0.Z;
            if (fz >= -1e-6 && fz <= 1e-6)
            {
                return 0;      // on the line
            }
            else if (fz > 0)
            {
                return 1;      // on the left of the line
            }
            else
            {
                return -1;      // on the right of the line
            }
        }
        public static bool IsPointOnLine2D(Vector64 p, Vector64 p1, Vector64 p2)
        {
            double x = p.x;
            double y = p.y;
            double x1 = p1.x;
            double y1 = p1.y;
            double x2 = p2.x;
            double y2 = p2.y;
            if (x1 == x2 && x == x1) return true;
            else if (y1 == y2 && y == y1) return true;
            else
            {
                double k1 = (y2 - y1) / (x2 - x1);
                double k2 = (y - y1) / (x - x1);
                if (k1 == k2) return true;
                else return false;
            }
        }
        /// <summary>
        /// 点p在线段p1,p2上判别
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="err">最小误差</param>
        /// <returns></returns>
        public static bool IsPointOnLine(Vector64 p, Vector64 p1, Vector64 p2,double err = 1.0E-6)
        {
            Vector64 p0 = Cross(p1 - p, p2 - p);
            //double fz = p0.x + p.y + p0.z;
            double fz = p0.x + p0.y + p0.z;
            if ( Math.Abs(fz) <= err )
            {
                return true;      // on the line
            }            
            else return false;
        }
        public static IntersectionType IsPointInTriangle(Vector64 p1, Vector64 p2, Vector64 p3, Vector64 p)
        {
            // on the corner points
            if (p == p1 || p == p2 || p == p3)
                return IntersectionType.corner;

            Vector32 v0 = p3 - p1;
            Vector32 v1 = p2 - p1;
            Vector32 v2 = p - p1;

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            double u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1) // if u out of range, return directly
            {
                return IntersectionType.none;
            }

            double v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1) // if v out of range, return directly
            {
                return IntersectionType.none;
            }
            //intersected
            if (u + v <= 1)
            {
                if (Vector64.GetPointLineRelation(p1, p2, p) == 0)
                    return IntersectionType.line;
                if (Vector64.GetPointLineRelation(p2, p3, p) == 0)
                    return IntersectionType.line;
                if (Vector64.GetPointLineRelation(p1, p3, p) == 0)
                    return IntersectionType.line;

                return IntersectionType.triangle;
            }
            return IntersectionType.none;
        }
        //rotate on Contour Clock
        public Vector64 RotateOnAngle(double degree_x, double degree_y, double degree_z)
        {
            //return Rotate(toRad(degree_x), toRad(degree_y), toRad(degree_z));
            if (degree_x != 0) Rotate(toRad(degree_x), 0);
            if (degree_y != 0) Rotate(toRad(degree_y), 1);
            if (degree_z != 0) Rotate(toRad(degree_z), 2);
            return new Vector64(X, Y, Z);
        }
        public Vector64 Rotate(double xa, double ya, double za)
        {
            double Sinx = Math.Sin(xa);
            double Siny = Math.Sin(ya);
            double Sinz = Math.Sin(za);
            double Cosx = Math.Cos(xa);
            double Cosy = Math.Cos(ya);
            double Cosz = Math.Cos(za);
            double x0 = X;
            double y0 = Y;
            double z0 = Z;
            X = x0 * (Cosy * Cosz - Sinx * Siny * Sinz) - y0 * Cosx * Sinz + z0 * (Siny * Cosz + Sinx * Cosy * Sinz);
            Y = x0 * (Cosy * Sinz + Sinx * Siny * Cosz) + y0 * Cosx * Cosz + z0 * (Siny * Sinz - Sinx * Cosy * Cosz);
            Z = x0 * (-Cosx * Siny) + y0 * Sinx + z0 * Cosx * Cosy;
            return new Vector64(X, Y, Z);
        }
        //axis == 0,1,2 ,x,y,z
        public Vector64 RotateOnAngle(double degree, int axis)
        {
            return Rotate(toRad(degree), axis);
        }
        public Vector64 Rotate(double angle, int axis)
        {
            double x0 = X;
            double y0 = Y;
            double z0 = Z;
            double sina = Math.Sin(angle);
            double cosa = Math.Cos(angle);
            //rotate x
            if (axis == 0)
            {
                Y = y0 * cosa - z0 * sina;
                Z = y0 * sina + z0 * cosa;
            }
            //rotate y
            else if (axis == 1)
            {
                Z = z0 * cosa - x0 * sina;
                X = z0 * sina + x0 * cosa;
            }
            //rotate z
            else if (axis == 2)
            {
                X = x0 * cosa - y0 * sina;
                Y = x0 * sina + y0 * cosa;
            }
            return new Vector64(X, Y, Z);
        }
        public static Vector64 Normalize(Vector64 p)
        {
            double r = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
            if (r == 0) return new Vector64(0, 0, 0);
            else return new Vector64(p.X / r, p.Y / r, p.Z / r);
        }
        public Vector64 Normalize()
        {
            double r = Length;
            if (r == 0) return new Vector64(0, 0, 0);
            else return new Vector64(X / r, Y / r, Z / r);
        }
        public static double toAngle(double rad)
        {
            double angle = 180 * rad / Math.PI;
            int n = (int)(angle / 360);

            if (n > 0) angle = angle - n * 360;

            return angle;
        }
        public static double toRad(double angle)
        {
            double rad = angle * Math.PI / 180;

            int n = (int)(0.5 * rad / Math.PI);

            if (n > 0) rad = rad - n * 2 * Math.PI;

            return rad;
        }
        //return angle ,not rad
        public static double VectorAngle(Vector64 p1, Vector64 p2)
        {
            double m1 = p1.Length;
            double m2 = p2.Length;

            //no angle
            if (m1 == 0 || m2 == 0) return 0;

            double acos = Dot(p1, p2) / (m1 * m2);
            double angle = toAngle(Math.Acos(acos));

            Vector32 vp = Cross(p1, p2);
            if (p1.X == p2.X)//yz
            {
                //clock <0 , counter clockwise >0
                if (vp.X < 0) return -angle;
            }
            if (p1.Y == p2.Y)//yz
            {
                //clock <0 , counter clockwise >0
                if (vp.Y < 0) return -angle;
            }
            if (p1.Z == p2.Z)//yz
            {
                //clock <0 , counter clockwise >0
                if (vp.Z < 0) return -angle;
            }
            return angle;
        }
        public static double Distance(Vector64 p1, Vector64 p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) +
                                (p1.Y - p2.Y) * (p1.Y - p2.Y) +
                                (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }
        public double Distance(Vector64 p)
        {
            return Distance(this, p);
        }
        public static double GetLength(Vector64 p1)
        {
            return Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y + p1.Z * p1.Z);
        }
        public double Length
        {
            get{ return Math.Sqrt(X * X + Y * Y + Z * Z); }            
        }
        public double sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }
        public double Magnitude
        {
            get { return Math.Sqrt(x * x + y * y + z * z); }
        }
        // Dot product
        public double Dot(Vector64 p)
        {
            return X * p.X + Y * p.Y + Z * p.Z;
        }
        // Cross product
        public Vector64 Cross(Vector64 v)
        {
            return new Vector64(
            Y * v.Z - Z * v.Y,
            Z * v.X - X * v.Z,
            X * v.Y - Y * v.X);
        }
        static public double Dot(Vector64 p1, Vector64 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }
        static public Vector64 Cross(Vector64 v1, Vector64 v2)
        {
            return new Vector64(
            v1.Y * v2.Z - v1.Z * v2.Y,
            v1.Z * v2.X - v1.X * v2.Z,
            v1.X * v2.Y - v1.Y * v2.X);
        }
        public double R { get { return X; } set { X = value; } }
        public double G { get { return Y; } set { Y = value; } }
        public double B { get { return Z; } set { Z = value; } }
        public double A { get { return V; } set { V = value; } }
        public double W { get { return V; } set { V = value; } }
        public double x { get { return X; } set { X = value; } }
        public double y { get { return Y; } set { Y = value; } }
        public double z { get { return Z; } set { Z = value; } }
        public double v { get { return V; } set { V = value; } }
        public double w { get { return V; } set { V = value; } }
        public double left { get { return X; } set { X = value; } }
        public double top { get { return Y; } set { Y = value; } }
        public double right { get { return Z; } set { Z = value; } }
        public double bottom { get { return V; } set { V = value; } }
        public double minx { get { return X; } set { X = value; } }
        public double maxx { get { return Y; } set { Y = value; } }
        public double miny { get { return Z; } set { Z = value; } }
        public double maxy { get { return V; } set { V = value; } }
        public double[] XYZV
        {
            get
            {
                double[] xyz = new double[4];
                xyz[0] = X; xyz[1] = Y; xyz[2] = Z; xyz[3] = V;
                return xyz;
            }
            set { X = value[0]; Y = value[1]; Z = value[2]; V = value[3]; }
        }
    }
    //金字塔，四面体模型
    public class CPyramid
    {
        public Vector32 p; //顶点
        public Vector32 p1, p2, p3; //底面
        public bool[] pShowState;
        public ColorRGBA[] pColor;
        public CPyramid(Vector32 _p, Vector32 _p1, Vector32 _p2, Vector32 _p3)
        {
            p = _p;
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            pShowState = new bool[4];
            pColor = new ColorRGBA[4];
            for (int i = 0; i < 4; i++)
            {
                pShowState[i] = true;
                pColor[i] = new ColorRGBA(0, 0, 0);
            }
        }
    }
    public struct CTriangle2f
    {
        //x,y,z = 0
        public Vector32 p1, p2, p3;        
        public CTriangle2f(Vector32 _p1, Vector32 _p2, Vector32 _p3)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;            
        }
        public void GetRange(out double minx, out double miny, out double maxx, out double maxy)
        {
            minx = maxx = p1.x;
            miny = maxy = p1.y;
            if (p2.x < minx) minx = p2.x;
            if (p3.x < minx) minx = p3.x;
            if (p2.x > maxx) maxx = p2.x;
            if (p3.x > maxx) maxx = p3.x;
            if (p2.y < miny) miny = p2.y;
            if (p3.y < miny) miny = p3.y;
            if (p2.y > maxy) maxy = p2.y;
            if (p3.y > maxy) maxy = p3.y;
        }
        public bool IsPointInRange(Vector32 p)
        {
            double x1, y1, x2, y2;
            GetRange(out x1, out y1, out x2, out y2);
            if (p.x < x1 && p.x > x2) return false;
            if (p.y < y1 && p.y > y2) return false;
            return true;
        }
        
        public bool IsPointInTriangle(Vector32 p)
        {
            if (!IsPointInRange(p)) return false;

            Vector32 AC = p3 - p1;
            Vector32 AB = p2 - p1;
            Vector32 AP = p - p1; 
            double f_i = Vector32.Dot(AP, AC) * Vector32.Dot(AB, AB) - Vector32.Dot(AP, AB) * Vector32.Dot(AC, AB);
            double f_j = Vector32.Dot(AP, AB) * Vector32.Dot(AC, AC) - Vector32.Dot(AP, AC) * Vector32.Dot(AB, AC);
            double f_d = Vector32.Dot(AC, AC) * Vector32.Dot(AB, AB) - Vector32.Dot(AC, AB) * Vector32.Dot(AC, AB);
            //if (f_d < 0) { Debug.Log("erro f_d<0"); }
            //p_i==f_i/f_d
            //p_j==f_j/f_d
            if( f_i >= 0 && f_j >= 0 && f_i + f_j - f_d <= 0 )
                return true;
            else
                return false;
        }
        public static bool IsPointInTriangle(Vector32 p, Vector32 v1, Vector32 v2, Vector32 v3 )
        {
            Vector32 AC = v3 - v1;
            Vector32 AB = v2 - v1;
            Vector32 AP = p - v1;
            double f_i = Vector32.Dot(AP, AC) * Vector32.Dot(AB, AB) - Vector32.Dot(AP, AB) * Vector32.Dot(AC, AB);
            double f_j = Vector32.Dot(AP, AB) * Vector32.Dot(AC, AC) - Vector32.Dot(AP, AC) * Vector32.Dot(AB, AC);
            double f_d = Vector32.Dot(AC, AC) * Vector32.Dot(AB, AB) - Vector32.Dot(AC, AB) * Vector32.Dot(AC, AB);
            //if (f_d < 0) { Debug.Log("erro f_d<0"); }
            //p_i==f_i/f_d
            //p_j==f_j/f_d
            if (f_i >= 0 && f_j >= 0 && f_i + f_j - f_d <= 0)
                return true;
            else
                return false;
        }
        /*https://www.cnblogs.com/kyokuhuang/p/4314173.html
         * 
        //附上一个判断四点共面的算法，这里四个点不能有浮点精度误差，，否则算法不成立。
        static public bool pointInTrianglePlane(Vector32 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 pa = a - p;
            Vector3 pb = b - p;
            Vector3 pc = c - p;

            Vector3 normal1 = Vector3.Cross(pa, pb);
            Vector3 normal2 = Vector3.Cross(pa, pc);
            Vector3 result = Vector3.Cross(normal1, normal2);
            //证明：若pab平面的法向量平行于pac平面的法向量，则说明平面pab和pac平行或重合，
            //且p点为两平面公共点，所以pab、pac平面重合，pabc四点共面。
            if (result == Vector3.zero)
            {
                Debug.Log(result);
                return true;
            }
            else
                return false;
        }
        public bool IsPointInTriangle(Vector32 point)
        {
            int i;
            int j = points.Count - 1;
            bool oddNodes = false;

            for (i = 0; i < points.Count; i++)
            {
                if ((points[i].Y < y && points[j].Y >= y || points[j].Y < y && points[i].Y >= y)
                    && (points[i].X <= x || points[j].X <= x))
                {
                    if (points[i].X + (y - points[i].Y) / (points[j].Y - points[i].Y) * (points[j].X - points[i].X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }
            return oddNodes;
        }
        */
    }
    public struct CTriangle3f
    {
        public Vector32 p1, p2, p3;
        public ColorRGBA[] pColor;
        public Vector32 pNormal;
        public bool bIsConverted;
        public void SetColor(int id, ColorRGBA c)
        {
            pColor[id] = c;
        }
        public CTriangle3f(Vector32 _p1, Vector32 _p2, Vector32 _p3)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            pColor = new ColorRGBA[3];
            pNormal = new Vector32(0, 0, 0);
            bIsConverted = false;
        }
        //point on the side of triangle face
        // > 0 positive side(right hand rule) 
        // < 0 negtive side(right hand rule)
        // = 0 on the face of triangle
        public int GetRelationOfPoint(Vector32 p)
        {
            Vector32 p32 = p2 - p3;
            Vector32 p31 = p1 - p3;
            Vector32 p01 = Vector32.Cross(p32, p31);
            Vector32 p02 = p - p3;
            double dd = Math.Round(Vector32.Dot(p01, p02),10);
            if (dd > 0) return 1;
            else if (dd < 0) return -1;
            else return 0;            
        }
        /// <summary>
        /// point in triangle judgment
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public bool IsPointInTriangle(Vector32 P)
        {
            Vector32 v0 = p3 - p1;
            Vector32 v1 = p2 - p1;
            Vector32 v2 = P - p1;

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            double u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1) // if u out of range, return directly
            {
                return false;
            }

            double v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1) // if v out of range, return directly
            {
                return false;
            }
            return u + v <= 1;
        }
        //point in triangle judgment and return their relationship
        //point and triangle relation,triangle must be valid
        public IntersectionType GetPointTriangleType(Vector32 p)
        {
            // on the corner points
            if (p == p1 || p == p2 || p == p3)
                return IntersectionType.corner;

            Vector32 v0 = p3 - p1;
            Vector32 v1 = p2 - p1;
            Vector32 v2 = p - p1;

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            double u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1) // if u out of range, return directly
            {
                return IntersectionType.none;
            }

            double v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1) // if v out of range, return directly
            {
                return IntersectionType.none;
            }
            //intersected
            if (u + v <= 1)
            {
                if (Vector32.IsPointOnLine(p1, p2, p)) return IntersectionType.line;
                if (Vector32.IsPointOnLine(p2, p3, p)) return IntersectionType.line;
                if (Vector32.IsPointOnLine(p1, p3, p)) return IntersectionType.line;

                return IntersectionType.triangle;
            }
            return IntersectionType.none;
        }

        /// <summary>
        /// point intersection status
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>

        public IntersectionType GetIntersectionType(Vector32 P)
        {
            // on the corner points
            if (P == p1 || P == p2 || P == p3)
                return IntersectionType.corner;

            Vector32 v0 = p3 - p1;
            Vector32 v1 = p2 - p1;
            Vector32 v2 = P - p1;

            double dot00 = v0.Dot(v0);
            double dot01 = v0.Dot(v1);
            double dot02 = v0.Dot(v2);
            double dot11 = v1.Dot(v1);
            double dot12 = v1.Dot(v2);

            double inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            double u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1) // if u out of range, return directly
            {
                return IntersectionType.none;
            }

            double v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1) // if v out of range, return directly
            {
                return IntersectionType.none;
            }
            //intersected
            if (u + v <= 1)
            {
                if (Vector32.GetPointLineRelation(p1, p2, P) == 0)
                    return IntersectionType.line;
                if (Vector32.GetPointLineRelation(p2, p3, P) == 0)
                    return IntersectionType.line;
                if (Vector32.GetPointLineRelation(p1, p3, P) == 0)
                    return IntersectionType.line;

                return IntersectionType.triangle;
            }
            return IntersectionType.none;
        }
        public void GetRange(out double _minx, out double _maxx,
                             out double _miny, out double _maxy,
                             out double _minz, out double _maxz)
        {
            //get triangle range            
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
        }
        //direct == 0 , x direction v1.x < v2.x, v1.y == v2.y, v1.z == v2.z
        //direct == 1 , y direction v1.y < v2.y, ...
        //direct == 2 , z direction v1.z < v2.z, ...
        public IntersectionType CheckLineCrossTriangle(Vector32 v1, Vector32 v2, int direct)
        {
            //triangle out of the range of line h1---h2 
            double minx, maxx, miny, maxy, minz, maxz;
            GetRange(out minx, out maxx, out miny, out maxy, out minz, out maxz);
            Vector32 sect = new Vector32(0, 0, 0);
            switch (direct)
            {
                case 0://x direction
                    if (maxx < v1.X || minx > v2.X) return IntersectionType.none;
                    if (v1.Y < miny || v1.Y > maxy) return IntersectionType.none;
                    if (v1.Z < minz || v1.Z > maxz) return IntersectionType.none;
                    //triangle project to yz plane
                    // p1.X = p2.X = p3.X = 0;
                    // v1.X = v2.X = 0;
                    // it would be a line on OYZ
                    if (p1.Y == p2.Y && p1.Z == p2.Z)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p3))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    if (p2.Y == p3.Y && p2.Z == p3.Z)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p2))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    if (p1.Y == p3.Y && p1.Z == p3.Z)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p2))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    //return GetPointTriangleType(v1);
                    if (GetIntersectionOnTriangle(v1, v2, out sect))
                        return IntersectionType.triangle;
                    break;
                case 1://y direction
                    if (maxy < v1.Y || miny > v2.Y) return IntersectionType.none;
                    if (v1.X < minx || v1.X > maxx) return IntersectionType.none;
                    if (v1.Z < minz || v1.Z > maxz) return IntersectionType.none;
                    //triangle project to yz plane
                    //p1.Y = p2.Y = p3.Y = 0;
                    // v1.Y = v2.Y = 0;
                    // it would be a line on OYZ
                    if (p1.X == p2.X && p1.Z == p2.Z)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p3))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    if (p2.X == p3.X && p2.Z == p3.Z)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p2))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    if (p1.X == p3.X && p1.Z == p3.Z)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p2))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    //return GetPointTriangleType(v1);
                    if (GetIntersectionOnTriangle(v1, v2, out sect))
                        return IntersectionType.triangle;
                    break;
                case 2://z direction
                    if (maxz < v1.Z || minz > v2.Z) return IntersectionType.none;
                    if (v1.Y < miny || v1.Y > maxy) return IntersectionType.none;
                    if (v1.X < minx || v1.X > maxx) return IntersectionType.none;
                    //triangle project to yz plane
                    // p1.Z = p2.Z = p3.Z = 0;
                    // v1.Z = v2.Z = 0;
                    // it would be a line on OYZ
                    if (p1.Y == p2.Y && p1.X == p2.X)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p3))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    if (p2.Y == p3.Y && p2.X == p3.X)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p2))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    if (p1.Y == p3.Y && p1.X == p3.X)
                    {
                        if (Vector32.IsPointOnLine(v1, p1, p2))
                            return IntersectionType.line;
                        else return IntersectionType.none;
                    }
                    //return GetPointTriangleType(v1);
                    if (GetIntersectionOnTriangle(v1, v2, out sect))
                        return IntersectionType.triangle;
                    break;
            }
            return IntersectionType.none;
        }
        // Determine whether a ray intersect with a triangle
        // Parameters
        // orig: origin of the ray
        // dir: direction of the ray
        // v0, v1, v2: vertices of triangle
        // t(out): weight of the intersection for the ray
        // u(out), v(out): barycentric coordinate of intersection
        public bool GetIntersectionOnTriangle(Vector32 v1, Vector32 v2, out Vector32 intersect)
        {
            double t, u, v;
            Vector32 orig = v1;
            Vector32 dir = v2 - v1;
            // E1
            Vector32 E1 = p2 - p1;
            // E2
            Vector32 E2 = p3 - p1;
            // P
            Vector32 P = dir.Cross(E2);
            // determinant
            double det = E1.Dot(P);
            // keep det > 0, modify T accordingly
            Vector32 T;
            if (det > 0)
            {
                T = orig - p1;
            }
            else
            {
                T = p1 - orig;
                det = -det;
            }

            // If determinant is near zero, ray lies in plane of triangle
            intersect = new Vector32(orig.x, orig.y, orig.z);
            if (det <= 1.0e-6) return false;

            // Calculate u and make sure u <= 1
            u = T.Dot(P);
            if (u < 0.0f || u > det) return false;

            // Q
            Vector32 Q = T.Cross(E1);

            // Calculate v and make sure u + v <= 1
            v = dir.Dot(Q);
            if (v < 0.0f || u + v > det) return false;

            // Calculate t, scale parameters, ray intersects triangle
            t = E2.Dot(Q);

            double fInvDet = 1.0f / det;
            t *= fInvDet;
            //u *= fInvDet;
            //v *= fInvDet;

            intersect = orig + (float)t * dir;

            //out of line v1,v2
            if ( t < 0 || t > 1.0000001 ) return false;
            else  return true;
        }

        /*
        /// <summary>
        /// Get Intersections of line v1v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="tri"></param>
        /// <param name="intersect"></param>
        /// <returns></returns>
        public bool GetIntersectionOnTriangle(Vector32 v1, Vector32 v2,Vector32 pp, out Vector32 intersect)
        {
            double minx, maxx, miny, maxy, minz, maxz;
            GetRange(out minx, out maxx, out miny, out maxy, out minz, out maxz);
            intersect = new Vector32(0, 0, 0);

            double x1, y1, z1, x2, y2, z2;
            x1 = x2 = v1.X;
            y1 = y2 = v1.Y;
            z1 = z2 = v1.Z;
            if (v2.X < x1) x1 = v2.X;
            if (v2.X > x2) x2 = v2.X;

            if (v2.Y < y1) y1 = v2.Y;
            if (v2.Y > y2) y2 = v2.Y;

            if (v2.Z < z1) z1 = v2.Z;
            if (v2.Z > z2) z2 = v2.Z;

            //triangle at the range of v1,v2
            if (minx > x2 || maxx < x1) return false;
            if (miny > y2 || maxy < y1) return false;
            if (minz > z2 || maxz < z1) return false;

           // bool ret = Vector32.TestLineThruTriangle(p[0], p[1], p[2], v1, v2, out intersect);

           // p = null;

            return false;

        }
        */
        // to check triangle is valid, return distinct point num
        // 1 - 1 are same point, 2- 2 points are same,3 -- 3 are valid 
        static public bool ValidTriangle(Vector32 _p1, Vector32 _p2, Vector32 _p3)
        {
            if (Vector32.GetPointLineRelation(_p1, _p2, _p3) == 0)
                return false;
            else return true;
        }
        public bool ValidTriangle()
        {
            return ValidTriangle(p1, p2, p3);
        }

        /////////////////////////////////////////
        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// </summary>

        //定义空间向量结构
        struct SpaceVector
        {
            public float m;
            public float n;
            public float p;
        };

        //利用海伦公式求变成为a,b,c的三角形的面积
        float Area(float a, float b, float c)
        {
            float s = (a + b + c) / 2;
            return (float)Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        int GetLineIntersection(Vector32 start, Vector32 end, out Vector32 CrossPoint)
        {
            SpaceVector LineV = new SpaceVector();
            Vector32 line = end - start;
            LineV.m = line.X;
            LineV.n = line.Y;
            LineV.p = line.Z;
            //三角形所在平面的法向量
            SpaceVector TriangleV;
            //三角形的边方向向量
            SpaceVector VP12, VP13;
            //直线与平面的交点
            CrossPoint = new Vector32(0, 0, 0);
            //平面方程常数项
            float TriD;

            /*-------计算平面的法向量及常数项-------*/
            //point1->point2
            VP12.m = p2.x - p1.x;
            VP12.n = p2.y - p1.y;
            VP12.p = p2.z - p1.z;
            //point1->point3
            VP13.m = p3.x - p1.x;
            VP13.n = p3.y - p1.y;
            VP13.p = p3.z - p1.z;
            //VP12xVP13
            TriangleV.m = VP12.n * VP13.p - VP12.p * VP13.n;
            TriangleV.n = -(VP12.m * VP13.p - VP12.p * VP13.m);
            TriangleV.p = VP12.m * VP13.n - VP12.n * VP13.m;
            //计算常数项
            TriD = -(TriangleV.m * p1.x + TriangleV.n * p1.y + TriangleV.p * p1.z);

            /*-------求解直线与平面的交点坐标---------*/
            /* 思路：
             *     首先将直线方程转换为参数方程形式，然后代入平面方程，求得参数t，
             * 将t代入直线的参数方程即可求出交点坐标
            */
            float tempU, tempD;  //临时变量
            tempU = TriangleV.m * start.x + TriangleV.n * start.y + TriangleV.p * start.z + TriD;
            tempD = TriangleV.m * LineV.m + TriangleV.n * LineV.n + TriangleV.p * LineV.p;
            //直线与平面平行或在平面上
            if (tempD == 0.0)
            {
                return 0;
            }

            //计算参数t
            float t = -tempU / tempD;

            //计算交点坐标
            CrossPoint.x = LineV.m * t + start.x;
            CrossPoint.y = LineV.n * t + start.y;
            CrossPoint.z = LineV.p * t + start.z;

            /*----------判断交点是否在三角形内部---------*/
            //计算三角形三条边的长度
            float d12 = (float)Vector32.Distance(p1, p2);
            float d13 = (float)Vector32.Distance(p1, p3);
            float d23 = (float)Vector32.Distance(p2, p3);
            //计算交点到三个顶点的长度
            float c1 = (float)Vector32.Distance(CrossPoint, p1);
            float c2 = (float)Vector32.Distance(CrossPoint, p2);
            float c3 = (float)Vector32.Distance(CrossPoint, p3);
            //求三角形及子三角形的面积
            float areaD = Area(d12, d13, d23);  //三角形面积
            float area1 = Area(c1, c2, d12);    //子三角形1
            float area2 = Area(c1, c3, d13);    //子三角形2
            float area3 = Area(c2, c3, d23);    //子三角形3

            //根据面积判断点是否在三角形内部
            if (Math.Abs(area1 + area2 + area3 - areaD) > 0.0001)
            {
                //printf("There is no valid point of intersection\n");
                return 0;
            }

            //printf("(%f, %f, %f)\n", CrossPoint.x, CrossPoint.y, CrossPoint.z);
            return 1;
        }
        
        public bool IsPointInRange(Vector32 p)
        {
            double x1, y1, x2, y2,z1,z2;

            GetRange(out x1, out x2, out y1, out y2, out z1, out z2);

            if (p.x < x1 && p.x > x2) return false;
            if (p.y < y1 && p.y > y2) return false;
            if (p.z < z1 && p.z > z2) return false;

            return true;
        }
        //right hand rules
        //return 0  -- point on the triangle
        //return 1  -- point on above triangle plan(front side) 
        //return -1 -- point on below triangle plan(back side)
        public int PointTriangleRelation(Vector32 p)
        {
            Vector32 p21 = p1 - p2;
            Vector32 p23 = p3 - p2;
            Vector32 p0 = Vector32.Cross(p21, p23);
            double dot = Vector32.Dot(p0, p);
            if (dot >= 1.0e-10 && dot <= 1.0e10) return 0;
            else if (dot < 0) return -1;
            else return 1;            
        }
    }
    
}
