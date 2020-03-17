namespace MagneticPixels
{
    partial class Form1
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
            this.videoSource = new System.Windows.Forms.ComboBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.originalImage = new System.Windows.Forms.PictureBox();
            this.editedImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.originalImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.editedImage)).BeginInit();
            this.SuspendLayout();
            // 
            // videoSource
            // 
            this.videoSource.FormattingEnabled = true;
            this.videoSource.Location = new System.Drawing.Point(0, 531);
            this.videoSource.Name = "videoSource";
            this.videoSource.Size = new System.Drawing.Size(365, 21);
            this.videoSource.TabIndex = 2;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(646, 531);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(175, 23);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(827, 531);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(173, 23);
            this.stopButton.TabIndex = 4;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // originalImage
            // 
            this.originalImage.Location = new System.Drawing.Point(0, 0);
            this.originalImage.Name = "originalImage";
            this.originalImage.Size = new System.Drawing.Size(502, 525);
            this.originalImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.originalImage.TabIndex = 5;
            this.originalImage.TabStop = false;
            // 
            // editedImage
            // 
            this.editedImage.Location = new System.Drawing.Point(508, 0);
            this.editedImage.Name = "editedImage";
            this.editedImage.Size = new System.Drawing.Size(492, 525);
            this.editedImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.editedImage.TabIndex = 6;
            this.editedImage.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 554);
            this.Controls.Add(this.editedImage);
            this.Controls.Add(this.originalImage);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.videoSource);
            this.Name = "Form1";
            this.Text = "MagneticPixels";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.originalImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.editedImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox videoSource;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.PictureBox originalImage;
        private System.Windows.Forms.PictureBox editedImage;
    }
}

