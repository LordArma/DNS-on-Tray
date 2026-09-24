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
            btnDNSCancel = new Button();
            btnDNSSet = new Button();
            btnDNSRemove = new Button();
            btnDNSEdit = new Button();
            btnDNSTestAll = new Button();
            btnDNSPing = new Button();
            labelPing = new Label();
            labelPingResult = new Label();
            optStartup = new CheckBox();
            optAdmin = new CheckBox();
            lblServers = new Label();
            label1 = new Label();
            btnClose = new Button();
            lblAdapter = new Label();
            cboAdapter = new ComboBox();
            lblCurrent = new Label();
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
            notifyIcon1.MouseClick += notifyIcon1_MouseClick;
            //
            // notifyMenu
            //
            notifyMenu.Name = "notifyMenu";
            notifyMenu.Size = new Size(61, 4);
            notifyMenu.Opening += notifyMenu_Opening;
            //
            // lblServers
            //
            lblServers.AutoSize = true;
            lblServers.ForeColor = Color.Gainsboro;
            lblServers.Location = new Point(12, 9);
            lblServers.Name = "lblServers";
            lblServers.Size = new Size(44, 15);
            lblServers.TabIndex = 20;
            lblServers.Text = "Servers";
            //
            // btnClose
            //
            btnClose.BackColor = Color.DimGray;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.Gainsboro;
            btnClose.Location = new Point(314, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(28, 24);
            btnClose.TabIndex = 14;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            //
            // lblAdapter
            //
            lblAdapter.AutoSize = true;
            lblAdapter.ForeColor = Color.Gainsboro;
            lblAdapter.Location = new Point(12, 37);
            lblAdapter.Name = "lblAdapter";
            lblAdapter.Size = new Size(52, 15);
            lblAdapter.TabIndex = 21;
            lblAdapter.Text = "Adapter:";
            //
            // cboAdapter
            //
            cboAdapter.BackColor = Color.Gainsboro;
            cboAdapter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAdapter.FlatStyle = FlatStyle.Flat;
            cboAdapter.ForeColor = Color.DimGray;
            cboAdapter.FormattingEnabled = true;
            cboAdapter.Location = new Point(93, 33);
            cboAdapter.Name = "cboAdapter";
            cboAdapter.Size = new Size(245, 23);
            cboAdapter.TabIndex = 0;
            cboAdapter.DropDown += cboAdapter_DropDown;
            cboAdapter.SelectionChangeCommitted += cboAdapter_SelectionChangeCommitted;
            //
            // lblCurrent
            //
            lblCurrent.AutoEllipsis = true;
            lblCurrent.ForeColor = Color.Gainsboro;
            lblCurrent.Location = new Point(12, 63);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.Size = new Size(326, 17);
            lblCurrent.TabIndex = 22;
            lblCurrent.Text = "Current DNS:";
            //
            // lstDNS
            //
            lstDNS.BackColor = Color.DimGray;
            lstDNS.BorderStyle = BorderStyle.FixedSingle;
            lstDNS.ForeColor = Color.Gainsboro;
            lstDNS.FormattingEnabled = true;
            lstDNS.ItemHeight = 15;
            lstDNS.Location = new Point(12, 84);
            lstDNS.Name = "lstDNS";
            lstDNS.ScrollAlwaysVisible = true;
            lstDNS.Size = new Size(326, 122);
            lstDNS.TabIndex = 1;
            lstDNS.DoubleClick += lstDNS_DoubleClick;
            //
            // btnDNSPing
            //
            btnDNSPing.BackColor = Color.DimGray;
            btnDNSPing.FlatStyle = FlatStyle.Flat;
            btnDNSPing.ForeColor = Color.Gainsboro;
            btnDNSPing.Location = new Point(12, 214);
            btnDNSPing.Name = "btnDNSPing";
            btnDNSPing.Size = new Size(62, 25);
            btnDNSPing.TabIndex = 2;
            btnDNSPing.Text = "Test";
            btnDNSPing.UseVisualStyleBackColor = false;
            btnDNSPing.Click += btnDNSPing_Click;
            //
            // btnDNSTestAll
            //
            btnDNSTestAll.BackColor = Color.DimGray;
            btnDNSTestAll.FlatStyle = FlatStyle.Flat;
            btnDNSTestAll.ForeColor = Color.Gainsboro;
            btnDNSTestAll.Location = new Point(78, 214);
            btnDNSTestAll.Name = "btnDNSTestAll";
            btnDNSTestAll.Size = new Size(62, 25);
            btnDNSTestAll.TabIndex = 3;
            btnDNSTestAll.Text = "Test all";
            btnDNSTestAll.UseVisualStyleBackColor = false;
            btnDNSTestAll.Click += btnDNSTestAll_Click;
            //
            // btnDNSEdit
            //
            btnDNSEdit.BackColor = Color.DimGray;
            btnDNSEdit.FlatStyle = FlatStyle.Flat;
            btnDNSEdit.ForeColor = Color.Gainsboro;
            btnDNSEdit.Location = new Point(144, 214);
            btnDNSEdit.Name = "btnDNSEdit";
            btnDNSEdit.Size = new Size(62, 25);
            btnDNSEdit.TabIndex = 4;
            btnDNSEdit.Text = "Edit";
            btnDNSEdit.UseVisualStyleBackColor = false;
            btnDNSEdit.Click += btnDNSEdit_Click;
            //
            // btnDNSRemove
            //
            btnDNSRemove.BackColor = Color.DimGray;
            btnDNSRemove.FlatStyle = FlatStyle.Flat;
            btnDNSRemove.ForeColor = Color.Gainsboro;
            btnDNSRemove.Location = new Point(210, 214);
            btnDNSRemove.Name = "btnDNSRemove";
            btnDNSRemove.Size = new Size(62, 25);
            btnDNSRemove.TabIndex = 5;
            btnDNSRemove.Text = "Remove";
            btnDNSRemove.UseVisualStyleBackColor = false;
            btnDNSRemove.Click += btnDNSRemove_Click;
            //
            // btnDNSSet
            //
            btnDNSSet.BackColor = Color.DimGray;
            btnDNSSet.FlatStyle = FlatStyle.Flat;
            btnDNSSet.ForeColor = Color.Gainsboro;
            btnDNSSet.Location = new Point(276, 214);
            btnDNSSet.Name = "btnDNSSet";
            btnDNSSet.Size = new Size(62, 25);
            btnDNSSet.TabIndex = 6;
            btnDNSSet.Text = "Set";
            btnDNSSet.UseVisualStyleBackColor = false;
            btnDNSSet.Click += btnDNSSet_Click;
            //
            // labelPing
            //
            labelPing.AutoSize = true;
            labelPing.ForeColor = Color.Gainsboro;
            labelPing.Location = new Point(12, 248);
            labelPing.Name = "labelPing";
            labelPing.Size = new Size(106, 15);
            labelPing.TabIndex = 23;
            labelPing.Text = "DNS Health Check:";
            //
            // labelPingResult
            //
            labelPingResult.AutoEllipsis = true;
            labelPingResult.ForeColor = Color.Gainsboro;
            labelPingResult.Location = new Point(12, 267);
            labelPingResult.Name = "labelPingResult";
            labelPingResult.Size = new Size(326, 20);
            labelPingResult.TabIndex = 24;
            labelPingResult.Text = "";
            //
            // label1
            //
            label1.AutoEllipsis = true;
            label1.ForeColor = Color.Gainsboro;
            label1.Location = new Point(12, 294);
            label1.Name = "label1";
            label1.Size = new Size(326, 15);
            label1.TabIndex = 25;
            label1.Text = "Add a new custom DNS";
            //
            // lblDNSName
            //
            lblDNSName.AutoSize = true;
            lblDNSName.ForeColor = Color.Gainsboro;
            lblDNSName.Location = new Point(12, 326);
            lblDNSName.Name = "lblDNSName";
            lblDNSName.Size = new Size(68, 15);
            lblDNSName.TabIndex = 26;
            lblDNSName.Text = "DNS Name:";
            //
            // txtDNSName
            //
            txtDNSName.BackColor = Color.Gainsboro;
            txtDNSName.BorderStyle = BorderStyle.FixedSingle;
            txtDNSName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDNSName.ForeColor = Color.DimGray;
            txtDNSName.Location = new Point(93, 323);
            txtDNSName.Name = "txtDNSName";
            txtDNSName.Size = new Size(245, 23);
            txtDNSName.TabIndex = 7;
            txtDNSName.TextChanged += txtDNSName_TextChanged;
            //
            // lblDNS1
            //
            lblDNS1.AutoSize = true;
            lblDNS1.ForeColor = Color.Gainsboro;
            lblDNS1.Location = new Point(12, 355);
            lblDNS1.Name = "lblDNS1";
            lblDNS1.Size = new Size(42, 15);
            lblDNS1.TabIndex = 27;
            lblDNS1.Text = "DNS 1:";
            //
            // txtDNS1
            //
            txtDNS1.BackColor = Color.Gainsboro;
            txtDNS1.BorderStyle = BorderStyle.FixedSingle;
            txtDNS1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDNS1.ForeColor = Color.DimGray;
            txtDNS1.Location = new Point(93, 352);
            txtDNS1.Name = "txtDNS1";
            txtDNS1.Size = new Size(245, 23);
            txtDNS1.TabIndex = 8;
            txtDNS1.TextChanged += txtDNS1_TextChanged;
            //
            // lblDNS2
            //
            lblDNS2.AutoSize = true;
            lblDNS2.ForeColor = Color.Gainsboro;
            lblDNS2.Location = new Point(12, 384);
            lblDNS2.Name = "lblDNS2";
            lblDNS2.Size = new Size(42, 15);
            lblDNS2.TabIndex = 28;
            lblDNS2.Text = "DNS 2:";
            //
            // txtDNS2
            //
            txtDNS2.BackColor = Color.Gainsboro;
            txtDNS2.BorderStyle = BorderStyle.FixedSingle;
            txtDNS2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDNS2.ForeColor = Color.DimGray;
            txtDNS2.Location = new Point(93, 381);
            txtDNS2.Name = "txtDNS2";
            txtDNS2.PlaceholderText = "optional";
            txtDNS2.Size = new Size(245, 23);
            txtDNS2.TabIndex = 9;
            txtDNS2.TextChanged += txtDNS2_TextChanged;
            //
            // btnDNSCancel
            //
            btnDNSCancel.BackColor = Color.DimGray;
            btnDNSCancel.FlatStyle = FlatStyle.Flat;
            btnDNSCancel.ForeColor = Color.Gainsboro;
            btnDNSCancel.Location = new Point(152, 410);
            btnDNSCancel.Name = "btnDNSCancel";
            btnDNSCancel.Size = new Size(90, 25);
            btnDNSCancel.TabIndex = 10;
            btnDNSCancel.Text = "Cancel";
            btnDNSCancel.UseVisualStyleBackColor = false;
            btnDNSCancel.Visible = false;
            btnDNSCancel.Click += btnDNSCancel_Click;
            //
            // btnDNSAdd
            //
            btnDNSAdd.BackColor = Color.DimGray;
            btnDNSAdd.Enabled = false;
            btnDNSAdd.FlatStyle = FlatStyle.Flat;
            btnDNSAdd.ForeColor = Color.Gainsboro;
            btnDNSAdd.Location = new Point(248, 410);
            btnDNSAdd.Name = "btnDNSAdd";
            btnDNSAdd.Size = new Size(90, 25);
            btnDNSAdd.TabIndex = 11;
            btnDNSAdd.Text = "Add";
            btnDNSAdd.UseVisualStyleBackColor = false;
            btnDNSAdd.Click += btnDNSAdd_Click;
            //
            // optStartup
            //
            optStartup.AutoSize = true;
            optStartup.FlatStyle = FlatStyle.Flat;
            optStartup.ForeColor = Color.Gainsboro;
            optStartup.Location = new Point(12, 447);
            optStartup.Name = "optStartup";
            optStartup.Size = new Size(119, 19);
            optStartup.TabIndex = 12;
            optStartup.Text = "Launch on startup";
            optStartup.UseVisualStyleBackColor = true;
            optStartup.Click += optStartup_Click;
            //
            // optAdmin
            //
            optAdmin.AutoSize = true;
            optAdmin.FlatStyle = FlatStyle.Flat;
            optAdmin.ForeColor = Color.Gainsboro;
            optAdmin.Location = new Point(12, 470);
            optAdmin.Name = "optAdmin";
            optAdmin.Size = new Size(283, 19);
            optAdmin.TabIndex = 13;
            optAdmin.Text = "Run as administrator (no prompt when switching)";
            optAdmin.UseVisualStyleBackColor = true;
            optAdmin.Click += optAdmin_Click;
            //
            // frmMain
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(350, 500);
            ControlBox = false;
            Controls.Add(optAdmin);
            Controls.Add(optStartup);
            Controls.Add(btnDNSAdd);
            Controls.Add(btnDNSCancel);
            Controls.Add(lblDNS2);
            Controls.Add(txtDNS2);
            Controls.Add(lblDNS1);
            Controls.Add(txtDNS1);
            Controls.Add(lblDNSName);
            Controls.Add(txtDNSName);
            Controls.Add(label1);
            Controls.Add(labelPingResult);
            Controls.Add(labelPing);
            Controls.Add(btnDNSSet);
            Controls.Add(btnDNSRemove);
            Controls.Add(btnDNSEdit);
            Controls.Add(btnDNSTestAll);
            Controls.Add(btnDNSPing);
            Controls.Add(lstDNS);
            Controls.Add(lblCurrent);
            Controls.Add(cboAdapter);
            Controls.Add(lblAdapter);
            Controls.Add(btnClose);
            Controls.Add(lblServers);
            ForeColor = SystemColors.ControlLight;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimizeBox = false;
            Name = "frmMain";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DNS on Tray";
            Deactivate += frmMain_Deactivate;
            FormClosing += frmMain_FormClosing;
            Load += frmMain_Load;
            DoubleClick += frmMain_DoubleClick;
            KeyDown += frmMain_KeyDown;
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
        private Button btnDNSCancel;
        private Button btnDNSSet;
        private Button btnDNSRemove;
        private Button btnDNSEdit;
        private Button btnDNSTestAll;
        private Button btnDNSPing;
        private Label labelPing;
        private Label labelPingResult;
        private CheckBox optStartup;
        private CheckBox optAdmin;
        private Label lblServers;
        private Label label1;
        private Button btnClose;
        private Label lblAdapter;
        private ComboBox cboAdapter;
        private Label lblCurrent;
    }
}
