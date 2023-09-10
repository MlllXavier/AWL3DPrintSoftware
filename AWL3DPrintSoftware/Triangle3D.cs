using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    [Serializable]
    public class Triangle3D
    {
        public Point3D normal;
        public Point3D p1;
        public Point3D p2;
        public Point3D p3;
        public float maxH;
        public float minH;

        public Triangle3D()
        {
            normal = new Point3D();
            p1 = new Point3D();
            p2 = new Point3D();
            p3 = new Point3D();
        }
    }
}
