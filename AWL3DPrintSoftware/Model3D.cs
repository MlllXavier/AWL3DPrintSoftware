using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    [Serializable]
    public class Model3D
    {
        public string path = "";

        public float moveX = 0, moveY = 0, moveZ = 0, multiple = 1, modelHeight = 0;

        public List<Triangle3D> triangles = new List<Triangle3D>();

        public Triangle3D[] triangle3Ds;

        public List<Line3D>[] layers = new List<Line3D>[0];
    }
}
