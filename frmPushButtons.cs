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
        TaskScheduler UiScheduler;
        ButtonGroup bgMain;

        public frmPushButtons()
        {
            InitializeComponent();
            UiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
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
                b.BackColor = Color.LawnGreen;
            }
            else 
            {
                MessageBox.Show("No Button group has been initialized.");
                btnInit.Focus();
            }
        }

        public void ToggleButtonColor(int buttonNo)
        {
            System.Windows.Forms.Button btn;

            switch (buttonNo)
            {
                case 9:
                    btn = button9;
                    break;
                case 10:
                    btn = button10;
                    break;
                case 11:
                    btn = button11;
                    break;
                case 12:
                    btn = button12;
                    break;
                case 13:
                    btn = button13;
                    break;
                case 14:
                    btn = button14;
                    break;
                case 15:
                    btn = button15;
                    break;
                case 16:
                    btn = button16;
                    break;
                case 1:
                    btn = button1;
                    break;
                case 2:
                    btn = button2;
                    break;
                case 3:
                    btn = button3;
                    break;
                case 4:
                    btn = button4;
                    break;
                case 5:
                    btn = button5;
                    break;
                case 6:
                    btn = button6;
                    break;
                case 7:
                    btn = button7;
                    break;
                case 8:
                    btn = button8;
                    break;
                default:
                    btn = button1;
                    break;


            }

            if (btn.BackColor == Color.Coral)
                btn.BackColor = Color.Green;
            else
                btn.BackColor = Color.Coral;
        }

        private void ButtonError()
        {

        }

        private void gbButtons_Enter(object sender, EventArgs e)
        {

        }
    }
}
