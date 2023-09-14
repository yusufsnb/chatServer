using System.ComponentModel;

namespace chatServer
{
    partial class Form1
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
            txtIP = new TextBox();
            txtPort = new TextBox();
            label1 = new Label();
            label2 = new Label();
            btnStartServer = new Button();
            btnServerStop = new Button();
            label3 = new Label();
            label4 = new Label();
            listUsers = new ListBox();
            txtMsg = new RichTextBox();
            btnSend = new Button();
            txtMessages = new TextBox();
            SuspendLayout();
            // 
            // txtIP
            // 
            txtIP.Enabled = false;
            txtIP.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtIP.Location = new Point(80, 21);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(100, 25);
            txtIP.TabIndex = 0;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(243, 21);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(100, 23);
            txtPort.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 26);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 2;
            label1.Text = "IP Address";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(202, 26);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 3;
            label2.Text = "PORT";
            // 
            // btnStartServer
            // 
            btnStartServer.Location = new Point(349, 10);
            btnStartServer.Name = "btnStartServer";
            btnStartServer.Size = new Size(86, 47);
            btnStartServer.TabIndex = 4;
            btnStartServer.Text = "START";
            btnStartServer.UseVisualStyleBackColor = true;
            btnStartServer.Click += btnStartServer_Click;
            // 
            // btnServerStop
            // 
            btnServerStop.Location = new Point(441, 10);
            btnServerStop.Name = "btnServerStop";
            btnServerStop.Size = new Size(86, 47);
            btnServerStop.TabIndex = 5;
            btnServerStop.Text = "STOP";
            btnServerStop.UseVisualStyleBackColor = true;
            btnServerStop.Click += btnServerStop_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(619, 9);
            label3.Name = "label3";
            label3.Size = new Size(74, 15);
            label3.TabIndex = 6;
            label3.Text = "Server Status";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(699, 10);
            label4.Name = "label4";
            label4.Size = new Size(12, 15);
            label4.TabIndex = 7;
            label4.Text = "-";
            // 
            // listUsers
            // 
            listUsers.FormattingEnabled = true;
            listUsers.ItemHeight = 15;
            listUsers.Location = new Point(619, 61);
            listUsers.Name = "listUsers";
            listUsers.Size = new Size(169, 319);
            listUsers.TabIndex = 8;
            // 
            // txtMsg
            // 
            txtMsg.Location = new Point(12, 394);
            txtMsg.Name = "txtMsg";
            txtMsg.Size = new Size(601, 44);
            txtMsg.TabIndex = 9;
            txtMsg.Text = "";
            // 
            // btnSend
            // 
            btnSend.Location = new Point(619, 394);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(169, 44);
            btnSend.TabIndex = 10;
            btnSend.Text = "SEND";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtMessages
            // 
            txtMessages.Location = new Point(12, 63);
            txtMessages.Multiline = true;
            txtMessages.Name = "txtMessages";
            txtMessages.Size = new Size(481, 325);
            txtMessages.TabIndex = 16;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(txtMessages);
            Controls.Add(btnSend);
            Controls.Add(txtMsg);
            Controls.Add(listUsers);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(btnServerStop);
            Controls.Add(btnStartServer);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPort);
            Controls.Add(txtIP);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtIP;
        private TextBox txtPort;
        private Label label1;
        private Label label2;
        private Button btnStartServer;
        private Button btnServerStop;
        private Label label3;
        private Label label4;
        private ListBox listUsers;
        private RichTextBox txtMsg;
        private Button btnSend;
        private TextBox txtMessages;

    }
}