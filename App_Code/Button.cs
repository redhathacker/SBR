using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace snmpd
{
    public class Button
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public int Port { get; set; }
        public int Channel { get; set; }
        public string OID { get; set; }
        public bool IsOn { get; set; }
        public Color BackColor { get; set; }
        public Color ClickedBackColor { get; set; }

        public Button() 
        {
            IsOn = false;
            BackColor = SystemColors.ButtonFace;
            ClickedBackColor = SystemColors.ButtonHighlight;
        }

        public Button(int _port, int _channel) : this()
        {
            ID = _channel * _port + 1;
            Text = "Port: " + _port + " Channel: " + _channel;
            Port = _port;
            Channel = _channel;
        }

        public Button(int _port, int _channel, string _oid) : this()
        {
            ID = _channel * _port + 1;
            Text = "Port: " + _port + " Channel: " + _channel;
            Port = _port;
            Channel = _channel;            
            OID = _oid;
        }

        public Button(string _text, int _port, int _channel, string _oid) : this()
        {
            ID = _channel * _port + 1;
            Text = _text;
            Port = _port;
            Channel = _channel;
            OID = _oid;
        }

        public Button(string _text, int _port, int _channel, string _oid, Color cDefaultColor, Color cClickedColor)
        {
            ID = _channel * (_port + 1);
            Text = _text;
            Port = _port;
            Channel = _channel;
            OID = _oid;
        }

        public override string ToString()
        {
            return "ID: " + ID + " Text: " + Text + " Port: " + Port + " Channel: " + Channel + " OID: " + OID;
        }
    }

    public class ButtonGroup
    {
        public List<Button> buttons;
        public string IP { get; set; }
        public int GroupNumber { get; set; }
        public int SendPort { get; set; }

        public ButtonGroup()
        {
            buttons = new List<Button>();
            SendPort = 162;
        }

        public ButtonGroup(int _id, string _IpAddress) : this()
        {
            GroupNumber = _id;
            IP = _IpAddress;
            GetButtons(IP);
        }

        private void GetButtons(string _IpAddress)
        {
            const int STARTING_PORT = 0;
            const int STARTING_CHANNEL = 1;

            int ButtonNo = 0;
            int NoPorts = 2;
            int NoChannels = 8;            
            int PortCount;
            int ChannelCount;

            IP = _IpAddress;

            // Build OIDs they are the same no mattert the IP
            for (PortCount = STARTING_PORT; PortCount < NoPorts; PortCount++)
            {
                for (ChannelCount = STARTING_CHANNEL; ChannelCount <= NoChannels; ChannelCount++)
                {
                    ButtonNo++;
                    string sOid = "1.3.6.1.4.1.19865.1.2." + PortCount + "." + ChannelCount + ".0";
                    Button _temp = new Button(ButtonNo.ToString(), PortCount, ChannelCount, sOid,
                            SystemColors.ButtonFace, Color.Yellow);
                    buttons.Add(_temp);
                }            
            }            
        }


    }
}
