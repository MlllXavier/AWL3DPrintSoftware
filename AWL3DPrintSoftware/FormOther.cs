using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AWL3DPrintSoftware
{
    public partial class FormOther : Form
    {
        public FormOther()
        {
            InitializeComponent();
        }

        private void btnToZero_Click(object sender, EventArgs e)
        {
            FormMain.printer.moveToAbsolute(0, 0, 0);
        }

        private void btnToPrintZero_Click(object sender, EventArgs e)
        {
            if (FormMain.printProject.printLayers.Count == 0)
            {
                return;
            }
            Point3D point = FormMain.printProject.printLayers[FormMain.printer.layerIndex].points[0];
            FormMain.printer.moveToRelative(point.X, point.Y, point.Z);
        }

        private void btnToSolidfyZero_Click(object sender, EventArgs e)
        {
            if (FormMain.printProject.printLayers.Count == 0)
            {
                return;
            }
            FormMain.printer.moveToSolidifyZero();
        }

        bool solodfyLight = false;
        private void btnSolidfyLight_Click(object sender, EventArgs e)
        {
            if (solodfyLight)
            {
                FormMain.printer.SendGCode("M251");
            }
            else
            {
                FormMain.printer.SendGCode("M250");
            }
            solodfyLight = !solodfyLight;
        }
    }
}
