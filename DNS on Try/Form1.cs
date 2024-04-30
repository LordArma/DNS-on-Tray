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

        private void btnTest_Click(object sender, EventArgs e)
        {
            
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
        }

        private void btnDNSRemove_Click(object sender, EventArgs e)
        {
            if (lstDNS.SelectedItem != null & lstDNS.SelectedItem != "Clear")
            {
                string strDNSName = lstDNS.SelectedItems[0].ToString();
                lstDNS.Items.Remove(strDNSName);
                DNS dns = new(strDNSName);
                dns.Remove();
            }
                
        }

        private void btnDNSSet_Click(object sender, EventArgs e)
        {
            if (lstDNS.SelectedItem == "Clear") { 
                ClearDNS();
            }
            else
            {
                AddDNS("178.22.122.100", "185.51.200.2");
            }
        }

        private void btnDNSAdd_Click(object sender, EventArgs e)
        {
            string strDNSName = txtDNSName.Text;
            lstDNS.Items.Add(strDNSName);
            DNS dns = new(strDNSName, txtDNS1.Text, txtDNS2.Text);
            dns.Save();
        }
    }
}
