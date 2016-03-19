using System;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace snmpd
{
    public partial class frmPushButtons : Form
    {

        ButtonGroup bgMain;

        public frmPushButtons()
        {
            InitializeComponent();
        }

        private void frmPushButtons_Load(object sender, EventArgs e)
        {
            lblMsg.Text = "Send SNMP Set commands like the buttons do. Simulate button behaviour.";
            SetIpLabel();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SetIpLabel();
        }

        private void SetIpLabel()
        {
            switch (Convert.ToInt32(NudGroup.Value))
            {
                case 1:
                    lblIP.Text = "192.168.1.40";
                    break;
                case 2:
                    lblIP.Text = "192.168.1.41";
                    break;
                case 3:
                    lblIP.Text = "192.168.1.42";
                    break;
                case 4:
                    lblIP.Text = "192.168.1.43";
                    break;
                case 5:
                    lblIP.Text = "192.168.1.44";
                    break;
                case 6:
                    lblIP.Text = "192.168.1.45";
                    break;
                case 7:
                    lblIP.Text = "192.168.1.46";
                    break;
                case 8:
                    lblIP.Text = "192.168.1.47";
                    break;
                default:
                    lblIP.Text = "127.0.0.1";
                    break;
            }
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            bgMain = new ButtonGroup(Convert.ToInt32(NudGroup.Value), lblIP.Text);      
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bgMain != null)
            {
                Button b = bgMain.buttons[0];

                txtOutput.Text += "ButtonID: " + b.ID + " OID: " + b.OID + " Text: " + b.Text;
            }
            else 
            {
                MessageBox.Show("No Button group has been initialized.");
                btnInit.Focus();
            }
        }

        private void ButtonError()
        {

        }
    }
}
