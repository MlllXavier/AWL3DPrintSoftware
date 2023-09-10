using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    [Serializable]
    public class Point3D
    {
        public float X;
        public float Y;
        public float Z;
        public int mode;    //移动模式：0（只移动）、1（边移动边打印）

        public Point3D()
        {
            mode = 0;
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Point3D(float x, float y, float z, int mode)
        {
            X = x;
            Y = y;
            Z = z;
            this.mode = mode;
        }

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float dot(Point3D point)
        {
            return X * point.X + Y * point.Y + Z * point.Z;
        }

        public Point3D cross(Point3D point)
        {
            return new Point3D(Y * point.Z - Z * point.Y, Z * point.X - X * point.Z, X * point.Y - Y * point.X);
        }

        public float distance(Point3D point)
        {
            return (float)Math.Sqrt(Math.Pow((X - point.X), 2) + Math.Pow((Y - point.Y), 2) + Math.Pow((Z - point.Z), 2));
        }

        public bool isClose(Point3D point)
        {
            double d = Math.Sqrt(Math.Pow((X - point.X), 2) + Math.Pow((Y - point.Y), 2) + Math.Pow((Z - point.Z), 2));
            return d < 0.0001;
        }

        public override bool Equals(object obj)
        {
            return obj is Point3D d &&
                   X == d.X &&
                   Y == d.Y &&
                   Z == d.Z;
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static Point3D operator +(Point3D a, Point3D b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3D operator -(Point3D a, Point3D b)
        {
            return new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point3D operator *(Point3D a, float b)
        {
            return new Point3D(a.X * b, a.Y * b, a.Z * b);
        }

        public static Point3D operator /(Point3D a, float b)
        {
            return new Point3D(a.X / b, a.Y / b, a.Z / b);
        }
    }
}
