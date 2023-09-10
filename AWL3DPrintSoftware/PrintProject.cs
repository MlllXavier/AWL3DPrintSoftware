using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWL3DPrintSoftware
{
    [Serializable]
    public class PrintProject
    {
        public string id;
        public string name;
        public string description;

        public Model3D model;

        public List<PrintLayer> printLayers = new List<PrintLayer>();//打印层
        public List<PrintChannel> printChannels = new List<PrintChannel>();//打印通道

        public DateTime createTime;
        public DateTime updateTime;

        public PrintProject()
        {
            // 当前日期转换成时间戳
            long time1 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            id = time1.ToString();
            name = "（新建项目）";
            model = new Model3D();
            //初始化通道
            printChannels.Add(new PrintChannel(1, Color.Salmon, new Point(0, 0), 25.35, 25, 5, 3, 10, 100));
            printChannels.Add(new PrintChannel(2, Color.ForestGreen, new Point(0, 0), 25.35, 25, 5, 3, 10, 100));
            printChannels.Add(new PrintChannel(3, Color.MediumBlue, new Point(0, 0), 25.35, 25, 5, 3, 10, 100));
            printChannels.Add(new PrintChannel(4, Color.Crimson, new Point(0, 0), 25.35, 25, 5, 3, 10, 100));
            printChannels.Add(new PrintChannel(5, Color.Yellow, new Point(0, 0), 25.35, 25, 5, 3, 10, 100));

            createTime = DateTime.Now;
            updateTime = DateTime.Now;

            description = "";
        }
    }
}
