using AForge.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AWL3DPrintSoftware
{
    public partial class FormImportGCode : Form
    {
        public FormImportGCode()
        {
            InitializeComponent();
        }

        private void buttonImportFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "GCode文件|*.gcode";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string contentPath = dialog.FileName;
                using (StreamReader sr = new StreamReader(contentPath))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        //去掉空串
                        if (line == "") continue;
                        //去掉注释
                        int v = line.IndexOf(';');
                        if (v == 0) continue;
                        if (v > 0) line = line.Substring(0, v);
                        //根据空格分割
                        string[] items = line.Split(' ');
                        //处理字符数组
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i] == "") continue;
                            //去掉E后面的数字
                            if (items[i][0] == 'E') items[i] = "E";
                        }
                        //拼接字符数组
                        string temp = "";
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i] != "") temp += items[i];
                        }
                        Invoke(new EventHandler(delegate
                        {
                            listBox1.Items.Add(temp);
                        }));
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label2.Text = (string)listBox1.Items[listBox1.SelectedIndex];
        }

        private void buttonSendOne_Click(object sender, EventArgs e)
        {
            FormMain.printer.serialPortController.sendGCode(label2.Text);
        }
    }
}
