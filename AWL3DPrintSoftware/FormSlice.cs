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
    public partial class FormSlice : Form
    {
        public FormSlice()
        {
            InitializeComponent();
        }

        private void FormSlice_Load(object sender, EventArgs e)
        {
            cbbFillMode.SelectedIndex = 2;
            cbbChannel.SelectedIndex = 0;
        }

        public delegate void SliceDelegate(float maxHeight, float minHeight, float precision, int fillMode, float fillInterval, int channel, int solidTime);
        
        public event SliceDelegate SliceEvent;

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            SliceEvent(float.Parse(txtMaxHeight.Text), float.Parse(txtMinHeight.Text), float.Parse(txtPrecision.Text), cbbFillMode.SelectedIndex, float.Parse(txtFillInterval.Text), cbbChannel.SelectedIndex, int.Parse(txtSolidTime.Text));
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
