using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Security.Principal;
using System.Windows.Forms;
using static DNS_on_Try.Helper;

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
            if (!IsAdministrator)
            {
                AddShieldToButton(this.btnDNSSet);
            }

            EnableAdd();
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
                AddDNS("178.22.122.100", "185.51.200.2");
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
    }
}
