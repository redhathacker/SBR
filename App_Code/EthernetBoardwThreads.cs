using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Messaging;
using System.Runtime.InteropServices;

namespace snmpd
{

     # region EthernetBoardPort_ClassDefinition

     public class EthernetBoardPortwThreads
     {
          List<ObjectIdentifier> lstOIDs = new List<ObjectIdentifier>();

          //EthernetBoardPortwTasks.Channel ch1_1 = new EthernetBoardPortwTasks.Channel(1, Io1_Ch1, "Chan1_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_2 = new EthernetBoardPortwTasks.Channel(2, Io1_Ch2, "Chan2_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_3 = new EthernetBoardPortwTasks.Channel(3, Io1_Ch3, "Chan3_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_4 = new EthernetBoardPortwTasks.Channel(4, Io1_Ch4, "Chan4_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_5 = new EthernetBoardPortwTasks.Channel(5, Io1_Ch5, "Chan5_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_6 = new EthernetBoardPortwTasks.Channel(6, Io1_Ch6, "Chan6_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_7 = new EthernetBoardPortwTasks.Channel(7, Io1_Ch7, "Chan7_Io1", 0);
          //EthernetBoardPortwTasks.Channel ch1_8 = new EthernetBoardPortwTasks.Channel(8, Io1_Ch8, "Chan8_Io1", 0);

          //EthernetBoardPortwTasks.Channel ch2_1 = new EthernetBoardPortwTasks.Channel(1, Io2_Ch1, "Chan1_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_2 = new EthernetBoardPortwTasks.Channel(2, Io2_Ch2, "Chan2_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_3 = new EthernetBoardPortwTasks.Channel(3, Io2_Ch3, "Chan3_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_4 = new EthernetBoardPortwTasks.Channel(4, Io2_Ch4, "Chan4_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_5 = new EthernetBoardPortwTasks.Channel(5, Io2_Ch5, "Chan5_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_6 = new EthernetBoardPortwTasks.Channel(6, Io2_Ch6, "Chan6_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_7 = new EthernetBoardPortwTasks.Channel(7, Io2_Ch7, "Chan7_Io2", 0);
          //EthernetBoardPortwTasks.Channel ch2_8 = new EthernetBoardPortwTasks.Channel(8, Io2_Ch8, "Chan8_Io2", 0);

          //EthernetBoardPortwTasks.Channel ch3_1 = new EthernetBoardPortwTasks.Channel(1, Ad1_Ch1, "Chan1_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_2 = new EthernetBoardPortwTasks.Channel(2, Ad1_Ch2, "Chan2_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_3 = new EthernetBoardPortwTasks.Channel(3, Ad1_Ch3, "Chan3_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_4 = new EthernetBoardPortwTasks.Channel(4, Ad1_Ch4, "Chan4_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_5 = new EthernetBoardPortwTasks.Channel(5, Ad1_Ch5, "Chan5_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_6 = new EthernetBoardPortwTasks.Channel(6, Ad1_Ch6, "Chan6_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_7 = new EthernetBoardPortwTasks.Channel(7, Ad1_Ch7, "Chan7_Ad1", 0);
          //EthernetBoardPortwTasks.Channel ch3_8 = new EthernetBoardPortwTasks.Channel(8, Ad1_Ch8, "Chan8_Ad1", 0);

          public int PortNo { get; set; }
          public int ParentId { get; set; }
          public bool IsAnalog { get; set; }
          public PortMode Mode { get; set; }
          public IPAddress ParentIP { get; set; }
          public List<Channel> channels { get; set; }

          public EthernetBoardPortwThreads() { channels = new List<Channel>(); }

          /// <summary>
          /// Creates a new Port object
          /// </summary>
          /// <param name="_ip">IP of the board this port belongs to</param>
          /// <param name="_portNo">This ports Number</param>
          /// <param name="_mode">PortMode.Input or PortMode.Output</param>
          public EthernetBoardPortwThreads(string _ip, int _portNo, PortMode _mode)
          {
               IPAddress IpObj;
               channels = new List<Channel>();

               if (IPAddress.TryParse(_ip, out IpObj))
               {
                    ParentIP = IpObj;
                    Mode = _mode;
                    PortNo = _portNo;
               }
               else
               {
                    Debug.Print("FAILURE!!!! Invalid IP. Cannot create Port object.");
               }
          }

          private void AddChannel(Channel ch)
          {
               if (this.channels == null)
                    this.channels = new List<Channel>();

               this.channels.Add(ch);
          }

          private void AddChannel(int _id, int _ioPort, string _oid, string _name, int _value)
          {
               Channel ch = new Channel();

               ch.Id = _id;
               ch.OID = new ObjectIdentifier(_oid);
               ch.Name = _name;
               ch.Value = _value;
               ch.IsDirty = false;
               channels.Add(ch);
          }

          public class Channel
          { // State and Value are the same thing
               public int Id { get; set; }
               public int Value { get; set; }
               public ObjectIdentifier OID { get; set; }
               public string Name { get; set; }
               public bool IsDirty { get; set; }
               public ChannelState State { get; private set; }

               public Channel() { }

               public Channel(int _id, string _oid, string _name, int _value)
               {
                    this.Id = _id;
                    this.OID = new ObjectIdentifier(_oid);
                    this.Name = _name;
                    this.Value = _value;
                    this.State = _value == 0 ? ChannelState.OFF : ChannelState.ON;
                    this.IsDirty = false;
               }

               public Channel(int _id, ObjectIdentifier _oid, string _name, int _value)
               {
                    this.Id = _id;
                    this.OID = _oid;
                    this.Name = _name;
                    this.Value = _value;
                    this.State = _value == 0 ? ChannelState.OFF : ChannelState.ON;
                    this.IsDirty = false;
               }
          }
     }

     # endregion

     public class EthernetBoardwThreads : IDisposable
     {

          # region VariableAndDelegateDeclarations

          // Declare delegate for access to error handling functions outside of the class
          public delegate void ErrorHandlerDelegate(object sender, ErrorEventArgs e);
          // Declare an error event. onError is the virtual function
          public event ErrorHandlerDelegate onError;

          //protected virtual void onError(object sender, ErrorEventArgs e)
          //{    // Error Handler
          //    EventHandler<ErrorEventArgs> handler = ErrorEventHandler;

          //    if (handler != null)
          //        handler(sender, e);
          //}

          public delegate void LoggingRaisedHandlerDelegate(string message, string color);
          public event LoggingRaisedHandlerDelegate onLoggingRaised;

          public delegate void DigitalInputHandlerDelegate(int boardID, int _ioPort, EthernetBoardPortwThreads.Channel channel);
          public event DigitalInputHandlerDelegate onDigitalInput;

          public delegate void AnalogInputHandlerDelegate(int boardID, int _ioPort, EthernetBoardPortwThreads.Channel channel);
          public event AnalogInputHandlerDelegate onAnalogInput;

          public delegate void InitializeHandlerDelegate(int boardID, List<EthernetBoardPortwThreads> ioPorts, List<EthernetBoardPortwThreads> adcPorts);
          public event InitializeHandlerDelegate onInit;

          private const int LISTEN_PORT = 161;
          private const int RECEIVE_PORT = 162;
          private const int DIG_CHAN_START_NO = 1;
          private const int DIG_CHAN_PER_PORT = 8;
          private const int ANLG_CHAN_START_NO = 1;
          private const int ANLG_CHAN_PER_PORT = 4;
          private const int SNMP_GET_TIMEOUT = 30;
          private const int SNMP_SET_TIMEOUT = 30;

          private string _BoardName;
          private int _BoardID;
          private int _Port;
          private string _IP_Address;
          private string _Community;
          private bool _IsInitialized = false;
          private bool _DigitalTimerActive = false;
          private bool _AnalogTimerActive = false;
          private IPEndPoint _EndPoint;
          private IPAddress _IP;
          private BoardState _ConnectionState;

          // Public readonly properties
          public string BoardName { get { return _BoardName; } }
          public int BoardID { get { return _BoardID; } }
          int Port { get { return _Port; } }
          string IP_Address { get { return _IP_Address; } }
          string Community { get { return _Community; } }
          public bool IsInitialized { get { return _IsInitialized; } }
          bool DigitalTimerActive { get { return _DigitalTimerActive; } }
          bool AnalogTimerActive { get { return _AnalogTimerActive; } }
          IPEndPoint EndPoint { get { return EndPoint; } }
          IPAddress IP { get { return _IP; } }
          BoardState ConnectionState { get { return _ConnectionState; } }

          private List<EthernetBoardPortwThreads> _DigitalPorts;
          private List<EthernetBoardPortwThreads> _AnalogPorts;

          public List<EthernetBoardPortwThreads> DigitalPorts { get { return _DigitalPorts; } }
          public List<EthernetBoardPortwThreads> AnalogPorts { get { return _AnalogPorts; } }

          // Analog timer values
          private int _AnalogInputRefreshInMs = -1;
          // Digital Timer values
          private int _DigitalInputRefreshInMs = -1;

          private System.Timers.Timer DigitalTimer;
          private System.Timers.Timer AnalogTimer;

          private Task task_DigitalTimer = null;
          private Task task_AnalogTimer = null;

          // port objects
          public EthernetBoardPortwThreads AdcPort1;
          public EthernetBoardPortwThreads IoPort1;
          public EthernetBoardPortwThreads IoPort2;

        private CancellationTokenSource CancelTokenSource = null;
        private CancellationToken GlobalCancelToken;


        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

          # endregion

          # region InitializationMethods

          public EthernetBoardwThreads()
          {   // store the current channel values for comparison when checking for new data
               _DigitalPorts = new List<EthernetBoardPortwThreads>();
               _AnalogPorts = new List<EthernetBoardPortwThreads>();

            CancelTokenSource = new CancellationTokenSource();
            GlobalCancelToken = CancelTokenSource.Token;

            IoPort1 = new EthernetBoardPortwThreads();
               IoPort1.IsAnalog = false;
               IoPort1.Mode = PortMode.INPUT;
               IoPort1.PortNo = 0;

               // These loops assign the channels names, numbers, and other values to
               // the List of Channel objects in the EthernetBoardPortwTasks object
               for (int ChannelCount = 0 + DIG_CHAN_START_NO; ChannelCount < DIG_CHAN_PER_PORT + DIG_CHAN_START_NO; ChannelCount++)
               {
                    EthernetBoardPortwThreads.Channel channel = new EthernetBoardPortwThreads.Channel();
                    channel.Id = ChannelCount;
                    channel.Name = "Digital Port # 0 Channel # " + ChannelCount;
                    channel.OID = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1." + ChannelCount + ".0");
                    channel.Value = 0;
                    IoPort1.channels.Add(channel);
               }

               IoPort2 = new EthernetBoardPortwThreads();
               IoPort2.IsAnalog = false;
               IoPort2.Mode = PortMode.INPUT;
               IoPort2.PortNo = 1;

               for (int ChannelCount = 0 + DIG_CHAN_START_NO; ChannelCount < DIG_CHAN_PER_PORT + DIG_CHAN_START_NO; ChannelCount++)
               {
                    EthernetBoardPortwThreads.Channel channel = new EthernetBoardPortwThreads.Channel();
                    channel.Id = ChannelCount;
                    channel.Name = "Digital Port # 1 Channel # " + ChannelCount;
                    channel.OID = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2." + ChannelCount + ".0");
                    channel.Value = 0;
                    IoPort2.channels.Add(channel);
               }

               if (BoardID == 0 || BoardID == 4)
               {
                    AdcPort1 = new EthernetBoardPortwThreads();
                    AdcPort1.IsAnalog = true;
                    AdcPort1.Mode = PortMode.INPUT;
                    AdcPort1.PortNo = 0;

                    // 1 analog port with 4 channels
                    for (int ChannelCount = 0 + ANLG_CHAN_START_NO; ChannelCount < ANLG_CHAN_PER_PORT + ANLG_CHAN_START_NO; ChannelCount++)
                    {
                         EthernetBoardPortwThreads.Channel channel = new EthernetBoardPortwThreads.Channel();
                         channel.Id = ChannelCount;
                         channel.Name = "Analog Port # 1 Channel #" + ChannelCount;
                         channel.OID = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3." + ChannelCount + ".0");
                         channel.Value = 0;
                         AdcPort1.channels.Add(channel);
                    }
               }
          }

          public bool init(int iBoardID, string sBoardName, string sIpAddress, int iPort, string sCommunity,
               int iDigitalInputRefreshInMs, int iAnalogInputRefreshInMs, bool bAutoRefreshDigitalInput,
               bool bAutoRefreshAnalogInput)
          {
               bool success = false;

               _BoardID = iBoardID;
               _Port = iPort;
               _BoardName = sBoardName;
               _IP = IPAddress.Parse(sIpAddress);
               _EndPoint = new IPEndPoint(_IP, _Port);
               _IP_Address = sIpAddress;
               _Community = sCommunity;
               _ConnectionState = BoardState.DISCONNECTED;

               // timer stuff- Timers dont start automatically 
               _DigitalInputRefreshInMs = iDigitalInputRefreshInMs;
               _AnalogInputRefreshInMs = iAnalogInputRefreshInMs;

               DigitalTimer = new System.Timers.Timer(iDigitalInputRefreshInMs);
               DigitalTimer.Elapsed += new ElapsedEventHandler(DigitalTimer_OnTick);

               AnalogTimer = new System.Timers.Timer(iAnalogInputRefreshInMs);
               AnalogTimer.Elapsed += new ElapsedEventHandler(AnalogTimer_OnTick);

               IoPort1.ParentIP = this.IP;
               IoPort1.ParentId = this.BoardID;
               _DigitalPorts.Add(IoPort1);

               IoPort2.ParentIP = this.IP;
               IoPort2.ParentId = this.BoardID;
               _DigitalPorts.Add(IoPort2);

               if (BoardID == 0 || BoardID == 4)
               {
                    AdcPort1.ParentIP = this.IP;
                    AdcPort1.ParentId = this.BoardID;
                    _AnalogPorts.Add(AdcPort1);
               }

               _ConnectionState = BoardState.CONNECTED;

               getInitialValues();

               _IsInitialized = true;
               success = true;

               if (onInit != null)
                    onInit(this.BoardID, this.DigitalPorts, this.AnalogPorts);

               //if (onLoggingRaised != null)
               //    onLoggingRaised("Initialized GPIO Ethernet Board with ID : " + _BoardID.ToString()
               //        + " and Name : " + _BoardName + " completed successfully", "#0000FF");

               return success;
          }


          # endregion

          # region ChannelMethods

          public void ToggleRelay(EthernetBoardPortwThreads.Channel channel)
          {
               if (channel != null)
               {
                    int value = Convert.ToInt32(!Convert.ToBoolean(Convert.ToInt32(channel.Value)));

                    setIOPortChannel(value, channel);
               }
          }

          public void setIOPortChannel(int _iChannelValue, EthernetBoardPortwThreads.Channel channel = null, int portNum = -1, int channelNum = -1)
          {
               EthernetBoardPortwThreads.Channel _channel;

               if (channel != null && portNum == -1 && channelNum == -1)
               {
                    _channel = channel;
               }
               else
               {
                    if (portNum == 0)
                         _channel = IoPort1.channels[channelNum];
                    else
                         _channel = IoPort2.channels[channelNum];
               }

               try
               {
                    SnmpSet(_channel.OID, _Community, _iChannelValue, SNMP_SET_TIMEOUT);
               }
               catch (Exception ex)
               {
                    if ((ex is Lextm.SharpSnmpLib.Messaging.TimeoutException) || (ex is System.Net.Sockets.SocketException))
                    {
                         ex = null; // ignore timeouts
                    }
                    else
                    {
                         //LogStuff("Error reading Digital Values for GPIO Ethernet Board ID : " + this.BoardID.ToString()
                         //    + " and Name : " + this.BoardName + " Failed !! " + ex.Message);

                         if (onError != null)
                              onError(this, new ErrorEventArgs(ex));
                    }
               }
          }

          #endregion

          # region CleanUpMethods

          ~EthernetBoardwThreads()
          {
               // free native resources if there are any.
               if (nativeResource != IntPtr.Zero)
               {
                    Marshal.FreeHGlobal(nativeResource);
                    nativeResource = IntPtr.Zero;
               }
              // Dispose(false);
          }

          public void Dispose()
          {
               DigitalTimer.Dispose();
               AnalogTimer.Dispose();

               GC.SuppressFinalize(this);
          }

          # endregion

          # region SnmpMethods

          /// <summary>Updates the EthernetBoard.Port.Channel value of a device.</summary>
          /// <returns>(List) of type Variable (octet dictionary) </returns>
          public async Task<string> SnmpSet(EthernetBoardPortwThreads.Channel ch, int _NewChannelValue)
          {
               IList<Lextm.SharpSnmpLib.Variable> result = null;
               List<Variable> lstVar = new List<Variable>();

               lstVar.Add(new Variable(ch.OID, new Integer32(_NewChannelValue)));

               try
               {
                    Debug.Print("SET Attempt - IP: " + EndPoint.Address.ToString() + " Port: "
                              + EndPoint.Port + " OID: " + ch.OID);

                    result = await Task.Factory.StartNew<IList<Variable>>(()
                         => Messenger.Set(VersionCode.V1, this.EndPoint,
                         new OctetString("private"),
                         lstVar,
                         SNMP_SET_TIMEOUT));
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return ch.Value.ToString();
          }

          private string SnmpSet(ObjectIdentifier _Oid, string _sCommunity, int _iChannelValue, int _iTimeout)
          {
               IList<Lextm.SharpSnmpLib.Variable> result = null;
               List<Variable> lstVar = new List<Variable>();

               string _sOid = _Oid.ToString();

               lstVar.Add(new Variable(_Oid, new Integer32(_iChannelValue)));

               try
               {
                    Debug.Print("SET Attempt - IP: " + this.IP_Address + " Port: " + this.Port + " OID: " + _sOid);
                    result = Messenger.Set(VersionCode.V1,
                    this.EndPoint,
                    new OctetString(_sCommunity),
                    lstVar,
                    _iTimeout);
               }
               catch (Exception ex)
               {
                    if ((ex is Lextm.SharpSnmpLib.Messaging.TimeoutException) || (ex is System.Net.Sockets.SocketException))
                    {
                         ex = null; // ignore timeouts
                    }
                    else
                    {
                         //LogStuff("Error setting Digital Values for Ethernet Board Name : "
                         //     + this.BoardName + " Message: " + ex.Message);

                         //if (onError != null)
                         //     Application onError(this, new ErrorEventArgs(ex));
                    }
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return "";
          }

          /// <summary>
          /// Retrieves values from an EndPoint (Ip, Port) 
          /// </summary>
          /// <returns>(List) of type Variable (octet dictionary). The new value of the channel</returns>
          public async Task<string> SnmpGet(EthernetBoardPortwThreads.Channel ch)
          {
               IList<Variable> result = null;

               try
               {

                    result = await Task.Factory.StartNew<IList<Variable>>(()
                            => Messenger.Get(VersionCode.V1,
                            EndPoint,
                            new OctetString("private"),
                            new List<Variable> { new Variable(ch.OID) },
                            SNMP_GET_TIMEOUT)
                        );
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return ch.Value.ToString();
          }

          # endregion

          # region TimerMethods

          /*********************************************************************************/
          /*************************** EVENT HANDLERS **************************************/

          public bool StartTimers()
          {
               bool success = false;

               try
               {
                    DigitalTimer.Enabled = true;
                    DigitalTimer.Start();

                    if (BoardID == 0 || BoardID == 4)
                    {
                         AnalogTimer.Enabled = true;
                         AnalogTimer.Start();
                    }

                    success = true;
               }
               catch
               {
                    success = false;
               }

               return success;
          }

          public bool ToggleDigitalInputTimer()
          {
               if (DigitalTimer.Enabled)
                    DigitalTimer.Stop();
               else
                    DigitalTimer.Start();

               return DigitalTimer.Enabled ? true : false;
          }

          public bool ToggleAnalogInputTimer()
          {
               if (BoardID == 0 || BoardID == 4)
               {
                    if (AnalogTimer.Enabled)
                         AnalogTimer.Stop();
                    else
                         AnalogTimer.Start();
               }

               return AnalogTimer.Enabled ? true : false;
          }

          // This is the timer tick code. Executes every (_DigitalInputRefreshInMs) milliseconds
          private void DigitalTimer_OnTick(object sender, ElapsedEventArgs e)
          {
               if (task_DigitalTimer == null || task_DigitalTimer.IsCompleted)
               {
                    task_DigitalTimer = Task.Factory.StartNew(()
                        => CheckForNewDigitalData(DigitalPorts));
               }
               else
               {
                    Debug.Print("DigTick Status: " + task_DigitalTimer.Status);
               }
          }

          private void AnalogTimer_OnTick(object sender, ElapsedEventArgs e)
          {
               if (task_AnalogTimer == null || task_AnalogTimer.IsCompleted)
               {
                    task_AnalogTimer = Task.Factory.StartNew(()
                        => CheckForNewAnalogData(AnalogPorts));
               }
               else
               {
                    Debug.Print("AnalogTick Status: " + task_AnalogTimer.Status);
               }
          }

          private async void getInitialValues()
          {
               Task<string> result = null;

               foreach (EthernetBoardPortwThreads.Channel channel in IoPort1.channels)
               {
                    result = null;

                    try
                    {
                         result = await Task.Factory.StartNew(() => SnmpGet(channel));

                         Task.WaitAll(result);

                         if (result != null)
                         {
                              channel.Value = Convert.ToInt32(result.Result);
                              Debug.Print("Init Board: " + IoPort1.ParentIP + " IoPort1 - channel: "
                                  + channel.Id + " Value: " + channel.Value);
                         }
                    }
                    catch (Exception ex)
                    {
                         HandleErrors(ex);
                    }
               }

               foreach (EthernetBoardPortwThreads.Channel channel in IoPort2.channels)
               {
                    result = null;

                    try
                    {
                         result = await Task.Factory.StartNew(() => SnmpGet(channel));

                         Task.WaitAll(result);

                         if (result != null)
                         {
                              channel.Value = Convert.ToInt32(result.Result);
                              Debug.Print("Init Board: " + IoPort2.ParentIP + " IoPort2 - channel: " + channel.Id + " Value: " + channel.Value);
                         }
                    }
                    catch (Exception ex)
                    {
                         HandleErrors(ex);
                    }
               }

               foreach (EthernetBoardPortwThreads.Channel channel in AdcPort1.channels)
               {
                    result = null;

                    try
                    {
                         result = await Task.Factory.StartNew(() => SnmpGet(channel));

                         Task.WaitAll(result);

                         if (result != null)
                         {
                              channel.Value = Convert.ToInt32(result.Result);
                              Debug.Print("Init Board: " + AdcPort1.ParentIP + " AdPort - channel: " + channel.Id + " Value: " + channel.Value);
                         }
                    }
                    catch (Exception ex)
                    {
                         HandleErrors(ex);
                    }
               }

               if (onInit != null)
                    onInit(BoardID, DigitalPorts, AnalogPorts);
          }

          private async void CheckForNewDigitalData(object state)
          {
               Stopwatch sw = new Stopwatch();
               sw.Start();

               List<EthernetBoardPortwThreads> lstBrd = (List<EthernetBoardPortwThreads>)state;

               Task<string> getResult = null;
               Task<string> setResult = null;

               int NewChannelValue;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to CurrentDigitalChannelValues list. Update list if new values are found //
               //*************************************************************************************//           

               foreach (EthernetBoardPortwThreads pt in lstBrd)
               {
                    foreach (EthernetBoardPortwThreads.Channel ch in pt.channels)
                    {
                         getResult = null;
                         setResult = null;

                         try
                         {
                              getResult = await Task.Factory.StartNew(() => SnmpGet(ch));

                              //task_DigitalTimer.Wait(20);

                              if (getResult != null && getResult.Status == TaskStatus.RanToCompletion)
                              {
                                   if (int.TryParse(getResult.Result, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light is on/off!!
                                        {
                                             Debug.Print("NEW DIG LIGHT VALUE!!!!");

                                             try
                                             {   // write new value 
                                                  setResult = await Task.Factory.StartNew(() => SnmpSet(ch, NewChannelValue));

                                                  lock (this)
                                                  {
                                                       ch.Value = NewChannelValue;

                                                       // do user interface stuff 
                                                       if (onDigitalInput != null)
                                                            onDigitalInput(this.BoardID, pt.PortNo, ch);
                                                  }

                                                  //task_DigitalTimer.Wait(20);

                                                  Debug.Print(sw.Elapsed + " CheckForNewDigitalData - After onDigitalInput");
                                             }
                                             catch (Exception ex)
                                             {
                                                  HandleErrors(ex);
                                             }
                                        }
                                   }
                              }
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }
               }

               //task_DigitalTimer.Dispose();
               task_DigitalTimer = null;

               sw.Stop();
               Debug.Print("Time Elapsed in CheckForNewDigitalData: " + sw.Elapsed);
          }

          private async void CheckForNewAnalogData(object state)
          {

               Task<string> getResult = null;
               Task<string> setResult = null;
               int NewChannelValue;

               Stopwatch sw = new Stopwatch();
               sw.Start();

               List<EthernetBoardPortwThreads> lstBrd = (List<EthernetBoardPortwThreads>)state;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to AnalogPorts list. Update list if new values are found              //
               //*************************************************************************************//

               foreach (EthernetBoardPortwThreads port in lstBrd)
               {
                    foreach (EthernetBoardPortwThreads.Channel channel in port.channels)
                    {
                         getResult = null;
                         setResult = null;

                         try
                         {
                              getResult = await Task.Factory.StartNew(() => SnmpGet(channel));

                              //task_DigitalTimer.Wait(20);

                              if (getResult != null && getResult.Status == TaskStatus.RanToCompletion)
                              {
                                   if (int.TryParse(getResult.Result, out NewChannelValue))
                                   {
                                        if (NewChannelValue != channel.Value) // Light has new value
                                        {
                                             Debug.Print("NEW ANALOG LIGHT VALUE!!!!");

                                             try
                                             {   // write new value 
                                                  setResult = await Task.Factory.StartNew(() => SnmpSet(channel, NewChannelValue));

                                                  channel.Value = NewChannelValue;

                                                  if (onAnalogInput != null)
                                                       onAnalogInput(this.BoardID, port.PortNo, channel);                                                 

                                                  //task_DigitalTimer.Wait(20);

                                                  Debug.Print(sw.Elapsed + "CheckForNewAnalogData - After onAnalogInput");
                                             }
                                             catch (Exception ex)
                                             {
                                                  HandleErrors(ex);
                                             }
                                        }
                                   }
                              }
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }
               }

               //task_AnalogTimer.Dispose();
               task_AnalogTimer = null;

               sw.Stop();
               Debug.Print("Time Elapsed in CheckForNewAnalogData: " + sw.Elapsed);
          }

          private static void HandleErrors(Exception ex)
          {
               if ((ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                    || (ex is System.Net.Sockets.SocketException))
               {
                    ex = null; // ignore timeouts                        
               }
               else
               {
                    throw ex;
               }
          }

          # endregion

          # region NotCurrentlyUsed

          public void LogStuff(string msg)
          {
               if (onLoggingRaised != null)
                    onLoggingRaised(msg, "#FF0000");
          }


          // uses parallel tasks
          public void StartListening(object state)
          {
               Stopwatch sw = new Stopwatch();
               sw.Start();

               string result = null;
               int retValue;

               if (IoPort1 != null)
               {
                    // Run all tasks parallel wait for them to finish
                    Parallel.ForEach<EthernetBoardPortwThreads.Channel>(IoPort1.channels, async channel =>
                    {
                         try
                         {
                              result = await SnmpGet(channel, IoPort1.ParentIP, IoPort1.PortNo, SNMP_GET_TIMEOUT);

                              if (result != null && int.TryParse(result, out retValue))
                                   await SnmpSet(channel, retValue);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    });

                    sw.Stop();
                    Debug.Print("Time Elapsed in StartListening Port1: " + sw.Elapsed);

                    if (IoPort2 != null)
                    {
                         // Run all tasks parallel wait for them to finish
                         Parallel.ForEach<EthernetBoardPortwThreads.Channel>(IoPort2.channels, async channel =>
                         {
                              try
                              {
                                   result = await SnmpGet(channel, IoPort2.ParentIP, IoPort2.PortNo, SNMP_GET_TIMEOUT);

                                   if (result != null && int.TryParse(result, out retValue))
                                        await SnmpSet(channel, retValue);
                              }
                              catch (Exception ex)
                              {
                                   HandleErrors(ex);
                              }
                         });

                         sw.Stop();
                         Debug.Print("Time Elapsed in StartListening Port2: " + sw.Elapsed);
                    }

                    if (AdcPort1 != null)
                    {
                         // Run all tasks parallel wait for them to finish
                         Parallel.ForEach<EthernetBoardPortwThreads.Channel>(AdcPort1.channels, async channel =>
                         {
                              try
                              {
                                   result = await SnmpGet(channel, IoPort2.ParentIP, IoPort2.PortNo, SNMP_GET_TIMEOUT);

                                   if (result != null && int.TryParse(result, out retValue))
                                        await SnmpSet(channel, retValue);
                              }
                              catch (Exception ex)
                              {
                                   HandleErrors(ex);
                              }
                         });

                         sw.Stop();
                         Debug.Print("Time Elapsed in StartListening AdcPort1: " + sw.Elapsed);
                    }
               }
          }

          public bool StopListening()
          {
               bool success = false;


               return success;
          }


          /// Uses Task Parallel Library - NOT DONE
          private void DigPort1_Listen_1_Tasks(object state)
          {
               // Create 8 diff threads, wait for them all to complete
               Stopwatch sw = new Stopwatch();
               sw.Start();

               List<Task> lstTasks = new List<Task>();

               Task tsk1 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[0]));
               Task tsk2 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[1]));
               Task tsk3 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[2]));
               Task tsk4 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[3]));
               Task tsk5 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[4]));
               Task tsk6 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[5]));
               Task tsk7 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[6]));
               Task tsk8 = Task.Factory.StartNew(() => SnmpGet(this.IoPort1.channels[7]));

               lstTasks.Add(tsk1);
               lstTasks.Add(tsk2);
               lstTasks.Add(tsk3);
               lstTasks.Add(tsk4);
               lstTasks.Add(tsk5);
               lstTasks.Add(tsk6);
               lstTasks.Add(tsk7);
               lstTasks.Add(tsk8);

               Task.WaitAll(lstTasks.ToArray());

               // dont need to sleep if i use WaitAll

               sw.Stop();
               Debug.Print("Time Elapsed in DigPort1_Listen_1_Tasks: " + sw.Elapsed);
          }

          /// Uses Task Parallel Library - NOT DONE
          private void DigPort2_Listen_1_Tasks(object state)
          {
               // Create 8 diff threads, wait for them all to complete
               Stopwatch sw = new Stopwatch();
               sw.Start();

               List<Task> lstTasks = new List<Task>();

               Task tsk1 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[0]));
               Task tsk2 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[1]));
               Task tsk3 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[2]));
               Task tsk4 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[3]));
               Task tsk5 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[4]));
               Task tsk6 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[5]));
               Task tsk7 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[6]));
               Task tsk8 = Task.Factory.StartNew(() => SnmpGet(this.IoPort2.channels[7]));

               lstTasks.Add(tsk1);
               lstTasks.Add(tsk2);
               lstTasks.Add(tsk3);
               lstTasks.Add(tsk4);
               lstTasks.Add(tsk5);
               lstTasks.Add(tsk6);
               lstTasks.Add(tsk7);
               lstTasks.Add(tsk8);

               Task.WaitAll(lstTasks.ToArray(), SNMP_GET_TIMEOUT);


               sw.Stop();
               Debug.Print("Time Elapsed in DigPort2_Listen_1_Tasks: " + sw.Elapsed);
          }

          /// Uses Task Parallel Library - DONE
          private void AdcPort1_Listen_1_Tasks(object state)
          {
               // Create 4 diff threads, wait for them all to complete
               Stopwatch sw = new Stopwatch();
               sw.Start();
               string result = null;

               List<Task<string>> lstTasks = new List<Task<string>>();

               if (AdcPort1 != null)
               {
                    //add threads that run SnmpGet to the list
                    Task t1 = Task.Factory.StartNew(() => SnmpGet(AdcPort1.channels[0]));
                    Task t2 = Task.Factory.StartNew(() => SnmpGet(AdcPort1.channels[1]));
                    Task t3 = Task.Factory.StartNew(() => SnmpGet(AdcPort1.channels[2]));
                    Task t4 = Task.Factory.StartNew(() => SnmpGet(AdcPort1.channels[3]));

                    // Execute all 4 threads in parallel
                    Parallel.ForEach(lstTasks, t =>
                    {
                         try
                         {
                              t.Start(); // run the threads
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    });

                    // wait on the the 4 threads to finish
                    try
                    {
                         Task.WaitAll(lstTasks.ToArray());
                    }
                    catch (AggregateException ex)
                    {
                         HandleErrors(ex);
                    }
                    // Check result and write to device if changed
                    Parallel.ForEach(lstTasks, t =>
                    {
                         result = t.Result;
                         if (result.Length > 0 && result != null)
                              t.ContinueWith((prevTask) =>
                                   SnmpSet(this.AdcPort1.channels[0], Convert.ToInt32(result)));
                    });

                    sw.Stop();
                    Debug.Print("Time Elapsed in AdcPort1_Listen_1_Tasks: " + sw.Elapsed);
               }
          }

          // Uses calls to static functions
          private async Task CheckForNewDigitalData2()
          {
               Stopwatch sw = new Stopwatch();
               sw.Start();

               Task<string> result = null;
               int NewChannelValue;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to CurrentDigitalChannelValues list. Update list if new values are found //
               //*************************************************************************************//           

               foreach (EthernetBoardPortwThreads pt in this.DigitalPorts)
               {
                    foreach (EthernetBoardPortwThreads.Channel ch in pt.channels)
                    {
                         result = null;

                         try
                         {
                              result = Task.Run(() =>
                                  SnmpTools.SnmpGet_Async(ch.OID, pt.ParentIP.ToString(), GlobalCancelToken));

                            await result;

                              if (result.Equals(TaskStatus.RanToCompletion))
                              {
                                   if (int.TryParse(result.Result, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light is on/off!!
                                        {
                                             try
                                             {   // write new value 
                                                  result = Task.Run(()
                                                      => SnmpTools.SnmpSet_Async(ch.OID, NewChannelValue, pt.ParentIP.ToString(), GlobalCancelToken));

                                                  ch.Value = NewChannelValue;

                                                // do user interface stuff 
                                                onDigitalInput?.Invoke(pt.ParentId, pt.PortNo, ch);

                                                //Task.WaitAny(result);

                                                Debug.Print(sw.Elapsed + " digitalInputGetAll - After onDigitalInput");
                                             }
                                             catch (Exception ex)
                                             {
                                                  HandleErrors(ex);
                                             }
                                        }
                                   }
                              }
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }
               }

               task_DigitalTimer.Dispose();

               sw.Stop();
               Debug.Print("Time Elapsed in CheckForNewDigitalData: " + sw.Elapsed);
          }

          // Uses calls to static functions
          //private async Task CheckForNewAnalogData2()
          //{
          //     Task<string> result;
          //     int NewChannelValue;

          //     Stopwatch sw = new Stopwatch();
          //     sw.Start();

          //     //******************************* SCAN PORT *******************************************//
          //     // check each channel in the port for updated data (button values)                     //
          //     // Compare it to AnalogPorts list. Update list if new values are found              //
          //     //*************************************************************************************//

          //     foreach (EthernetBoardPortwThreads port in this.AnalogPorts)
          //     {
          //          foreach (EthernetBoardPortwThreads.Channel channel in port.channels)
          //          {
          //               result = null;
          //               try
          //               {
          //                    result = Task.Run(() =>
          //                        SnmpTools.GetVals_Async(port, port.ParentIP, GlobalCancelToken));

          //                    Task.WaitAny(result);

          //                    if (result.Result.Equals(TaskStatus.RanToCompletion))
          //                    {
          //                         if (int.TryParse(result.Result, out NewChannelValue))
          //                         {
          //                              if (NewChannelValue != channel.Value) // Light is on!!
          //                              {
          //                                   try
          //                                   {   // write new value 
          //                                        result = await Task.Factory.StartNew(()
          //                                            => SnmpSet(channel, port.ParentIP, port.PortNo, NewChannelValue));

          //                                        channel.Value = NewChannelValue;

          //                                        if (onAnalogInput != null)
          //                                             onAnalogInput(port.ParentId, port.PortNo, channel);

          //                                        Task.WaitAny(result);

          //                                        Debug.Print(sw.Elapsed + "analogInputGetAll - After onAnalogInput");
          //                                   }
          //                                   catch (Exception ex)
          //                                   {
          //                                        HandleErrors(ex);
          //                                   }
          //                              }
          //                         }
          //                    }
          //               }
          //               catch (Exception ex)
          //               {
          //                    HandleErrors(ex);
          //               }
          //          }
          //     }

          //     task_AnalogTimer.Dispose();

          //     sw.Stop();
          //     Debug.Print("Time Elapsed in CheckForNewAnalogData: " + sw.Elapsed);
          //}

          // STATIC 
          public static async Task<string> SnmpSet(EthernetBoardPortwThreads.Channel ch, IPAddress ip, int pt, int _NewChannelValue)
          {
               IList<Lextm.SharpSnmpLib.Variable> result = null;
               List<Variable> lstVar = new List<Variable>();

               lstVar.Add(new Variable(ch.OID, new Integer32(_NewChannelValue)));

               IPEndPoint MyEp = new IPEndPoint(ip, pt);

               try
               {
                    Debug.Print("SET Attempt Static - IP: " + ip + " Port: "
                              + pt + " OID: " + ch.OID);

                    result = await Task.Factory.StartNew<IList<Variable>>(()
                         => Messenger.Set(VersionCode.V1, MyEp,
                         new OctetString("private"),
                         lstVar, SNMP_SET_TIMEOUT));
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return ch.Value.ToString();
          }

          // STATIC
          public static async Task<string> SnmpGet(EthernetBoardPortwThreads.Channel ch, IPAddress ip, int pt, int _timOut)
          {
               IList<Variable> result = null;
               IPEndPoint MyEp = new IPEndPoint(ip, pt);

               try
               {
                    Debug.Print("GET Attempt Static - IP: " + ip + " Port: " + pt + " OID: " + ch.OID);

                    result = await Task.Factory.StartNew<IList<Variable>>(()
                         => Messenger.Get(VersionCode.V1, MyEp,
                         new OctetString("private"),
                         new List<Variable> { new Variable(ch.OID) },
                         _timOut));
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return ch.Value.ToString();
          }

          # endregion

     }
}