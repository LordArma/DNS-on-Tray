namespace DNS_on_Try
{
    partial class frmMain
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            notifyIcon1 = new NotifyIcon(components);
            notifyMenu = new ContextMenuStrip(components);
            btnServers = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            btnClear = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            btnSettings = new ToolStripMenuItem();
            btnExit = new ToolStripMenuItem();
            lstDNS = new ListBox();
            txtDNSName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtDNS1 = new TextBox();
            label3 = new Label();
            txtDNS2 = new TextBox();
            btnDNSAdd = new Button();
            btnDNSSet = new Button();
            btnDNSRemove = new Button();
            notifyMenu.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "Hi";
            notifyIcon1.ContextMenuStrip = notifyMenu;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            // 
            // notifyMenu
            // 
            notifyMenu.Items.AddRange(new ToolStripItem[] { btnServers, btnClear, toolStripSeparator1, btnSettings, btnExit });
            notifyMenu.Name = "notifyMenu";
            notifyMenu.Size = new Size(128, 98);
            // 
            // btnServers
            // 
            btnServers.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            btnServers.Name = "btnServers";
            btnServers.Size = new Size(127, 22);
            btnServers.Text = "Servers";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(101, 22);
            toolStripMenuItem1.Text = "Clear";
            // 
            // btnClear
            // 
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(127, 22);
            btnClear.Text = "Clear DNS";
            btnClear.Click += btnClear_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(124, 6);
            // 
            // btnSettings
            // 
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(127, 22);
            btnSettings.Text = "Settings";
            btnSettings.Click += btnSettings_Click;
            // 
            // btnExit
            // 
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(127, 22);
            btnExit.Text = "Exit";
            btnExit.Click += btnExit_Click;
            // 
            // lstDNS
            // 
            lstDNS.FormattingEnabled = true;
            lstDNS.ItemHeight = 15;
            lstDNS.Items.AddRange(new object[] { "Clear" });
            lstDNS.Location = new Point(12, 134);
            lstDNS.Name = "lstDNS";
            lstDNS.ScrollAlwaysVisible = true;
            lstDNS.Size = new Size(237, 94);
            lstDNS.TabIndex = 2;
            // 
            // txtDNSName
            // 
            txtDNSName.Location = new Point(106, 18);
            txtDNSName.Name = "txtDNSName";
            txtDNSName.Size = new Size(143, 23);
            txtDNSName.TabIndex = 3;
            txtDNSName.Text = "Shekan";
            txtDNSName.TextChanged += txtDNSName_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 21);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 4;
            label1.Text = "DNS Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 50);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 6;
            label2.Text = "DNS 1:";
            // 
            // txtDNS1
            // 
            txtDNS1.Location = new Point(106, 47);
            txtDNS1.Name = "txtDNS1";
            txtDNS1.Size = new Size(143, 23);
            txtDNS1.TabIndex = 5;
            txtDNS1.Text = "178.22.122.100";
            txtDNS1.TextChanged += txtDNS1_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 79);
            label3.Name = "label3";
            label3.Size = new Size(42, 15);
            label3.TabIndex = 8;
            label3.Text = "DNS 2:";
            // 
            // txtDNS2
            // 
            txtDNS2.Location = new Point(106, 76);
            txtDNS2.Name = "txtDNS2";
            txtDNS2.Size = new Size(143, 23);
            txtDNS2.TabIndex = 7;
            txtDNS2.Text = "185.51.200.2";
            txtDNS2.TextChanged += txtDNS2_TextChanged;
            // 
            // btnDNSAdd
            // 
            btnDNSAdd.Enabled = false;
            btnDNSAdd.Location = new Point(174, 105);
            btnDNSAdd.Name = "btnDNSAdd";
            btnDNSAdd.Size = new Size(75, 23);
            btnDNSAdd.TabIndex = 9;
            btnDNSAdd.Text = "Add";
            btnDNSAdd.UseVisualStyleBackColor = true;
            btnDNSAdd.Click += btnDNSAdd_Click;
            // 
            // btnDNSSet
            // 
            btnDNSSet.Location = new Point(174, 234);
            btnDNSSet.Name = "btnDNSSet";
            btnDNSSet.Size = new Size(75, 23);
            btnDNSSet.TabIndex = 10;
            btnDNSSet.Text = "Set";
            btnDNSSet.UseVisualStyleBackColor = true;
            btnDNSSet.Click += btnDNSSet_Click;
            // 
            // btnDNSRemove
            // 
            btnDNSRemove.Location = new Point(93, 234);
            btnDNSRemove.Name = "btnDNSRemove";
            btnDNSRemove.Size = new Size(75, 23);
            btnDNSRemove.TabIndex = 12;
            btnDNSRemove.Text = "Remove";
            btnDNSRemove.UseVisualStyleBackColor = true;
            btnDNSRemove.Click += btnDNSRemove_Click;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(265, 402);
            ControlBox = false;
            Controls.Add(btnDNSRemove);
            Controls.Add(btnDNSSet);
            Controls.Add(btnDNSAdd);
            Controls.Add(label3);
            Controls.Add(txtDNS2);
            Controls.Add(label2);
            Controls.Add(txtDNS1);
            Controls.Add(label1);
            Controls.Add(txtDNSName);
            Controls.Add(lstDNS);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimizeBox = false;
            Name = "frmMain";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DNS on Try";
            Activated += frmMain_Activated;
            Load += frmMain_Load;
            notifyMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip notifyMenu;
        private ToolStripMenuItem btnServers;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem btnClear;
        private ToolStripMenuItem btnExit;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem btnSettings;
        private ListBox lstDNS;
        private TextBox txtDNSName;
        private Label label1;
        private Label label2;
        private TextBox txtDNS1;
        private Label label3;
        private TextBox txtDNS2;
        private Button btnDNSAdd;
        private Button btnDNSSet;
        private Button btnDNSRemove;
    }
}
