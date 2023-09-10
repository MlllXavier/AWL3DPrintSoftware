using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AWL3DPrintSoftware
{
    public partial class FormLayer : Form
    {
        public FormLayer()
        {
            InitializeComponent();
        }

        int tag = 0;                //..用于存储选中的打印方式
        PrintLayer printLayer;

        public FormLayer(PrintLayer layer)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
            //设置默认值
            tag = layer.mode;
            txtX.Text = layer.x_start + "";
            txtY.Text = layer.y_start + "";
            txtZ.Text = layer.z_start + "";
            txtWidth.Text = layer.x_size + "";
            txtLength.Text = layer.y_size + "";
            txtDensity.Text = layer.interval + "";
            txtHeight.Text = layer.height + "";
            txtSolidfyTime.Text = layer.solidfyTime + "";
            cbbPrintChannel.SelectedIndex = layer.channel;
            printLayer = layer;

            if (printLayer.mode >= 10)
            {
                txtX.Enabled = false; 
                txtY.Enabled = false;
                txtWidth.Enabled = false; 
                txtLength.Enabled = false;
                txtDensity.Enabled = false;
                txtHeight.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
        }

        //声明委托
        public delegate void AddLayerDelegate(PrintLayer layerChanged);
        //声明事件
        public event AddLayerDelegate AddLayerEvent;

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (printLayer.mode >= 10)//如果是通过分层切片方式自动添加的打印层
            {
                //修改层高
                for (int i = 0; i < printLayer.points.Count; i++)
                {
                    printLayer.points[i].Z = float.Parse(txtZ.Text);
                }
                printLayer.z_start = float.Parse(txtZ.Text);
                //修改固化时间
                printLayer.solidfyTime = int.Parse(txtSolidfyTime.Text);
                //修改通道
                printLayer.channel = cbbPrintChannel.SelectedIndex;
                AddLayerEvent(printLayer);
            }
            else
            {
                AddLayerEvent(new PrintLayer(tag, cbbPrintChannel.SelectedIndex, float.Parse(txtDensity.Text), float.Parse(txtWidth.Text), float.Parse(txtLength.Text), float.Parse(txtX.Text), float.Parse(txtY.Text), float.Parse(txtZ.Text), int.Parse(txtSolidfyTime.Text), float.Parse(txtHeight.Text)));
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tag = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tag = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tag = 2;
        }
    }
}
