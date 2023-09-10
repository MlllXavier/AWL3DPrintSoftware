using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    [Serializable]
    public class PrintChannel
    {
        public int ChannelID;// 通道ID
        public Color color;
        public Point position;
        public double AirPress; // 气压     KPa     需通过电流值换算
        public double Temperature; // 温度    ℃
        public double PrintSpeed; // 移动速度
        public double T1;
        public double T2;
        public double T3;

        public PrintChannel()
        {

        }

        public PrintChannel(int id, Color c, Point p, double a, double t, double s, double t1, double t2, double t3)
        {
            ChannelID = id;
            color = c;
            position = p;
            AirPress = a;
            Temperature = t;
            PrintSpeed = s;
            T1 = t1;
            T2 = t2;
            T3 = t3;
        }

        public static int setChannel(int index)//切换通道
        {
            return FormMain.printer.SendGCode("T" + (index + 1));
        }

        public int setAirPress(double p)
        {
            AirPress = p;
            return setAirPress();
        }

        public int setAirPress()
        {
            return FormMain.printer.SendGCode("G1P" + Utilities.pressureTocurrent(AirPress).ToString("0.0"));
        }

        public int setPrintSpeed(double r)
        {
            PrintSpeed = r;
            return setPrintSpeed();
        }

        public int setPrintSpeed()
        {
            return FormMain.printer.SendGCode("G1F" + PrintSpeed.ToString("0.0"));
        }

        public int setT1T2T3(double w, double v, double u)
        {
            T1 = w;
            T2 = v;
            T3 = u;
            return setT1T2T3();
        }

        public int setT1T2T3()
        {
            return FormMain.printer.SendGCode("G1W" + T1.ToString("0.0") + "V" + T2.ToString("0.0") + "U" + T3.ToString("0.0"));
        }
    }
}
