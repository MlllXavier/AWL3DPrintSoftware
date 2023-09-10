using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    public class Printer
    {
        private static Printer printer;//单例类

        public float X = 0;//X轴
        public float Y = 0;//Y轴
        public float Z = 0;//Z轴
        public float maxX = 0;//X的最大范围
        public float maxY = 0;//Y的最大范围
        public float maxZ = 0;//Z的最大范围
        public int printerState = 0;//打印机状态 0：空闲 1.：打印中 2.暂停
        public int layerIndex = 0;//当前打印层索引
        public int channelIndex = 0;//当前打印通道索引
        public SerialPortController serialPortController = new SerialPortController();//串口管理对象
        public List<string> commandHistory = new List<string>();//指令历史

        public static Printer GetPrinter(float mx, float my, float mz)
        {
            if (printer == null) return new Printer(mx, my, mz);
            return printer;
        }

        private Printer(float mx, float my, float mz)
        {
            maxX = mx;
            maxY = my;
            maxZ = mz;
        }

        public int SendGCode(string str)//0：正确，-1：异常，1：串口未打开，-2：超出范围
        {
            return serialPortController.sendGCode(str);
        }

        public int moveToAbsolute(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            return SendGCode(Utilities.pointToGCode("G0", X, Y, Z));
        }

        public int moveToRelative(float x, float y, float z)
        {
            if (x > maxX || y > maxY || z > maxZ || x < 0 || y < 0 || z < 0)
            {
                return -2;
            }
            X = x;
            Y = y;
            Z = z;
            return SendGCode(Utilities.pointToGCode("G0", X - FormMain.printProject.printChannels[channelIndex].position.X, Y - FormMain.printProject.printChannels[channelIndex].position.Y, Z));
        }

        public int printToAbsolute(float x, float y, float z)
        {
            if (x > maxX || y > maxY || z > maxZ || x < 0 || y < 0 || z < 0)
            {
                return -2;
            }
            X = x;
            Y = y;
            Z = z;
            return SendGCode(Utilities.pointToGCode("G1", X, Y, Z));
        }

        public int printToRelative(float x, float y, float z)
        {
            if (x > maxX || y > maxY || z > maxZ || x < 0 || y < 0 || z < 0)
            {
                return -2;
            }
            X = x;
            Y = y;
            Z = z;
            return SendGCode(Utilities.pointToGCode("G1", X - FormMain.printProject.printChannels[channelIndex].position.X, Y - FormMain.printProject.printChannels[channelIndex].position.Y, Z));
        }

        public int toPoint(Point3D point)
        {
            if (point.X > maxX || point.Y > maxY || point.Z > maxZ || point.X < 0 || point.Y < 0 || point.Z < 0)
            {
                return -2;
            }
            X = point.X;
            Y = point.Y;
            Z = point.Z;
            if (point.mode == 0)
            {
                return moveToRelative(X, Y, Z);
            }
            else
            {
                return printToRelative(X, Y, Z);
            }
        }

        public int Pause()//暂停
        {
            printerState = 3;
            return SendGCode("G4P");
        }

        public int Continue()//继续
        {
            printerState = 2;
            return SendGCode("G4C");
        }

        public int x_moveUp(float step)
        {
            X += step;
            return moveToAbsolute(X, Y, Z);
        }

        public int x_moveDown(float step)
        {
            X -= step;
            return moveToAbsolute(X, Y, Z);
        }

        public int y_moveUp(float step)
        {
            Y += step;
            return moveToAbsolute(X, Y, Z);
        }

        public int y_moveDown(float step)
        {
            Y -= step;
            return moveToAbsolute(X, Y, Z);
        }

        public int z_moveUp(float step)
        {
            Z += step;
            return moveToAbsolute(X, Y, Z);
        }

        public int z_moveDown(float step)
        {
            Z -= step;
            return moveToAbsolute(X, Y, Z);
        }

        public int moveToSolidifyZero()
        {
            //此x,y为固化灯相对喷头1的位置
            float x = 20;
            float y = 40;
            return moveToAbsolute(FormMain.printProject.printLayers[layerIndex].x_start + FormMain.printProject.printLayers[layerIndex].x_size / 2 - x, FormMain.printProject.printLayers[layerIndex].y_start + FormMain.printProject.printLayers[layerIndex].y_size / 2 - y, 0);
        }
    }
}
