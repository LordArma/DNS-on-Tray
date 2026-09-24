using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{

    public partial class frmMain : Form
    {
        private const string ClearItem = "Clear";

        private CancellationTokenSource? pingCancellationTokenSource;

        public frmMain()
        {
            InitializeComponent();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            if (this.Opacity == 0)
            {
                this.Visible = true;
                this.Opacity = 100;
                this.ShowInTaskbar = true;
            }
            else
            {
                this.Visible = false;
                this.Opacity = 0;
                this.ShowInTaskbar = false;
            }
        }

        private void SetupFormStartPosition()
        {
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - 40,
                                      workingArea.Bottom - Size.Height - 16);

            this.Opacity = 0;
        }

        private void MakeMenuItems()
        {

            notifyMenu.Items.Clear();
            lstDNS.Items.Clear();

            lstDNS.Items.Add(ClearItem);

            ToolStripMenuItem item;

            foreach (var dns in DNS.All())
            {
                lstDNS.Items.Add(dns.Name());

                item = new ToolStripMenuItem();
                item.Text = dns.Name();
                item.Tag = dns;
                item.Click += new EventHandler(DnsMenuItem_Click);

                notifyMenu.Items.Add(item);
            }


            notifyMenu.Items.Add("-");

            string strMenuItemName = "Clear";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = strMenuItemName;
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

        private void frmMain_Load(object sender, EventArgs e)
        {
            SetupFormStartPosition();

            EnableAddButton();

            if (CanRunAsStartup())
            {
                optStartup.Checked = true;
            }
            else
            {
                optStartup.Checked = false;
            }

            try
            {
                MakeMenuItems();
            }
            catch (Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show($"Could not load the saved DNS list:\n{ex.Message}", "DNS on Tray",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            System.Windows.Forms.Application.Exit();
        }

        private async Task ApplyDNS(DNS dns)
        {
            DnsChangeResult result = await AddDNS(dns.DNS1(), dns.DNS2());
            ReportResult(result, $"DNS set to {dns.Name()} ({dns.DNS1()}, {dns.DNS2()}).");
        }

        private async Task ApplyClear()
        {
            DnsChangeResult result = await ClearDNS();
            ReportResult(result, "DNS reset to automatic (DHCP).");
        }

        private void ReportResult(DnsChangeResult result, string successMessage)
        {
            switch (result)
            {
                case DnsChangeResult.Success:
                    notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", successMessage, ToolTipIcon.Info);
                    break;
                case DnsChangeResult.Cancelled:
                    notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", "DNS was not changed: administrator permission was declined.", ToolTipIcon.Warning);
                    break;
                case DnsChangeResult.NoAdapter:
                    notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", "DNS was not changed: no connected network adapter was found.", ToolTipIcon.Warning);
                    break;
                case DnsChangeResult.InvalidAddress:
                    notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", "DNS was not changed: the saved addresses are not valid IPv4 addresses.", ToolTipIcon.Error);
                    break;
                default:
                    notifyIcon1.ShowBalloonTip(3000, "DNS on Tray", "Failed to change DNS.", ToolTipIcon.Error);
                    break;
            }
        }

        /// <summary>
        /// Returns the DNS selected in the list, or null when nothing (or the Clear entry) is selected.
        /// </summary>
        private DNS? SelectedDNS()
        {
            string? selectedItem = lstDNS.SelectedItem as string;
            if (selectedItem == null || selectedItem == ClearItem)
                return null;

            return DNS.Find(selectedItem);
        }

        private bool IsValidNewName(string name)
        {
            return name != ""
                && !string.Equals(name, ClearItem, StringComparison.OrdinalIgnoreCase)
                && !DNS.NameTaken(name);
        }

        private void EnableAddButton()
        {
            string strDNSName = txtDNSName.Text.Trim();
            string strDNS1 = txtDNS1.Text.Trim();
            string strDNS2 = txtDNS2.Text.Trim();

            bool nameOk = IsValidNewName(strDNSName);
            bool dns1Ok = IsValidIPv4(strDNS1);
            bool dns2Ok = IsValidIPv4(strDNS2);

            // Only flag fields the user has started typing in.
            txtDNSName.ForeColor = nameOk || strDNSName == "" ? SystemColors.WindowText : Color.Red;
            txtDNS1.ForeColor = dns1Ok || strDNS1 == "" ? SystemColors.WindowText : Color.Red;
            txtDNS2.ForeColor = dns2Ok || strDNS2 == "" ? SystemColors.WindowText : Color.Red;

            btnDNSAdd.Enabled = nameOk && dns1Ok && dns2Ok;
        }

        private async void btnDNSPing_Click(object sender, EventArgs e)
        {
            // Cancel previous ping
            pingCancellationTokenSource?.Cancel();
            pingCancellationTokenSource?.Dispose();

            pingCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = pingCancellationTokenSource.Token;

            DNS? dns = SelectedDNS();
            if (dns == null)
            {
                labelPingResult.Text = "Select a server to test.";
                labelPingResult.ForeColor = SystemColors.GrayText;
                return;
            }

            string selectedItem = dns.Name();

            labelPingResult.Text = $"Testing {selectedItem}...";
            labelPingResult.ForeColor = Color.LightBlue;

            try
            {
                var result = await PingDNS(
                    dns.DNS1(),
                    dns.DNS2(),
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (result.Success)
                {
                    labelPingResult.Text =
                        $"{selectedItem} - DNS1: {result.Dns1!.RoundtripTime} ms, " +
                        $"DNS2: {result.Dns2!.RoundtripTime} ms";

                    labelPingResult.ForeColor = Color.LightGreen;
                }
                else
                {
                    labelPingResult.Text = $"{selectedItem} failed";
                    labelPingResult.ForeColor = Color.Red;
                }
            }
            catch (OperationCanceledException)
            {
                // Previous ping was cancelled.
            }
        }

        private async Task<DnsPingResult> PingDNS(string dns1, string dns2, CancellationToken cancellationToken)
        {
            var firstTask = SendPing(dns1, cancellationToken);
            var secondTask = SendPing(dns2, cancellationToken);

            var results = await Task.WhenAll(firstTask, secondTask);
            return new DnsPingResult(results[0], results[1]);
        }

        private async Task<PingReply?> SendPing(string dns1, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dns1))
                return null!;

            try
            {
                using var ping = new Ping();

                var reply = await ping.SendPingAsync(dns1, TimeSpan.FromSeconds(4), cancellationToken: cancellationToken);
                return reply;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        private void btnDNSRemove_Click(object sender, EventArgs e)
        {
            DNS? dns = SelectedDNS();
            if (dns == null)
                return;

            dns.Remove();
            MakeMenuItems();
            EnableAddButton();
        }

        private async void btnDNSSet_Click(object sender, EventArgs e)
        {
            if (lstDNS.SelectedItem as string == ClearItem)
            {
                await ApplyClear();
                return;
            }

            DNS? dns = SelectedDNS();
            if (dns != null)
                await ApplyDNS(dns);
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

            if (!IsValidNewName(strDNSName) || !IsValidIPv4(strDNS1) || !IsValidIPv4(strDNS2))
                return;

            DNS dns = new(strDNSName, strDNS1, strDNS2);
            dns.Save();

            ClearForm();
            MakeMenuItems();
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

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (!IsAdministrator)
            {
                // AddShieldToButton(this.btnDNSSet);
            }
        }

        private void optStartup_Click(object sender, EventArgs e)
        {
            if (optStartup.Checked)
            {
                RunAsStartup(true);
            }
            else
            {
                RunAsStartup(false);
            }
        }

        private void frmMain_DoubleClick(object sender, EventArgs e)
        {
            ShowMainWindow();
        }
    }
}
