using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snmpd
{

     public sealed class EthernetBoardPort
     {
          public int TcpPortNo { get { return 161; } }
          public int IoPortNo { get; private set; }
          public bool IsAnalog { get; private set; }
          public PortMode Mode { get; private set; }
          public string ParentIP { get; private set; }
          public int ParentID { get; private set; }
          public List<Channel> ChannelsList { get; private set; }
          public EthernetBoardPortNo EbPortNo { get; private set; }
          public EthernetBoardPort() { ChannelsList = new List<Channel>(); }

          /// <summary>
          /// Creates a new EthernetBoardPort object
          /// </summary>
          /// <param name="_ip">IP of the board this port belongs to</param>
          /// <param name="_portNo">This ports Number</param>
          /// <param name="_mode">PortMode.Input or PortMode.Output</param>
          /// <remarks>If you enter and incorrect (not 1, 2, or 3) port no, it defaults to 2 as this is most common.</remarks>
          public EthernetBoardPort(EthernetBoardPortNo _ebPortNo, bool _isAnalog, PortMode _mode, string _parentIp, int _parentBoardId)
          {
               if (_ebPortNo == EthernetBoardPortNo.DIGITAL_PORT_1) { IoPortNo = 0; }
               else if (_ebPortNo == EthernetBoardPortNo.DIGITAL_PORT_2) { IoPortNo = 1; }
               else if (_ebPortNo == EthernetBoardPortNo.ADC_PORT_1) { IoPortNo = 0; }

               EbPortNo = _ebPortNo;
               ParentIP = _parentIp;
               ParentID = _parentBoardId;
               IsAnalog = _isAnalog;
               Mode = _mode;
               ChannelsList = new List<Channel>();
          }

          private void AddChannel(Channel ch)
          {
               if (ChannelsList == null)
                    ChannelsList = new List<Channel>();

               ChannelsList.Add(ch);
          }

          private void AddChannel(int _channelId, string _oid, string _name, ChannelState _state)
          {
               if (ChannelsList == null)
                    ChannelsList = new List<Channel>();

               Channel ch = new Channel(_channelId, _oid, _name, _state);
               ChannelsList.Add(ch);
          }
     }

     public sealed class EthernetBoardPorts : IEnumerable, IEnumerable<EthernetBoardPort>
     {
          private EthernetBoardPorts[] _ports;

          public EthernetBoardPorts(EthernetBoardPorts[] pArray)
          {
               _ports = new EthernetBoardPorts[pArray.Length];

               for (int i = 0; i < pArray.Length; i++)
               {
                    _ports[i] = pArray[i];
               }
          }

          public IEnumerator<EthernetBoardPort> GetEnumerator()
          {
               throw new NotImplementedException();
          }

          IEnumerator IEnumerable.GetEnumerator()
          {
               return GetEnumerator();
          }

          public int Count
          {
               get
               {
                    return _ports.Length;
               }
          }
     }

     // This class is responsible for handling the navigation
     public class EthernetBoardPortsEnumerator 

     {
          EthernetBoardPorts[] __contents;


          public EthernetBoardPortsEnumerator()
          {

          }
     }

}
