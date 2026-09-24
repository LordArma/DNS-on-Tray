using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{

    public partial class frmMain : Form
    {
        private const string ClearItem = "Clear";

        /// <summary>
        /// One row of the server list: a saved DNS, or the Clear entry when Dns is null.
        /// </summary>
        private sealed class ServerItem
        {
            public DNS? Dns { get; init; }
            public bool IsCurrent { get; init; }
            public bool Tested { get; init; }
            public int? Latency { get; init; }

            public string Name => Dns?.Name() ?? ClearItem;

            public override string ToString()
            {
                string text = (IsCurrent ? "✓ " : "    ") + (Dns == null ? L.ClearEntry : L.Ltr(Name));
                if (Tested)
                    text += "  —  " + (Latency is int ms ? L.Milliseconds(ms) : L.NoAnswer);

                return text;
            }
        }

        private sealed record AdapterItem(string? Id, string Text)
        {
            public override string ToString() => Text;
        }

        private CancellationTokenSource? pingCancellationTokenSource;

        // Best latency per DNS name from the last test; null means it did not answer.
        private readonly Dictionary<string, int?> latencies = new();

        private CurrentDnsInfo? currentDns;

        // Name of the entry being edited, or null when the form adds a new one.
        private string? editingName;

        // Clicking the tray icon deactivates (and hides) the window before the click arrives;
        // this keeps that click from immediately showing it again.
        private DateTime lastAutoHide = DateTime.MinValue;
        private bool suppressAutoHide;

        // The window stays hidden until the user first opens it; see SetVisibleCore.
        private bool initialized;
        private bool allowShow;

        // Global hotkey (Ctrl+Alt+D) that shows or hides the window.
        private const int HotkeyId = 1;
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001, MOD_CONTROL = 0x0002, MOD_NOREPEAT = 0x4000;
        private bool hotkeyRegistered;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Tray icons: the normal one, and one with a green dot while a custom DNS is active.
        private readonly Icon automaticIcon;
        private readonly Icon customIcon;

        public frmMain()
        {
            InitializeComponent();

            automaticIcon = notifyIcon1.Icon!;
            customIcon = WithStatusDot(automaticIcon, Color.LimeGreen);

            NetworkChange.NetworkAddressChanged += (_, _) => RefreshCurrentDNSFromAnyThread();
            NetworkChange.NetworkAvailabilityChanged += (_, _) => RefreshCurrentDNSFromAnyThread();
        }

        #region Window

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            hotkeyRegistered = RegisterHotKey(Handle, HotkeyId, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)Keys.D);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (hotkeyRegistered)
                UnregisterHotKey(Handle, HotkeyId);

            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam == (IntPtr)HotkeyId)
            {
                ToggleMainWindow();
                return;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Draws a small colored dot in the bottom-right corner of the icon.
        /// </summary>
        private static Icon WithStatusDot(Icon baseIcon, Color color)
        {
            Size size = SystemInformation.SmallIconSize;
            using Bitmap bitmap = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Icon sized = new Icon(baseIcon, size))
                    g.DrawIcon(sized, new Rectangle(Point.Empty, size));

                int d = Math.Max(6, size.Width / 2);
                Rectangle dot = new Rectangle(size.Width - d, size.Height - d, d - 1, d - 1);
                using (Brush brush = new SolidBrush(color))
                    g.FillEllipse(brush, dot);
                using (Pen pen = new Pen(Color.FromArgb(40, 40, 40), Math.Max(1, d / 6)))
                    g.DrawEllipse(pen, dot);
            }

            // Icon.FromHandle does not own the handle; clone so the returned icon does, then free it.
            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);
            return icon;
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Keeps the window hidden until the user asks for it. Application.Run makes the form
        /// visible at startup; this blocks that, but still creates the handle (needed for the
        /// hotkey and for BeginInvoke) and runs the one-time setup.
        /// </summary>
        protected override void SetVisibleCore(bool value)
        {
            if (!initialized)
            {
                initialized = true;
                CreateHandle();
                InitializeState();
            }

            base.SetVisibleCore(value && allowShow);
        }

        public void ShowMainWindow()
        {
            LoadAdapters();
            RefreshCurrentDNS();
            SetupFormStartPosition();

            allowShow = true;
            this.Visible = true;
            this.Activate();
        }

        private void HideMainWindow()
        {
            this.Visible = false;
        }

        private void ToggleMainWindow()
        {
            if (this.Visible)
                HideMainWindow();
            else
                ShowMainWindow();
        }

        /// <summary>
        /// Places the window above the tray, on the screen it is on (recomputed on every show
        /// so resolution or monitor changes are picked up).
        /// </summary>
        private void SetupFormStartPosition()
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - 40,
                                      workingArea.Bottom - Size.Height - 16);
        }

        private void notifyIcon1_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if ((DateTime.Now - lastAutoHide).TotalMilliseconds < 500)
                return;

            ToggleMainWindow();
        }

        private void frmMain_Deactivate(object? sender, EventArgs e)
        {
            if (this.Visible && !suppressAutoHide)
            {
                // Hide after the activation change has finished rather than in the middle of it.
                BeginInvoke(HideMainWindow);
                lastAutoHide = DateTime.Now;
            }
        }

        private void frmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Alt+F4 hides the window; only Exit in the tray menu quits.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideMainWindow();
            }
        }

        private void frmMain_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                HideMainWindow();
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            HideMainWindow();
        }

        private void frmMain_DoubleClick(object sender, EventArgs e)
        {
            HideMainWindow();
        }

        #endregion

        /// <summary>
        /// One-time setup, run when the handle is first created (the window itself stays hidden).
        /// </summary>
        private void InitializeState()
        {
            ApplyLanguage();
            EnableAddButton();

            optAdmin.Checked = ElevatedTaskExists();
            optStartup.Checked = optAdmin.Checked ? ElevatedTaskRunsAtLogon() : CanRunAsStartup();

            try
            {
                LoadAdapters();
                RefreshCurrentDNS();
            }
            catch (Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException or IOException or UnauthorizedAccessException)
            {
                ShowMessage(L.LoadFailed(ex.Message), MessageBoxIcon.Error);
            }
        }

        #region Server list and tray menu

        private bool IsCurrent(DNS dns)
        {
            if (currentDns == null || currentDns.Automatic)
                return false;

            return dns.IPv4Servers().ToHashSet().SetEquals(currentDns.Servers);
        }

        private void MakeMenuItems()
        {
            string? selectedName = (lstDNS.SelectedItem as ServerItem)?.Name;
            int topIndex = lstDNS.TopIndex;

            notifyMenu.Items.Clear();
            lstDNS.Items.Clear();

            bool automatic = currentDns?.Automatic == true;
            lstDNS.Items.Add(new ServerItem { IsCurrent = automatic });

            List<DNS> all = DNS.All();

            ToolStripMenuItem item;

            foreach (var dns in all)
            {
                item = new ToolStripMenuItem();
                item.Text = L.Ltr(dns.Name());
                item.Tag = dns;
                item.Checked = IsCurrent(dns);
                item.Click += new EventHandler(DnsMenuItem_Click);

                notifyMenu.Items.Add(item);
            }

            // After a test, list the fastest first, then untested, then the ones that did not answer.
            IEnumerable<DNS> ordered = all;
            if (latencies.Count > 0)
                ordered = all.OrderBy(d => latencies.TryGetValue(d.Name(), out int? ms) ? (ms ?? int.MaxValue) : int.MaxValue - 1);

            foreach (var dns in ordered)
            {
                bool tested = latencies.TryGetValue(dns.Name(), out int? latency);
                lstDNS.Items.Add(new ServerItem { Dns = dns, IsCurrent = IsCurrent(dns), Tested = tested, Latency = latency });
            }

            foreach (ServerItem serverItem in lstDNS.Items)
            {
                if (serverItem.Name == selectedName)
                {
                    lstDNS.SelectedItem = serverItem;
                    break;
                }
            }

            if (topIndex < lstDNS.Items.Count)
                lstDNS.TopIndex = topIndex;

            notifyMenu.Items.Add("-");

            string strMenuItemName = "Clear";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = L.Clear;
            item.Checked = automatic;
            item.Image = Resources.clear.ToBitmap();
            item.Click += new EventHandler(ClearMenuItem_Click);
            notifyMenu.Items.Add(item);

            notifyMenu.Items.Add("-");

            notifyMenu.Items.Add(LanguageMenu());

            strMenuItemName = "Settings";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = L.Settings;
            item.Image = Resources.dns.ToBitmap();
            if (hotkeyRegistered)
                item.ShortcutKeyDisplayString = "Ctrl+Alt+D";
            item.Click += new EventHandler(SettingsMenuItem_Click);
            notifyMenu.Items.Add(item);

            notifyMenu.Items.Add("-");

            strMenuItemName = "Exit";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = L.Exit;
            item.Image = Resources.exit.ToBitmap();
            item.Click += new EventHandler(ExitMenuItem_Click);
            notifyMenu.Items.Add(item);
        }

        private void notifyMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            RefreshCurrentDNS();
        }

        private async void DnsMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem { Tag: DNS dns })
                await ApplyDNS(dns);
        }

        private async void ClearMenuItem_Click(object? sender, EventArgs e)
        {
            await ApplyClear();
        }

        private void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            System.Windows.Forms.Application.Exit();
        }

        /// <summary>
        /// Returns the DNS selected in the list, or null when nothing (or the Clear entry) is selected.
        /// </summary>
        private DNS? SelectedDNS()
        {
            return (lstDNS.SelectedItem as ServerItem)?.Dns;
        }

        #endregion

        #region Language

        /// <summary>
        /// Sets every visible text for the current language and mirrors the layout for Farsi.
        /// </summary>
        private void ApplyLanguage()
        {
            RightToLeft direction = L.IsRtl ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeft = direction;
            this.RightToLeftLayout = L.IsRtl;
            notifyMenu.RightToLeft = direction;

            // Addresses and URLs always read left to right.
            foreach (TextBox box in new[] { txtDNS1, txtDNS2, txtDNS1v6, txtDNS2v6, txtDoH })
                box.RightToLeft = RightToLeft.No;

            lblServers.Text = L.Servers;
            lblAdapter.Text = L.Adapter;
            btnDNSPing.Text = L.Test;
            btnDNSTestAll.Text = L.TestAll;
            btnDNSEdit.Text = L.Edit;
            btnDNSRemove.Text = L.Remove;
            btnDNSSet.Text = L.Set;
            labelPing.Text = L.HealthCheck;
            labelPingResult.Text = "";
            lblDNSName.Text = L.DnsName;
            lblDoH.Text = L.DoHUrl;
            lblDNS1.Text = L.FieldLabel("IPv4 1");
            lblDNS2.Text = L.FieldLabel("IPv4 2");
            lblDNS1v6.Text = L.FieldLabel("IPv6 1");
            lblDNS2v6.Text = L.FieldLabel("IPv6 2");
            txtDNS2.PlaceholderText = L.Optional;
            txtDNS1v6.PlaceholderText = L.Optional;
            txtDNS2v6.PlaceholderText = L.Optional;
            txtDoH.PlaceholderText = SupportsDoH ? L.OptionalDoH : L.DoHNeedsWindows11;
            btnImport.Text = L.Import;
            btnExport.Text = L.Export;
            btnDNSCancel.Text = L.Cancel;
            optStartup.Text = L.LaunchOnStartup;
            optAdmin.Text = L.RunAsAdmin;

            if (editingName != null)
            {
                label1.Text = L.EditEntry(editingName);
                btnDNSAdd.Text = L.Save;
            }
            else
            {
                label1.Text = L.AddNew;
                btnDNSAdd.Text = L.Add;
            }
        }

        private ToolStripMenuItem LanguageMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(L.Language);

            foreach (var (code, text) in new[] { (L.Auto, L.LanguageAuto), (L.English, "English"), (L.Farsi, "فارسی") })
            {
                ToolStripMenuItem item = new ToolStripMenuItem(text);
                item.Checked = L.Setting == code;
                item.Click += (_, _) => SetLanguage(code);
                menu.DropDownItems.Add(item);
            }

            return menu;
        }

        private void SetLanguage(string code)
        {
            L.Setting = code;
            ApplyLanguage();
            LoadAdapters();
            RefreshCurrentDNS();
        }

        #endregion

        #region Current DNS and adapters

        private void RefreshCurrentDNSFromAnyThread()
        {
            if (IsHandleCreated)
                BeginInvoke(RefreshCurrentDNS);
        }

        private void RefreshCurrentDNS()
        {
            try
            {
                currentDns = GetCurrentDNS();
            }
            catch (NetworkInformationException)
            {
                currentDns = null;
            }

            MakeMenuItems();

            string description;
            if (currentDns == null)
            {
                description = SelectedAdapterId != null ? L.SelectedAdapterNotConnected : L.NoConnectedAdapter;
            }
            else if (currentDns.Automatic)
            {
                description = L.AutomaticDhcp;
                if (currentDns.Servers.Count > 0)
                    description += " — " + L.Ltr(string.Join(", ", currentDns.Servers.Take(2)));
            }
            else
            {
                DNS? match = DNS.All().FirstOrDefault(IsCurrent);
                description = match != null
                    ? $"{L.Ltr(match.Name())} ({L.Ltr(match.ServersText())})"
                    : L.Ltr(string.Join(", ", currentDns.Servers.Take(2)));
            }

            lblCurrent.Text = L.CurrentDns(description);

            bool custom = currentDns != null && !currentDns.Automatic;
            notifyIcon1.Icon = custom ? customIcon : automaticIcon;

            // The tray tooltip is limited to 127 characters.
            string tooltip = $"DNS on Tray\n{description}";
            notifyIcon1.Text = tooltip.Length > 127 ? tooltip.Substring(0, 124) + "..." : tooltip;
        }

        private void LoadAdapters()
        {
            string? selectedId = SelectedAdapterId;

            cboAdapter.Items.Clear();
            cboAdapter.Items.Add(new AdapterItem(null, L.AutomaticAdapters));
            cboAdapter.SelectedIndex = 0;

            bool found = false;
            foreach (Adapter adapter in ListAdapters())
            {
                var adapterItem = new AdapterItem(adapter.Id, adapter.IsUp ? L.Ltr(adapter.Name) : L.Disconnected(adapter.Name));
                cboAdapter.Items.Add(adapterItem);

                if (adapter.Id == selectedId)
                {
                    cboAdapter.SelectedItem = adapterItem;
                    found = true;
                }
            }

            if (selectedId != null && !found)
            {
                var missing = new AdapterItem(selectedId, L.SavedAdapterMissing);
                cboAdapter.Items.Add(missing);
                cboAdapter.SelectedItem = missing;
            }
        }

        private void cboAdapter_DropDown(object? sender, EventArgs e)
        {
            LoadAdapters();
        }

        private void cboAdapter_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (cboAdapter.SelectedItem is AdapterItem adapterItem)
            {
                SelectedAdapterId = adapterItem.Id;
                RefreshCurrentDNS();
            }
        }

        #endregion

        #region Applying DNS

        private async Task ApplyDNS(DNS dns)
        {
            DnsChangeResult result = await AddDNS(dns);
            ReportResult(result, L.DnsSet(dns.Name(), dns.ServersText()));
            RefreshCurrentDNS();
        }

        private async Task ApplyClear()
        {
            DnsChangeResult result = await ClearDNS();
            ReportResult(result, L.DnsReset);
            RefreshCurrentDNS();
        }

        private void ShowBalloon(string text, ToolTipIcon icon)
        {
            notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", text, icon);
        }

        private void ShowMessage(string text, MessageBoxIcon icon)
        {
            MessageBoxOptions options = L.IsRtl ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0;
            MessageBox.Show(this, text, "DNS on Tray", MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, options);
        }

        private void ReportResult(DnsChangeResult result, string successMessage)
        {
            switch (result)
            {
                case DnsChangeResult.Success:
                    ShowBalloon(successMessage, ToolTipIcon.Info);
                    break;
                case DnsChangeResult.Cancelled:
                    ShowBalloon(L.PermissionDeclined, ToolTipIcon.Warning);
                    break;
                case DnsChangeResult.NoAdapter:
                    ShowBalloon(SelectedAdapterId != null ? L.SelectedAdapterNotConnectedError : L.NoAdapterError, ToolTipIcon.Warning);
                    break;
                case DnsChangeResult.InvalidAddress:
                    ShowBalloon(L.InvalidAddressError, ToolTipIcon.Error);
                    break;
                default:
                    ShowBalloon(L.ChangeFailed, ToolTipIcon.Error);
                    break;
            }
        }

        private async void btnDNSSet_Click(object sender, EventArgs e)
        {
            if (lstDNS.SelectedItem is ServerItem { Dns: null })
            {
                await ApplyClear();
                return;
            }

            DNS? dns = SelectedDNS();
            if (dns != null)
                await ApplyDNS(dns);
        }

        #endregion

        #region Health check

        /// <summary>
        /// Starts a new test, cancelling any test still running.
        /// </summary>
        private CancellationToken StartTest()
        {
            pingCancellationTokenSource?.Cancel();
            pingCancellationTokenSource?.Dispose();

            pingCancellationTokenSource = new CancellationTokenSource();
            return pingCancellationTokenSource.Token;
        }

        /// <summary>
        /// Shows a warning and returns true when DNS traffic is intercepted, since the
        /// measured times would then be meaningless.
        /// </summary>
        private async Task<bool> ShowInterceptedWarning(Task<bool> intercepted)
        {
            if (!await intercepted)
                return false;

            labelPingResult.Text = L.Intercepted;
            labelPingResult.ForeColor = Color.Khaki;
            return true;
        }

        private async void btnDNSPing_Click(object sender, EventArgs e)
        {
            var cancellationToken = StartTest();

            DNS? dns = SelectedDNS();
            if (dns == null)
            {
                labelPingResult.Text = L.SelectServerToTest;
                labelPingResult.ForeColor = Color.Gainsboro;
                return;
            }

            string selectedItem = dns.Name();

            labelPingResult.Text = L.Testing(selectedItem);
            labelPingResult.ForeColor = Color.LightBlue;

            try
            {
                Task<bool> intercepted = DnsProbe.IsIntercepted(cancellationToken);
                int?[] result = await DnsProbe.MeasureAll(dns, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (await ShowInterceptedWarning(intercepted))
                    return;

                // Same order as DNS.Servers(): IPv4 first, then IPv6.
                List<string> names = dns.IPv4Servers().Select((_, i) => $"DNS{i + 1}")
                    .Concat(dns.IPv6Servers().Select((_, i) => $"IPv6 {i + 1}")).ToList();
                IEnumerable<string> parts = result.Select((ms, i) =>
                    $"{names[i]}: " + (ms is int value ? L.Milliseconds(value) : L.NoAnswer));
                labelPingResult.Text = L.Ltr($"{selectedItem} - {string.Join(", ", parts)}");

                int answered = result.Count(ms => ms.HasValue);
                labelPingResult.ForeColor = answered == result.Length ? Color.LightGreen
                    : answered > 0 ? Color.Khaki
                    : Color.LightCoral;

                latencies[selectedItem] = result.Min();
                MakeMenuItems();
            }
            catch (OperationCanceledException)
            {
                // A newer test replaced this one.
            }
        }

        private async void btnDNSTestAll_Click(object sender, EventArgs e)
        {
            var cancellationToken = StartTest();

            List<DNS> all = DNS.All();
            labelPingResult.Text = L.TestingCount(all.Count);
            labelPingResult.ForeColor = Color.LightBlue;

            try
            {
                Task<bool> intercepted = DnsProbe.IsIntercepted(cancellationToken);
                var results = await Task.WhenAll(all.Select(async dns =>
                    (Dns: dns, Latency: (await DnsProbe.MeasureAll(dns, cancellationToken)).Min())));

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (await ShowInterceptedWarning(intercepted))
                    return;

                latencies.Clear();
                foreach (var r in results)
                    latencies[r.Dns.Name()] = r.Latency;

                MakeMenuItems();
                lstDNS.TopIndex = 0;

                var answered = results.Where(r => r.Latency.HasValue).OrderBy(r => r.Latency).ToList();
                if (answered.Count > 0)
                {
                    labelPingResult.Text = L.Fastest(answered[0].Dns.Name(), answered[0].Latency!.Value, answered.Count, all.Count);
                    labelPingResult.ForeColor = Color.LightGreen;
                }
                else
                {
                    labelPingResult.Text = L.NoServerAnswered;
                    labelPingResult.ForeColor = Color.LightCoral;
                }
            }
            catch (OperationCanceledException)
            {
                // A newer test replaced this one.
            }
        }

        #endregion

        #region Add, edit and remove

        private void btnDNSRemove_Click(object sender, EventArgs e)
        {
            DNS? dns = SelectedDNS();
            if (dns == null)
                return;

            if (editingName == dns.Name())
                ExitEditMode();

            dns.Remove();
            latencies.Remove(dns.Name());
            MakeMenuItems();
            EnableAddButton();
        }

        private void btnDNSEdit_Click(object sender, EventArgs e)
        {
            DNS? dns = SelectedDNS();
            if (dns != null)
                EnterEditMode(dns);
        }

        private void lstDNS_DoubleClick(object? sender, EventArgs e)
        {
            DNS? dns = SelectedDNS();
            if (dns != null)
                EnterEditMode(dns);
        }

        private void EnterEditMode(DNS dns)
        {
            editingName = dns.Name();

            txtDNSName.Text = dns.Name();
            txtDNS1.Text = dns.DNS1();
            txtDNS2.Text = dns.DNS2();
            txtDNS1v6.Text = dns.DNS1v6();
            txtDNS2v6.Text = dns.DNS2v6();
            txtDoH.Text = dns.DoH();

            label1.Text = L.EditEntry(dns.Name());
            btnDNSAdd.Text = L.Save;
            btnDNSCancel.Visible = true;

            EnableAddButton();
            txtDNSName.Focus();
        }

        private void ExitEditMode()
        {
            editingName = null;

            label1.Text = L.AddNew;
            btnDNSAdd.Text = L.Add;
            btnDNSCancel.Visible = false;

            ClearForm();
        }

        private void btnDNSCancel_Click(object? sender, EventArgs e)
        {
            ExitEditMode();
        }

        private bool IsValidNewName(string name)
        {
            if (name == "" || string.Equals(name, ClearItem, StringComparison.OrdinalIgnoreCase))
                return false;

            // When editing, keeping the same name (or changing only its case) is fine.
            if (editingName != null && string.Equals(name, editingName, StringComparison.OrdinalIgnoreCase))
                return true;

            return !DNS.NameTaken(name);
        }

        /// <summary>
        /// Colors a field red when it has text that does not pass <paramref name="isValid"/>;
        /// returns whether the field is acceptable (empty counts as acceptable).
        /// </summary>
        private static bool CheckField(TextBox textBox, Func<string, bool> isValid)
        {
            string text = textBox.Text.Trim();
            bool ok = text == "" || isValid(text);
            textBox.ForeColor = ok ? Color.DimGray : Color.Red;
            return ok;
        }

        private DNS FormEntry()
        {
            return new DNS(txtDNSName.Text.Trim(), txtDNS1.Text.Trim(), txtDNS2.Text.Trim(),
                           txtDNS1v6.Text.Trim(), txtDNS2v6.Text.Trim(), txtDoH.Text.Trim());
        }

        private void EnableAddButton()
        {
            bool nameOk = CheckField(txtDNSName, IsValidNewName);
            bool fieldsOk = CheckField(txtDNS1, IsValidIPv4)
                          & CheckField(txtDNS2, IsValidIPv4)
                          & CheckField(txtDNS1v6, IsValidIPv6)
                          & CheckField(txtDNS2v6, IsValidIPv6)
                          & CheckField(txtDoH, IsValidDoH);

            // Name and DNS 1 are required.
            btnDNSAdd.Enabled = nameOk && fieldsOk && txtDNSName.Text.Trim() != "" && txtDNS1.Text.Trim() != "";
        }

        private void ClearForm()
        {
            txtDNSName.Text = "";
            txtDNS1.Text = "";
            txtDNS2.Text = "";
            txtDNS1v6.Text = "";
            txtDNS2v6.Text = "";
            txtDoH.Text = "";
        }

        private void btnDNSAdd_Click(object sender, EventArgs e)
        {
            DNS dns = FormEntry();

            if (!IsValidNewName(dns.Name()) || !IsValidEntry(dns))
                return;

            if (editingName != null)
            {
                dns.Update(editingName);
                latencies.Remove(editingName);
                ExitEditMode();
            }
            else
            {
                dns.Save();
                ClearForm();
            }

            RefreshCurrentDNS();
        }

        private void txtDNS_TextChanged(object? sender, EventArgs e)
        {
            EnableAddButton();
        }

        private void btnImport_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = L.FileFilter;
            dialog.Title = L.ImportTitle;

            suppressAutoHide = true;
            try
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                var (added, skipped) = ServerListFile.Import(dialog.FileName);
                RefreshCurrentDNS();

                string message = L.Imported(added);
                if (skipped > 0)
                    message += " " + L.Skipped(skipped);
                ShowMessage(message, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
            {
                ShowMessage(L.ImportFailed(ex.Message), MessageBoxIcon.Error);
            }
            finally
            {
                suppressAutoHide = false;
            }
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = L.ExportFilter;
            dialog.FileName = "dns-servers.json";
            dialog.Title = L.ExportTitle;

            suppressAutoHide = true;
            try
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                int count = ServerListFile.Export(dialog.FileName);
                ShowMessage(L.Exported(count), MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                ShowMessage(L.ExportFailed(ex.Message), MessageBoxIcon.Error);
            }
            finally
            {
                suppressAutoHide = false;
            }
        }

        #endregion

        #region Startup and administrator mode

        private async void optStartup_Click(object sender, EventArgs e)
        {
            if (!optAdmin.Checked)
            {
                RunAsStartup(optStartup.Checked);
                return;
            }

            // In administrator mode the scheduled task starts the app at logon.
            suppressAutoHide = true;
            try
            {
                bool atLogon = optStartup.Checked;
                if (await Task.Run(() => RegisterElevatedTask(atLogon)) != DnsChangeResult.Success)
                {
                    optStartup.Checked = !atLogon;
                    ShowBalloon(L.StartupUpdateFailed, ToolTipIcon.Error);
                }
            }
            finally
            {
                suppressAutoHide = false;
            }
        }

        private async void optAdmin_Click(object sender, EventArgs e)
        {
            suppressAutoHide = true;
            try
            {
                if (optAdmin.Checked)
                    await EnableAdminMode();
                else
                    await DisableAdminMode();
            }
            finally
            {
                suppressAutoHide = false;
            }
        }

        private async Task EnableAdminMode()
        {
            bool atLogon = optStartup.Checked;
            if (await Task.Run(() => RegisterElevatedTask(atLogon)) != DnsChangeResult.Success)
            {
                optAdmin.Checked = false;
                ShowBalloon(L.AdminNotOn, ToolTipIcon.Warning);
                return;
            }

            // The task now handles startup.
            RunAsStartup(false);

            if (IsAdministrator)
                return;

            // Restart through the task so this session is elevated too.
            Program.ReleaseSingleInstance();
            if (StartElevatedTask())
            {
                notifyIcon1.Visible = false;
                System.Windows.Forms.Application.Exit();
            }
        }

        private async Task DisableAdminMode()
        {
            if (await Task.Run(UnregisterElevatedTask) != DnsChangeResult.Success)
            {
                optAdmin.Checked = true;
                ShowBalloon(L.AdminNotOff, ToolTipIcon.Warning);
                return;
            }

            if (optStartup.Checked)
                RunAsStartup(true);

            ShowBalloon(L.AdminOff, ToolTipIcon.Info);
        }

        #endregion
    }
}
