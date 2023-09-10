using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AWL3DPrintSoftware
{
    class Utilities
    {
        public static double currentToPressure(double current)//电流值转气压值
        {
            return (current * 0.04987 - 0.1841);
        }

        public static double pressureTocurrent(double pressure)//气压值转电流值
        {
            return (pressure + 0.1841) / 0.04987;
        }

        public static UploadInfo hexStrToInfo(string str)//将16进制的帧字符串转换为UploadInfo
        {
            string[] tempData = str.Split(' ');

            int[] intData = new int[tempData.Length - 3];
            for (int i = 0; i < tempData.Length - 3; i++)
            {
                intData[i] = ushort.Parse(Convert.ToInt32(tempData[i], 16).ToString());
            }

            UploadInfo info = new UploadInfo();
            info.CurChannelNum = int.Parse(tempData[1], System.Globalization.NumberStyles.HexNumber);
            info.xLocation = int.Parse(tempData[5] + tempData[4] + tempData[3] + tempData[2], System.Globalization.NumberStyles.HexNumber);
            info.yLocation = int.Parse(tempData[9] + tempData[8] + tempData[7] + tempData[6], System.Globalization.NumberStyles.HexNumber);
            info.zLocation = int.Parse(tempData[13] + tempData[12] + tempData[11] + tempData[10], System.Globalization.NumberStyles.HexNumber);
            info.sysStatus = tempData[17];//14、15、16没用上
            info.Speed = int.Parse(tempData[19] + tempData[18], System.Globalization.NumberStyles.HexNumber);
            info.OpenTime = int.Parse(tempData[21] + tempData[20], System.Globalization.NumberStyles.HexNumber);
            info.PrintDistance = int.Parse(tempData[23] + tempData[22], System.Globalization.NumberStyles.HexNumber);
            info.AirPress = int.Parse(tempData[25] + tempData[24], System.Globalization.NumberStyles.HexNumber);
            info.Temperature = int.Parse(tempData[27] + tempData[26], System.Globalization.NumberStyles.HexNumber);
            info.crcByte = tempData[29] + tempData[28];

            int tempCrc = Convert.ToInt32(info.crcByte, 16);
            int crcResult = Crc16_Calc(intData);

            return info;
        }

        public static string pointToGCode(string head, double x, double y, double z)//将坐标转换为G指令
        {
            return head + "X" + x.ToString("0.0") + "Y" + y.ToString("0.0") + "Z" + z.ToString("0.0") + "E";
        }

        static int[] auchCRCHi =
{
0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,
0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
0x40,
};

        static int[] auchCRCLo =
        {
0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4,
0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD,
0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7,
0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE,
0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2,
0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB,
0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 0x50, 0x90, 0x91,
0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88,
0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80,
0x40
};

        public static int Crc16_Calc(int[] updata)//crc循环冗余算法
        {
            int uchCRCHi = 0xff;
            int uchCRCLo = 0xff;
            for (int i = 0; i < updata.Length; i++)
            {
                int index = uchCRCHi ^ updata[i];
                uchCRCHi = uchCRCLo ^ auchCRCHi[index];
                uchCRCLo = auchCRCLo[index];
            }
            return uchCRCHi << 8 | uchCRCLo;
        }

        public static void SaveObject(string path, object obj)//保存对象到文件路径
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                Stream stream = new FileStream(@"" + path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, obj);
                stream.Close();
                MessageBox.Show("文件保存成功!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n文件保存失败!");
            }
        }

        public static T ReadObject<T>(string path)//从文件路径读取对象
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(@"" + path, FileMode.Open, FileAccess.Read, FileShare.None);
                T myObj = (T)formatter.Deserialize(stream);
                stream.Close();
                return myObj;
            }
            catch (Exception)
            {
                if (File.Exists(path))//如果文件不存在，创建文件
                {
                }
                else
                {
                    File.Create(path).Dispose();
                }
            }
            //catch 

            T t = default(T);
            return t;
        }

        public static bool ReadSTLFile(string filename, Model3D model)//读取STL文件
        {
            BinaryReader br = new BinaryReader(File.OpenRead(filename));
            byte[] data = br.ReadBytes(80);//文件头
            int count = (int)br.ReadUInt32();//三角面片数量
            if (br.BaseStream.Length != count * 50 + 84)
            {
                return ReadASCIISTLFile(filename, model);
            }
            return ReadBinarySTLFile(filename, model);
        }

        public static bool ReadBinarySTLFile(string filename, Model3D model)//读取STL文件，文件类型是二进制文件
        {
            model.triangles = new List<Triangle3D>();
            if (filename == null) return false;
            BinaryReader br = new BinaryReader(File.OpenRead(filename));
            byte[] data = br.ReadBytes(80);//文件头
            int count = (int)br.ReadUInt32();//三角面片数量
            if (br.BaseStream.Length != count * 50 + 84)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                Triangle3D triangle = new Triangle3D();
                for (int j = 0; j < 4; j++)
                {
                    float x = br.ReadSingle();
                    float y = br.ReadSingle();
                    float z = br.ReadSingle();
                    if (j == 0)
                    {
                        triangle.normal = new Point3D(x, y, z);
                        continue;
                    }
                    x = x * model.multiple + model.moveX;
                    y = y * model.multiple + model.moveY;
                    z = z * model.multiple + model.moveZ;
                    if (model.modelHeight < z)
                    {
                        model.modelHeight = z;
                    }
                    switch (j)
                    {
                        case 1:
                            triangle.p1 = new Point3D(x, y, z);
                            break;
                        case 2:
                            triangle.p2 = new Point3D(x, y, z);
                            break;
                        case 3:
                            triangle.p3 = new Point3D(x, y, z);
                            break;
                    }
                }
                triangle.maxH = triangle.p1.Z;
                triangle.minH = triangle.p1.Z;
                if (triangle.p2.Z > triangle.maxH)
                {
                    triangle.maxH = triangle.p2.Z;
                }
                if (triangle.p2.Z < triangle.minH)
                {
                    triangle.minH = triangle.p2.Z;
                }
                if (triangle.p3.Z > triangle.maxH)
                {
                    triangle.maxH = triangle.p3.Z;
                }
                if (triangle.p3.Z < triangle.minH)
                {
                    triangle.minH = triangle.p3.Z;
                }
                model.triangles.Add(triangle);
                br.ReadUInt16();
            }
            return true;
        }

        public static bool ReadASCIISTLFile(string filename, Model3D model)//读取STL文件，文件类型是ASCII
        {
            model.triangles = new List<Triangle3D>();
            if (filename == null) return false;
            StreamReader sr = new StreamReader(filename);
            string line = sr.ReadLine();
            while (line != null)
            {
                line = sr.ReadLine();
                Triangle3D triangle = new Triangle3D();
                string[] parts = line.Split(' ');
                if (parts[0] == "endsolid") break;
                triangle.normal = new Point3D(float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4]));
                line = sr.ReadLine();
                for (int j = 1; j < 4; j++)
                {
                    string[] splits = sr.ReadLine().Split(' ');
                    float x = float.Parse(splits[1]) * model.multiple + model.moveX;
                    float y = float.Parse(splits[2]) * model.multiple + model.moveY;
                    float z = float.Parse(splits[3]) * model.multiple + model.moveZ;
                    if (model.modelHeight < z)
                    {
                        model.modelHeight = z;
                    }
                    switch (j)
                    {
                        case 1:
                            triangle.p1 = new Point3D(x, y, z);
                            break;
                        case 2:
                            triangle.p2 = new Point3D(x, y, z);
                            break;
                        case 3:
                            triangle.p3 = new Point3D(x, y, z);
                            break;
                    }
                }
                triangle.maxH = triangle.p1.Z;
                triangle.minH = triangle.p1.Z;
                if (triangle.p2.Z > triangle.maxH)
                {
                    triangle.maxH = triangle.p2.Z;
                }
                if (triangle.p2.Z < triangle.minH)
                {
                    triangle.minH = triangle.p2.Z;
                }
                if (triangle.p3.Z > triangle.maxH)
                {
                    triangle.maxH = triangle.p3.Z;
                }
                if (triangle.p3.Z < triangle.minH)
                {
                    triangle.minH = triangle.p3.Z;
                }
                model.triangles.Add(triangle);
                line = sr.ReadLine();
                line = sr.ReadLine();
            }
            return true;
        }

        public static float[] ModifyLayerHeight(Triangle3D triangle, float[] layersHeights, float maxHeight, float minHeight, float precision)
        {
            double cosn = Math.Abs(triangle.normal.Z) / Math.Sqrt(triangle.normal.X * triangle.normal.X + triangle.normal.Y * triangle.normal.Y + triangle.normal.Z * triangle.normal.Z);
            double angle = Math.Abs(Math.Acos(cosn) * 180.0f / 3.14f);
            float changedHeight = minHeight + precision * Convert.ToInt32((float)angle / (90 / ((maxHeight - minHeight) / precision)));//根据斜率确定层高

            float correctionMin = 0, correctionMax = layersHeights[layersHeights.Length - 1];//需要重新调整的层高区间
            float maxLayerHeight;//最大层高

            float[] temp1 = new float[0], temp2 = new float[0], temp3 = new float[0];
            float[] result = new float[0];

            int i = 1;
            for (; i < layersHeights.Length; i++)
            {
                if (layersHeights[i] > triangle.minH)
                {
                    temp1 = new float[i];
                    Array.Copy(layersHeights, 0, temp1, 0, i);

                    correctionMin = layersHeights[i - 1];
                    break;
                }
            }
            if (i == layersHeights.Length) return result;
            if (layersHeights[i] > triangle.maxH) return result;
            maxLayerHeight = layersHeights[i] - layersHeights[i - 1];
            for (; i < layersHeights.Length; i++)
            {
                if (layersHeights[i] - layersHeights[i - 1] > maxLayerHeight)
                {
                    maxLayerHeight = layersHeights[i] - layersHeights[i - 1];
                }
                if (layersHeights[i] > triangle.maxH)
                {
                    temp3 = new float[layersHeights.Length - i];
                    Array.Copy(layersHeights, i, temp3, 0, layersHeights.Length - i);

                    correctionMax = layersHeights[i];
                    break;
                }
            }
            if ((maxLayerHeight - changedHeight) > 0.01f)
            {
                if (Math.Abs((correctionMax - correctionMin) % changedHeight) < 0.0001)
                {
                    temp2 = new float[(int)((correctionMax - correctionMin) / changedHeight) - 1];
                }
                else
                {
                    temp2 = new float[(int)((correctionMax - correctionMin) / changedHeight)];
                }
                for (int j = 0; j < temp2.Length; j++)
                {
                    temp2[j] = correctionMin + (j + 1) * changedHeight;
                }
                result = new float[temp1.Length + temp2.Length + temp3.Length];
                Array.Copy(temp1, 0, result, 0, temp1.Length);
                Array.Copy(temp2, 0, result, temp1.Length, temp2.Length);
                Array.Copy(temp3, 0, result, temp1.Length + temp2.Length, temp3.Length);
            }
            return result;
        }

        public static void getTriangleIntersectionLines(Triangle3D triangle, float[] layersHeights, List<Line3D>[] layers)
        {
            Line3D line = new Line3D();
            for (int i = 0; i < layersHeights.Length; i++)
            {
                if (layersHeights[i] > triangle.minH && layersHeights[i] < triangle.maxH)
                {
                    TrianglePlaneIntersections(triangle, layersHeights[i], out line);
                    layers[i].Add(line);
                }
            }
        }

        public static void TrianglePlaneIntersections(Triangle3D triangle, float plane, out Line3D line)
        {
            line = new Line3D();
            if (triangle.p1.Z > plane)
            {
                if (triangle.p2.Z > plane)
                {
                    LinePlaneIntersection(new Line3D(triangle.p1, triangle.p3), plane, out line.start);
                    LinePlaneIntersection(new Line3D(triangle.p2, triangle.p3), plane, out line.end);
                }
                else
                {
                    if (triangle.p3.Z > plane)
                    {
                        LinePlaneIntersection(new Line3D(triangle.p1, triangle.p2), plane, out line.start);
                        LinePlaneIntersection(new Line3D(triangle.p2, triangle.p3), plane, out line.end);
                    }
                    else
                    {
                        LinePlaneIntersection(new Line3D(triangle.p1, triangle.p2), plane, out line.start);
                        LinePlaneIntersection(new Line3D(triangle.p1, triangle.p3), plane, out line.end);
                    }
                }
            }
            else
            {
                if (triangle.p2.Z > plane)
                {
                    if (triangle.p3.Z > plane)
                    {
                        LinePlaneIntersection(new Line3D(triangle.p1, triangle.p2), plane, out line.start);
                        LinePlaneIntersection(new Line3D(triangle.p1, triangle.p3), plane, out line.end);
                    }
                    else
                    {
                        LinePlaneIntersection(new Line3D(triangle.p2, triangle.p1), plane, out line.start);
                        LinePlaneIntersection(new Line3D(triangle.p2, triangle.p3), plane, out line.end);
                    }
                }
                else
                {
                    LinePlaneIntersection(new Line3D(triangle.p3, triangle.p2), plane, out line.start);
                    LinePlaneIntersection(new Line3D(triangle.p3, triangle.p1), plane, out line.end);
                }
            }
        }

        public static bool LinePlaneIntersection(Line3D line, float plane, out Point3D point)
        {
            Point3D n = new Point3D(0, 0, 1);//平面法向量
            Point3D p = new Point3D(0, 0, plane);//平面上的点
            Point3D u = line.start - line.end;
            float s = (n.dot(p) - n.dot(line.start)) / n.dot(u);
            point = u * s + line.start;
            return true;
        }

        public static Polygon3D LinesToPolygon(List<Line3D> lines)
        {
            Polygon3D polygon = new Polygon3D();
            if (lines.Count == 0) return polygon;
            List<Point3D> points = new List<Point3D>();
            int forCount = lines.Count;
            if (lines.Count > 0)
            {
                points.Add(lines[0].start);
                points.Add(lines[0].end);
                lines.RemoveAt(0);
                while (lines.Count > 0)
                {
                    if (forCount-- < 0)
                    {
                        break;
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (points.Last().isClose(lines[i].start))
                        {
                            points.Add(lines[i].end);
                            lines.RemoveAt(i);
                            break;
                        }
                        else if (points.Last().isClose(lines[i].end))
                        {
                            points.Add(lines[i].start);
                            lines.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            polygon.points = points;
            return polygon;
        }

        public static List<Polygon3D> LinesToPolygons(List<Line3D> lines)
        {
            List<Polygon3D> polygons = new List<Polygon3D>();
            if (lines.Count == 0) return polygons;
            Polygon3D polygon = new Polygon3D();
            polygon.points.Add(lines[0].start);
            polygon.points.Add(lines[0].end);
            lines.RemoveAt(0);
            int forCount = lines.Count;
            while (lines.Count > 0)
            {
                if (forCount-- < 0) break;
                bool isFound = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (polygon.points.Last().isClose(lines[i].start))
                    {
                        isFound = true;
                        polygon.points.Add(lines[i].end);
                        lines.RemoveAt(i);
                        break;
                    }
                    else if (polygon.points.Last().isClose(lines[i].end))
                    {
                        isFound = true;
                        polygon.points.Add(lines[i].start);
                        lines.RemoveAt(i);
                        break;
                    }
                }
                if (isFound == false)
                {
                    polygon.points.Add(polygon.points.First());
                    polygons.Add(polygon);
                    if (lines.Count > 0)
                    {
                        polygon = new Polygon3D();
                        polygon.points.Add(lines[0].start);
                        polygon.points.Add(lines[0].end);
                        lines.RemoveAt(0);
                    }
                }
                if (polygon.points[0].isClose(polygon.points[polygon.points.Count - 1]))
                {
                    polygons.Add(polygon);
                    if (lines.Count > 0)
                    {
                        polygon = new Polygon3D();
                        polygon.points.Add(lines[0].start);
                        polygon.points.Add(lines[0].end);
                        lines.RemoveAt(0);
                    }
                }
            }
            return polygons;
        }

        public static List<Line3D> FillPolygon(Polygon3D polygon, float interval, int mode)//如果mode为0则纵向填充，若mode为1则横向填充
        {
            polygon = AddPolygonInformation(polygon);//补全多边形信息

            List<Line3D> lines = new List<Line3D>();
            if (mode == 0)
            {
                for (float i = polygon.minY + interval; i < polygon.maxY; i += interval)
                {
                    List<Point3D> intersectionPoint = new List<Point3D>();
                    for (int j = 0; j < polygon.points.Count; j++)
                    {
                        Line3D line3D = new Line3D();
                        if (j == 0) line3D.start = polygon.points[polygon.points.Count - 2];
                        else line3D.start = polygon.points[j - 1];
                        line3D.end = polygon.points[j];
                        float x = 0;
                        if (GetXOfIntersectionPoint(line3D, i, ref x))
                        {
                            intersectionPoint.Add(new Point3D(x, i, line3D.start.Z));
                        }
                    }
                    for (int k = 1; k < intersectionPoint.Count; k += 2)
                    {
                        lines.Add(new Line3D(intersectionPoint[k - 1], intersectionPoint[k]));
                    }
                }
            }
            else if (mode == 1)
            {
                for (float i = polygon.minX + interval; i < polygon.maxX; i += interval)
                {
                    List<Point3D> intersectionPoint = new List<Point3D>();
                    for (int j = 0; j < polygon.points.Count; j++)
                    {
                        Line3D line3D = new Line3D();
                        if (j == 0) line3D.start = polygon.points[polygon.points.Count - 2];
                        else line3D.start = polygon.points[j - 1];
                        line3D.end = polygon.points[j];
                        float y = 0;
                        if (GetYOfIntersectionPoint(line3D, i, ref y))
                        {
                            intersectionPoint.Add(new Point3D(i, y, line3D.start.Z));
                        }
                    }
                    for (int k = 1; k < intersectionPoint.Count; k += 2)
                    {
                        lines.Add(new Line3D(intersectionPoint[k - 1], intersectionPoint[k]));
                    }
                }
            }
            return lines;
        }

        public static Polygon3D AddPolygonInformation(Polygon3D polygon)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (Point3D point in polygon.points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
            polygon.minX = minX;
            polygon.minY = minY;
            polygon.maxX = maxX;
            polygon.maxY = maxY;
            return polygon;
        }

        public static List<Polygon3D> OffsetPolygon(Polygon3D polygon, float offset, int times)
        {
            List<Polygon3D> polygons = new List<Polygon3D>();
            Polygon3D pol = polygon;
            polygons.Add(polygon);
            for (int i = 0; i < times; i++)
            {
                pol = ContourOffset(pol, offset);
                polygons.Add(pol);
            }
            return polygons;
        }

        //轮廓偏置算法
        public static Polygon3D ContourOffset(Polygon3D polygon, float offset)
        {
            Polygon3D polygon3D = new Polygon3D();
            polygon = RemoveExcessPoints(polygon);
            List<Point3D> points = polygon.points;

            List<Point3D> newPoints = new List<Point3D>();


            for (int i = 0; i < points.Count; i++)
            {
                Point3D last = i - 1 < 0 ? points[points.Count - 2] : points[i - 1];
                Point3D next = i + 1 >= points.Count ? points[1] : points[i + 1];
                Point3D u = (last - points[i]) / last.distance(points[i]);
                Point3D v = (next - points[i]) / next.distance(points[i]);
                Point3D r = u + v;
                r = new Point3D((float)(r.X / Math.Sqrt(r.X * r.X + r.Y * r.Y)), (float)(r.Y / Math.Sqrt(r.X * r.X + r.Y * r.Y)), r.Z);
                float len = (float)(offset / Math.Sqrt(1 - (u.X * v.X + u.Y * v.Y) / 2));
                r = r * len;

                float cross = (points[i].X - last.X) * (next.Y - points[i].Y) - (points[i].Y - last.Y) * (next.X - points[i].X);
                if (cross < 0)
                {
                    newPoints.Add(points[i] - r);
                }
                else
                {
                    newPoints.Add(points[i] + r);
                }
            }

            polygon3D.points = newPoints;
            return polygon3D;
        }

        public static Polygon3D RemoveExcessPoints(Polygon3D polygon)//去除多余的轮廓点
        {
            Polygon3D polygon3D = new Polygon3D();
            List<Point3D> points = polygon.points;
            List<Point3D> needRemove = new List<Point3D>();
            for (int i = 0; i < points.Count; i++)
            {
                Point3D last = i - 1 < 0 ? points[points.Count - 2] : points[i - 1];
                Point3D next = i + 1 >= points.Count ? points[1] : points[i + 1];
                Point3D u = (last - points[i]) / last.distance(points[i]);
                Point3D v = (next - points[i]) / next.distance(points[i]);
                Point3D r = u + v;
                if (r.X == 0 && r.Y == 0)
                {
                    needRemove.Add(points[i]);
                }
            }
            foreach (Point3D p in needRemove)
            {
                points.Remove(p);
            }
            if (!points[0].Equals(points[points.Count - 1]))
            {
                points.Add(points[0]);
            }
            polygon3D.points = points;
            return polygon3D;
        }

        //public static List<Line3D> FillPolygon(Polygon3D polygon, float interval, int mode)//如果mode为0则纵向填充，若mode为1则横向填充
        //{
        //    List<Line3D> lines = new List<Line3D>();
        //    if (polygon.isShut)
        //    {
        //        if (mode == 0)
        //        {
        //            for (float i = polygon.minY + interval; i < polygon.maxY; i += interval)
        //            {
        //                List<Point3D> intersectionPoint = new List<Point3D>();
        //                for (int j = 0; j < polygon.points.Length; j++)
        //                {
        //                    Line3D line3D = new Line3D();
        //                    if (j == 0) line3D.start = polygon.points[polygon.points.Length - 1];
        //                    else line3D.start = polygon.points[j - 1];
        //                    line3D.end = polygon.points[j];
        //                    float x = 0;
        //                    if (GetXOfIntersectionPoint(line3D, i, ref x))
        //                    {
        //                        intersectionPoint.Add(new Point3D(x, i, line3D.start.Z));
        //                    }
        //                }
        //                for (int k = 1; k < intersectionPoint.Count; k += 2)
        //                {
        //                    lines.Add(new Line3D(intersectionPoint[k - 1], intersectionPoint[k]));
        //                }
        //            }
        //        }
        //        else if (mode == 1)
        //        {
        //            for (float i = polygon.minX + interval; i < polygon.maxX; i += interval)
        //            {
        //                List<Point3D> intersectionPoint = new List<Point3D>();
        //                for (int j = 0; j < polygon.points.Length; j++)
        //                {
        //                    Line3D line3D = new Line3D();
        //                    if (j == 0) line3D.start = polygon.points[polygon.points.Length - 1];
        //                    else line3D.start = polygon.points[j - 1];
        //                    line3D.end = polygon.points[j];
        //                    float y = 0;
        //                    if (GetYOfIntersectionPoint(line3D, i, ref y))
        //                    {
        //                        intersectionPoint.Add(new Point3D(i, y, line3D.start.Z));
        //                    }
        //                }
        //                for (int k = 1; k < intersectionPoint.Count; k += 2)
        //                {
        //                    lines.Add(new Line3D(intersectionPoint[k - 1], intersectionPoint[k]));
        //                }
        //            }
        //        }
        //    }
        //    return lines;
        //}

        public static bool GetXOfIntersectionPoint(Line3D line, float y, ref float x)
        {
            x = line.start.X + (y - line.start.Y) * (line.end.X - line.start.X) / (line.end.Y - line.start.Y);
            if (line.start.X < line.end.X)
            {
                if (x > line.start.X && x < line.end.X) return true;
            }
            else
            {
                if (x > line.end.X && x < line.start.X) return true;
            }
            return false;
        }

        public static bool GetYOfIntersectionPoint(Line3D line, float x, ref float y)
        {
            y = line.start.Y + (x - line.start.X) * (line.end.Y - line.start.Y) / (line.end.X - line.start.X);
            if (line.start.Y < line.end.Y)
            {
                if (y > line.start.Y && y < line.end.Y) return true;
            }
            else
            {
                if (y > line.end.Y && y < line.start.Y) return true;
            }
            return false;
        }

        public static List<Polygon3D> CutPolygon(Polygon3D polygon)
        {
            List<Polygon3D> polygons1 = new List<Polygon3D>();
            polygons1.Add(polygon);
            List<Polygon3D> polygons2 = new List<Polygon3D>();
            bool isF = true;
            bool isCut = true;
            while (isCut)
            {
                isCut = false;
                if (isF)
                {
                    polygons2.Clear();
                    for (int i = 0; i < polygons1.Count; i++)
                    {
                        List<Polygon3D> re = PolygonConvexDecomposition(polygons1[i]);
                        if (re != null)
                        {
                            isCut = true;
                            polygons2.Add(re[0]);
                            polygons2.Add(re[1]);
                        }
                        else
                        {
                            polygons2.Add(polygons1[i]);
                        }
                    }
                    isF = false;
                }
                else
                {
                    polygons1.Clear();
                    for (int i = 0; i < polygons2.Count; i++)
                    {
                        List<Polygon3D> re = PolygonConvexDecomposition(polygons2[i]);
                        if (re != null)
                        {
                            isCut = true;
                            polygons1.Add(re[0]);
                            polygons1.Add(re[1]);
                        }
                        else
                        {
                            polygons1.Add(polygons2[i]);
                        }
                    }
                    isF = true;
                }
            }
            if (isF)
            {
                return polygons1;
            }
            else
            {
                return polygons2;
            }
        }

        public static bool isPit(Point3D p1, Point3D p2, Point3D p3)
        {
            float cross = (p2.X - p1.X) * (p3.Y - p2.Y) - (p2.Y - p1.Y) * (p3.X - p2.X);
            return cross < -1;///////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        //凹多边形凸分解算法
        public static List<Polygon3D> PolygonConvexDecomposition(Polygon3D polygon)
        {
            List<Polygon3D> polygons = null;
            List<Point3D> points = polygon.points;
            for (int i = 2; i < points.Count; i++)//根据第一个凹点分，后面可以改为最合适的凹点//////////////////////////////////////
            {
                if (isPit(points[i - 2], points[i - 1], points[i]))//points[i - 1]为凹点
                {
                    polygons = new List<Polygon3D>();
                    polygons.Add(new Polygon3D());
                    polygons.Add(new Polygon3D());
                    Point3D u = (points[i - 2] - points[i - 1]) / points[i - 2].distance(points[i - 1]);
                    Point3D v = (points[i] - points[i - 1]) / points[i].distance(points[i - 1]);
                    Point3D r = u + v;
                    Point3D p = r + points[i - 1];
                    Line3D angularBisector = new Line3D(points[i - 1], p);
                    //获取反向延长线上的交点
                    Point3D intersection1 = null;//交点1
                    Point3D intersection2 = null;//交点2
                    int index1 = 0, index2 = 0;
                    Point3D last = points[0];
                    for (int j = 1; j < points.Count; j++)//找出距离最短的交点1
                    {
                        Point3D point3D = GetIntersection(last, points[j], points[i - 1], points[i - 2]);//获取反向延长线与轮廓线交点
                        if (point3D != null)
                        {
                            if (GetPointIsInLine(point3D, last, points[j], 0.001))//如果交点在轮廓上
                            {
                                Point3D v1 = point3D - points[i - 1];
                                Point3D v2 = points[i - 1] - points[i - 2];
                                float tempD = v1.dot(v2);
                                if (tempD > 0)//保证交点在反向延长线上
                                {
                                    if (intersection1 == null)
                                    {
                                        intersection1 = point3D;
                                        index1 = j;
                                    }
                                    else
                                    {
                                        if (points[i - 1].distance(intersection1) > points[i - 1].distance(point3D))
                                        {
                                            intersection1 = point3D;
                                            index1 = j;
                                        }
                                    }
                                }
                            }
                        }
                        last = points[j];
                    }
                    for (int j = 1; j < points.Count; j++)//找出距离最短的交点2
                    {
                        Point3D point3D2 = GetIntersection(last, points[j], points[i - 1], points[i]);
                        if (point3D2 != null)
                        {
                            if (GetPointIsInLine(point3D2, last, points[j], 0.001))//保证交点在轮廓上
                            {
                                Point3D v1 = point3D2 - points[i - 1];
                                Point3D v2 = points[i - 1] - points[i];
                                float tempD = v1.dot(v2);
                                if (tempD > 0)//保证交点在反向延长线上
                                {
                                    if (intersection2 == null)
                                    {
                                        intersection2 = point3D2;
                                        index2 = j;
                                    }
                                    else
                                    {
                                        if (points[i - 1].distance(intersection2) > points[i - 1].distance(point3D2))
                                        {
                                            intersection2 = point3D2;
                                            index2 = j;
                                        }
                                    }
                                }
                            }
                        }
                        last = points[j];
                    }
                    //确定可能为分割点的点
                    List<Point3D> possiblePoints = new List<Point3D>();
                    List<Point3D> morePossiblePoints = new List<Point3D>();
                    if (index1 == index2)//若交点在同一直线上，说明可能的区域里没有点，则选择交点为分割点
                    {
                        float dis1 = GetDistanceP2L(angularBisector.start, angularBisector.end, intersection1);
                        float dis2 = GetDistanceP2L(angularBisector.start, angularBisector.end, intersection2);
                        Point3D cutP = null;
                        if (dis1 < dis2)//用intersection1和points[i - 1]分割
                        {
                            cutP = intersection1;
                        }
                        else//用intersection2和points[i - 1]分割
                        {
                            cutP = intersection2;
                        }
                        polygons[0].points = new List<Point3D>();
                        if (index1 < i - 1)
                        {
                            for (int ind = 0; ind < points.Count; ind++)
                            {
                                if (ind < index1)
                                {
                                    polygons[0].points.Add(points[ind]);
                                }
                                else
                                {
                                    polygons[0].points.Add(cutP);
                                    break;
                                }
                            }
                            for (int ind = i - 1; ind < points.Count; ind++)
                            {
                                polygons[0].points.Add(points[ind]);
                            }
                            polygons[1].points.Add(cutP);
                            for (int ind = index1; ind <= i - 1; ind++)
                            {
                                polygons[1].points.Add(points[ind]);
                            }
                            polygons[1].points.Add(cutP);
                        }
                        else//i-1 < index1
                        {
                            for (int ind = 0; ind < points.Count; ind++)
                            {
                                if (ind <= i - 1)
                                {
                                    polygons[0].points.Add(points[ind]);
                                }
                                else
                                {
                                    polygons[0].points.Add(cutP);
                                    break;
                                }
                            }
                            for (int ind = index1; ind < points.Count; ind++)
                            {
                                polygons[0].points.Add(points[ind]);
                            }

                            for (int ind = i - 1; ind < index1; ind++)
                            {
                                polygons[1].points.Add(points[ind]);
                            }
                            polygons[1].points.Add(cutP);
                            polygons[1].points.Add(points[i - 1]);
                        }
                        break;
                    }
                    else if (index1 < index2)
                    {
                        for (int k = index1; k < index2; k++)
                        {
                            possiblePoints.Add(points[k]);
                            int per = k - 1 < 0 ? points.Count - 2 : k - 1;//如果上一个点为第一个点的前一个点，则为倒数第二个点
                            int next = k + 1 > points.Count - 1 ? 1 : k + 1;//如果下一个点超过最后一个点，则为第二个点
                            if (isPit(points[per], points[k], points[next]))
                            {
                                morePossiblePoints.Add(points[k]);
                            }
                        }
                    }
                    else
                    {
                        for (int k = index1; k < points.Count; k++)
                        {
                            possiblePoints.Add(points[k]);
                            int per = k - 1 < 0 ? points.Count - 2 : k - 1;//如果上一个点为第一个点的前一个点，则为倒数第二个点
                            int next = k + 1 > points.Count - 1 ? 1 : k + 1;//如果下一个点超过最后一个点，则为第二个点
                            if (isPit(points[per], points[k], points[next]))
                            {
                                morePossiblePoints.Add(points[k]);
                            }
                        }
                        for (int k = 1; k < index2; k++)
                        {
                            possiblePoints.Add(points[k]);
                            int per = k - 1 < 0 ? points.Count - 2 : k - 1;//如果上一个点为第一个点的前一个点，则为倒数第二个点
                            int next = k + 1 > points.Count - 1 ? 1 : k + 1;//如果下一个点超过最后一个点，则为第二个点
                            if (isPit(points[per], points[k], points[next]))
                            {
                                morePossiblePoints.Add(points[k]);
                            }
                        }
                    }
                    Point3D cutPoint = new Point3D();
                    if (morePossiblePoints.Count > 0)//可能的点里有凹点
                    {
                        float min = float.MaxValue;
                        for (int k = 0; k < morePossiblePoints.Count; k++)
                        {
                            //排除不能取的点，即point3Ds有多个交点
                            List<Point3D> point3Ds = GetPolygonIntersection(new Line3D(points[i - 1], morePossiblePoints[k]), polygon);
                            if (point3Ds.Count > 1) continue;
                            float dis = GetDistanceP2L(angularBisector.start, angularBisector.end, morePossiblePoints[k]);
                            if (dis < min)
                            {
                                if (dis < 0.0001) continue;
                                min = dis;
                                cutPoint = morePossiblePoints[k];
                            }
                        }
                    }
                    else
                    {
                        float min = float.MaxValue;
                        for (int k = 0; k < possiblePoints.Count; k++)
                        {
                            float dis = GetDistanceP2L(angularBisector.start, angularBisector.end, possiblePoints[k]);
                            if (dis < min)
                            {
                                min = dis;
                                cutPoint = possiblePoints[k];
                            }
                        }
                    }
                    //用cutPoint与points[i - 1]分割
                    int indexC = 0;
                    for (int ind = 0; ind < points.Count; ind++)
                    {
                        if (points[ind].Equals(cutPoint))
                        {
                            indexC = ind;
                            break;
                        }
                    }
                    if (indexC < i - 1)
                    {
                        for (int ind = 0; ind <= indexC; ind++)
                        {
                            polygons[0].points.Add(points[ind]);
                        }
                        for (int ind = i - 1; ind < points.Count; ind++)
                        {
                            polygons[0].points.Add(points[ind]);
                        }
                        for (int ind = indexC; ind <= i - 1; ind++)
                        {
                            polygons[1].points.Add(points[ind]);
                        }
                        polygons[1].points.Add(points[indexC]);
                    }
                    else
                    {
                        for (int ind = 0; ind <= i - 1; ind++)
                        {
                            polygons[0].points.Add(points[ind]);
                        }
                        for (int ind = indexC; ind < points.Count; ind++)
                        {
                            polygons[0].points.Add(points[ind]);
                        }
                        for (int ind = i - 1; ind <= indexC; ind++)
                        {
                            polygons[1].points.Add(points[ind]);
                        }
                        polygons[1].points.Add(points[i - 1]);
                    }
                    break;
                }
            }
            return polygons;
        }

        public static List<Point3D> GetPolygonIntersection(Line3D line, Polygon3D polygon)
        {
            List<Point3D> result = new List<Point3D>();
            List<Point3D> points = polygon.points;
            Point3D last = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                Point3D now = points[i];
                Point3D temp = GetIntersection(last, now, line.start, line.end);
                if (GetPointIsInLine(temp, last, now, 0.001) && GetPointIsInLine(temp, line.start, line.end, 0.001))
                {
                    result.Add(temp);
                }
                last = now;
            }
            return result;
        }

        public static Point3D GetIntersection(Point3D lineFirstStar, Point3D lineFirstEnd, Point3D lineSecondStar, Point3D lineSecondEnd)
        {
            /*
             * L1，L2都存在斜率的情况：
             * 直线方程L1: ( y - y1 ) / ( y2 - y1 ) = ( x - x1 ) / ( x2 - x1 ) 
             * => y = [ ( y2 - y1 ) / ( x2 - x1 ) ]( x - x1 ) + y1
             * 令 a = ( y2 - y1 ) / ( x2 - x1 )
             * 有 y = a * x - a * x1 + y1   .........1
             * 直线方程L2: ( y - y3 ) / ( y4 - y3 ) = ( x - x3 ) / ( x4 - x3 )
             * 令 b = ( y4 - y3 ) / ( x4 - x3 )
             * 有 y = b * x - b * x3 + y3 ..........2
             * 
             * 如果 a = b，则两直线平等，否则， 联解方程 1,2，得:
             * x = ( a * x1 - b * x3 - y1 + y3 ) / ( a - b )
             * y = a * x - a * x1 + y1
             * 
             * L1存在斜率, L2平行Y轴的情况：
             * x = x3
             * y = a * x3 - a * x1 + y1
             * 
             * L1 平行Y轴，L2存在斜率的情况：
             * x = x1
             * y = b * x - b * x3 + y3
             * 
             * L1与L2都平行Y轴的情况：
             * 如果 x1 = x3，那么L1与L2重合，否则平等
             * 
            */
            float a = 0, b = 0;
            int state = 0;
            if (lineFirstStar.X != lineFirstEnd.X)
            {
                a = (lineFirstEnd.Y - lineFirstStar.Y) / (lineFirstEnd.X - lineFirstStar.X);
                state |= 1;
            }
            if (lineSecondStar.X != lineSecondEnd.X)
            {
                b = (lineSecondEnd.Y - lineSecondStar.Y) / (lineSecondEnd.X - lineSecondStar.X);
                state |= 2;
            }
            switch (state)
            {
                case 0: //L1与L2都平行Y轴
                    {
                        if (lineFirstStar.X == lineSecondStar.X)
                        {
                            //throw new Exception("两条直线互相重合，且平行于Y轴，无法计算交点。");
                            return null;
                        }
                        else
                        {
                            //throw new Exception("两条直线互相平行，且平行于Y轴，无法计算交点。");
                            return null;
                        }
                    }
                case 1: //L1存在斜率, L2平行Y轴
                    {
                        float x = lineSecondStar.X;
                        float y = (lineFirstStar.X - x) * (-a) + lineFirstStar.Y;
                        return new Point3D(x, y, lineFirstStar.Z);
                    }
                case 2: //L1 平行Y轴，L2存在斜率
                    {
                        float x = lineFirstStar.X;
                        //网上有相似代码的，这一处是错误的。你可以对比case 1 的逻辑 进行分析
                        //源code:lineSecondStar * x + lineSecondStar * lineSecondStar.X + p3.Y;
                        float y = (lineSecondStar.X - x) * (-b) + lineSecondStar.Y;
                        return new Point3D(x, y, lineFirstStar.Z);
                    }
                case 3: //L1，L2都存在斜率
                    {
                        if (a == b)
                        {
                            // throw new Exception("两条直线平行或重合，无法计算交点。");
                            return new Point3D(0, 0, 0);
                        }
                        float x = (a * lineFirstStar.X - b * lineSecondStar.X - lineFirstStar.Y + lineSecondStar.Y) / (a - b);
                        float y = a * x - a * lineFirstStar.X + lineFirstStar.Y;
                        return new Point3D(x, y, lineFirstStar.Z);
                    }
            }
            // throw new Exception("不可能发生的情况");
            return null;
        }

        public static bool GetPointIsInLine(Point3D pf, Point3D p1, Point3D p2, double range)
        {
            //点在线段首尾两端之外则return false
            double cross = (p2.X - p1.X) * (pf.X - p1.X) + (p2.Y - p1.Y) * (pf.Y - p1.Y);
            if (cross <= 0) return false;
            double d2 = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
            if (cross >= d2) return false;

            double r = cross / d2;
            double px = p1.X + (p2.X - p1.X) * r;
            double py = p1.Y + (p2.Y - p1.Y) * r;

            //判断距离是否小于误差
            return Math.Sqrt((pf.X - px) * (pf.X - px) + (py - pf.Y) * (py - pf.Y)) <= range;
        }

        public static float GetDistanceP2L(Point3D pointA, Point3D pointB, Point3D pointP)
        {
            //直线公式 A*X+B*Y+C=0
            //点到直线公式
            //分子 绝对值|A*X`+B*Y~+C|
            //分母 A*A+B*B 开方
            float A = pointA.Y - pointB.Y;
            float B = pointB.X - pointA.X;
            float C = pointA.X * pointB.Y - pointA.Y * pointB.X;
            //点到直线公式
            return (float)(Math.Abs(A * pointP.X + B * pointP.Y + C) / Math.Sqrt(A * A + B * B));
        }
    }
}
