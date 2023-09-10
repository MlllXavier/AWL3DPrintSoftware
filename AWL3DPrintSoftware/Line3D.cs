using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    [Serializable]
    public class Line3D
    {
        public Point3D start;
        public Point3D end;

        public Line3D()
        {
            start = new Point3D();
            end = new Point3D();
        }

        public Line3D(Point3D start, Point3D end)
        {
            this.start = start;
            this.end = end;
        }
    }
}
