using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dart.Snmp;

namespace snmpd
{
     class PowerSnmpTools
     {
          Manager manager = new Manager();
          SnmpSocket managerSocket; 

          public string SnmpGet()
          {
               managerSocket = new SnmpSocket(manager);

               manager.Start(manager1_SendGetRequest, new Variable(".1.3.6.1.4.1.19865.1.2.2.1.0"));
          }

          private void manager1_SendGetRequest(SnmpSocket managerSocket, object state)
          {
               //Create Get Request              
               GetMessage request = new GetMessage();
               request.Variables.Add(state as Variable);
               request.Variables.Add(manager.Mib.CreateVariable(NodeName.sysObjectID));
               request.Community = EthernetBoard.COMMUNITY;
               System.Net.IPEndPoint EndPt = new System.Net.IPEndPoint(IPAddress.Parse("192.168.1.40"), EthernetBoard.LISTEN_PORT);

               //Send request and get response
               ResponseMessage response = managerSocket.GetResponse(request, EndPt);

               //Marshal response to the UI thread using the Message event
               manager.Marshal(new ResponseMessage[] { response }, "", null);
          }

          private string manager1_Message(object sender, MessageEventArgs e)
          {
               string msg;
               //Display info about the first variable in the response, and its value
               Variable vari = e.Messages[0].Variables[0];
               msg = vari.Definition.ToString() + vari.Value.ToString() + "\r\n";

               return msg;

          }
     }
}
