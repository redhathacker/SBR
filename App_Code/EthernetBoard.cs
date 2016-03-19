using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Security;
using Lextm.SharpSnmpLib.Messaging;
using System.Runtime.InteropServices;

//Polling loops?
// Task.Run most efficient
// StartNew to pass state info, specif scheduler, task creat options
//Task.Factory.FromAsync
//  CancellationTokenSource cts; 
// Async Await Queue manager class>?
// use ConfigureAwait(false) for fire anf forget methods
// dispossing objects with  using
// Make delegates use object sender and Args e
// 
namespace snmpd
{
     // sealed - cannot be inherited. offers a little optimization

     public sealed class EthernetBoard : IDisposable
     {
          #region VariableAndDelegateDeclarations

          // Declare delegate for access to error handling functions outside of the class
          public delegate void ErrorHandlerDelegate(object sender, ErrorEventArgs e);
          // Declare an error event for the client to subscribe to
          public event ErrorHandlerDelegate onError;

          public delegate void AsyncErrorHandlerDelegate(object sender, AsyncErrorEventArgs e);
          public event AsyncErrorHandlerDelegate onAsyncError;

          public delegate void LoggingRaisedHandlerDelegate(string message, string color);
          public event LoggingRaisedHandlerDelegate onLoggingRaised;

          public delegate void DigitalInputHandlerDelegate(int boardID, int _ioPort, Channel channel);
          public event DigitalInputHandlerDelegate onDigitalInput;

          public delegate void AnalogInputHandlerDelegate(int boardID, int _ioPort, Channel channel);
          public event AnalogInputHandlerDelegate onAnalogInput;

          public delegate void InitializeHandlerDelegate(int boardID, IList<EthernetBoardPort> ioPorts, IList<EthernetBoardPort> adcPorts);
          public event InitializeHandlerDelegate onInit;

          public const int LISTEN_PORT = 161;
          public const int RECEIVE_PORT = 162;
          public const int DIG_CHAN_START_NO = 1;
          public const int DIG_CHAN_PER_PORT = 8;
          public const int ANLG_CHAN_START_NO = 1;
          public const int ANLG_CHAN_PER_PORT = 4;
          public const int SNMP_GET_TIMEOUT = 50;
          public const int SNMP_SET_TIMEOUT = 100;
          public const int NUMBER_OF_DIGITAL_PORTS = 2;
          public const int NUMBER_OF_ANALOG_PORTS = 1;
          public const string COMMUNITY = "private";

          public const string Board1_IP = "162.198.1.40";
          public const string Board2_IP = "162.198.1.41";
          public const string Board3_IP = "162.198.1.42";
          public const string Board4_IP = "162.198.1.43";
          public const string Board5_IP = "162.198.1.44";
          public const string Board6_IP = "162.198.1.45";
          public const string Board7_IP = "162.198.1.46";
          public const string Board8_IP = "162.198.1.47";

          // public properties
          public string BoardName { get; private set; }
          public int BoardID { get; private set; }
          public int IoPort { get; private set; }
          public string IP_Address { get; private set; }

          private IPEndPoint EndPoint = null;
          private IPAddress IP = null;
          private BoardState ConnectionState = BoardState.DISCONNECTED;

          public List<EthernetBoardPort> DigitalPortsList { get; private set; }
          public List<EthernetBoardPort> AnalogPortsList { get; private set; }

          // Analog timer values
          private int AnalogInputRefreshInMs = 100;
          // Digital Timer values
          private int DigitalInputRefreshInMs = 30;

          private System.Timers.Timer DigitalTimer = null;
          private System.Timers.Timer AnalogTimer = null;

          private Task task_DigitalTimer = null;
          private Task task_AnalogTimer = null;

          private CancellationTokenSource CancelTokenSource = null;
          private CancellationToken GlobalCancelToken;

          private IntPtr nativeResource = Marshal.AllocHGlobal(100);
          private readonly object EthernetBoardLock = new object();

          #endregion

          #region InitializationMethods

          //public EthernetBoard()
          //{

          //}

          /// <summary>
          /// Creates a new instance of the EthernetBoard class
          /// </summary>
          public EthernetBoard(int _boardId, string _IpAddress)
          {
               // store the current channel values for comparison when checking for new data
               System.Text.StringBuilder sb = new System.Text.StringBuilder();
               EthernetBoardPort tempPort = null;

               string sOid = null;
               string sNm = null;

               DigitalPortsList = new List<EthernetBoardPort>();
               AnalogPortsList = new List<EthernetBoardPort>();

               // These loops assign the channels names, numbers, and other values to
               // the List of Channel objects in the EthernetBoardPort object
               // lock it while reading so no other thread messes with the counter variables
               tempPort = new EthernetBoardPort(EthernetBoardPortNo.DIGITAL_PORT_1, false, PortMode.INPUT, _IpAddress, _boardId); // Port#, not analog, port mode

               for (int ChannelCount1 = 0 + DIG_CHAN_START_NO; ChannelCount1 < DIG_CHAN_PER_PORT + DIG_CHAN_START_NO; ChannelCount1++)
               {
                    sb.Clear();
                    sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.1.").Append(ChannelCount1).Append(".0").ToString();
                    sNm = "Digital Port # 0 Channel # " + ChannelCount1;

                    Channel channel = new Channel(ChannelCount1, new ObjectIdentifier(sOid), sNm, 0);
                    tempPort.ChannelsList.Add(channel);
               }

               DigitalPortsList.Add(tempPort);

               tempPort = new EthernetBoardPort(EthernetBoardPortNo.DIGITAL_PORT_2, false, PortMode.INPUT, _IpAddress, _boardId); // Port#, not analog, port mode

               for (int ChannelCount2 = 0 + DIG_CHAN_START_NO; ChannelCount2 < DIG_CHAN_PER_PORT + DIG_CHAN_START_NO; ChannelCount2++)
               {
                    sb.Clear();
                    sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.2.").Append(ChannelCount2).Append(".0").ToString();
                    sNm = "Digital Port # 1 Channel # " + ChannelCount2;

                    Channel channel = new Channel(ChannelCount2, new ObjectIdentifier(sOid), sNm, 0);
                    tempPort.ChannelsList.Add(channel);
               }

               DigitalPortsList.Add(tempPort);

               if (BoardID == 0 || BoardID == 4)
               {
                    tempPort = new EthernetBoardPort(0, true, PortMode.INPUT, _IpAddress, _boardId); // Port#, is analog, port mode

                    // 1 analog port with 4 channels
                    for (int ChannelCount3 = 0 + ANLG_CHAN_START_NO; ChannelCount3 < ANLG_CHAN_PER_PORT + ANLG_CHAN_START_NO; ChannelCount3++)
                    {
                         sb.Clear();
                         sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.3.").Append(ChannelCount3).Append(".0").ToString();
                         sNm = "Analog Port # 1 Channel #" + ChannelCount3;

                         Channel channel = new Channel(ChannelCount3, new ObjectIdentifier(sOid), sNm, 0);
                         tempPort.ChannelsList.Add(channel);
                    }

                    AnalogPortsList.Add(tempPort);
               }
          }

          public async Task<bool> init(int iBoardID, string sBoardName, string sIpAddress, int iPort, string sCommunity,
               int iDigitalInputRefreshInMs, int iAnalogInputRefreshInMs, bool bAutoRefreshDigitalInput,
               bool bAutoRefreshAnalogInput)
          {
               bool success = false;

               CancelTokenSource = new CancellationTokenSource();
               GlobalCancelToken = CancelTokenSource.Token;

               BoardID = iBoardID;
               IoPort = iPort;
               BoardName = sBoardName;
               IP_Address = sIpAddress;

               IP = IPAddress.Parse(sIpAddress);
               EndPoint = new IPEndPoint(IP, IoPort);

               DigitalInputRefreshInMs = iDigitalInputRefreshInMs;
               AnalogInputRefreshInMs = iAnalogInputRefreshInMs;

               await Task.Run(() => GetInitialValues_Async(GlobalCancelToken)).ConfigureAwait(false);

               ConnectionState = BoardState.CONNECTED;
               success = true;

               if (onInit != null)
                    onInit(BoardID, DigitalPortsList, AnalogPortsList);

               //if (onLoggingRaised != null)
               //    onLoggingRaised("Initialized GPIO Ethernet Board with ID : " + _BoardID.ToString()
               //        + " and Name : " + _BoardName + " completed successfully", "#0000FF");

               return success;
          }

          private async void getInitialValues(int BoardId, List<EthernetBoardPort> _DigPtList, List<EthernetBoardPort> _AnaPtList)
          {
               var sw = Stopwatch.StartNew();
               Task<string> getResult;

               foreach (EthernetBoardPort port in _DigPtList)
               {
                    foreach (Channel channel in port.ChannelsList)
                    {
                         getResult = null;

                         try
                         {
                              getResult = await Task.Factory.StartNew(() =>
                                  SnmpGet_Async(channel.OID, GlobalCancelToken)); //, port.ParentIP, port.EbPortNo, SNMP_GET_TIMEOUT));

                              Task.WaitAny(getResult);

                              if (getResult != null && getResult.Status == TaskStatus.RanToCompletion)
                                   channel.State = (getResult.Result == "0" ? ChannelState.OFF : ChannelState.ON);
                              else
                                   channel.State = ChannelState.OFF;
                         }
                         catch (Exception ex)
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
                    }
               }

               foreach (EthernetBoardPort port in _AnaPtList)
               {
                    foreach (Channel channel in port.ChannelsList)
                    {
                         getResult = null;

                         try
                         {
                              getResult = await Task.Factory.StartNew(() =>
                                  SnmpGet_Async(channel.OID, GlobalCancelToken));

                              Task.WaitAny(getResult);

                              if (getResult != null && getResult.Status == TaskStatus.RanToCompletion)
                                   channel.State = (getResult.Result == "0" ? ChannelState.OFF : ChannelState.ON);
                              else
                                   channel.State =  ChannelState.OFF;
                         }
                         catch (Exception ex)
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
                    }
               }


               if (onInit != null)
                    onInit(this.BoardID, this.DigitalPortsList, this.AnalogPortsList);

               Debug.Print(sw.Elapsed + " getInitValues");
          }

          public bool GetInitialValues_Async(CancellationToken ct)
          {
               // Task<string> result = null;
               string result = null;
               int newVal;
               bool success = false;

               Debug.Print("GetInitialValues_Async BEGIN--------");

               Parallel.ForEach(DigitalPortsList[0].ChannelsList, async ch =>
               {
                    result = null;
                    try
                    {
                         // result = await Task.Run(() => SnmpGet_Async(channel));
                         result = await TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3);

                         if (result != null && int.TryParse(result, out newVal))
                              ch.State = (newVal == 0 ? ChannelState.OFF : ChannelState.ON);
                         success = true;
                         Debug.Print("GetInitialValues_Async IO1 ID: " + ch.Id + " Value: " + result);
                    }
                    catch (AggregateException ex)
                    {
                         Debug.Print("GetInitialValues_Async AGGREAGATE");
                         HandleAggregateErrors(ex);
                    }
                    catch (Exception ex)
                    {
                         Debug.Print("GetInitialValues_Async EXCEPTION");
                         HandleErrors(ex);
                    }
               });

               Parallel.ForEach(DigitalPortsList[1].ChannelsList, async ch =>
               {
                    result = null;

                    try
                    {
                         //result = await Task.Run(() => Board0.SnmpGet_Async(ch));
                         result = await TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3);

                         if (result != null && int.TryParse(result, out newVal))
                              ch.State = (newVal == 0 ? ChannelState.OFF : ChannelState.ON);
                         success = true;
                         Debug.Print("GetInitialValues_Async IO2 ID: " + ch.Id + " Value: " + result);
                    }
                    catch (AggregateException ex)
                    {
                         Debug.Print("GetInitialValues_Async AGGREAGATE");
                         HandleAggregateErrors(ex);
                    }
                    catch (Exception ex)
                    {
                         Debug.Print("GetInitialValues_Async EXCEPTION");
                         HandleErrors(ex);
                    }
               });

               if (BoardID == 0 || BoardID == 4)
               {
                    Parallel.ForEach(AnalogPortsList[0].ChannelsList, async ch =>
                    {
                         result = null;

                         try
                         {
                              //result = await Task.Run(() => SnmpGet_Async(channel, 100));
                              result = await TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3);

                              if (result != null && int.TryParse(result, out newVal))
                                   ch.State = (newVal == 0 ? ChannelState.OFF : ChannelState.ON);
                              success = true;
                              Debug.Print("GetInitialValues_Async ADc1 ID: " + ch.Id + " Value: " + result);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    });
               }

               if (onInit != null)
                    onInit(BoardID, DigitalPortsList, AnalogPortsList);

               return success;
          }


          #endregion

          #region ChannelMethods

          public async Task ToggleRelay(ObjectIdentifier pOid, ChannelState pState)
          {
               int newValue = (pState == ChannelState.ON ? 0 : 1); // Set it to the opposite
               await setIOPortChannel(pOid, newValue).ConfigureAwait(false);            
          }

          public async Task setIOPortChannel(ObjectIdentifier pOid, int pNewChannelValue)
          {
               try
               {
                    await Task.Run(() => SnmpSet_Async(pOid, pNewChannelValue, GlobalCancelToken)).ConfigureAwait(false);
               }
               catch (AggregateException ex)
               {
                    HandleAggregateErrors(ex);
               }
          }

          #endregion

          #region CleanUpMethods

          ~EthernetBoard()
          {
               // free native resources if there are any.
               if (nativeResource != IntPtr.Zero)
               {
                    Marshal.FreeHGlobal(nativeResource);
                    nativeResource = IntPtr.Zero;
               }
          }

          public void Dispose()
          {
               DigitalTimer.Dispose();
               AnalogTimer.Dispose();

               task_AnalogTimer.Dispose();
               task_DigitalTimer.Dispose();

               GC.ReRegisterForFinalize(this);
          }

          #endregion

          #region SnmpMethods

          /// <summary>
          /// A syncronous call to Snmp Get method.
          /// </summary>
          /// <param name="OID">The Object Identifier object associated with the channel to read.</param>
          /// <returns>The value read from the channel.</returns>
          public string SnmpGet(ObjectIdentifier OID)
          {
               IList<Variable> result = null;
               string retVal;

               try
               {
                    result = Messenger.Get(VersionCode.V1, EndPoint,
                         new OctetString(COMMUNITY),
                         new List<Variable> { new Variable(OID) },
                         SNMP_GET_TIMEOUT);
               }
               catch (Exception ex)
               {
                    if ((!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)) &&
                            (!(ex is System.Net.Sockets.SocketException)))
                    {
                         throw;
                    }
               }

               if (result != null && result.Count > 0)
                    retVal = result[0].Data.ToString();
               else
                    retVal = string.Empty;

               return retVal;
          }

          /// <summary>
          /// Snmp GET for all channels in specified port. 
          /// </summary>
          /// <param name="_portToScan">The Port to try to read.</param>
          /// <returns></returns>
          public IList<Variable> SnmpGetAll(EthernetBoardPortNo _portToScan)
          {
               IList<Variable> result = null;
               ObjectIdentifier _oid;

               switch (_portToScan)
               {
                    case EthernetBoardPortNo.DIGITAL_PORT_1:
                         _oid = ChannelHelper.Io1_All;
                         break;
                    case EthernetBoardPortNo.DIGITAL_PORT_2:
                         _oid = ChannelHelper.Io2_All;
                         break;
                    case EthernetBoardPortNo.ADC_PORT_1:
                         _oid = ChannelHelper.Ad1_All;
                         break;
                    default:
                         throw new InvalidOperationException("SnmpGetAll: The port number to scan was incorrect.");
               }

               try
               {
                    result = Messenger.Get(VersionCode.V1, EndPoint,
                         new OctetString(COMMUNITY),
                         new List<Variable> { new Variable(_oid) },
                         SNMP_GET_TIMEOUT);
               }
               catch (Exception ex)
               {
                    if ((!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)) &&
                            (!(ex is System.Net.Sockets.SocketException)))
                    {
                         throw;
                    }
               }

               if (result != null && result.Count > 0)
                    return result.ToList();
               else
                    return null;
          }

          /// <summary>
          /// An asyncronous call to the Snmp Get method.
          /// </summary>
          /// <param name="ch"></param>
          /// <param name="ct"></param>
          /// <returns></returns>
          public async Task<string> SnmpGet_Async(ObjectIdentifier pOid, CancellationToken ct)
          {
               IList<Variable> result = null;
               string retVal;

               if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

               // This wrapper is super IMPORTANT recheck convention
               try
               {
                    await Task.Factory.StartNew(() =>
                    {
                         result = Messenger.Get(VersionCode.V1,
                             EndPoint, new OctetString(COMMUNITY),
                             new List<Variable> { new Variable(pOid) }, SNMP_GET_TIMEOUT);

                    }, ct, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).ConfigureAwait(false);
               }
               catch (AggregateException ex)
               {
                    HandleAggregateErrors(ex);
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               if (result != null && result.Count > 0)
                    retVal = result[0].Data.ToString();
               else
                    retVal = string.Empty;

               return retVal;
          }

          /// <summary>Updates the EthernetBoard.Port.Channel value of a device.</summary>
          /// <returns>(List) of type Variable (octet dictionary) </returns>
          public async Task<string> SnmpSet_Async(ObjectIdentifier pOid, int pNewChannelValue, CancellationToken ct)
          {
               // use TPL 
               string retVal = null;
               IList<Variable> result = null;
               List<Variable> lstVar = new List<Variable>();
               Variable VarOid = new Variable(pOid, new Integer32(pNewChannelValue));
               lstVar.Add(VarOid);

               try
               {
                    if (ct.IsCancellationRequested || ct.IsCancellationRequested)
                         ct.ThrowIfCancellationRequested();

                    // This wrapper is super IMPORTANT recheck convention
                    await Task.Run(() =>
                    {
                         result = null;

                         try
                         {
                              result = Messenger.Set(VersionCode.V1, EndPoint,
                                  new OctetString(COMMUNITY), lstVar, SNMP_SET_TIMEOUT);
                         }
                         catch (Exception e)
                         {
                              if (!(e is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                                  && !(e is System.Net.Sockets.SocketException))
                              {
                                   throw;
                              }
                         }
                    }).ConfigureAwait(false);
               }
               catch (AggregateException ex)
               {
                    HandleAggregateErrors(ex);
               }

               if (result != null && result.Count > 0)
                    retVal = result[0].Data.ToString();
               else
                    retVal = string.Empty;

               return retVal;
          }

          /// <summary>
          /// Not Async not Parallel nothing special. Does spawn a Task though
          /// </summary>
          /// <param name="_Oid"></param>
          /// <param name="_sCommunity"></param>
          /// <param name="_iChannelValue"></param>
          /// <param name="ct">The CancellationToken to stop this </param>
          /// <returns></returns>
          /// 
          private string SnmpSet(ObjectIdentifier _Oid, string _sCommunity, int _iChannelValue, CancellationToken ct)
          {
               IList<Variable> result = null;
               List<Variable> lstVar = new List<Variable>();
               lstVar.Add(new Variable(_Oid, new Integer32(_iChannelValue)));
               string retVal = null;
               string _sOid = _Oid.ToString();

               if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

               try
               {
                    Task.Factory.StartNew(() =>
                    {                        
                         result = Messenger.Set(VersionCode.V1, EndPoint,
                             new OctetString(_sCommunity), lstVar, SNMP_SET_TIMEOUT);
                    },
                    ct, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current).ConfigureAwait(false);
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               if (result != null && result.Count > 0)
                    retVal = result[0].Data.ToString();
               else
                    retVal = string.Empty;

               return retVal;
          }

          /// <summary>
          /// Not Async not Parallel nothing special. Does spawn a Task though
          /// </summary>
          /// <param name="_Oid"></param>
          /// <param name="_sCommunity"></param>
          /// <param name="_iChannelValue"></param>
          /// <param name="ct">The CancellationToken to stop this </param>
          /// <returns></returns>
          /// 
          private IList<Variable> SnmpSetAll(ObjectIdentifier _Oid, string _sCommunity, int _iChannelValue, CancellationToken ct)
          {
               IList<Variable> result = null;
               List<Variable> lstVar = new List<Variable>();
               lstVar.Add(new Variable(_Oid, new Integer32(_iChannelValue)));
               string retVal = null;
               string _sOid = _Oid.ToString();

               if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

               try
               {
                    Task.Factory.StartNew(() =>
                    {
                         result = Messenger.Set(VersionCode.V1, EndPoint,
                             new OctetString(_sCommunity), lstVar, SNMP_SET_TIMEOUT);
                    },
                    ct, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current).ConfigureAwait(false);
               }
               catch (Exception ex)
               {
                    HandleErrors(ex);
               }

               return result;
          }

          /// <summary>
          /// 
          /// </summary>
          /// <param name="_Oid">The object Identifier of the channel.</param>
          /// <param name="_sCommunity">Ths community string (password).</param>
          /// <param name="_iChannelValue">The value to set the channel to</param>
          /// <param name="ct">The Task CancellationToken</param>
          /// <returns>The result of the Set command.
          private async Task<string> SnmpSet_Async(ObjectIdentifier _Oid, string _sCommunity, int _iChannelValue, CancellationToken ct)
          {
               IList<Variable> result = null;
               List<Variable> lstVar = new List<Variable>();
               lstVar.Add(new Variable(_Oid, new Integer32(_iChannelValue)));
               string retVal = null;
               string _sOid = _Oid.ToString();

               try
               {
                    await Task.Factory.StartNew(() =>
                    {
                         if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                              ct.ThrowIfCancellationRequested();

                         result = Messenger.Set(VersionCode.V1, EndPoint,
                             new OctetString(_sCommunity), lstVar, SNMP_SET_TIMEOUT);
                    },
                    ct, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current).ConfigureAwait(false);
               }
               catch (AggregateException ex)
               {
                    HandleAggregateErrors(ex);
               }

               if (result != null && result.Count > 0)
                    retVal = result[0].Data.ToString();
               else
                    retVal = string.Empty;

               return retVal;
          }

          /// <summary>
          /// Retrieves values from an EndPoint (Ip, Port) 
          /// </summary>
          /// <returns>(List) of type Variable (octet dictionary). The new value of the channel</returns>
          public async Task<string> SnmpGet_AsyncCallback(Channel ch, CancellationToken ct)
          {
               IList<Variable> result = null;
               List<Variable> lstVar = new List<Variable>();
               RequestState MyGetStateObject = new RequestState();

               GetRequestMessage GetReqMessage;
               IAsyncResult asyncGetResults;

               GetReqMessage = new GetRequestMessage(111, VersionCode.V1,
                       new OctetString("private"), new List<Variable> { new Variable(ch.OID) });

               MyGetStateObject.Get_Request_Message = GetReqMessage;

               //// IAsyncResult
               //asyncGetResults = GetReqMessage.BeginGetResponse(EndPoint, new UserRegistry(), EndPoint.GetSocket(), 
               //    new AsyncCallback(GetResponse_Callback), MyGetStateObject);

               asyncGetResults = GetReqMessage.BeginGetResponse(EndPoint, new UserRegistry(), EndPoint.GetSocket(), async ar =>
               {
                    MyGetStateObject.Get_Response_Message = await Task.Factory.StartNew(() => GetReqMessage.EndGetResponse(ar));

                    if (MyGetStateObject.Get_Response_Message.Pdu().ErrorStatus.ToInt32() != 0) // Error?
                    {
                         result = MyGetStateObject.Get_Response_Message.Variables();
                         Debug.Print(MyGetStateObject.Get_Response_Message.ToString());
                    }

               }, null);

               return asyncGetResults.ToString();
          }

          // **************************************************

          // uses the ONLY async calls in  snmpLibrary
          private static void GetResponse_Callback(IAsyncResult asynchronousResult)
          {
               IList<Variable> result = null;
               RequestState getRequestState = (RequestState)asynchronousResult.AsyncState;
               GetRequestMessage GetRequestMsg = getRequestState.Get_Request_Message;

               // ISnmpMessage
               getRequestState.Get_Response_Message = GetRequestMsg.EndGetResponse(asynchronousResult);

               if (getRequestState.Get_Response_Message.Pdu().ErrorStatus.ToInt32() != 0) // Error?
               {
                    result = getRequestState.Get_Response_Message.Variables();
                    Debug.Print(getRequestState.Get_Response_Message.ToString());
               }
               return;
          }

          public string SnmpSet_AsyncCallback(Channel ch)
          {
               List<Variable> lstVar = new List<Variable>();

               RequestState MySetStateObject = new RequestState();
               SetRequestMessage SetReqMsg;
               IAsyncResult asyncResponse;

               int _NewChannelValue = ch.Value;
               lstVar.Add(new Variable(ch.OID, new Integer32(_NewChannelValue)));

               // build the SET message
               SetReqMsg = new SetRequestMessage(211, VersionCode.V1, new OctetString("private"), lstVar);

               MySetStateObject.Set_Request_Message = SetReqMsg;

               // Start the SET Request - is already async 
               asyncResponse = SetReqMsg.BeginGetResponse(EndPoint, new UserRegistry(),
                   EndPoint.GetSocket(), new AsyncCallback(SetResponse_Callback), MySetStateObject);

               return asyncResponse.ToString();
          }

          private static void SetResponse_Callback(IAsyncResult asynchronousResult)
          {
               RequestState setRequestState = (RequestState)asynchronousResult.AsyncState;
               SetRequestMessage SetRequestMsg = setRequestState.Set_Request_Message;

               // ISnmpMessage
               setRequestState.Get_Response_Message = SetRequestMsg.EndGetResponse(asynchronousResult);

               // Read the response into a Stream object.
               //asyncResponse = myRequestState.Respose_Message.BeginGetResponse()
               return;
          }

          #endregion

          #region TimerMethods

          /*********************************************************************************/
          /*************************** EVENT HANDLERS **************************************/
          public bool StartTimers()
          {
               bool success = false;

               try
               {
                    DigitalTimer = new System.Timers.Timer(DigitalInputRefreshInMs);
                    DigitalTimer.AutoReset = false; // To keep threads from piling up, manually control the timer
                    DigitalTimer.Elapsed += new ElapsedEventHandler(DigitalTimer_OnTick);
                    DigitalTimer.Enabled = true;

                    if (BoardID == 0 || BoardID == 4)
                    {
                         AnalogTimer = new System.Timers.Timer(AnalogInputRefreshInMs);
                         AnalogTimer.AutoReset = false; // To keep threads from piling up, manually control the timer
                         AnalogTimer.Elapsed += new ElapsedEventHandler(AnalogTimer_OnTick);
                         AnalogTimer.Enabled = true;
                    }

                    success = true;
               }
               catch(Exception ex)
               {
                    HandleErrors(ex);
               }

               return success;               
          }

          public bool ToggleDigitalInputTimer()
          {
               if (DigitalTimer.Enabled)
               {
                    DigitalTimer.Enabled = false;
               }
               else
               {
                    DigitalTimer.AutoReset = false;
                    DigitalTimer.Interval = DigitalInputRefreshInMs;
                    DigitalTimer.Enabled = true;
               }

               return DigitalTimer.Enabled;
          }

          public bool ToggleAnalogInputTimer()
          {
               if (BoardID == 0 || BoardID == 4)
               {
                    if (AnalogTimer.Enabled)
                    {
                         AnalogTimer.Enabled = false;
                    }
                    else
                    {
                         AnalogTimer.AutoReset = false;
                         AnalogTimer.Interval = this.AnalogInputRefreshInMs;
                         AnalogTimer.Enabled = true;
                    }
               }

               return AnalogTimer.Enabled ? true : false;
          }

          // This is the timer tick code. Executes every (_DigitalInputRefreshInMs) milliseconds
          private void DigitalTimer_OnTick(object sender, ElapsedEventArgs e)
          {
               //Task.Run(() => CheckForNewDigitalData_Async(_DigitalPortsList, GlobalCancelToken));
               // Stil have to nail this damn tomer down

               // Cancel requested?
               if (GlobalCancelToken.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
               {
                    GlobalCancelToken.ThrowIfCancellationRequested();
               }

               task_DigitalTimer = Task.Factory.StartNew(() =>
                   CheckForNewDigitalData_Async(DigitalPortsList, GlobalCancelToken).ConfigureAwait(false),
                   GlobalCancelToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
          }

          private void AnalogTimer_OnTick(object sender, ElapsedEventArgs e)
          {
               //Task.Run(() => CheckForNewAnalogData_Async(_AnalogPortsList, GlobalCancelToken));

               // Cancel requested?
               if (GlobalCancelToken.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
               {
                    GlobalCancelToken.ThrowIfCancellationRequested();
               }

               task_AnalogTimer = Task.Factory.StartNew(() =>
                   CheckForNewAnalogData_Async(AnalogPortsList, GlobalCancelToken).ConfigureAwait(false),
               GlobalCancelToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
          }

          private async Task CheckForNewDigitalData_Async(object state, CancellationToken ct)
          {
               List<EthernetBoardPort> lstBrd = (List<EthernetBoardPort>)state;

               string getResult = null;
               string setResult = null;
               ObjectIdentifier[] _OidVals = null; //new[] { lstBrd.SelectMany(port => port.ChannelsList };
               int NewChannelValue;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to CurrentDigitalChannelValues list. Update list if new values are found //
               //*************************************************************************************//           

               foreach (EthernetBoardPort pt in lstBrd)
               {
                    // Array of ObjectIdentifiers for the OIDs
                    _OidVals = pt.ChannelsList.Select(c => c.OID).ToArray();

                    foreach (Channel ch in pt.ChannelsList)
                    {
                         if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                              ct.ThrowIfCancellationRequested();

                         getResult = null;
                         setResult = null;

                         try
                         {
                              //getResult = await Task.Run(() => SnmpGet_Async(ch));
                              getResult = await TaskTools.RetryOnFault(() => SnmpGet_Async(_OidVals[ch.Id-1], ct), 2).ConfigureAwait(false);

                              if (getResult != null && getResult != string.Empty)
                              {
                                   if (int.TryParse(getResult, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light is on/off!!
                                        {
                                             Debug.Print("NEW DIG LIGHT VALUE!!!!");

                                             if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                                                  ct.ThrowIfCancellationRequested();

                                             lock (pt) // Lock Port
                                             {
                                                  //setResult = await Task.Run(() => SnmpSet_Async(ch, NewChannelValue));
                                                  //setResult = TaskTools.RetryOnFault(() => SnmpSet_Async(ch, NewChannelValue), 3);
                                                  setResult = SnmpSet(ch.OID, COMMUNITY, NewChannelValue, ct);

                                                  if (setResult != null && setResult != string.Empty)
                                                  {
                                                       ch.State = (NewChannelValue == 0 ? ChannelState.OFF : ChannelState.ON);

                                                       // do user interface stuff 
                                                       if (onDigitalInput != null)
                                                            onDigitalInput(BoardID, pt.IoPortNo, ch);
                                                  }
                                             }
                                        }
                                   }
                              }
                         }
                         catch (AggregateException ex)
                         {
                              HandleAggregateErrors(ex);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }
               }

               DigitalTimer.Start();
          }

          private async Task CheckForNewAnalogData_Async(object state, CancellationToken ct)
          {
               string getResult = null;
               string setResult = null;
               int NewChannelValue;

               ObjectIdentifier[] _OidVals = null; //new[] { lstBrd.SelectMany(port => port.ChannelsList };
               List<EthernetBoardPort> lstBrd = (List<EthernetBoardPort>)state;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to AnalogPortsList list. Update list if new values are found              //
               //*************************************************************************************//

               foreach (EthernetBoardPort pt in lstBrd)
               {
                    // Array of ObjectIdentifiers for the OIDs
                    _OidVals = pt.ChannelsList.Select(c => c.OID).ToArray();

                    if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                         ct.ThrowIfCancellationRequested();

                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;
                         setResult = null;

                         try
                         {

                              if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                                   ct.ThrowIfCancellationRequested();

                              //getResult = await TaskTools.RetryOnFault(() => SnmpGet_Async(_OidVals[ch.Id - 1], ct), 2).ConfigureAwait(false);
                              getResult = await Task.Run(() => SnmpGet_Async(_OidVals[ch.Id - 1], ct)).ConfigureAwait(false);

                              if (getResult != null && getResult != string.Empty)
                              {
                                   if (int.TryParse(getResult, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light has new value
                                        {
                                             Debug.Print("NEW ANALOG LIGHT VALUE!!!!");

                                             lock (this)
                                             {
                                                  setResult = SnmpSet(ch.OID, COMMUNITY, NewChannelValue, ct);

                                                  if (setResult != null && setResult != string.Empty)
                                                  {
                                                       ch.State = (NewChannelValue == 0 ? ChannelState.OFF : ChannelState.ON);

                                                       if (onAnalogInput != null)
                                                            onAnalogInput(this.BoardID, pt.IoPortNo, ch);
                                                  }
                                             }
                                        }
                                   }
                              }
                         }
                         catch (AggregateException ex)
                         {
                              HandleAggregateErrors(ex);
                         }
                    }
               }

               //task_AnalogTimer = null;
          }

          private void HandleErrors(Exception ex)
          {
               if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                    && !(ex is System.Net.Sockets.SocketException)
                    && !(ex is OperationCanceledException)
                    && !(ex is TaskCanceledException))
               {
                    if (onError != null)
                         onError(this, new ErrorEventArgs(ex));
               }
          }

          private void HandleAggregateErrors(AggregateException ex)
          {
               bool isValidError = false;
               System.Text.StringBuilder sb = new System.Text.StringBuilder();

               // if any of the errors are unexpected pass all of them
               foreach (Exception e in ex.InnerExceptions)
               {
                    if (!(e is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                        && !(e is System.Net.Sockets.SocketException)
                        && !(e is OperationCanceledException)
                        && !(e is TaskCanceledException))
                    {
                         isValidError = true;
                    }
               }

               if (isValidError)
               {
                    if (onAsyncError != null)
                         onAsyncError(this, new AsyncErrorEventArgs(ex));
                    else
                         Debug.Print(ex.Flatten().ToString());
               }
          }

          #endregion

          #region NotCurrentlyUsed

          public void LogStuff(string msg)
          {
               if (onLoggingRaised != null)
                    onLoggingRaised(msg, "#FF0000");
          }

          private void OutputInfo(string msg, string OID, string chValue)
          {
               Debug.Print("OutputInfo: " + msg + " OID: " + OID + " Value: " + chValue);
          }


          #endregion

          #region StaticMethods



          // Uses calls to static functions
          private async Task CheckForNewDigitalData2_Async(CancellationToken _ct)
          {
               Task<string> getResult = null;
               Task<string> setResult = null;
               int NewChannelValue;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to CurrentDigitalChannelValues list. Update list if new values are found //
               //*************************************************************************************//           

               foreach (EthernetBoardPort pt in this.DigitalPortsList)
               {
                    IPAddress _Ip = IPAddress.Parse(pt.ParentIP);
                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;
                         setResult = null;

                         try
                         {
                              getResult = await Task.Factory.StartNew(() =>
                                  SnmpGet_StaticAsync(ch.OID, pt.ParentIP, pt.IoPortNo, _ct)).ConfigureAwait(false);

                              if ((await getResult).Equals(TaskStatus.RanToCompletion))
                              {
                                   if (int.TryParse(getResult.Result, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light is on/off!!
                                        {
                                             //lock(pt)
                                             //{
                                             // write new value 
                                             setResult = await Task.Factory.StartNew(()
                                                 => SnmpSet_StaticAsync(ch.OID, pt.ParentIP, pt.IoPortNo, NewChannelValue, _ct)).ConfigureAwait(false);

                                             ch.State = (NewChannelValue == 0 ? ChannelState.OFF : ChannelState.ON);

                                             // do user interface stuff 
                                             if (onDigitalInput != null)
                                                  onDigitalInput(BoardID, pt.IoPortNo, ch);
                                        }
                                   }
                              }
                         }
                         catch (AggregateException ae)
                         {
                              HandleAggregateErrors(ae);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }
               }

               task_DigitalTimer = null;
          }

          // Uses calls to static functions
          private async Task CheckForNewAnalogData2_Async(CancellationToken ct)
          {
               Task<string> result;
               int NewChannelValue;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to AnalogPortsList list. Update list if new values are found              //
               //*************************************************************************************//

               foreach (EthernetBoardPort port in this.AnalogPortsList)
               {
                    foreach (Channel channel in port.ChannelsList)
                    {
                         result = null;
                         try
                         {
                              result = await Task.Factory.StartNew(() =>
                              SnmpGet_StaticAsync(channel.OID, IP.ToString(), port.IoPortNo, ct), ct).ConfigureAwait(false);

                              if ((await result).Equals(TaskStatus.RanToCompletion))
                              {
                                   if (int.TryParse(result.Result, out NewChannelValue))
                                   {
                                        if (NewChannelValue != channel.Value) // Light is on!!
                                        {
                                             result = await Task.Factory.StartNew(()
                                                 => SnmpSet_StaticAsync(channel.OID, IP.ToString(), port.IoPortNo, NewChannelValue, ct), ct).ConfigureAwait(false);

                                             channel.State = (NewChannelValue == 0 ? ChannelState.OFF : ChannelState.ON);

                                             if (onAnalogInput != null)
                                                  onAnalogInput(BoardID, port.IoPortNo, channel);
                                        }
                                   }
                              }
                         }
                         catch (AggregateException ae)
                         {
                              HandleAggregateErrors(ae);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }

                    }
               }

               task_AnalogTimer = null;
          }

          // STATIC 
          public static async Task<string> SnmpSet_StaticAsync(ObjectIdentifier pOid, string sIPAddress, int pPortNo, int pNewChannelValue, CancellationToken ct)
          {
               IList<Variable> result = null;
               List<Variable> lstVar = new List<Variable>();

               lstVar.Add(new Variable(pOid, new Integer32(pNewChannelValue)));

               IPEndPoint MyEp = new IPEndPoint(IPAddress.Parse(sIPAddress), pPortNo);

               try
               {
                    result = await Task.Run(() => Messenger.Set(VersionCode.V1, MyEp,
                            new OctetString("private"), lstVar, SNMP_SET_TIMEOUT), ct).ConfigureAwait(false);

                    if (result != null && result.Count > 0)
                    {
                         Debug.Print("Result is: " + result[0].Data.ToString());
                    }
                    else
                    {
                         Debug.Print("Result is: BROKEN");
                    }
               }
               catch (Exception ex)
               {
                    if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                       && !(ex is System.Net.Sockets.SocketException)
                       && !(ex is OperationCanceledException)
                       && !(ex is TaskCanceledException))
                    {
                         throw;
                    }
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return string.Empty;
          }

          // STATIC
          public static async Task<string> SnmpGet_StaticAsync(ObjectIdentifier pOID, string ip, int pt, CancellationToken ct)
          {
               IList<Variable> result = null;
               IPEndPoint MyEp = new IPEndPoint(IPAddress.Parse(ip), pt);

               try
               {
                    result = await Task.Run(() => Messenger.Get(VersionCode.V1, MyEp,
                            new OctetString("private"),
                            new List<Variable> { new Variable(pOID) },
                            SNMP_GET_TIMEOUT), ct).ConfigureAwait(false);
               }
               catch (Exception ex)
               {
                    if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                       && !(ex is System.Net.Sockets.SocketException)
                       && !(ex is OperationCanceledException)
                       && !(ex is TaskCanceledException))
                    {
                         throw;
                    }
               }

               if (result != null && result.Count > 0)
                    return result[0].Data.ToString();
               else
                    return string.Empty;
          }

          #endregion


          // 8888888888888 BEYONF DHD HERE TEST FUNCTIONS

          // just async - shouldnt block
          public async Task<IEnumerable<Channel>> GetChannels_Async(int ioPortNo)
          {
               IEnumerable<Channel> tsk;

               tsk = await Task.Factory.StartNew(() =>
                       DigitalPortsList[1].ChannelsList.Select(ch => ch));

               return tsk;
          }

          // async and parallel - doesnt block
          public async Task<IEnumerable<Channel>> AllChannelsInParallelNonBlockingAsync(EthernetBoardPort ebPort, CancellationToken ct)
          {
               if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

               IEnumerable<Task<IEnumerable<Channel>>> allTasks =
                    ChannelHelper.AllChannels.Select(ch => GetChannels_Async(ebPort.IoPortNo));

               IEnumerable<Channel>[] allResults = await Task.WhenAll(allTasks);

               return allResults.SelectMany(ch => ch);
          }

          /// <summary>
          /// Parallel Async NON BLIOCKINGT
          /// </summary>
          /// <returns></returns>


          /// <summary>
          /// Parallel no inner task - Async 
          /// </summary>
          /// <returns></returns>
          public async Task GetInitialValues_Parallel(CancellationToken ct)
          {
               System.Text.StringBuilder sb = new System.Text.StringBuilder();

               string result = null;
               int retValue;

               Debug.Print(sb.AppendLine("GetInitialValues_Parallel BEGIN -------------------------").ToString());

               if (DigitalPortsList != null)
               {
                    // Run all tasks parallel wait for them to finish
                    Parallel.ForEach(DigitalPortsList[0].ChannelsList, async ch =>
                    {
                         result = null;

                         try
                         {
                              result = await SnmpGet_Async(ch.OID, GlobalCancelToken).ConfigureAwait(false);

                              if (result != null && int.TryParse(result, out retValue))
                              {
                                   Debug.Print(sb.Append("Channel Io1 ID: ").Append(ch.Id).Append(" Value: ").AppendLine(result).ToString());

                                   if (onDigitalInput != null)
                                        onDigitalInput(BoardID, DigitalPortsList[0].IoPortNo, ch);
                              }
                              else
                              {
                                   Debug.Print(sb.Append("Channel Io1 : FAIL--------------------").ToString());
                              }

                         }
                         catch (AggregateException ex)
                         {
                              HandleAggregateErrors(ex);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    });

                    if (DigitalPortsList != null)
                    {
                         // Run all tasks parallel wait for them to finish
                         Parallel.ForEach(DigitalPortsList[1].ChannelsList, async ch =>
                         {
                              try
                              {
                                   result = await SnmpGet_Async(ch.OID, GlobalCancelToken).ConfigureAwait(false);

                                   if (result != null && int.TryParse(result, out retValue))
                                   {
                                        Debug.Print(sb.Append("Channel Io2 ID: ").Append(ch.Id).Append(" VAlue: ").AppendLine(result).ToString());

                                        if (onDigitalInput != null)
                                             onDigitalInput(BoardID, DigitalPortsList[1].IoPortNo, ch);
                                   }
                                   else
                                   {
                                        Debug.Print(sb.Append("Channel Io2 : FAIL--------------------").ToString());
                                   }
                              }
                              catch (AggregateException ex)
                              {
                                   HandleAggregateErrors(ex);
                              }
                              catch (Exception ex)
                              {
                                   HandleErrors(ex);
                              }
                         });
                    }

                    if (AnalogPortsList != null)
                    {
                         // Run all tasks parallel wait for them to finish
                         Parallel.ForEach(AnalogPortsList[0].ChannelsList, async ch =>
                         {
                              try
                              {
                                   result = await SnmpGet_Async(ch.OID, ct).ConfigureAwait(false);

                                   if (result != null && int.TryParse(result, out retValue))
                                   {
                                        Debug.Print(sb.Append("Channel Adc1 ID: ").Append(ch.Id).Append(" Value: ").AppendLine(result).ToString());

                                        if (onAnalogInput != null)
                                             onAnalogInput(BoardID, AnalogPortsList[0].IoPortNo, ch);
                                   }
                                   else
                                        Debug.Print(sb.Append("Channel Adc1 : FAIL--------------------").ToString());
                              }
                              catch (Exception ex)
                              {
                                   HandleErrors(ex);
                              }
                         });
                    }
               }
          }


          /// uses Linq and .Select - Returns an array of string  (channel values)
          public async Task<String[]> GetVals_Async(EthernetBoardPort _chLst, CancellationToken ct)
          {
               List<Exception> Exceptions = null;
               string[] chValues = null;

               try
               {
                    // best for IO Ports??!?!?      
                    IEnumerable<Task<string>> _channels = _chLst.ChannelsList.Select(ch =>
                        TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3));

                    chValues = await Task.WhenAll(_channels);
               }
               catch (Exception ex)
               {
                    if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                        && !(ex is System.Net.Sockets.SocketException))
                    {
                         if (Exceptions == null)
                              Exceptions = new List<Exception>();

                         Exceptions.Add(ex);
                    }
               }

               if (Exceptions != null)
                    throw new AggregateException(Exceptions);

               return chValues;
          }


          /// <summary>
          ///  Uses Task Parallel Library - Not ASYNC DigitalPortsList[0] - .40
          /// </summary>
          /// <returns></returns>
          public string TPLGet_2(CancellationToken ct)
          {
               string result = null;
               int curIndex = 0;

               Task<string> tsk1, tsk2, tsk3, tsk4, tsk5, tsk6, tsk7, tsk8;

               System.Text.StringBuilder sb = new System.Text.StringBuilder();
               for (int indx = 0; indx < 2; indx++)
               {
                    if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                         ct.ThrowIfCancellationRequested();

                    tsk1 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[0].OID));
                    tsk2 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[1].OID));
                    tsk3 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[2].OID));
                    tsk4 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[3].OID));
                    tsk5 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[4].OID));
                    tsk6 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[5].OID));
                    tsk7 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[6].OID));
                    tsk8 = Task.Factory.StartNew(() => SnmpGet(DigitalPortsList[indx].ChannelsList[7].OID));

                    List<Task<string>> lstTasks = new List<Task<string>>() { { tsk1 }, { tsk2 }, { tsk3 }, { tsk4 }, { tsk5 }, { tsk6 }, { tsk7 }, { tsk8 } };

                    if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                         ct.ThrowIfCancellationRequested();

                    // Begins processing tasks as soon as the first one is finished
                    Task.WaitAll(lstTasks.ToArray());

                    sb.AppendLine("-------------- GetTPL_2 -------------------");

                    // Check result and write to device if changed
                    Parallel.ForEach(lstTasks, t =>
                    {
                         result = string.Empty;
                         curIndex = lstTasks.FindIndex(a => a.Id == t.Id);

                         if (ct.IsCancellationRequested || CancelTokenSource.IsCancellationRequested)
                              ct.ThrowIfCancellationRequested();

                         if (t.IsFaulted)
                         {
                              sb.AppendLine("----------- WTF ERROR -----------------");
                              sb.AppendLine("Task ID: " + t.Id + " Status: " + t.Status);
                         }
                         else
                         {
                              result = t.Result;

                              if (result.Length > 0 && result != null)
                              {
                                   sb.AppendLine("------------ SUCCESS --------------");
                                   sb.AppendLine("Task ID: " + t.Id + " Status: " + t.Status);
                              }
                              else
                              {
                                   sb.AppendLine("-----------NOT FUCKING SUCCESS --------------");
                                   sb.AppendLine("Task ID: " + t.Id + " Status: " + t.Status);
                              }
                              OutputInfo("Task ID: " + t.Id + " Status: " + t.Status,
                                  DigitalPortsList[1].ChannelsList[curIndex].OID.ToString(), t.Result.ToString());
                         }
                    });
               }

               return sb.ToString();
          }

          public async Task InitGetValues_AsyncRetry(CancellationToken ct)
          {
               Task<string> getResult = null;
               //Task<string> setResult = null;

               Debug.Print("InitGetValues_AsyncRetry BEGIN--------");

               foreach (EthernetBoardPort pt in DigitalPortsList)
                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;

                         try
                         {
                              // Works
                              getResult = await Task.Factory.StartNew(() => TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3));
                              //setResult = await getResult.ContinueWith(tc => TaskTools.RetryOnFault(() =>
                              //    SnmpSet_Async(ch, Convert.ToInt32(tc.Result)), 3), TaskContinuationOptions.OnlyOnRanToCompletion);

                              // works
                              //getResult = await Task.Factory.StartNew(() => SnmpGet_Async(ch));
                              //setResult = await getResult.ContinueWith(t1 => SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)),
                              //    TaskContinuationOptions.OnlyOnRanToCompletion);
                              if (getResult != null && getResult.IsCompleted)
                              {
                                   Debug.Print("Digital Success! Channel ID: " + ch.Id + " Get Value: " + getResult.Result); // + " Set Value: " + setResult.Result);
                              }

                         }
                         catch (AggregateException ex)
                         {
                              HandleAggregateErrors(ex);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }

               foreach (EthernetBoardPort pt in AnalogPortsList)
                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;

                         try
                         {
                              // works
                              getResult = await Task.Factory.StartNew(() => TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3));
                              //setResult = await getResult.ContinueWith(tc => TaskTools.RetryOnFault(() =>
                              //    SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)), 3), TaskContinuationOptions.OnlyOnRanToCompletion);

                              // works
                              //getResult = await Task.Factory.StartNew(() => SnmpGet_Async(ch));
                              //setResult = await getResult.ContinueWith(t1 => SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)),
                              //   TaskContinuationOptions.OnlyOnRanToCompletion);
                              if (getResult != null && getResult.IsCompleted)
                              {
                                   Debug.Print("Analog Success! Channel ID: " + ch.Id + " Get Value: " + getResult.Result); // + " Set Value: " + setResult.Result);
                              }
                         }
                         catch (AggregateException ex)
                         {
                              HandleAggregateErrors(ex);
                         }
                         catch (Exception ex)
                         {
                              HandleErrors(ex);
                         }
                    }
          }
          public async Task<string> TPLGet_1(CancellationToken ct)
          {
               Task<string> tsk1, tsk2, tsk3, tsk4, tsk5, tsk6, tsk7, tsk8;
               System.Text.StringBuilder sb = new System.Text.StringBuilder();
               int newVal, curIndex;

               //TaskFactory<string> sFactory = new TaskFactory<string>(TaskCreationOptions.None, TaskContinuationOptions.NotOnFaulted);
               //tsk1 = sFactory.FromAsync<string>(SnmpGet_StaticAsync(DigitalPortsList[0].channels[0], _IP, DigitalPortsList[0].PortNo, SNMP_GET_TIMEOUT), )
               //TaskScheduler Ts = Task.Factory.Scheduler;
               //tsk1 = new Task<string>(SnmpGet_StaticAsync(DigitalPortsList[0].channels[0], _IP, DigitalPortsList[0].PortNo, SNMP_GET_TIMEOUT), null, TaskCreationOptions.None, TaskScheduler.BelowNormal);

               for (int ind = 0; ind < 2; ind++)
               {
                    tsk1 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[0].OID, ct)).ConfigureAwait(false);
                    tsk2 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[1].OID, ct)).ConfigureAwait(false);
                    tsk3 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[2].OID, ct)).ConfigureAwait(false);
                    tsk4 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[3].OID, ct)).ConfigureAwait(false);
                    tsk5 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[4].OID, ct)).ConfigureAwait(false);
                    tsk6 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[5].OID, ct)).ConfigureAwait(false);
                    tsk7 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[6].OID, ct)).ConfigureAwait(false);
                    tsk8 = await Task.Factory.StartNew(() => SnmpGet_Async(DigitalPortsList[ind].ChannelsList[7].OID, ct)).ConfigureAwait(false);

                    List<Task<string>> lstTasks = new List<Task<string>>() { { tsk1 }, { tsk2 }, { tsk3 }, { tsk4 }, { tsk5 }, { tsk6 }, { tsk7 }, { tsk8 } };

                    sb.AppendLine("-------------- GetTPL_1 -------------------");

                    Parallel.ForEach(lstTasks, async t =>
                    {

                         await t.ConfigureAwait(false);

                         curIndex = lstTasks.FindIndex(a => a.Id == t.Id);

                         if (t.IsFaulted)
                         {
                              sb.AppendLine("-------------- ERROR -------------------");
                         }
                         else
                         {
                              newVal = t.Result == "0" ? 1 : 0;
                         }

                         OutputInfo("Task ID: " + t.Id + " Status: " + t.Status,
                             DigitalPortsList[ind].ChannelsList[curIndex].OID.ToString(), t.Result.ToString());
                    });

               } // end For
               return sb.ToString();
          }

     }



}