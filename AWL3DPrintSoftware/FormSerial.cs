using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AWL3DPrintSoftware
{
    public partial class FormSerial : Form
    {
        FormMain main;

        public FormSerial(FormMain formMain)
        {
            InitializeComponent();
            main = formMain;
        }

        private void FormSerial_Load(object sender, EventArgs e)
        {
            //初始化串口
            cbbComList.Items.AddRange(FormMain.printer.serialPortController.names);//串口设置选项卡中的端口选择框,选择框内容为GetPortNames()获取计算机的端口
            if (cbbComList.Items.Count > 0)
            {
                cbbComList.SelectedIndex = cbbComList.Items.Count - 1;//如果端口选择框可选项数目大于0，默认选择最后一项
            }
            cbbBaudRate.SelectedIndex = 3;          //..串口设置选项卡中的波特率选择框，默认选择第3项
            cbbDataBits.SelectedIndex = 0;
            cbbParity.SelectedIndex = 0;
            cbbStopBits.SelectedIndex = 0;

            btnOpenSerial.Text = FormMain.printer.serialPortController.serialPort.IsOpen ? "关闭串口" : "打开串口";
            //..打开串口状态下，串口参数不可设置；否则可设置。可见，串口可设置性与串口状态是相反的关系
            cbbComList.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbBaudRate.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbParity.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbDataBits.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbStopBits.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
        }

        private void cbbComList_DropDown(object sender, EventArgs e)
        {
            cbbComList.Text = null;
            cbbComList.Items.Clear();
            FormMain.printer.serialPortController.names = SerialPort.GetPortNames();
            cbbComList.Items.AddRange(FormMain.printer.serialPortController.names);
            if (cbbComList.Items.Count > 0)
            {
                cbbComList.SelectedIndex = 0;
            }
        }

        private void btnOpenSerial_Click(object sender, EventArgs e)
        {
            SerialSwitch();
        }

        public void SerialSwitch()
        {
            if (FormMain.printer.serialPortController.serialPort.IsOpen == false)  //..此时如果串口未开启，则打开串口
            {
                FormMain.printer.serialPortController.serialPort.PortName = cbbComList.SelectedItem.ToString();           //..串口名：COM1等
                FormMain.printer.serialPortController.serialPort.BaudRate = Convert.ToInt32(cbbBaudRate.SelectedItem.ToString());         //..比特率
                FormMain.printer.serialPortController.serialPort.Parity = (Parity)Convert.ToInt32(cbbParity.SelectedIndex.ToString());        //..校验位
                FormMain.printer.serialPortController.serialPort.DataBits = Convert.ToInt32(cbbDataBits.SelectedItem.ToString());         //..数据位
                FormMain.printer.serialPortController.serialPort.StopBits = (StopBits)Convert.ToInt32(cbbStopBits.SelectedItem.ToString());           //..停止位
                try
                {
                    main.switchSerial();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else                  //..如果此时串口已开启，则关闭串口
            {
                try
                {
                    main.switchSerial();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            //btnSend.Enabled = serial.serialPort.IsOpen;
            btnOpenSerial.Text = FormMain.printer.serialPortController.serialPort.IsOpen ? "关闭串口" : "打开串口";
            //..打开串口状态下，串口参数不可设置；否则可设置。可见，串口可设置性与串口状态是相反的关系
            cbbComList.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbBaudRate.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbParity.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbDataBits.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
            cbbStopBits.Enabled = !FormMain.printer.serialPortController.serialPort.IsOpen;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            FormMain.printer.serialPortController.sendGCode(txtGCode.Text);
        }

        public void SendDataEvent(string str)
        {
            if (IsHandleCreated)
            {
                Invoke(new EventHandler(delegate
                {
                    listBox2.Items.Add(str);
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                }));
            }
        }

        public void GetDataEvent(string str)
        {
            if (IsHandleCreated)
            {
                Invoke(new EventHandler(delegate
                {
                    listBox1.Items.Add(str);
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                }));
            }
        }

        private void FormSerial_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
