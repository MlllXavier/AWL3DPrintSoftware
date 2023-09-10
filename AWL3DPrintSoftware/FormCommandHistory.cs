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
    public partial class FormCommandHistory : Form
    {
        public FormCommandHistory()
        {
            InitializeComponent();
        }

        private void FormCommandHistory_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            for (int i = 0; i < FormMain.printer.commandHistory.Count(); i++)
            {
                listBox1.Items.Add(FormMain.printer.commandHistory[i]);
            }
        }
    }
}
