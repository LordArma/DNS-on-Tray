using Microsoft.Win32;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{

    public partial class frmMain : Form
    {
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

            lstDNS.Items.Add("Clear");

            ToolStripMenuItem item = new ToolStripMenuItem();

            foreach (var dns in DNS.All())
            {
                lstDNS.Items.Add(dns.Name());

                item = new ToolStripMenuItem();
                item.Name = "dns" + Convert.ToString(new Random().Next(1, 99999));
                item.Text = dns.Name();
                item.Click += new EventHandler(MenuItemClickHandler);

                notifyMenu.Items.Add(item);

            }


            notifyMenu.Items.Add("-");

            string strMenuItemName = "Clear";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = strMenuItemName;
            item.Image = Resources.clear.ToBitmap();
            item.Click += new EventHandler(MenuItemClickHandler);
            notifyMenu.Items.Add(item);

            notifyMenu.Items.Add("-");

            strMenuItemName = "Settings";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = strMenuItemName;
            item.Image = Resources.dns.ToBitmap();
            item.Click += new EventHandler(MenuItemClickHandler);
            notifyMenu.Items.Add(item);

            notifyMenu.Items.Add("-");

            strMenuItemName = "Exit";
            item = new ToolStripMenuItem();
            item.Name = strMenuItemName;
            item.Text = strMenuItemName;
            item.Image = Resources.exit.ToBitmap();
            item.Click += new EventHandler(MenuItemClickHandler);
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

            MakeMenuItems();
        }

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            string strItem = clickedItem.Text;
            DNS dns = new DNS(strItem);

            if (strItem != "Exit" & strItem != "Settings" & strItem != "Clear")
            {
                AddDNS(dns.DNS1(), dns.DNS2());
            }
            else if (strItem.ToString() == "Exit")
            {
                System.Windows.Forms.Application.Exit();
            }
            else if (strItem.ToString() == "Settings")
            {
                ShowMainWindow();
            }
            else if (strItem.ToString() == "Clear")
            {
                ClearDNS();
            }
        }

        private void EnableAddButton()
        {
            string strDNSName = txtDNSName.Text.Trim();
            string strDNS1 = txtDNS1.Text.Trim();
            string strDNS2 = txtDNS2.Text.Trim();

            if (strDNSName != "" & strDNS1 != "" & strDNS2 != "")
            {
                btnDNSAdd.Enabled = true;
            }
            else
            {
                btnDNSAdd.Enabled = false;
            }
        }

        private void btnDNSRemove_Click(object sender, EventArgs e)
        {
            string strSlelectedItem = Convert.ToString(lstDNS.SelectedItem) + "";
            if (strSlelectedItem != "Clear")
            {
                lstDNS.Items.Remove(strSlelectedItem);
                DNS dns = new(strSlelectedItem);
                dns.Remove();
                MakeMenuItems();
            }

        }

        private void btnDNSSet_Click(object sender, EventArgs e)
        {
            string strSlelectedItem = Convert.ToString(lstDNS.SelectedItem) + "";
            if (strSlelectedItem == "Clear")
            {
                ClearDNS();
            }
            else
            {
                string dnsName = strSlelectedItem;
                DNS dns = new DNS(dnsName);
                AddDNS(dns.DNS1(), dns.DNS2());
            }
        }

        private void ClearForm()
        {
            txtDNSName.Text = "";
            txtDNS1.Text = "";
            txtDNS2.Text = "";
        }

        private void btnDNSAdd_Click(object sender, EventArgs e)
        {
            string strDNSName = txtDNSName.Text;
            lstDNS.Items.Add(strDNSName);

            DNS dns = new(strDNSName, txtDNS1.Text, txtDNS2.Text);
            if (!dns.Exist())
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
