using AForge.Controls;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace AWL3DPrintSoftware
{
    public partial class FormMonitor : Form
    {
        public FormMonitor()
        {
            InitializeComponent();
        }

        //摄像头
        FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        VideoCaptureDevice videoSource;
        VideoFileWriter videoWriter = new VideoFileWriter();
        System.Timers.Timer timeCount = new System.Timers.Timer();
        bool isRecord = false;
        int tickNum = 0;

        private void FormMonitor_Load(object sender, EventArgs e)
        {
            //初始化摄像头
            foreach (FilterInfo device in videoDevices)
            {
                cbbVideoDevice.Items.Add(device.Name);
            }
            timeCount.Interval = 1000;
            timeCount.AutoReset = true;
            timeCount.Elapsed += TimeCount_Elapsed;
        }

        private void TimeCount_Elapsed(object sender, ElapsedEventArgs e)
        {
            tickNum++;
            int temp = tickNum;
            int sec = temp % 60;
            int min = temp / 60;
            if (60 == min)
            {
                min = 0;
                min++;
            }
            int hour = min / 60;
            String tick = "";
            if (hour < 10)
            {
                tick += "0" + hour.ToString();
            }
            else
            {
                tick += hour.ToString();
            }
            tick += ":";
            if (min < 10)
            {
                tick += "0" + min.ToString();
            }
            else
            {
                tick += min.ToString();
            }
            tick += ":";
            if (sec < 10)
            {
                tick += "0" + sec.ToString();
            }
            else
            {
                tick += sec.ToString();
            }
            Invoke(new EventHandler(delegate
            {
                lblRECTime.Text = tick;
            }));
        }

        private void cbbVideoDevice_DropDown(object sender, EventArgs e)
        {
            cbbVideoDevice.Items.Clear();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                cbbVideoDevice.Items.Add(device.Name);
            }
        }

        private void cbbVideoDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShutCamera();
            videoSource = new VideoCaptureDevice(videoDevices[cbbVideoDevice.SelectedIndex].MonikerString);
            videoSourcePlayer1.VideoSource = videoSource;
            videoSourcePlayer1.NewFrame += VideoSourcePlayer_NewFrame;
            videoSourcePlayer1.Start();
        }

        private void VideoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (isRecord && videoWriter.IsOpen)
                {
                    videoWriter.WriteVideoFrame(image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("视频写入问题：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenMyVideo_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GetVideoPath());
        }

        private void btnOpenMyPhoto_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GetImagePath());
        }

        public void ShutCamera()
        {
            if (videoSourcePlayer1.VideoSource != null)
            {
                if (videoWriter.IsOpen)
                {
                    videoWriter.Close();
                    isRecord = false;
                    timeCount.Stop();
                    tickNum = 0;
                    //lightIsREC.State = UILightState.Off;
                    lblRECTime.Text = "00:00:00";
                }
                videoSourcePlayer1.SignalToStop();
                videoSourcePlayer1.WaitForStop();
                videoSourcePlayer1.VideoSource = null;
            }
        }

        private string GetVideoPath()
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "MyVideo";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private void btnBeginRecord_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoSourcePlayer1.IsRunning)
                {
                    Bitmap bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                    if (bitmap == null)
                    {
                        MessageBox.Show("摄像头未准备好", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //创建一个视频文件
                    String picName = GetVideoPath() + "\\" + DateTime.Now.ToString("yyyyMMdd HH-mm-ss") + ".avi";
                    timeCount.Start();//是否执行System.Timers.Timer.Elapsed事件；
                    isRecord = true;
                    //lightIsREC.State = UILightState.Blink;
                    videoWriter = new VideoFileWriter();
                    videoWriter.Open(picName, bitmap.Width, bitmap.Height, 10);
                }
                else
                {
                    MessageBox.Show("没有视频源输入，无法录制视频。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("摄像头异常：" + ex.Message);
            }
        }

        private void btnPauseRecord_Click(object sender, EventArgs e)
        {
            if (btnPauseRecord.Text.Trim() == "暂停录像")
            {
                isRecord = false;
                btnPauseRecord.Text = "恢复录像";
                timeCount.Enabled = false;  //暂停计时
                //lightIsREC.State = UILightState.Off;
                return;
            }
            if (btnPauseRecord.Text.Trim() == "恢复录像")
            {
                isRecord = true;
                btnPauseRecord.Text = "暂停录像";
                timeCount.Enabled = true;   //恢复计时
                //lightIsREC.State = UILightState.Blink;
            }
        }

        private void btnStopRecord_Click(object sender, EventArgs e)
        {
            try
            {
                videoWriter.Close();
                isRecord = false;
                //lightIsREC.State = UILightState.Off;
                timeCount.Stop();
                tickNum = 0;
                lblRECTime.Text = "00:00:00";
                MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetImagePath()
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "MyPicture";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        Bitmap photo;

        private void btnSnap_Click(object sender, EventArgs e)
        {
            photo = videoSourcePlayer1.GetCurrentVideoFrame();
            pictureBox1.Image = photo;
            try
            {
                string picName = GetImagePath() + "\\" + DateTime.Now.ToString("yyyyMMdd HH-mm-ss") + ".jpg";
                photo.Save(picName);
                MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FormMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShutCamera();
        }
    }
}
