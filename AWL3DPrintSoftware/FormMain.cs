using SharpGL;
using SharpGL.SceneGraph.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace AWL3DPrintSoftware
{
    /**
     * 主窗体代码部分
     */
    public partial class FormMain : Form
    {
        public static Printer printer = Printer.GetPrinter(150, 130, 100);//打印机
        public static PrintProject printProject;//打印项目

        FormSerial formSerial;//串口窗口
        List<Line3D> lines = new List<Line3D>();//用于在状态监测界面画路径的线

        System.Timers.Timer timer = new System.Timers.Timer(interval: 2000);//用于检测是否是打印中空闲的定时器
        System.Timers.Timer timerIsStop = new System.Timers.Timer(interval: 100);
        System.Timers.Timer closeSolidfyLight = new System.Timers.Timer(100);

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //初始化串口
            formSerial = new FormSerial(this);
            printer.serialPortController.names = SerialPort.GetPortNames();
            if (printer.serialPortController.names.Length > 0)
            {
                printer.serialPortController.serialPort.PortName = printer.serialPortController.names[printer.serialPortController.names.Length - 1];
            }
            printer.serialPortController.serialPort.BaudRate = 115200;
            printer.serialPortController.serialPort.DataBits = 8;
            printer.serialPortController.serialPort.StopBits = StopBits.One;
            printer.serialPortController.serialPort.Parity = Parity.None;

            HidePages();

            //初始化事件
            printer.serialPortController.sendDataEvent += Serial_sendDataEvent;
            printer.serialPortController.receiveDataEvent += Serial_receiveDataEvent;
            timer.Elapsed += Timer_Elapsed;
            timerIsStop.Elapsed += TimerIsStop_Elapsed;
            timerIsStop.AutoReset = true;//必须加，否则停不下来
            closeSolidfyLight.Elapsed += CloseSolidfyLight_Elapsed;
            openGLControl1.MouseWheel += OpenGLControl1_MouseWheel;
            openGLControl2.MouseWheel += OpenGLControl2_MouseWheel;
        }

        private void HidePages()//隐藏其他标签页
        {
            tabPage2.Parent = null;
            tabPage3.Parent = null;
            tabPage4.Parent = null;
        }

        private void ShowPages()//显示其他标签页
        {
            tabPage2.Parent = tabControl1;
            tabPage3.Parent = tabControl1;
            tabPage4.Parent = tabControl1;
        }

        int tagTimer = 0;
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            tagTimer++;
            if (tagTimer > 1)
            {
                Invoke(new EventHandler(delegate
                {
                    lblPrinterState.Text = "停止";
                }));
                printer.printerState = 0;
                timer.Stop();
                tagTimer = 0;
            }
        }

        private void Serial_sendDataEvent(string value)
        {
            printer.commandHistory.Add(value);
            formSerial.SendDataEvent(value);
        }

        private void Serial_receiveDataEvent(string value)
        {
            formSerial.GetDataEvent(value);
            UploadInfo info = Utilities.hexStrToInfo(value);
            printer.X = info.xLocation / 1000;
            printer.Y = info.yLocation / 1000;
            printer.Z = info.zLocation / 1000;
            //因为要访问ui资源，所以需要使用invoke方式同步ui。  
            this.Invoke((EventHandler)delegate
            {
                switch (info.sysStatus)
                {
                    case "80":
                        if (printer.printerState == 0)
                        {
                            break;
                        }
                        timer.Start();
                        break;
                    case "82":
                        tagTimer = 0;
                        if (printer.printerState == 1)
                        {
                            break;
                        }
                        printer.printerState = 1;
                        lblPrinterState.Text = "打印中";
                        break;
                    case "83":
                        timer.Stop();
                        if (printer.printerState == 2)
                        {
                            break;
                        }
                        printer.printerState = 2;
                        lblPrinterState.Text = "打印中暂停";
                        break;
                }
                //显示状态
                lblX.Text = string.Format("{0:F2}", printer.X);
                lblY.Text = string.Format("{0:F2}", printer.Y);
                lblZ.Text = string.Format("{0:F2}", printer.Z);
                lblChannel.Text = info.CurChannelNum + "";
                lblAirPress.Text = Utilities.currentToPressure(info.AirPress) + "（KPa）";
                lblTemperature.Text = info.Temperature + "（℃）";
                lblPrintDistance.Text = info.PrintDistance + "（um）";
                lblOpenTime.Text = info.OpenTime + "（us）";
            });
        }

        private void TimerIsStop_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (printer.printerState == 0)
            {
                timerIsStop.Stop();
                printer.printerState = 1;
                printer.SendGCode("M250");//打开固化灯
                Thread.Sleep(printProject.printLayers[printer.layerIndex].solidfyTime * 1000);
                printer.SendGCode("M251");//关闭固化灯
                printer.printerState = 0;
                if (printer.layerIndex < printProject.printLayers.Count - 1)
                {
                    printer.layerIndex++;
                    Thread thread = new Thread(new ThreadStart(LoadLayerData))
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
                else
                {
                    MessageBox.Show("全部打印完成");
                }
                return;
            }
        }

        private void flushDgv()//刷新打印层的dgv
        {
            lines.Clear();
            dgvPrintLayer.Rows.Clear();
            for (int i = 0; i < printProject.printLayers.Count; i++)
            {
                dgvPrintLayer.Rows.Add();
                dgvPrintLayer.Rows[i].Cells[0].Value = i + 1;
                switch (printProject.printLayers[i].mode)
                {
                    case 0:
                        dgvPrintLayer.Rows[i].Cells[1].Value = "蛇形";
                        break;
                    case 1:
                        dgvPrintLayer.Rows[i].Cells[1].Value = "回形";
                        break;
                    case 2:
                        dgvPrintLayer.Rows[i].Cells[1].Value = "网格形";
                        break;
                    case 10:
                        dgvPrintLayer.Rows[i].Cells[1].Value = "纵向填充";
                        break;
                    case 11:
                        dgvPrintLayer.Rows[i].Cells[1].Value = "横向填充";
                        break;
                    case 12:
                        dgvPrintLayer.Rows[i].Cells[1].Value = "纵横交替";
                        break;
                }
                dgvPrintLayer.Rows[i].Cells[2].Value = printProject.printLayers[i].channel + 1;

                for (int j = 1; j < printProject.printLayers[i].points.Count - 1; j++)
                {
                    Line3D line3D = new Line3D
                    {
                        start = printProject.printLayers[i].points[j],
                        end = printProject.printLayers[i].points[j + 1]
                    };
                    if (line3D.end.mode == 1)
                    {
                        lines.Add(line3D);
                    }
                }
            }
        }

        private void flushProjectLabel()
        {
            if (printProject != null)
            {
                txtProjectName.Text = printProject.name;
                lblProjectId.Text = printProject.id;
                lblProjectCreateTime.Text = printProject.createTime.ToString();
                lblProjectUpdateTime.Text = printProject.updateTime.ToString();
                rtbProjectNote.Text = printProject.description.ToString();
                txtModelMult.Text = printProject.model.multiple.ToString();
                txtModelX.Text = printProject.model.moveX.ToString();
                txtModelY.Text = printProject.model.moveY.ToString();
                txtModelZ.Text = printProject.model.moveZ.ToString();
                flashChannellabel();
            }
        }

        private void addProjectMenuItem_Click(object sender, EventArgs e)
        {
            printProject = new PrintProject();
            //初始化通道
            //printer.printChannel = printProject.printChannels[indexChannel];
            flushProjectLabel();
            ShowPages();
            cbbChannel.SelectedIndex = printer.channelIndex;
            tabControl1.SelectedIndex = 1;
            statusTips.Text = "新建项目完成";
        }

        private void openProjectMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "PrintProject";
            OpenFileDialog openFile = new OpenFileDialog
            {
                Title = "打开项目",
                InitialDirectory = path
            };
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                PrintProject project = Utilities.ReadObject<PrintProject>(openFile.FileName);
                printProject = project;
                flushProjectLabel();
                ShowPages();
                flushDgv();
                cbbChannel.SelectedIndex = printer.channelIndex;
                tabControl1.SelectedIndex = 1;
                statusTips.Text = "已打开项目";
            }
        }

        private void saveProjectMenuItem_Click(object sender, EventArgs e)
        {
            if (printProject == null)
            {
                MessageBox.Show("当前未打开项目");
                return;
            }
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "PrintProject";
            SaveFileDialog save = new SaveFileDialog
            {
                Title = "保存项目",
                InitialDirectory = path,
                FileName = printProject.name + printProject.id + ".obj"
            };
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            save.ShowDialog();
            if (save.FileName != string.Empty)
            {
                Utilities.SaveObject(save.FileName, printProject);
                statusTips.Text = "项目已保存";
            }
        }

        private void closeProjectMenuItem_Click(object sender, EventArgs e)
        {
            printProject = null;
            HidePages();
            statusTips.Text = "项目已关闭";
        }

        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void commandHistoryMenuItem_Click(object sender, EventArgs e)
        {
            FormCommandHistory commandHistory = new FormCommandHistory();
            commandHistory.Show();
        }

        private void printLayerMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void printChannelMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

        private void xyzDebugMenuItem_Click(object sender, EventArgs e)
        {
            FormXYZ formXYZ = new FormXYZ();
            formXYZ.Show();
        }

        private void serialDebugMenuItem_Click(object sender, EventArgs e)
        {
            //formSerial = new FormSerial(this);
            formSerial.Show();
        }

        private void otherDebugMenuItem_Click(object sender, EventArgs e)
        {
            FormOther formOther = new FormOther();
            formOther.Show();
        }

        private void ParameterDebugMenuItem_Click(object sender, EventArgs e)
        {
            FormParameterTest parameterTest = new FormParameterTest();
            parameterTest.Show();
        }

        private void monitorToolMenuItem_Click(object sender, EventArgs e)
        {
            FormMonitor formMonitor = new FormMonitor();
            formMonitor.Show();
        }

        private void gCodeToolMenuItem_Click(object sender, EventArgs e)
        {
            FormImportGCode formImportGCode = new FormImportGCode();
            formImportGCode.Show();
        }

        private void commandProtocolMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "Help";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Process.Start(path);
        }

        private void materialParameterMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "Help";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Process.Start(path);
        }

        int multiply = 3;
        Bitmap bmp;
        Graphics graphics;
        private void changeLayer(int index)
        {
            printer.layerIndex = index;
            printer.channelIndex = printProject.printLayers[printer.layerIndex].channel;
            if (dgvPrintLayer.SelectedRows.Count > 0)
            {
                //由于此线程(非主线程)会改变界面外观，需要用Invoke来避免程序出错。另一例见btnSend_Click()
                Invoke(new EventHandler(delegate
                {
                    lblLayerNow.Text = "" + (1 + printer.layerIndex);
                    lblLayerStart.Text = printProject.printLayers[printer.layerIndex].x_start.ToString("0.00") + " mm * " + printProject.printLayers[printer.layerIndex].y_start.ToString("0.00") + " mm * " + printProject.printLayers[printer.layerIndex].z_start.ToString("0.00") + " mm";
                    lblLayerSize.Text = printProject.printLayers[printer.layerIndex].x_size.ToString("0.00") + " mm * " + printProject.printLayers[printer.layerIndex].y_size.ToString("0.00") + " mm ";
                    lblLayerInterval.Text = printProject.printLayers[printer.layerIndex].interval + " mm";
                    lblLayerSolidTime.Text = printProject.printLayers[printer.layerIndex].solidfyTime + "s";
                    
                }));
            }
            else
            {
                Invoke(new EventHandler(delegate
                {
                    lblLayerNow.Text = "";
                    lblLayerSize.Text = "";
                    lblLayerInterval.Text = "";
                    lblLayerStart.Text = "";
                    lblLayerSolidTime.Text = "";
                }));
            }
            bmp = new Bitmap((int)printer.maxY * multiply, (int)printer.maxX * multiply);
            graphics = Graphics.FromImage(bmp);
            if (printProject.printLayers[printer.layerIndex].points.Count != 0)
            {
                Point3D begin = printProject.printLayers[printer.layerIndex].points[0];
                for (int i = 1; i < printProject.printLayers[printer.layerIndex].points.Count; i++)
                {
                    Point3D next = printProject.printLayers[printer.layerIndex].points[i];
                    if (next.mode == 1)
                    {
                        graphics.DrawLine(new Pen(Color.Black, 2), float.Parse((begin.Y * multiply).ToString()), float.Parse((begin.X * multiply).ToString()), float.Parse((next.Y * multiply).ToString()), float.Parse((next.X * multiply).ToString()));
                    }
                    begin = next;
                }
            }
            picLayerPath.Image = bmp;
        }

        private void btnAddLayer_Click(object sender, EventArgs e)
        {
            FormLayer addLayer = new FormLayer();
            if (printProject.printLayers.Count == 0)
            {
                addLayer = new FormLayer(new PrintLayer(0, 0, 2, 20, 20, 0, 0, 0, 0, 0.1f));
            }
            else
            {
                addLayer = new FormLayer(printProject.printLayers[printProject.printLayers.Count-1]);
            }
            addLayer.AddLayerEvent += AddLayer_AddLayerEvent;
            addLayer.ShowDialog();
        }

        private void AddLayer_AddLayerEvent(PrintLayer layerChanged)
        {
            printProject.printLayers.Add(layerChanged);
            flushDgv();
            if (printProject.printLayers.Count == 1)
            {
                changeLayer(0);
            }
        }

        private void btnDelLayer_Click(object sender, EventArgs e)
        {
            if (printProject.printLayers.Count == 0)
            {
                MessageBox.Show("无数据!");
                return;
            }
            DialogResult RSS = MessageBox.Show(this, "确定要删除选中行数据码？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            switch (RSS)
            {
                case DialogResult.Yes:
                    printProject.printLayers.RemoveAt(printer.layerIndex);
                    if (printProject.printLayers.Count == 0)
                    {
                        printer.layerIndex = -1;
                    }
                    else
                    {
                        changeLayer(0);
                    }
                    flushDgv();
                    break;
                case DialogResult.No:
                    break;
            }
        }

        private void btnEditLayer_Click(object sender, EventArgs e)
        {
            if (printProject.printLayers.Count == 0)
            {
                MessageBox.Show("无数据!");
                return;
            }
            FormLayer editLayer = new FormLayer(printProject.printLayers[printer.layerIndex]);
            editLayer.AddLayerEvent += EditLayer_AddLayerEvent;
            editLayer.ShowDialog();
        }

        private void EditLayer_AddLayerEvent(PrintLayer layerChanged)
        {
            printProject.printLayers[printer.layerIndex] = layerChanged;
            flushDgv();
        }

        private void btnUpLayer_Click(object sender, EventArgs e)
        {
            if (printProject.printLayers.Count == 0)
            {
                MessageBox.Show("无数据!");
                return;
            }
            if (printer.layerIndex == 0)
            {
                MessageBox.Show("已经是第一行了!");
                return;
            }
            printProject.printLayers.Insert(printer.layerIndex - 1, printProject.printLayers[printer.layerIndex]);
            printProject.printLayers.RemoveAt(printer.layerIndex + 1);
            int temp = printer.layerIndex;
            flushDgv();
            dgvPrintLayer.Rows[temp - 1].Selected = true;
            printer.layerIndex = dgvPrintLayer.SelectedRows[0].Index;
        }

        private void btnDownLayer_Click(object sender, EventArgs e)
        {
            if (printProject.printLayers.Count == 0)
            {
                MessageBox.Show("无数据!");
                return;
            }
            if (printer.layerIndex == printProject.printLayers.Count - 1)
            {
                MessageBox.Show("已经是最后一行了!");
                return;
            }
            printProject.printLayers.Insert(printer.layerIndex + 2, printProject.printLayers[printer.layerIndex]);
            printProject.printLayers.RemoveAt(printer.layerIndex);
            int temp = printer.layerIndex;
            flushDgv();
            dgvPrintLayer.Rows[temp + 1].Selected = true;
            printer.layerIndex = dgvPrintLayer.SelectedRows[0].Index;
        }

        private void dgvPrintLayer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (printer.printerState != 0)
            {
                MessageBox.Show("正在打印中，不能切换层");
                return;
            }
            changeLayer(dgvPrintLayer.SelectedRows[0].Index);
        }

        private void btnPrintOneLayer_Click(object sender, EventArgs e)
        {
            if (!printer.serialPortController.serialPort.IsOpen)
            {
                MessageBox.Show("串口未打开！");
                return;
            }
            if (printer.layerIndex == -1)
            {
                MessageBox.Show("无打印层！");
                return;
            }
            if (printer.printerState != 0)
            {
                MessageBox.Show("正在打印，请稍后再试！");
                return;
            }
            Thread thread = new Thread(new ThreadStart(LoadOneLayerData))
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void LoadOneLayerData()
        {
            Invoke(new EventHandler(delegate
            {
                statusTips.Text = "正在设置通道...";
            }));
            setChannel(printProject.printLayers[printer.layerIndex].channel);//打印一层之前先设定当前层的通道
            Invoke(new EventHandler(delegate
            {
                statusTips.Text = "正在发送路径指令";
            }));
            List<Point3D> points = printProject.printLayers[printer.layerIndex].points;
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Maximum = points.Count;
            }));
            for (int i = 0; i < points.Count; i++)
            {
                Invoke(new EventHandler(delegate
                {
                    toolStripProgressBar1.Value = i + 1;
                }));
                printer.toPoint(points[i]);
            }
            //new PrintChannel().setAirPress(0);//打印完一层后将气压设置为0
            if (printProject.printLayers[printer.layerIndex].solidfyTime != 0)
            {
                printer.moveToSolidifyZero();
                printer.SendGCode("M250");//打开固化灯
                closeSolidfyLight.Start();
            }
            if (ckbIsToZero.Checked)
            {
                printer.moveToAbsolute(0, 0, 0);
            }
            Invoke(new EventHandler(delegate
            {
                statusTips.Text = "指令发送完成";
            }));
        }

        private void CloseSolidfyLight_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (printer.printerState == 0)
            {
                closeSolidfyLight.Stop();
                printer.printerState = 1;
                Thread.Sleep(printProject.printLayers[printer.layerIndex].solidfyTime * 1000);
                printer.printerState = 0;
                printer.SendGCode("M251");//关闭固化灯
            }
        }

        private void btnPrintAllLayer_Click(object sender, EventArgs e)
        {
            if (!printer.serialPortController.serialPort.IsOpen)
            {
                MessageBox.Show("串口未打开！");
                return;
            }
            if (printer.layerIndex == -1)
            {
                MessageBox.Show("无打印层！");
                return;
            }
            if (printer.printerState != 0)
            {
                MessageBox.Show("正在打印，请稍后再试！");
                return;
            }
            printer.layerIndex = 0;
            Thread thread = new Thread(new ThreadStart(LoadLayerData))
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void LoadLayerData()
        {
            List<Point3D> points = printProject.printLayers[printer.layerIndex].points;
            printer.channelIndex = printProject.printLayers[printer.layerIndex].channel;
            if (points.Count > 0)
            {
                printer.toPoint(points[0]);//该行是为了正确移动到打印顶点
            }
            setChannel(printProject.printLayers[printer.layerIndex].channel);//打印一层之前先设定当前层的通道
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Maximum = points.Count;
            }));
            for (int j = 1; j < points.Count; j++)
            {
                Invoke(new EventHandler(delegate
                {
                    toolStripProgressBar1.Value = j + 1;
                }));
                printer.toPoint(points[j]);
            }
            //new PrintChannel().setAirPress(0);//打印完一层后将气压设置为0
            if (printProject.printLayers[printer.layerIndex].solidfyTime != 0)
            {
                printer.moveToSolidifyZero();
            }
            timerIsStop.Start();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (printProject == null) return;
            if (MessageBox.Show(
            "窗口关闭后，未保存数据即将丢失！是否现在关闭窗口",
            "提示",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question) != DialogResult.OK)
            {
                e.Cancel = true;
                return;
            }
        }

        private bool isOpen = false;

        public void switchSerial()
        {
            if (isOpen)
            {
                try
                {
                    printer.serialPortController.serialPort.Close();
                    lblSerialState.Text = "关闭";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                toolStripButton1.Image = Properties.Resources.开关3;
                statusTips.Text = "串口已关闭";
            }
            else
            {
                if (printer.serialPortController.names.Length == 0)
                {
                    MessageBox.Show("无串口");
                    return;
                }
                try
                {
                    printer.serialPortController.serialPort.Open();
                    lblSerialState.Text = "打开";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                toolStripButton1.Image = Properties.Resources.开关4;
                statusTips.Text = "串口已打开";
            }
            isOpen = !isOpen;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            switchSerial();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            printer.moveToAbsolute(0, 0, 0);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            timerIsStop.Stop();
            printer.Pause();
            //new PrintChannel().setAirPress(0);//暂停后将气压设置为0
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (printProject == null)
            {
                MessageBox.Show("未打开项目");
                return;
            }
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "PrintProject\\" + printProject.name + printProject.id + ".obj";
            Utilities.SaveObject(path, printProject);
            statusTips.Text = "项目已保存至" + path;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (printProject == null)
            {
                MessageBox.Show("未创建项目！");
                return ;
            }
            if (printProject.printLayers.Count == 0)
            {
                MessageBox.Show("未创建打印层！");
                return;
            }
            setChannel(printProject.printLayers[printer.layerIndex].channel);//继续之前先设定当前层的通道
            printer.Continue();
        }

        bool solodfyLight = false;
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (solodfyLight)
            {
                printer.SendGCode("M251");
            }
            else
            {
                printer.SendGCode("M250");
            }
            solodfyLight = !solodfyLight;
        }

        int look = 300;
        private void OpenGLControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (look - e.Delta / 10 < 0)
                {
                    return;
                }
                look -= e.Delta / 10;
            }
        }

        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl1.OpenGL;
            gl.ClearColor(0.8f, 0.8f, 0.8f, 0.0f);
            //设置清除颜色缓冲区的值：red、green、blue、alpha，参数范围：[0.0,1.0]
            //注意：ClearColor只是在设置，触发由Clear(GL_Color_Buffer_Bit)处理
        }

        private double rotation = 0;
        private bool isRight = false;
        double eyez = -80;

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            OpenGL gl = openGLControl1.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            //清除缓存操作，可以清除的缓存有：
            //GL_Color_Buffer_Bit颜色缓存、GL_Depth_Buffer_Bit深度缓存、
            //GL_Accum_Buffer_Bit累积缓存、GL_Stencil_Buffer_Bit模板缓存
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(30.0f, (double)Width / (double)Height, 0.01, 1000.0);
            gl.LookAt(look, look, eyez, printer.maxX / 2, printer.maxY / 2, printer.maxZ / 2, 0, 0, -1);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            //加载单位矩阵，目的在于清除之前的矩阵操作(平移、缩放、旋转等操作)
            gl.Rotate(rotation, 0.0f, 0.0f, 1.0f);
            //旋转矩阵操作，Rotate四个参数分别是：旋转角度（角度制单位）、X、Y、Z
            //X、Y、Z表示：原点到（X，Y，Z）方向的矢量
            gl.Begin(OpenGL.GL_TRIANGLES);
            gl.Color(0.5f, 1.0f, 1.0f);
            gl.Vertex(printer.X + 0f, printer.Y + 0f, printer.Z + 0f);
            gl.Color(1.0f, 0.5f, 1.0f);
            gl.Vertex(printer.X - 10f, printer.Y + 10f, printer.Z - 20f);
            gl.Color(1.0f, 1.0f, 0.5f);
            gl.Vertex(printer.X - 10f, printer.Y - 10f, printer.Z - 20f);

            gl.Color(0.5f, 1.0f, 1.0f);
            gl.Vertex(printer.X + 0f, printer.Y + 0f, printer.Z + 0f);
            gl.Color(1.0f, 1.0f, 0.5f);
            gl.Vertex(printer.X - 10f, printer.Y - 10f, printer.Z - 20f);
            gl.Color(1.0f, 0.5f, 1.0f);
            gl.Vertex(printer.X + 10f, printer.Y - 10f, printer.Z - 20f);

            gl.Color(0.5f, 1.0f, 1.0f);
            gl.Vertex(printer.X + 0f, printer.Y + 0f, printer.Z + 0f);
            gl.Color(1.0f, 0.5f, 1.0f);
            gl.Vertex(printer.X + 10f, printer.Y - 10f, printer.Z - 20f);
            gl.Color(1.0f, 1.0f, 0.5f);
            gl.Vertex(printer.X + 10f, printer.Y + 10f, printer.Z - 20f);

            gl.Color(0.5f, 1.0f, 1.0f);
            gl.Vertex(printer.X + 0f, printer.Y + 0f, printer.Z + 0f);
            gl.Color(1.0f, 1.0f, 0.5f);
            gl.Vertex(printer.X - 10f, printer.Y + 10f, printer.Z - 20f);
            gl.Color(1.0f, 0.5f, 1.0f);
            gl.Vertex(printer.X + 10f, printer.Y + 10f, printer.Z - 20f);

            gl.Color(0.5f, 0.5f, 0.5f);
            gl.Vertex(printer.maxX, printer.maxY, planeHeight);
            gl.Vertex(printer.maxX, 0, planeHeight);
            gl.Vertex(0, printer.maxY, planeHeight);
            gl.Vertex(0, 0, planeHeight);
            gl.Vertex(printer.maxX, 0, planeHeight);
            gl.Vertex(0, printer.maxY, planeHeight);

            gl.End();

            //路径
            gl.Begin(OpenGL.GL_LINES);
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(400.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 400.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 400.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            for (int i = 0; i < lines.Count; i++)
            {
                gl.Vertex(lines[i].start.X, lines[i].start.Y, lines[i].start.Z);
                gl.Vertex(lines[i].end.X, lines[i].end.Y, lines[i].end.Z);
            }
            gl.End();
            //OpenGL每绘制一次，rotation累加3.0f，gl.Rotate的旋转角就会增加3.0°，进而实现了图元绕Y轴旋转的效果
            if (ckbIsAutoRotate.Checked)
            {
                if (rotation > 30) isRight = true;
                if (rotation < -30) isRight = false;
                rotation = isRight?rotation-0.5f: rotation + 0.5f;
            }
        }

        private void openGLControl1_Resized(object sender, EventArgs e)
        {

        }

        bool isMouseDown = false;
        Point downPoint = new Point(0, 0);
        Point movePoint = new Point(0, 0);
        Point upPoint = new Point(0, 0);

        private void openGLControl1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            downPoint = new Point(e.X, e.Y);
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                movePoint = e.Location;
                rotation -= (movePoint.X - downPoint.X) / 500.0;
                eyez -= (movePoint.Y - downPoint.Y) / 500.0;
            }
        }

        private void openGLControl1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            upPoint = new Point(e.X, e.Y);
            rotation -= (movePoint.X - upPoint.X) / 500.0;
            eyez -= (movePoint.Y - upPoint.Y) / 500.0;
        }

        private void btnChangeName_Click(object sender, EventArgs e)
        {
            if (txtProjectName.ReadOnly)
            {
                txtProjectName.ReadOnly = false;
                btnChangeName.Text = "确定";
            }
            else
            {
                txtProjectName.ReadOnly = true;
                btnChangeName.Text = "修改名称";
                printProject.name = txtProjectName.Text;
                statusTips.Text = "项目名称已修改为" + printProject.name;
            }
        }

        private void rtbProjectNote_TextChanged(object sender, EventArgs e)
        {
            printProject.description = rtbProjectNote.Text;
        }

        private void setChannel(int index)
        {
            printer.channelIndex = index;
            PrintChannel.setChannel(printer.channelIndex);
            printProject.printChannels[printer.channelIndex].setAirPress();
            printProject.printChannels[printer.channelIndex].setPrintSpeed();
            printProject.printChannels[printer.channelIndex].setT1T2T3();
        }

        private void btnModifyChannel_Click(object sender, EventArgs e)
        {
            if (btnModifyChannel.Text == "修改")
            {
                txtAirPress.ReadOnly = false;
                txtTemperature.ReadOnly = false;
                txtPrintSpeed.ReadOnly = false;
                txtT1.ReadOnly = false;
                txtT2.ReadOnly = false;
                txtT3.ReadOnly = false;
                txtPositionX.ReadOnly = false;
                txtPositionY.ReadOnly = false;
                btnModifyChannel.Text = "确认修改";
            }
            else
            {
                txtAirPress.ReadOnly = true;
                txtTemperature.ReadOnly = true;
                txtPrintSpeed.ReadOnly = true;
                txtT1.ReadOnly = true;
                txtT2.ReadOnly = true;
                txtT3.ReadOnly = true;
                txtPositionX.ReadOnly = true;
                txtPositionY.ReadOnly = true;

                printProject.printChannels[cbbChannel.SelectedIndex].AirPress = double.Parse(txtAirPress.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].Temperature = double.Parse(txtTemperature.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].PrintSpeed = double.Parse(txtPrintSpeed.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].T1 = double.Parse(txtT1.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].T2 = double.Parse(txtT2.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].T3 = double.Parse(txtT3.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].position.X = int.Parse(txtPositionX.Text);
                printProject.printChannels[cbbChannel.SelectedIndex].position.Y = int.Parse(txtPositionY.Text);

                flashChannellabel();
                setChannel(cbbChannel.SelectedIndex);//修改后重新设定通道

                Invoke(new EventHandler(delegate
                {
                    statusTips.Text = "通道参数修改完成";
                }));

                btnModifyChannel.Text = "修改";
            }
        }

        private void cbbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            txtAirPress.Text = printProject.printChannels[cbbChannel.SelectedIndex].AirPress + "";
            txtTemperature.Text = printProject.printChannels[cbbChannel.SelectedIndex].Temperature + "";
            txtPrintSpeed.Text = printProject.printChannels[cbbChannel.SelectedIndex].PrintSpeed + "";
            txtT1.Text = printProject.printChannels[cbbChannel.SelectedIndex].T1 + "";
            txtT2.Text = printProject.printChannels[cbbChannel.SelectedIndex].T2 + "";
            txtT3.Text = printProject.printChannels[cbbChannel.SelectedIndex].T3 + "";
            txtPositionX.Text = printProject.printChannels[cbbChannel.SelectedIndex].position.X + "";
            txtPositionY.Text = printProject.printChannels[cbbChannel.SelectedIndex].position.Y + "";
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = 4;
                statusTips.Text = "正在切换通道";
            }));
            printer.channelIndex = cbbChannel.SelectedIndex;
            PrintChannel.setChannel(cbbChannel.SelectedIndex);
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Value = 1;
            }));
            printProject.printChannels[cbbChannel.SelectedIndex].setAirPress(double.Parse(txtAirPress.Text));
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Value = 2;
            }));
            printProject.printChannels[cbbChannel.SelectedIndex].setPrintSpeed(double.Parse(txtPrintSpeed.Text));
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Value = 3;
            }));
            printProject.printChannels[cbbChannel.SelectedIndex].setT1T2T3(double.Parse(txtT1.Text), double.Parse(txtT2.Text), double.Parse(txtT3.Text));
            Invoke(new EventHandler(delegate
            {
                toolStripProgressBar1.Value = 4;
            }));
            Invoke(new EventHandler(delegate
            {
                statusTips.Text = "切换通道完成";
            }));
        }

        private void flashChannellabel()
        {
            lblOne1.Text = printProject.printChannels[0].AirPress + "";
            lblTwo1.Text = printProject.printChannels[1].AirPress + "";
            lblThree1.Text = printProject.printChannels[2].AirPress + "";
            lblFour1.Text = printProject.printChannels[3].AirPress + "";
            lblFive1.Text = printProject.printChannels[4].AirPress + "";

            lblOne2.Text = printProject.printChannels[0].Temperature + "";
            lblTwo2.Text = printProject.printChannels[1].Temperature + "";
            lblThree2.Text = printProject.printChannels[2].Temperature + "";
            lblFour2.Text = printProject.printChannels[3].Temperature + "";
            lblFive2.Text = printProject.printChannels[4].Temperature + "";

            lblOne3.Text = printProject.printChannels[0].PrintSpeed + "";
            lblTwo3.Text = printProject.printChannels[1].PrintSpeed + "";
            lblThree3.Text = printProject.printChannels[2].PrintSpeed + "";
            lblFour3.Text = printProject.printChannels[3].PrintSpeed + "";
            lblFive3.Text = printProject.printChannels[4].PrintSpeed + "";

            lblOne4.Text = printProject.printChannels[0].T1 + "";
            lblTwo4.Text = printProject.printChannels[1].T1 + "";
            lblThree4.Text = printProject.printChannels[2].T1 + "";
            lblFour4.Text = printProject.printChannels[3].T1 + "";
            lblFive4.Text = printProject.printChannels[4].T1 + "";

            lblOne5.Text = printProject.printChannels[0].T2 + "";
            lblTwo5.Text = printProject.printChannels[1].T2 + "";
            lblThree5.Text = printProject.printChannels[2].T2 + "";
            lblFour5.Text = printProject.printChannels[3].T2 + "";
            lblFive5.Text = printProject.printChannels[4].T2 + "";

            lblOne6.Text = printProject.printChannels[0].T3 + "";
            lblTwo6.Text = printProject.printChannels[1].T3 + "";
            lblThree6.Text = printProject.printChannels[2].T3 + "";
            lblFour6.Text = printProject.printChannels[3].T3 + "";
            lblFive6.Text = printProject.printChannels[4].T3 + "";

            lblOne7.Text = printProject.printChannels[0].position.X + "";
            lblTwo7.Text = printProject.printChannels[1].position.X + "";
            lblThree7.Text = printProject.printChannels[2].position.X + "";
            lblFour7.Text = printProject.printChannels[3].position.X + "";
            lblFive7.Text = printProject.printChannels[4].position.X + "";

            lblOne8.Text = printProject.printChannels[0].position.Y + "";
            lblTwo8.Text = printProject.printChannels[1].position.Y + "";
            lblThree8.Text = printProject.printChannels[2].position.Y + "";
            lblFour8.Text = printProject.printChannels[3].position.Y + "";
            lblFive8.Text = printProject.printChannels[4].position.Y + "";
        }

        private void btnToPrintZero_Click(object sender, EventArgs e)
        {
            if (printProject.printLayers.Count == 0)
            {
                return;
            }
            Point3D point = printProject.printLayers[printer.layerIndex].points[0];
            printer.moveToRelative(point.X, point.Y, point.Z);
        }

        private void btnMoveZ_Click(object sender, EventArgs e)
        {
            printer.moveToAbsolute(printer.X, printer.Y, (float)numericUpDown3.Value);
        }

        private void openGLControl2_OpenGLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = openGLControl2.OpenGL;
            gl.ClearColor(1f, 1f, 1f, 0.0f);
        }

        private double rotation2;
        int look2 = 150;

        private void openGLControl2_OpenGLDraw(object sender, RenderEventArgs args)
        {
            OpenGL gl = openGLControl2.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 500.0);
            gl.LookAt(look2, look2, look2, 0, 0, 0, 0, 0, 1);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.LoadIdentity();
            gl.Rotate(rotation2, 0.0f, 0.0f, 1.0f);

            gl.Begin(OpenGL.GL_LINES);
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(400.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 400.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 400.0f);

            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(printer.maxX, printer.maxY, 0);
            gl.Vertex(printer.maxX, 0, 0);
            gl.Vertex(printer.maxX, printer.maxY, 0);
            gl.Vertex(0, printer.maxY, 0);
            gl.Vertex(printer.maxX, 0, printer.maxZ);
            gl.Vertex(printer.maxX, 0, 0);
            gl.Vertex(printer.maxX, 0, printer.maxZ);
            gl.Vertex(0, 0, printer.maxZ);
            gl.Vertex(0, printer.maxY, printer.maxZ);
            gl.Vertex(0, printer.maxY, 0);
            gl.Vertex(0, printer.maxY, printer.maxZ);
            gl.Vertex(0, 0, printer.maxZ);
            gl.Vertex(printer.maxX, printer.maxY, printer.maxZ);
            gl.Vertex(0, printer.maxY, printer.maxZ);
            gl.Vertex(printer.maxX, printer.maxY, printer.maxZ);
            gl.Vertex(printer.maxX, 0, printer.maxZ);
            gl.Vertex(printer.maxX, printer.maxY, printer.maxZ);
            gl.Vertex(printer.maxX, printer.maxY, 0);

            //模型切片后显示
            gl.Color(1.0f, 0.0f, 0.0f);
            foreach (var item in printProject.model.layers)
            {
                List<Line3D> lines = item;
                for (int i = 0; i < lines.Count; i++)
                {
                    gl.Vertex(lines[i].start.X, lines[i].start.Y, lines[i].start.Z);
                    gl.Vertex(lines[i].end.X, lines[i].end.Y, lines[i].end.Z);
                }
            }

            gl.End();

            //原始模型显示
            gl.Begin(OpenGL.GL_TRIANGLES);
            for (int i = 0; i < printProject.model.triangles.Count; i++)
            {
                gl.Color(0.5f, 1.0f, 1.0f, 1.0f);
                gl.Vertex(printProject.model.triangles[i].p1.X, printProject.model.triangles[i].p1.Y, printProject.model.triangles[i].p1.Z);
                gl.Color(1.0f, 0.5f, 1.0f, 1.0f);
                gl.Vertex(printProject.model.triangles[i].p2.X, printProject.model.triangles[i].p2.Y, printProject.model.triangles[i].p2.Z);
                gl.Color(1.0f, 1.0f, 0.5f, 1.0f);
                gl.Vertex(printProject.model.triangles[i].p3.X, printProject.model.triangles[i].p3.Y, printProject.model.triangles[i].p3.Z);
            }
            gl.End();

            rotation2 += 1f;
        }

        private void OpenGLControl2_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (e.Delta < 0)
                {
                    look2 -= e.Delta / 50;
                }
                else
                {
                    look2 -= e.Delta / 50;
                }
            }
        }

        private void btnImportModel_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "stl文件|*.stl|STL文件|*.STL"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                printProject.model = new Model3D
                {
                    path = dialog.FileName
                };
                Utilities.ReadSTLFile(dialog.FileName, printProject.model);
            }
            flushProjectLabel();
        }

        private void btnModifyModel_Click(object sender, EventArgs e)
        {
            if (printProject.model.path == "")
            {
                MessageBox.Show("请先导入模型");
                return;
            }
            printProject.model.moveX = float.Parse(txtModelX.Text);
            printProject.model.moveY = float.Parse(txtModelY.Text);
            printProject.model.moveZ = float.Parse(txtModelZ.Text);
            printProject.model.multiple = float.Parse(txtModelMult.Text);
            Utilities.ReadSTLFile(printProject.model.path, printProject.model);
        }

        
        private void btnSlice_Click(object sender, EventArgs e)
        {
            if (printProject.model.path == "")
            {
                MessageBox.Show("请先导入模型");
                return;
            }
            FormSlice formSlice = new FormSlice();
            formSlice.SliceEvent += FormSlice_SliceEvent;
            formSlice.ShowDialog();
        }

        private void FormSlice_SliceEvent(float maxHeight, float minHeight, float precision, int fillMode, float fillInterval, int channel, int solidifyTime)
        {
            //分层
            SliceContext sliceContext = new SliceContext(new IsothickSliceStrategy());
            printProject.model.layers = sliceContext.executeStrategy(printProject.model, maxHeight, minHeight);
            //填充
            FillContext fillContext = new FillContext(new SimpleFillStrategy());
            for (int i = 0; i < printProject.model.layers.Length; i++)
            {
                List<Point3D> tempPoints = fillContext.executeStrategy(printProject.model.layers[i], fillInterval, i % 2);
                foreach (Point3D point in tempPoints)//换算Z轴坐标到打印平面
                {
                    point.Z = planeHeight - point.Z;
                }
                PrintLayer printLayer = new PrintLayer();//打印层
                printLayer.points = tempPoints;
                printLayer.mode = 10 + fillMode;
                printLayer.channel = channel;
                printLayer.interval = fillInterval;
                printLayer.solidfyTime = solidifyTime;
                printProject.printLayers.Add(printLayer);
            }
            flushDgv();
        }

        float planeHeight = 60;

        private void btnMovePlane_Click(object sender, EventArgs e)
        {
            planeHeight = (float)numPlaneHeight.Value;
        }

        private void buttonEndPrint_Click(object sender, EventArgs e)
        {
            timerIsStop.Stop();
        }
    }
}
