namespace AWL3DPrintSoftware
{
    partial class FormOther
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
            this.btnSolidfyLight = new System.Windows.Forms.Button();
            this.btnToSolidfyZero = new System.Windows.Forms.Button();
            this.btnToPrintZero = new System.Windows.Forms.Button();
            this.btnToZero = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSolidfyLight
            // 
            this.btnSolidfyLight.Location = new System.Drawing.Point(264, 89);
            this.btnSolidfyLight.Name = "btnSolidfyLight";
            this.btnSolidfyLight.Size = new System.Drawing.Size(146, 39);
            this.btnSolidfyLight.TabIndex = 7;
            this.btnSolidfyLight.Text = "开关固化灯";
            this.btnSolidfyLight.UseVisualStyleBackColor = true;
            this.btnSolidfyLight.Click += new System.EventHandler(this.btnSolidfyLight_Click);
            // 
            // btnToSolidfyZero
            // 
            this.btnToSolidfyZero.Location = new System.Drawing.Point(46, 134);
            this.btnToSolidfyZero.Name = "btnToSolidfyZero";
            this.btnToSolidfyZero.Size = new System.Drawing.Size(146, 39);
            this.btnToSolidfyZero.TabIndex = 6;
            this.btnToSolidfyZero.Text = "回到固化原点";
            this.btnToSolidfyZero.UseVisualStyleBackColor = true;
            this.btnToSolidfyZero.Click += new System.EventHandler(this.btnToSolidfyZero_Click);
            // 
            // btnToPrintZero
            // 
            this.btnToPrintZero.Location = new System.Drawing.Point(46, 89);
            this.btnToPrintZero.Name = "btnToPrintZero";
            this.btnToPrintZero.Size = new System.Drawing.Size(146, 39);
            this.btnToPrintZero.TabIndex = 5;
            this.btnToPrintZero.Text = "回到打印原点";
            this.btnToPrintZero.UseVisualStyleBackColor = true;
            this.btnToPrintZero.Click += new System.EventHandler(this.btnToPrintZero_Click);
            // 
            // btnToZero
            // 
            this.btnToZero.Location = new System.Drawing.Point(46, 44);
            this.btnToZero.Name = "btnToZero";
            this.btnToZero.Size = new System.Drawing.Size(146, 39);
            this.btnToZero.TabIndex = 4;
            this.btnToZero.Text = "回到机器零点";
            this.btnToZero.UseVisualStyleBackColor = true;
            this.btnToZero.Click += new System.EventHandler(this.btnToZero_Click);
            // 
            // FormOther
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 223);
            this.Controls.Add(this.btnSolidfyLight);
            this.Controls.Add(this.btnToSolidfyZero);
            this.Controls.Add(this.btnToPrintZero);
            this.Controls.Add(this.btnToZero);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormOther";
            this.Text = "其他调试";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSolidfyLight;
        private System.Windows.Forms.Button btnToSolidfyZero;
        private System.Windows.Forms.Button btnToPrintZero;
        private System.Windows.Forms.Button btnToZero;
    }
}