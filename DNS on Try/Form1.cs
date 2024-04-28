using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            //DNS dns = new("Shekan", "178.22.122.100", "185.51.200.2");
            AddDNS("178.22.122.100", "185.51.200.2");
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
                AddShieldToButton(this.btnDNSClear);
            }
        }

        private void btnDNSClear_Click(object sender, EventArgs e)
        {
            ClearDNS();
        }

        private void btnDNSRemove_Click(object sender, EventArgs e)
        {
            if (lstDNS.SelectedItem != null)
                lstDNS.Items.Remove(lstDNS.SelectedItems[0]);
        }

        private void btnDNSSet_Click(object sender, EventArgs e)
        {

        }

        private void btnDNSAdd_Click(object sender, EventArgs e)
        {
            lstDNS.Items.Add(txtDNSName.Text);
        }
    }
}
