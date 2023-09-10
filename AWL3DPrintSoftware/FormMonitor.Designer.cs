namespace AWL3DPrintSoftware
{
    partial class FormMonitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblRECTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOpenMyPhoto = new System.Windows.Forms.Button();
            this.btnOpenMyVideo = new System.Windows.Forms.Button();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.btnSnap = new System.Windows.Forms.Button();
            this.btnPauseRecord = new System.Windows.Forms.Button();
            this.btnBeginRecord = new System.Windows.Forms.Button();
            this.cbbVideoDevice = new System.Windows.Forms.ComboBox();
            this.videoSourcePlayer1 = new AForge.Controls.VideoSourcePlayer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRECTime
            // 
            this.lblRECTime.AutoSize = true;
            this.lblRECTime.Location = new System.Drawing.Point(691, 52);
            this.lblRECTime.Name = "lblRECTime";
            this.lblRECTime.Size = new System.Drawing.Size(72, 21);
            this.lblRECTime.TabIndex = 18;
            this.lblRECTime.Text = "00:00:00";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(691, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 21);
            this.label1.TabIndex = 17;
            this.label1.Text = "REC";
            // 
            // btnOpenMyPhoto
            // 
            this.btnOpenMyPhoto.Location = new System.Drawing.Point(999, 446);
            this.btnOpenMyPhoto.Name = "btnOpenMyPhoto";
            this.btnOpenMyPhoto.Size = new System.Drawing.Size(188, 81);
            this.btnOpenMyPhoto.TabIndex = 16;
            this.btnOpenMyPhoto.Text = "打开照片文件夹";
            this.btnOpenMyPhoto.UseVisualStyleBackColor = true;
            this.btnOpenMyPhoto.Click += new System.EventHandler(this.btnOpenMyPhoto_Click);
            // 
            // btnOpenMyVideo
            // 
            this.btnOpenMyVideo.Location = new System.Drawing.Point(803, 446);
            this.btnOpenMyVideo.Name = "btnOpenMyVideo";
            this.btnOpenMyVideo.Size = new System.Drawing.Size(188, 81);
            this.btnOpenMyVideo.TabIndex = 14;
            this.btnOpenMyVideo.Text = "打开视频文件夹";
            this.btnOpenMyVideo.UseVisualStyleBackColor = true;
            this.btnOpenMyVideo.Click += new System.EventHandler(this.btnOpenMyVideo_Click);
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.Location = new System.Drawing.Point(592, 555);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(188, 60);
            this.btnStopRecord.TabIndex = 12;
            this.btnStopRecord.Text = "结束录像";
            this.btnStopRecord.UseVisualStyleBackColor = true;
            this.btnStopRecord.Click += new System.EventHandler(this.btnStopRecord_Click);
            // 
            // btnSnap
            // 
            this.btnSnap.Location = new System.Drawing.Point(803, 92);
            this.btnSnap.Name = "btnSnap";
            this.btnSnap.Size = new System.Drawing.Size(384, 59);
            this.btnSnap.TabIndex = 13;
            this.btnSnap.Text = "拍照";
            this.btnSnap.UseVisualStyleBackColor = true;
            this.btnSnap.Click += new System.EventHandler(this.btnSnap_Click);
            // 
            // btnPauseRecord
            // 
            this.btnPauseRecord.Location = new System.Drawing.Point(306, 555);
            this.btnPauseRecord.Name = "btnPauseRecord";
            this.btnPauseRecord.Size = new System.Drawing.Size(188, 60);
            this.btnPauseRecord.TabIndex = 11;
            this.btnPauseRecord.Text = "暂停录像";
            this.btnPauseRecord.UseVisualStyleBackColor = true;
            this.btnPauseRecord.Click += new System.EventHandler(this.btnPauseRecord_Click);
            // 
            // btnBeginRecord
            // 
            this.btnBeginRecord.Location = new System.Drawing.Point(25, 555);
            this.btnBeginRecord.Name = "btnBeginRecord";
            this.btnBeginRecord.Size = new System.Drawing.Size(188, 60);
            this.btnBeginRecord.TabIndex = 10;
            this.btnBeginRecord.Text = "开始录像";
            this.btnBeginRecord.UseVisualStyleBackColor = true;
            this.btnBeginRecord.Click += new System.EventHandler(this.btnBeginRecord_Click);
            // 
            // cbbVideoDevice
            // 
            this.cbbVideoDevice.FormattingEnabled = true;
            this.cbbVideoDevice.Location = new System.Drawing.Point(27, 20);
            this.cbbVideoDevice.Name = "cbbVideoDevice";
            this.cbbVideoDevice.Size = new System.Drawing.Size(186, 29);
            this.cbbVideoDevice.TabIndex = 9;
            this.cbbVideoDevice.DropDown += new System.EventHandler(this.cbbVideoDevice_DropDown);
            this.cbbVideoDevice.SelectedIndexChanged += new System.EventHandler(this.cbbVideoDevice_SelectedIndexChanged);
            // 
            // videoSourcePlayer1
            // 
            this.videoSourcePlayer1.Location = new System.Drawing.Point(12, 92);
            this.videoSourcePlayer1.Name = "videoSourcePlayer1";
            this.videoSourcePlayer1.Size = new System.Drawing.Size(768, 435);
            this.videoSourcePlayer1.TabIndex = 19;
            this.videoSourcePlayer1.Text = "videoSourcePlayer1";
            this.videoSourcePlayer1.VideoSource = null;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(803, 188);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(384, 217);
            this.pictureBox1.TabIndex = 20;
            this.pictureBox1.TabStop = false;
            // 
            // FormMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1201, 646);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.videoSourcePlayer1);
            this.Controls.Add(this.lblRECTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOpenMyPhoto);
            this.Controls.Add(this.btnOpenMyVideo);
            this.Controls.Add(this.btnStopRecord);
            this.Controls.Add(this.btnSnap);
            this.Controls.Add(this.btnPauseRecord);
            this.Controls.Add(this.btnBeginRecord);
            this.Controls.Add(this.cbbVideoDevice);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormMonitor";
            this.Text = "视频监控";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMonitor_FormClosing);
            this.Load += new System.EventHandler(this.FormMonitor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRECTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOpenMyPhoto;
        private System.Windows.Forms.Button btnOpenMyVideo;
        private System.Windows.Forms.Button btnStopRecord;
        private System.Windows.Forms.Button btnSnap;
        private System.Windows.Forms.Button btnPauseRecord;
        private System.Windows.Forms.Button btnBeginRecord;
        private System.Windows.Forms.ComboBox cbbVideoDevice;
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}