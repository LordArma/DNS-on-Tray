using System.Net.NetworkInformation;
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
                string text = (IsCurrent ? "✓ " : "    ") + (Dns == null ? "Clear (automatic / DHCP)" : Name);
                if (Tested)
                    text += Latency is int ms ? $"  —  {ms} ms" : "  —  no answer";

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

        public frmMain()
        {
            InitializeComponent();

            NetworkChange.NetworkAddressChanged += (_, _) => RefreshCurrentDNSFromAnyThread();
            NetworkChange.NetworkAvailabilityChanged += (_, _) => RefreshCurrentDNSFromAnyThread();
        }

        #region Window

        public void ShowMainWindow()
        {
            LoadAdapters();
            RefreshCurrentDNS();

            this.Visible = true;
            this.Opacity = 1;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void HideMainWindow()
        {
            this.Visible = false;
            this.Opacity = 0;
            this.ShowInTaskbar = false;
        }

        private void ToggleMainWindow()
        {
            if (this.Opacity == 0)
                ShowMainWindow();
            else
                HideMainWindow();
        }

        private void SetupFormStartPosition()
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - 40,
                                      workingArea.Bottom - Size.Height - 16);

            this.Opacity = 0;
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
            if (this.Opacity > 0 && !suppressAutoHide)
            {
                HideMainWindow();
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

        private void frmMain_Load(object sender, EventArgs e)
        {
            SetupFormStartPosition();

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
                MessageBox.Show($"Could not load the saved DNS list:\n{ex.Message}", "DNS on Tray",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Server list and tray menu

        private bool IsCurrent(DNS dns)
        {
            if (currentDns == null || currentDns.Automatic)
                return false;

            return dns.Servers().ToHashSet().SetEquals(currentDns.Servers);
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
                item.Text = dns.Name();
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
            item.Text = strMenuItemName;
            item.Checked = automatic;
            item.Image = Resources.clear.ToBitmap();
            item.Click += new EventHandler(ClearMenuItem_Click);
            notifyMenu.Items.Add(item);

            notifyMenu.Items.Add("-");

            strMenuItemName = "Settings";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = strMenuItemName;
            item.Image = Resources.dns.ToBitmap();
            item.Click += new EventHandler(SettingsMenuItem_Click);
            notifyMenu.Items.Add(item);

            notifyMenu.Items.Add("-");

            strMenuItemName = "Exit";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = strMenuItemName;
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
                description = SelectedAdapterId != null ? "selected adapter is not connected" : "no connected adapter";
            }
            else if (currentDns.Automatic)
            {
                description = "Automatic (DHCP)";
                if (currentDns.Servers.Count > 0)
                    description += " — " + string.Join(", ", currentDns.Servers);
            }
            else
            {
                DNS? match = DNS.All().FirstOrDefault(IsCurrent);
                description = match != null
                    ? $"{match.Name()} ({match.ServersText()})"
                    : string.Join(", ", currentDns.Servers);
            }

            lblCurrent.Text = $"Current DNS: {description}";

            // The tray tooltip is limited to 127 characters.
            string tooltip = $"DNS on Tray\n{description}";
            notifyIcon1.Text = tooltip.Length > 127 ? tooltip.Substring(0, 124) + "..." : tooltip;
        }

        private void LoadAdapters()
        {
            string? selectedId = SelectedAdapterId;

            cboAdapter.Items.Clear();
            cboAdapter.Items.Add(new AdapterItem(null, "Automatic (connected adapters)"));
            cboAdapter.SelectedIndex = 0;

            bool found = false;
            foreach (Adapter adapter in ListAdapters())
            {
                var adapterItem = new AdapterItem(adapter.Id, adapter.IsUp ? adapter.Name : $"{adapter.Name} (disconnected)");
                cboAdapter.Items.Add(adapterItem);

                if (adapter.Id == selectedId)
                {
                    cboAdapter.SelectedItem = adapterItem;
                    found = true;
                }
            }

            if (selectedId != null && !found)
            {
                var missing = new AdapterItem(selectedId, "Saved adapter (not available)");
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
            DnsChangeResult result = await AddDNS(dns.DNS1(), dns.DNS2());
            ReportResult(result, $"DNS set to {dns.Name()} ({dns.ServersText()}).");
            RefreshCurrentDNS();
        }

        private async Task ApplyClear()
        {
            DnsChangeResult result = await ClearDNS();
            ReportResult(result, "DNS reset to automatic (DHCP).");
            RefreshCurrentDNS();
        }

        private void ShowBalloon(string text, ToolTipIcon icon)
        {
            notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", text, icon);
        }

        private void ReportResult(DnsChangeResult result, string successMessage)
        {
            switch (result)
            {
                case DnsChangeResult.Success:
                    ShowBalloon(successMessage, ToolTipIcon.Info);
                    break;
                case DnsChangeResult.Cancelled:
                    ShowBalloon("DNS was not changed: administrator permission was declined.", ToolTipIcon.Warning);
                    break;
                case DnsChangeResult.NoAdapter:
                    ShowBalloon(SelectedAdapterId != null
                        ? "DNS was not changed: the selected adapter is not connected."
                        : "DNS was not changed: no connected network adapter was found.", ToolTipIcon.Warning);
                    break;
                case DnsChangeResult.InvalidAddress:
                    ShowBalloon("DNS was not changed: the saved addresses are not valid IPv4 addresses.", ToolTipIcon.Error);
                    break;
                default:
                    ShowBalloon("Failed to change DNS.", ToolTipIcon.Error);
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

            labelPingResult.Text = "Can't test: a VPN or proxy is answering all DNS queries.";
            labelPingResult.ForeColor = Color.Khaki;
            return true;
        }

        private async void btnDNSPing_Click(object sender, EventArgs e)
        {
            var cancellationToken = StartTest();

            DNS? dns = SelectedDNS();
            if (dns == null)
            {
                labelPingResult.Text = "Select a server to test.";
                labelPingResult.ForeColor = Color.Gainsboro;
                return;
            }

            string selectedItem = dns.Name();

            labelPingResult.Text = $"Testing {selectedItem}...";
            labelPingResult.ForeColor = Color.LightBlue;

            try
            {
                Task<bool> intercepted = DnsProbe.IsIntercepted(cancellationToken);
                int?[] result = await DnsProbe.MeasureAll(dns, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (await ShowInterceptedWarning(intercepted))
                    return;

                IEnumerable<string> parts = result.Select((ms, i) =>
                    $"DNS{i + 1}: " + (ms is int value ? $"{value} ms" : "no answer"));
                labelPingResult.Text = $"{selectedItem} - {string.Join(", ", parts)}";

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
            labelPingResult.Text = $"Testing {all.Count} servers...";
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
                    labelPingResult.Text = $"Fastest: {answered[0].Dns.Name()} ({answered[0].Latency} ms), {answered.Count}/{all.Count} answered";
                    labelPingResult.ForeColor = Color.LightGreen;
                }
                else
                {
                    labelPingResult.Text = "No server answered.";
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

            label1.Text = $"Edit \"{dns.Name()}\"";
            btnDNSAdd.Text = "Save";
            btnDNSCancel.Visible = true;

            EnableAddButton();
            txtDNSName.Focus();
        }

        private void ExitEditMode()
        {
            editingName = null;

            label1.Text = "Add a new custom DNS";
            btnDNSAdd.Text = "Add";
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

        private void EnableAddButton()
        {
            string strDNSName = txtDNSName.Text.Trim();
            string strDNS1 = txtDNS1.Text.Trim();
            string strDNS2 = txtDNS2.Text.Trim();

            bool nameOk = IsValidNewName(strDNSName);
            bool dns1Ok = IsValidIPv4(strDNS1);
            bool dns2Ok = strDNS2 == "" || IsValidIPv4(strDNS2);

            // Only flag fields the user has started typing in.
            txtDNSName.ForeColor = nameOk || strDNSName == "" ? Color.DimGray : Color.Red;
            txtDNS1.ForeColor = dns1Ok || strDNS1 == "" ? Color.DimGray : Color.Red;
            txtDNS2.ForeColor = dns2Ok ? Color.DimGray : Color.Red;

            btnDNSAdd.Enabled = nameOk && dns1Ok && dns2Ok;
        }

        private void ClearForm()
        {
            txtDNSName.Text = "";
            txtDNS1.Text = "";
            txtDNS2.Text = "";
        }

        private void btnDNSAdd_Click(object sender, EventArgs e)
        {
            string strDNSName = txtDNSName.Text.Trim();
            string strDNS1 = txtDNS1.Text.Trim();
            string strDNS2 = txtDNS2.Text.Trim();

            if (!IsValidNewName(strDNSName) || !IsValidIPv4(strDNS1) || (strDNS2 != "" && !IsValidIPv4(strDNS2)))
                return;

            DNS dns = new(strDNSName, strDNS1, strDNS2);

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

        private void txtDNSName_TextChanged(object sender, EventArgs e)
        {
            EnableAddButton();
        }

        private void txtDNS1_TextChanged(object sender, EventArgs e)
        {
            EnableAddButton();
        }

        private void txtDNS2_TextChanged(object sender, EventArgs e)
        {
            EnableAddButton();
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
                    ShowBalloon("Could not update the startup setting.", ToolTipIcon.Error);
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
                ShowBalloon("Administrator mode was not turned on.", ToolTipIcon.Warning);
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
                ShowBalloon("Administrator mode was not turned off.", ToolTipIcon.Warning);
                return;
            }

            if (optStartup.Checked)
                RunAsStartup(true);

            ShowBalloon("Administrator mode is off. Windows will ask for permission on each DNS change after the app restarts.", ToolTipIcon.Info);
        }

        #endregion
    }
}
