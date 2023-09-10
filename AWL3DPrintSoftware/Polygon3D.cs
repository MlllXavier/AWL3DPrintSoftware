using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    public class Polygon3D
    {
        public List<Point3D> points = new List<Point3D>();

        public float minX, maxX, minY, maxY;//多边形的最大最小长宽
    }
}
