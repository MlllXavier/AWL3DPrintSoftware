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
    public partial class FormXYZ : Form
    {
        public FormXYZ()
        {
            InitializeComponent();
        }

        private void FormXYZ_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            trackBar1.Minimum = 0;
            trackBar1.Maximum = (int)FormMain.printer.maxX;
            trackBar2.Minimum = 0;
            trackBar2.Maximum = (int)FormMain.printer.maxY;
            trackBar3.Minimum = 0;
            trackBar3.Maximum = (int)FormMain.printer.maxZ;
            numericUpDown1.Minimum = -1000;
            numericUpDown1.Maximum = 1000;
            numericUpDown2.Minimum = -1000;
            numericUpDown2.Maximum = 1000;
            numericUpDown3.Minimum = -1000;
            numericUpDown3.Maximum = 1000;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            trackBar2.Value = (int)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            trackBar3.Value = (int)numericUpDown3.Value;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            numericUpDown1.Value = trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            numericUpDown2.Value = trackBar2.Value;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            numericUpDown3.Value = trackBar3.Value;
        }

        private bool isAbsolute = true;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                isAbsolute = true;
                trackBar1.Minimum = 0;
                trackBar1.Maximum = (int)FormMain.printer.maxX;
                trackBar2.Minimum = 0;
                trackBar2.Maximum = (int)FormMain.printer.maxY;
                trackBar3.Minimum = 0;
                trackBar3.Maximum = (int)FormMain.printer.maxZ;
            }
            else
            {
                isAbsolute = false;
                trackBar1.Minimum = -50;
                trackBar1.Maximum = 50;
                trackBar2.Minimum = -50;
                trackBar2.Maximum = 50;
                trackBar3.Minimum = -50;
                trackBar3.Maximum = 50;
            }
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isAbsolute)
            {
                FormMain.printer.moveToAbsolute((float)numericUpDown1.Value, FormMain.printer.Y, FormMain.printer.Z);
            }
            else
            {
                FormMain.printer.moveToAbsolute(FormMain.printer.X + (float)numericUpDown1.Value, FormMain.printer.Y, FormMain.printer.Z);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isAbsolute)
            {
                FormMain.printer.moveToAbsolute(FormMain.printer.X, (float)numericUpDown2.Value, FormMain.printer.Z);
            }
            else
            {
                FormMain.printer.moveToAbsolute(FormMain.printer.X, FormMain.printer.Y + (float)numericUpDown2.Value, FormMain.printer.Z);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isAbsolute)
            {
                FormMain.printer.moveToAbsolute(FormMain.printer.X, FormMain.printer.Y, (float)numericUpDown3.Value);
            }
            else
            {
                FormMain.printer.moveToAbsolute(FormMain.printer.X, FormMain.printer.Y, FormMain.printer.Z + (float)numericUpDown3.Value);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (isAbsolute)
            {
                FormMain.printer.moveToAbsolute((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value);
            }
            else
            {
                FormMain.printer.moveToAbsolute(FormMain.printer.X + (float)numericUpDown1.Value, FormMain.printer.Y + (float)numericUpDown2.Value, FormMain.printer.Z + (float)numericUpDown3.Value);
            }
        }
    }
}
