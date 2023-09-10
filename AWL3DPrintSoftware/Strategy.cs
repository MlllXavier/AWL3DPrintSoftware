using SharpGL.SceneGraph.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    public interface SliceStrategy
    {
        List<Line3D>[] LayerSlicing(Model3D model, float maxHeight, float minHeight);
    }

    public class IsothickSliceStrategy : SliceStrategy
    {
        public List<Line3D>[] LayerSlicing(Model3D model, float maxHeight, float minHeight)
        {
            Triangle3D[] triangles = model.triangles.ToArray();
            float[] layersHeights = new float[(int)((model.modelHeight * model.multiple + model.moveZ) / maxHeight) + 1];
            for (int i = 0; i < layersHeights.Length + model.moveZ; i++)
            {
                layersHeights[i] = minHeight + i * maxHeight;
            }
            List<Line3D>[] layers = new List<Line3D>[layersHeights.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = new List<Line3D>();
            }
            for (int i = 0; i < triangles.Length; i++)
            {
                Utilities.getTriangleIntersectionLines(triangles[i], layersHeights, layers);
            }
            return layers;
        }
    }

    public class AdaptiveSliceStrategy : SliceStrategy
    {
        public List<Line3D>[] LayerSlicing(Model3D model, float maxHeight, float minHeight)
        {
            Triangle3D[] triangles = model.triangles.ToArray();
            float[] layersHeights = new float[(int)(model.modelHeight / maxHeight) + 1];
            for (int i = 0; i < layersHeights.Length; i++)
            {
                layersHeights[i] = minHeight + i * maxHeight;
            }
            for (int i = 0; i < triangles.Length; i++)
            {
                float[] result = Utilities.ModifyLayerHeight(triangles[i], layersHeights, maxHeight, minHeight, 0.1f);
                if (result.Length > 0) layersHeights = result;
            }
            List<Line3D>[] layers = new List<Line3D>[layersHeights.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = new List<Line3D>();
            }
            for (int i = 0; i < triangles.Length; i++)
            {
                Utilities.getTriangleIntersectionLines(triangles[i], layersHeights, layers);
            }
            return layers;
        }
    }

    public class SliceContext
    {
        private SliceStrategy strategy;

        public SliceContext(SliceStrategy strategy)
        {
            this.strategy = strategy;
        }

        public List<Line3D>[] executeStrategy(Model3D model, float maxHeight, float minHeight)
        {
            return strategy.LayerSlicing(model, maxHeight, minHeight);
        }
    }

    public interface FillStrategy
    {
        List<Point3D> PathFilling(List<Line3D> layer, float fillInterval, int mode);
    }

    public class SimpleFillStrategy : FillStrategy
    {
        public List<Point3D> PathFilling(List<Line3D> layer, float fillInterval, int mode)
        {
            List<Point3D> points = new List<Point3D>();
            if (layer.Count == 0) return points;
            Polygon3D polygon = Utilities.LinesToPolygons(layer)[0];//根据层数据获得第一个多边形
            if (polygon.points.Count() > 0)
            {
                Point3D tempPoint = polygon.points[0];
                points.Add(new Point3D(tempPoint.X, tempPoint.Y, tempPoint.Z, 0));
                for (int j = 1; j < polygon.points.Count(); j++)
                {
                    tempPoint = polygon.points[j];
                    points.Add(new Point3D(tempPoint.X, tempPoint.Y, tempPoint.Z, 1));
                }
            }
            //获取填充路径
            List<Line3D> line3Ds = Utilities.FillPolygon(polygon, fillInterval, mode);
            for (int j = 0; j < line3Ds.Count; j++)
            {
                Point3D start = line3Ds[j].start;
                points.Add(start);
                Point3D end = line3Ds[j].end;
                end.mode = 1;//设置从start到end是移动中打印模式
                points.Add(end);
            }
            return points;
        }
    }

    public class ComplexFillStrategy : FillStrategy
    {
        public List<Point3D> PathFilling(List<Line3D> layer, float fillInterval, int mode)
        {
            List<Point3D> points = new List<Point3D>();
            List<Polygon3D> polygons = Utilities.LinesToPolygons(layer);//根据层数据获得多边形
            //凹多边形凸分解
            List<Polygon3D> afterCut = new List<Polygon3D>();
            foreach (Polygon3D polygon in polygons)
            {
                double d = 0;//判断多边形是否为顺时针
                for (int j = 0; j < polygon.points.Count - 1; j++)
                {
                    d += -0.5 * (polygon.points[j + 1].Y + polygon.points[j].Y) * (polygon.points[j + 1].X - polygon.points[j].X);
                }
                if (d < 0) polygon.points.Reverse();
                List<Polygon3D> temp = Utilities.CutPolygon(polygon);
                foreach (Polygon3D poly in temp)
                {
                    afterCut.Add(poly);
                }
            }
            polygons = afterCut;
            //轮廓偏置
            foreach (Polygon3D polygon in polygons)
            {
                List<Polygon3D> afterOffset = new List<Polygon3D>();
                List<Polygon3D> temp = Utilities.OffsetPolygon(polygon, fillInterval, 2);//对轮廓进行偏置
                foreach (Polygon3D poly in temp)
                {
                    afterOffset.Add(poly);
                }
                //形成打印路径
                for (int j = 0; j < afterOffset.Count - 1; j++)
                {
                    List<Point3D> point3Ds = afterOffset[j].points;
                    for (int k = 0; k < point3Ds.Count; k++)
                    {
                        Point3D tempPoi = new Point3D(point3Ds[k].X, point3Ds[k].Y, point3Ds[k].Z, 1);
                        if (k == 0) tempPoi.mode = 0;
                        points.Add(tempPoi);
                    }
                }
                //对最后一个轮廓进行填充
                List<Line3D> line3DTemp = Utilities.FillPolygon(afterOffset[afterOffset.Count - 1], fillInterval, mode);
                foreach (Line3D line in line3DTemp)
                {
                    Point3D tempSta = new Point3D(line.start.X, line.start.Y, line.start.Z, 0);
                    Point3D tempEnd = new Point3D(line.end.X, line.end.Y, line.end.Z, 1);
                    points.Add(tempSta);
                    points.Add(tempEnd);
                }
            }
            return points;
        }
    }

    public class FillContext
    {
        private FillStrategy strategy;

        public FillContext(FillStrategy strategy)
        {
            this.strategy = strategy;
        }

        public List<Point3D> executeStrategy(List<Line3D> layer, float fillInterval, int mode)
        {
            return strategy.PathFilling(layer, fillInterval, mode);
        }
    }
}
