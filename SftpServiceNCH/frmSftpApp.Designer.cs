namespace SftpServiceNCH
{
    partial class frmSftpApp
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtLog = new TextBox();
            progressBar = new ProgressBar();
            lblProgress = new Label();
            lblStatus = new Label();
            btnUpload = new Button();
            btnStartCycle = new Button();
            btnStopCycle = new Button();
            SuspendLayout();
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 13);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(776, 363);
            txtLog.TabIndex = 0;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 397);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(355, 23);
            progressBar.TabIndex = 1;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(373, 405);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(38, 15);
            lblProgress.TabIndex = 2;
            lblProgress.Text = "label1";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 423);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(38, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "label1";
            // 
            // btnUpload
            // 
            btnUpload.Location = new Point(551, 415);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(75, 23);
            btnUpload.TabIndex = 6;
            btnUpload.Text = "Upload";
            btnUpload.UseVisualStyleBackColor = true;
            btnUpload.Click += BtnUpload_Click;
            // 
            // btnStartCycle
            // 
            btnStartCycle.Location = new Point(632, 415);
            btnStartCycle.Name = "btnStartCycle";
            btnStartCycle.Size = new Size(75, 23);
            btnStartCycle.TabIndex = 9;
            btnStartCycle.Text = "Start";
            btnStartCycle.UseVisualStyleBackColor = true;
            btnStartCycle.Click += btnStartCycle_Click;
            // 
            // btnStopCycle
            // 
            btnStopCycle.Location = new Point(713, 415);
            btnStopCycle.Name = "btnStopCycle";
            btnStopCycle.Size = new Size(75, 23);
            btnStopCycle.TabIndex = 10;
            btnStopCycle.Text = "Stop";
            btnStopCycle.UseVisualStyleBackColor = true;
            btnStopCycle.Click += btnStopCycle_Click;
            // 
            // frmSftpApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnStopCycle);
            Controls.Add(btnStartCycle);
            Controls.Add(btnUpload);
            Controls.Add(lblStatus);
            Controls.Add(lblProgress);
            Controls.Add(progressBar);
            Controls.Add(txtLog);
            Name = "frmSftpApp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "nCH SFTP Upload";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLog;
        private ProgressBar progressBar;
        private Label lblProgress;
        private Label lblStatus;
        private Button btnUpload;
        private Button btnStartCycle;
        private Button btnStopCycle;
    }
}
