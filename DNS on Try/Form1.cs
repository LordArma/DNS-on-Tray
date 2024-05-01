using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Security.Principal;
using System.Windows.Forms;
using static DNS_on_Try.Helper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DNS_on_Try
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
            if (this.WindowState != FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearDNS();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            EnableAdd();

            foreach (var dns in DNS.All())
            {
                lstDNS.Items.Add(dns.Name());

                ToolStripMenuItem item = new ToolStripMenuItem();
                item = new ToolStripMenuItem();
                item.Name = "dns" + Convert.ToString(new Random().Next(1, 99999));
                item.Text = dns.Name();
                item.Click += new EventHandler(MenuItemClickHandler);

                btnServers.DropDownItems.Add(item);

            }
        }

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            string dnsName = clickedItem.Text;
            DNS dns = new DNS(dnsName);
            AddDNS(dns.DNS1(), dns.DNS2());
        }

        private void EnableAdd()
        {
            string strDNSName = txtDNSName.Text.Trim();
            string strDNS1 = txtDNS1.Text.Trim();
            string strDNS2 = txtDNS2.Text.Trim();

            if (strDNSName != "" & strDNS1 != "")
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
        }

        private void txtDNSName_TextChanged(object sender, EventArgs e)
        {
            EnableAdd();
        }

        private void txtDNS1_TextChanged(object sender, EventArgs e)
        {
            EnableAdd();
        }

        private void txtDNS2_TextChanged(object sender, EventArgs e)
        {
            EnableAdd();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (!IsAdministrator)
                AddShieldToButton(this.btnDNSSet);
        }
    }
}
