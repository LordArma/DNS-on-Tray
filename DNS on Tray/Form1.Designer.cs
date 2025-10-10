namespace DNS_on_Tray
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
            lstDNS = new ListBox();
            txtDNSName = new TextBox();
            lblDNSName = new Label();
            lblDNS1 = new Label();
            txtDNS1 = new TextBox();
            lblDNS2 = new Label();
            txtDNS2 = new TextBox();
            btnDNSAdd = new Button();
            btnDNSSet = new Button();
            btnDNSRemove = new Button();
            btnDNSPing = new Button();
            labelPing = new Label();
            labelPingResult = new Label();
            optStartup = new CheckBox();
            lblServers = new Label();
            label1 = new Label();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "Hi";
            notifyIcon1.ContextMenuStrip = notifyMenu;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "DNS on Tray";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            // 
            // notifyMenu
            // 
            notifyMenu.Name = "notifyMenu";
            notifyMenu.Size = new Size(61, 4);
            // 
            // lstDNS
            // 
            lstDNS.BackColor = Color.DimGray;
            lstDNS.BorderStyle = BorderStyle.FixedSingle;
            lstDNS.ForeColor = Color.Gainsboro;
            lstDNS.FormattingEnabled = true;
            lstDNS.ItemHeight = 15;
            lstDNS.Items.AddRange(new object[] { "Clear" });
            lstDNS.Location = new Point(12, 36);
            lstDNS.Name = "lstDNS";
            lstDNS.ScrollAlwaysVisible = true;
            lstDNS.Size = new Size(237, 122);
            lstDNS.TabIndex = 2;
            // 
            // txtDNSName
            // 
            txtDNSName.BackColor = Color.Gainsboro;
            txtDNSName.BorderStyle = BorderStyle.FixedSingle;
            txtDNSName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDNSName.ForeColor = Color.DimGray;
            txtDNSName.Location = new Point(93, 251);
            txtDNSName.Name = "txtDNSName";
            txtDNSName.Size = new Size(156, 23);
            txtDNSName.TabIndex = 3;
            txtDNSName.TextChanged += txtDNSName_TextChanged;
            // 
            // lblDNSName
            // 
            lblDNSName.AutoSize = true;
            lblDNSName.ForeColor = Color.Gainsboro;
            lblDNSName.Location = new Point(12, 254);
            lblDNSName.Name = "lblDNSName";
            lblDNSName.Size = new Size(68, 15);
            lblDNSName.TabIndex = 4;
            lblDNSName.Text = "DNS Name:";
            // 
            // lblDNS1
            // 
            lblDNS1.AutoSize = true;
            lblDNS1.ForeColor = Color.Gainsboro;
            lblDNS1.Location = new Point(12, 283);
            lblDNS1.Name = "lblDNS1";
            lblDNS1.Size = new Size(42, 15);
            lblDNS1.TabIndex = 6;
            lblDNS1.Text = "DNS 1:";
            // 
            // txtDNS1
            // 
            txtDNS1.BackColor = Color.Gainsboro;
            txtDNS1.BorderStyle = BorderStyle.FixedSingle;
            txtDNS1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDNS1.ForeColor = Color.DimGray;
            txtDNS1.Location = new Point(93, 280);
            txtDNS1.Name = "txtDNS1";
            txtDNS1.Size = new Size(156, 23);
            txtDNS1.TabIndex = 5;
            txtDNS1.TextChanged += txtDNS1_TextChanged;
            // 
            // lblDNS2
            // 
            lblDNS2.AutoSize = true;
            lblDNS2.ForeColor = Color.Gainsboro;
            lblDNS2.Location = new Point(12, 312);
            lblDNS2.Name = "lblDNS2";
            lblDNS2.Size = new Size(42, 15);
            lblDNS2.TabIndex = 8;
            lblDNS2.Text = "DNS 2:";
            // 
            // txtDNS2
            // 
            txtDNS2.BackColor = Color.Gainsboro;
            txtDNS2.BorderStyle = BorderStyle.FixedSingle;
            txtDNS2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDNS2.ForeColor = Color.DimGray;
            txtDNS2.Location = new Point(93, 309);
            txtDNS2.Name = "txtDNS2";
            txtDNS2.Size = new Size(156, 23);
            txtDNS2.TabIndex = 7;
            txtDNS2.TextChanged += txtDNS2_TextChanged;
            // 
            // btnDNSAdd
            // 
            btnDNSAdd.BackColor = Color.DimGray;
            btnDNSAdd.Enabled = false;
            btnDNSAdd.FlatStyle = FlatStyle.Flat;
            btnDNSAdd.ForeColor = Color.Gainsboro;
            btnDNSAdd.Location = new Point(174, 338);
            btnDNSAdd.Name = "btnDNSAdd";
            btnDNSAdd.Size = new Size(75, 23);
            btnDNSAdd.TabIndex = 9;
            btnDNSAdd.Text = "Add";
            btnDNSAdd.UseVisualStyleBackColor = false;
            btnDNSAdd.Click += btnDNSAdd_Click;
            // 
            // btnDNSSet
            // 
            btnDNSSet.BackColor = Color.DimGray;
            btnDNSSet.FlatStyle = FlatStyle.Flat;
            btnDNSSet.ForeColor = Color.Gainsboro;
            btnDNSSet.Location = new Point(174, 172);
            btnDNSSet.Name = "btnDNSSet";
            btnDNSSet.Size = new Size(75, 25);
            btnDNSSet.TabIndex = 10;
            btnDNSSet.Text = "Set";
            btnDNSSet.UseVisualStyleBackColor = false;
            btnDNSSet.Click += btnDNSSet_Click;
            // 
            // btnDNSRemove
            // 
            btnDNSRemove.BackColor = Color.DimGray;
            btnDNSRemove.FlatStyle = FlatStyle.Flat;
            btnDNSRemove.ForeColor = Color.Gainsboro;
            btnDNSRemove.Location = new Point(93, 172);
            btnDNSRemove.Name = "btnDNSRemove";
            btnDNSRemove.Size = new Size(75, 25);
            btnDNSRemove.TabIndex = 12;
            btnDNSRemove.Text = "Remove";
            btnDNSRemove.UseVisualStyleBackColor = false;
            btnDNSRemove.Click += btnDNSRemove_Click;
            // 
            // optStartup
            // 
            optStartup.AutoSize = true;
            optStartup.FlatStyle = FlatStyle.Flat;
            optStartup.ForeColor = Color.Gainsboro;
            optStartup.Location = new Point(12, 371);
            optStartup.Name = "optStartup";
            optStartup.Size = new Size(119, 19);
            optStartup.TabIndex = 13;
            optStartup.Text = "Launch on startup";
            optStartup.UseVisualStyleBackColor = true;
            optStartup.Click += optStartup_Click;
            // 
            // lblServers
            // 
            lblServers.AutoSize = true;
            lblServers.ForeColor = Color.Gainsboro;
            lblServers.Location = new Point(12, 9);
            lblServers.Name = "lblServers";
            lblServers.Size = new Size(44, 15);
            lblServers.TabIndex = 14;
            lblServers.Text = "Servers";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.Gainsboro;
            label1.Location = new Point(12, 222);
            label1.Name = "label1";
            label1.Size = new Size(132, 15);
            label1.TabIndex = 15;
            label1.Text = "Add a new custom DNS";
            // 
            // btnDNSPing
            // 
            btnDNSPing.BackColor = Color.DimGray;
            btnDNSPing.FlatStyle = FlatStyle.Flat;
            btnDNSPing.ForeColor = Color.Gainsboro;
            btnDNSPing.Location = new Point(10, 172);
            btnDNSPing.Name = "btnDNSPing";
            btnDNSPing.Size = new Size(75, 25);
            btnDNSPing.TabIndex = 16;
            btnDNSPing.Text = "Ping";
            btnDNSPing.UseVisualStyleBackColor = false;
            btnDNSPing.Click += btnDNSPing_Click;
            // 
            // labelPing
            // 
            labelPing.AutoSize = true;
            labelPing.ForeColor = Color.Gainsboro;
            labelPing.Location = new Point(12, 206);
            labelPing.Name = "labelPing";
            labelPing.Size = new Size(50, 15);
            labelPing.TabIndex = 17;
            labelPing.Text = "DNS Health Check: ";
            // 
            // labelPingResult
            // 
            labelPingResult.AutoSize = true;
            labelPingResult.Location = new Point(130, 206);
            labelPingResult.Name = "labelPingResult";
            labelPingResult.Size = new Size(132, 15);
            labelPingResult.TabIndex = 18;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(265, 402);
            ControlBox = false;
            Controls.Add(label1);
            Controls.Add(lblServers);
            Controls.Add(optStartup);
            Controls.Add(btnDNSRemove);
            Controls.Add(btnDNSPing);
            Controls.Add(labelPing);
            Controls.Add(labelPingResult);
            Controls.Add(btnDNSSet);
            Controls.Add(btnDNSAdd);
            Controls.Add(lblDNS2);
            Controls.Add(txtDNS2);
            Controls.Add(lblDNS1);
            Controls.Add(txtDNS1);
            Controls.Add(lblDNSName);
            Controls.Add(txtDNSName);
            Controls.Add(lstDNS);
            ForeColor = SystemColors.ControlLight;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimizeBox = false;
            Name = "frmMain";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DNS on Tray";
            Activated += frmMain_Activated;
            Load += frmMain_Load;
            DoubleClick += frmMain_DoubleClick;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip notifyMenu;
        private ListBox lstDNS;
        private TextBox txtDNSName;
        private Label lblDNSName;
        private Label lblDNS1;
        private TextBox txtDNS1;
        private Label lblDNS2;
        private TextBox txtDNS2;
        private Button btnDNSAdd;
        private Button btnDNSSet;
        private Button btnDNSRemove;
        private Button btnDNSPing;
        private Label labelPing;
        private Label labelPingResult;
        private CheckBox optStartup;
        private Label lblServers;
        private Label label1;
    }
}
