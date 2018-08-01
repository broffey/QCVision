namespace QC_Vision
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.panel1 = new System.Windows.Forms.Panel();
            this.trayImageBox = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trayImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.trayImageBox);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1135, 711);
            this.panel1.TabIndex = 0;
            // 
            // trayImageBox
            // 
            this.trayImageBox.Enabled = false;
            this.trayImageBox.Image = ((System.Drawing.Image)(resources.GetObject("trayImageBox.Image")));
            this.trayImageBox.InitialImage = ((System.Drawing.Image)(resources.GetObject("trayImageBox.InitialImage")));
            this.trayImageBox.Location = new System.Drawing.Point(889, 481);
            this.trayImageBox.Name = "trayImageBox";
            this.trayImageBox.Size = new System.Drawing.Size(245, 229);
            this.trayImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.trayImageBox.TabIndex = 0;
            this.trayImageBox.TabStop = false;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1272, 803);
            this.Controls.Add(this.panel1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trayImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox trayImageBox;
    }
}