using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dart.Snmp;

namespace snmpd
{
     public partial class PowerSnmpAgent : Form
     {
          Manager manager1 = new Manager();          
          Manager manager2 = new Manager();
          SnmpSocket managerSocket;

          string myAgentAddress;


          public PowerSnmpAgent()
          {
               InitializeComponent();
          }

          private void button1_Click(object sender, EventArgs e)
          {
               manager1.Start(manager1_SendGetRequest, manager1.Mib.CreateVariable(NodeName.sysContact));
          }

          private void button2_Click(object sender, EventArgs e)
          {
               //If you don't have the MIB, retrieve the value by IID:
               manager1.Start(manager1_SendGetRequest, new Variable("1.3.6.1.4.1.19865.1.2.2.1.0"));
          }

          private void PowerSnmpAgent_Load(object sender, EventArgs e)
          {
               myAgentAddress = "192.168.1.40";
          }

          private void manager1_SendGetRequest(SnmpSocket managerSocket, object state)
          {
               System.Net.IPEndPoint EndPt = new System.Net.IPEndPoint(IPAddress.Parse(myAgentAddress), Utilities.LISTEN_PORT);
               //Create Get Request
               GetMessage request = new GetMessage();
               request.Variables.Add(state as Variable);

               //Send request and get response
               ResponseMessage response = managerSocket.GetResponse(request, EndPt);

               //Marshal response to the UI thread using the Message event
               manager1.Marshal(new ResponseMessage[] { response }, "", null);
          }

          private void manager1_Message(object sender, MessageEventArgs e)
          {
               //Display info about the first variable in the response, and its value
               Variable vari = e.Messages[0].Variables[0];
               label1.Text += vari.Definition.ToString() + vari.Value.ToString() + "\r\n";
          }

          private void button4_Click(object sender, EventArgs e)
          {
               manager2.Start(sendRequest, manager1.Mib.CreateVariable(NodeName.sysContact, "Systems Admin"));
          }


          private void button3_Click(object sender, EventArgs e)
          {
               manager2.Start(sendRequest, new Variable("1.3.6.1.4.1.19865.1.2.2.1.0"));
          }

          private void sendRequest(SnmpSocket managerSocket, object state)
          {
               //Create Set Request
               SetMessage request = new SetMessage();
               request.Variables.Add(state as Variable);
               request.Version = SnmpVersion.One;
               request.Community = "private";

               //Send request and get response
               ResponseMessage response = managerSocket.GetResponse(request, new Dart.Snmp.IPEndPoint(myAgentAddress, Utilities.LISTEN_PORT));

               //Marshal message to the UI thread using the Message event
               manager2.Marshal(new ResponseMessage[] { response }, "", null);
          }

          private void manager2_Message(object sender, Dart.Snmp.MessageEventArgs e)
          {
               //Fires on the UI thread
               //Display info about the first variable in the response, and its value
               Variable vari = e.Messages[0].Variables[0];
               label2.Text = vari.Definition.ToString() + "\r\nValue: " + vari.Value.ToString();
          }

     }
}
