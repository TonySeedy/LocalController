namespace LocalController
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Controls
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.Label _lblAddress;
        private System.Windows.Forms.TextBox _txtPort;
        private System.Windows.Forms.TextBox _txtRefreshInterval;
        private System.Windows.Forms.TextBox _txtPassword;
        private System.Windows.Forms.Button _btnStart;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.TextBox _txtLogs;
        private System.Windows.Forms.TableLayoutPanel panel;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        
        // Static labels
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;

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
            _lblStatus = new Label();
            _lblAddress = new Label();
            _txtPort = new TextBox();
            _txtRefreshInterval = new TextBox();
            _txtPassword = new TextBox();
            _btnStart = new Button();
            _btnStop = new Button();
            _txtLogs = new TextBox();
            panel = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            buttonPanel = new FlowLayoutPanel();
            label6 = new Label();
            panel.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _lblStatus
            // 
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.Location = new Point(228, 14);
            _lblStatus.Margin = new Padding(4, 0, 4, 0);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(641, 37);
            _lblStatus.TabIndex = 1;
            _lblStatus.Text = "STOPPED";
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblAddress
            // 
            _lblAddress.Dock = DockStyle.Fill;
            _lblAddress.Location = new Point(228, 51);
            _lblAddress.Margin = new Padding(4, 0, 4, 0);
            _lblAddress.Name = "_lblAddress";
            _lblAddress.Size = new Size(641, 37);
            _lblAddress.TabIndex = 3;
            _lblAddress.Text = "-";
            _lblAddress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _txtPort
            // 
            _txtPort.Dock = DockStyle.Fill;
            _txtPort.Location = new Point(228, 91);
            _txtPort.Margin = new Padding(4, 3, 4, 3);
            _txtPort.Name = "_txtPort";
            _txtPort.Size = new Size(641, 23);
            _txtPort.TabIndex = 5;
            // 
            // _txtRefreshInterval
            // 
            _txtRefreshInterval.Dock = DockStyle.Fill;
            _txtRefreshInterval.Location = new Point(228, 128);
            _txtRefreshInterval.Margin = new Padding(4, 3, 4, 3);
            _txtRefreshInterval.Name = "_txtRefreshInterval";
            _txtRefreshInterval.Size = new Size(641, 23);
            _txtRefreshInterval.TabIndex = 7;
            // 
            // _txtPassword
            // 
            _txtPassword.Dock = DockStyle.Fill;
            _txtPassword.Location = new Point(228, 165);
            _txtPassword.Margin = new Padding(4, 3, 4, 3);
            _txtPassword.Name = "_txtPassword";
            _txtPassword.Size = new Size(641, 23);
            _txtPassword.TabIndex = 9;
            _txtPassword.UseSystemPasswordChar = true;
            // 
            // _btnStart
            // 
            _btnStart.Location = new Point(4, 3);
            _btnStart.Margin = new Padding(4, 3, 4, 3);
            _btnStart.Name = "_btnStart";
            _btnStart.Size = new Size(117, 35);
            _btnStart.TabIndex = 0;
            _btnStart.Text = "Start";
            // 
            // _btnStop
            // 
            _btnStop.Enabled = false;
            _btnStop.Location = new Point(129, 3);
            _btnStop.Margin = new Padding(4, 3, 4, 3);
            _btnStop.Name = "_btnStop";
            _btnStop.Size = new Size(117, 35);
            _btnStop.TabIndex = 1;
            _btnStop.Text = "Stop";
            // 
            // _txtLogs
            // 
            panel.SetColumnSpan(_txtLogs, 2);
            _txtLogs.Dock = DockStyle.Fill;
            _txtLogs.Location = new Point(18, 296);
            _txtLogs.Margin = new Padding(4, 3, 4, 3);
            _txtLogs.Multiline = true;
            _txtLogs.Name = "_txtLogs";
            _txtLogs.ReadOnly = true;
            _txtLogs.ScrollBars = ScrollBars.Vertical;
            _txtLogs.Size = new Size(851, 333);
            _txtLogs.TabIndex = 13;
            // 
            // panel
            // 
            panel.ColumnCount = 2;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panel.Controls.Add(label1, 0, 0);
            panel.Controls.Add(_lblStatus, 1, 0);
            panel.Controls.Add(label2, 0, 1);
            panel.Controls.Add(_lblAddress, 1, 1);
            panel.Controls.Add(label3, 0, 2);
            panel.Controls.Add(_txtPort, 1, 2);
            panel.Controls.Add(label4, 0, 3);
            panel.Controls.Add(_txtRefreshInterval, 1, 3);
            panel.Controls.Add(label5, 0, 4);
            panel.Controls.Add(_txtPassword, 1, 4);
            panel.Controls.Add(buttonPanel, 1, 5);
            panel.Controls.Add(label6, 0, 7);
            panel.Controls.Add(_txtLogs, 0, 8);
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(0, 0);
            panel.Margin = new Padding(4, 3, 4, 3);
            panel.Name = "panel";
            panel.Padding = new Padding(14);
            panel.RowCount = 9;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 14F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.Size = new Size(887, 646);
            panel.TabIndex = 0;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(18, 14);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(202, 37);
            label1.TabIndex = 0;
            label1.Text = "Status:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(18, 51);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(202, 37);
            label2.TabIndex = 2;
            label2.Text = "Địa chỉ truy cập:";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(18, 88);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(202, 37);
            label3.TabIndex = 4;
            label3.Text = "Port:";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(18, 125);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(202, 37);
            label4.TabIndex = 6;
            label4.Text = "Refresh interval (ms):";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            label5.Dock = DockStyle.Fill;
            label5.Location = new Point(18, 162);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(202, 37);
            label5.TabIndex = 8;
            label5.Text = "Mật khẩu (chỉ nhập số, để trống nếu không dùng):";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(_btnStart);
            buttonPanel.Controls.Add(_btnStop);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Location = new Point(228, 202);
            buttonPanel.Margin = new Padding(4, 3, 4, 3);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(641, 42);
            buttonPanel.TabIndex = 11;
            // 
            // label6
            // 
            label6.Dock = DockStyle.Fill;
            label6.Location = new Point(18, 261);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(202, 32);
            label6.TabIndex = 12;
            label6.Text = "Logs:";
            label6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(887, 646);
            Controls.Add(panel);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PC Remote Server";
            panel.ResumeLayout(false);
            panel.PerformLayout();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
