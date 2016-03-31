using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;

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

          // public properties
          public string BoardName { get; private set; }
          public int BoardID { get; private set; }
          public int IoPort { get; private set; }
          public string IP_Address { get; private set; }

          private IPEndPoint EndPoint = null;
          private IPAddress IP = null;

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

          public CancellationTokenSource CancelTokenSource = null;
          public CancellationToken GlobalCancelToken;

          private IntPtr nativeResource = Marshal.AllocHGlobal(100);
          private readonly object EthernetBoardLock = new object();

          private readonly object LockHelper = new object();

          Stopwatch sw = new Stopwatch();
          Stopwatch sw2 = new Stopwatch();
          Stopwatch sw3 = new Stopwatch();
          Stopwatch sw4 = new Stopwatch();
          Stopwatch sw5 = new Stopwatch();
          Stopwatch sw6 = new Stopwatch();
          Stopwatch sw7 = new Stopwatch();

          int? dTaskID, aTaskId;
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
               StringBuilder sb = new StringBuilder();
               EthernetBoardPort tempPort = null;

               BoardID = _boardId;
               IP_Address = _IpAddress;

               string sOid = null;
               string sNm = null;

               DigitalPortsList = new List<EthernetBoardPort>();
               AnalogPortsList = new List<EthernetBoardPort>();

               // These loops assign the channels names, numbers, and other values to
               // the List of Channel objects in the EthernetBoardPort object
               // lock it while reading so no other thread messes with the counter variables
               tempPort = new EthernetBoardPort(EthernetBoardPortNo.DIGITAL_PORT_1, false, PortMode.INPUT, _IpAddress, _boardId); // Port#, not analog, port mode

               for (int ChannelCount1 = 0 + Utilities.DIG_CHAN_START_NO; ChannelCount1 < Utilities.DIG_CHAN_PER_PORT + Utilities.DIG_CHAN_START_NO; ChannelCount1++)
               {
                    sb.Clear();
                    sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.1.").Append(ChannelCount1).Append(".0").ToString();
                    sNm = "Digital Port # 0 Channel # " + ChannelCount1;

                    tempPort.ChannelsList.Add(new Channel(ChannelCount1, new ObjectIdentifier(sOid), sNm, 0));
               }

               DigitalPortsList.Add(tempPort);

               tempPort = new EthernetBoardPort(EthernetBoardPortNo.DIGITAL_PORT_2, false, PortMode.INPUT, _IpAddress, _boardId); // Port#, not analog, port mode

               for (int ChannelCount2 = 0 + Utilities.DIG_CHAN_START_NO; ChannelCount2 < Utilities.DIG_CHAN_PER_PORT + Utilities.DIG_CHAN_START_NO; ChannelCount2++)
               {
                    sb.Clear();
                    sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.2.").Append(ChannelCount2).Append(".0").ToString();
                    sNm = "Digital Port # 1 Channel # " + ChannelCount2;

                    tempPort.ChannelsList.Add(new Channel(ChannelCount2, new ObjectIdentifier(sOid), sNm, 0));
               }

               DigitalPortsList.Add(tempPort);

               if (BoardID == 0 || BoardID == 4)
               {
                    tempPort = new EthernetBoardPort(0, true, PortMode.INPUT, _IpAddress, _boardId); // Port#, is analog, port mode

                    // 1 analog port with 4 channels
                    for (int ChannelCount3 = 0 + Utilities.ANLG_CHAN_START_NO; ChannelCount3 < Utilities.ANLG_CHAN_PER_PORT + Utilities.ANLG_CHAN_START_NO; ChannelCount3++)
                    {
                         sb.Clear();
                         sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.3.").Append(ChannelCount3).Append(".0").ToString();
                         sNm = "Analog Port # 1 Channel #" + ChannelCount3;

                         tempPort.ChannelsList.Add(new Channel(ChannelCount3, new ObjectIdentifier(sOid), sNm, 0));
                    }

                    AnalogPortsList.Add(tempPort);
               }
          }

          public async Task<bool> init(string sBoardName, int iPort, string sCommunity, int iDigitalInputRefreshInMs,
              int iAnalogInputRefreshInMs, bool bAutoRefreshDigitalInput, bool bAutoRefreshAnalogInput)
          {
               bool success = false;

               CancelTokenSource = new CancellationTokenSource();
               GlobalCancelToken = CancelTokenSource.Token;

               IoPort = iPort;
               BoardName = sBoardName;

               IP = IPAddress.Parse(IP_Address);
               EndPoint = new IPEndPoint(IP, IoPort);

               DigitalInputRefreshInMs = iDigitalInputRefreshInMs;
               AnalogInputRefreshInMs = iAnalogInputRefreshInMs;

               await Task.Run(() => GetInitialValues_Async(GlobalCancelToken)).ConfigureAwait(false);

               success = true;

               if (onInit != null)
                    onInit(BoardID, DigitalPortsList, AnalogPortsList);

               //if (onLoggingRaised != null)
               //    onLoggingRaised("Initialized GPIO Ethernet Board with ID : " + _BoardID.ToString()
               //        + " and Name : " + _BoardName + " completed successfully", "#0000FF");

               return success;
          }


          public bool GetInitialValues_Async(CancellationToken ct)
          {
               // Task<string> result = null;
               string result = null;
               int newVal;
               bool success = false;

               ct.ThrowIfCancellationRequested();

               Parallel.ForEach(DigitalPortsList[0].ChannelsList, async ch =>
               {
                    result = null;
                    try
                    {
                         // result = await Task.Run(() => SnmpGet_Async(channel));
                         result = await TaskTools.RetryOnFault(() => SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct), 3);

                         if (result != null && int.TryParse(result, out newVal))
                              ch.State = (newVal == 0 ? ChannelState.OFF : ChannelState.ON);

                         success = true;
                         Debug.Print("GetInitialValues_Async IO1 ID: " + ch.Id + " Value: " + result);
                    }
                    catch (AggregateException ex)
                    {
                         if (Utilities.IsRealError(ex))
                         {
                              if (onAsyncError != null)
                                   onAsyncError(this, new AsyncErrorEventArgs(ex));
                              else
                                   Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                         }
                    }
                    catch (Exception ex)
                    {
                         if (Utilities.IsRealError(ex))
                         {
                              if (onError != null)
                                   onError(this, new ErrorEventArgs(ex));
                              else
                                   Debug.Print("Unhandled Error: " + ex.Message);
                         }
                    }
               });

               ct.ThrowIfCancellationRequested();

               Parallel.ForEach(DigitalPortsList[1].ChannelsList, async ch =>
               {
                    result = null;

                    try
                    {
                         //result = await Task.Run(() => Board0.SnmpGet_Async(ch));
                         result = await TaskTools.RetryOnFault(() => SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct), 3);

                         if (result != null && int.TryParse(result, out newVal))
                              ch.State = (newVal == 0 ? ChannelState.OFF : ChannelState.ON);

                         success = true;
                    }
                    catch (AggregateException ex)
                    {
                         if (Utilities.IsRealError(ex))
                         {
                              if (onAsyncError != null)
                                   onAsyncError(this, new AsyncErrorEventArgs(ex));
                              else
                                   Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                         }
                    }
                    catch (Exception ex)
                    {
                         if (Utilities.IsRealError(ex))
                         {
                              if (onError != null)
                                   onError(this, new ErrorEventArgs(ex));
                              else
                                   Debug.Print("Unhandled Error: " + ex.Message);
                         }
                    }
               });

               ct.ThrowIfCancellationRequested();

               if (BoardID == 0 || BoardID == 4)
               {
                    Parallel.ForEach(AnalogPortsList[0].ChannelsList, async ch =>
                    {
                         result = null;

                         try
                         {
                              //result = await Task.Run(() => SnmpGet_Async(channel, 100));
                              result = await TaskTools.RetryOnFault(() => SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct), 3);

                              if (result != null && int.TryParse(result, out newVal))
                                   ch.State = (newVal == 0 ? ChannelState.OFF : ChannelState.ON);

                              success = true;
                         }
                         catch (AggregateException ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    });
               }

               if (onInit != null)
                    onInit(BoardID, DigitalPortsList, AnalogPortsList);

               return success;
          }

          public async Task InitGetValues_AsyncRetry(CancellationToken ct)
          {
               string getResult = null;
               //Task<string> setResult = null;

               foreach (EthernetBoardPort pt in DigitalPortsList)
                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;

                         try
                         {
                              ct.ThrowIfCancellationRequested();

                              // Works
                              getResult = await Task.Run(() => TaskTools.RetryOnFault(() => SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct), 3));
                              //setResult = await getResult.ContinueWith(tc => TaskTools.RetryOnFault(() =>
                              //    SnmpSet_Async(ch, Convert.ToInt32(tc.Result)), 3), TaskContinuationOptions.OnlyOnRanToCompletion);

                              // works
                              //getResult = await Task.Factory.StartNew(() => SnmpGet_Async(ch));
                              //setResult = await getResult.ContinueWith(t1 => SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)),
                              //    TaskContinuationOptions.OnlyOnRanToCompletion);
                              if (getResult != null && getResult.Length > 0)
                              {
                                   Debug.Print("Port: " + pt.IoPortNo + " Channel ID: " + ch.Id + " Value: " + getResult); // + " Set Value: " + setResult.Result);
                              }

                         }
                         catch (AggregateException ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " +
                                            ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    }

               foreach (EthernetBoardPort pt in AnalogPortsList)
                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;

                         try
                         {

                              ct.ThrowIfCancellationRequested();

                              // works
                              getResult = await Task.Run(() => TaskTools.RetryOnFault(() =>
                                  SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct), 3));
                              //setResult = await getResult.ContinueWith(tc => TaskTools.RetryOnFault(() =>
                              //    SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)), 3), TaskContinuationOptions.OnlyOnRanToCompletion);

                              // works
                              //getResult = await Task.Factory.StartNew(() => SnmpGet_Async(ch));
                              //setResult = await getResult.ContinueWith(t1 => SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)),
                              //   TaskContinuationOptions.OnlyOnRanToCompletion);
                              if (getResult != null && getResult.Length > 0)
                              {
                                   Debug.Print("Analog Success! Channel ID: " + ch.Id + " Get Value: " + getResult); // + " Set Value: " + setResult.Result);
                              }
                         }
                         catch (AggregateException ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " +
                                            ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    }
          }

          #endregion

          #region ChannelMethods

          public void ToggleRelay(ObjectIdentifier pOid, ChannelState pState)
          {
               int newValue = (pState == ChannelState.ON ? 0 : 1); // Set it to the opposite
               setIOPortChannel(pOid, newValue).ConfigureAwait(false);
          }

          public async Task setIOPortChannel(ObjectIdentifier pOid, int pNewChannelValue)
          {
               try
               {
                    await Task.Run(() => SnmpTools.SnmpSet_Async(pOid, pNewChannelValue, IP_Address, GlobalCancelToken)).ConfigureAwait(false);
               }
               catch (AggregateException ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onAsyncError != null)
                              onAsyncError(this, new AsyncErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " +
                                  ex.Flatten().InnerExceptions.Select(er => er.Message));
                    }
               }
               catch (Exception ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onError != null)
                              onError(this, new ErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " + ex.Message);
                    }
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
               catch (Exception ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onError != null)
                              onError(this, new ErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " + ex.Message);
                    }
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
                         AnalogTimer.Interval = AnalogInputRefreshInMs;
                         AnalogTimer.Enabled = true;
                    }
               }

               return AnalogTimer.Enabled ? true : false;
          }

          // This is the timer tick code. Executes every (_DigitalInputRefreshInMs) milliseconds
          private void DigitalTimer_OnTick(object sender, ElapsedEventArgs e)
          {
               // Stil have to nail this damn tomer down
               try
               {
                    GlobalCancelToken.ThrowIfCancellationRequested();
                    Debug.Print("DTICK ID: " + BoardID);
                    Task.Run(() => CheckForNewDigitalData_Async(DigitalPortsList, GlobalCancelToken));
                    dTaskID = Task.CurrentId;


                    task_DigitalTimer = Task.Factory.StartNew(() =>
                        CheckForNewDigitalData_Async(DigitalPortsList, GlobalCancelToken).ConfigureAwait(false),
                        GlobalCancelToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
               }

               catch (AggregateException ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onAsyncError != null)
                              onAsyncError(this, new AsyncErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " +
                                  ex.Flatten().InnerExceptions.Select(er => er.Message));
                    }
               }
               catch (Exception ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onError != null)
                              onError(this, new ErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " + ex.Message);
                    }
               }
          }

          private void AnalogTimer_OnTick(object sender, ElapsedEventArgs e)
          {
               try
               {
                    GlobalCancelToken.ThrowIfCancellationRequested();
                    Debug.Print("ATICK");

                    Task.Run(() => CheckForNewAnalogData_Async(AnalogPortsList, GlobalCancelToken));

                    //task_AnalogTimer = Task.Factory.StartNew(() =>
                    //    CheckForNewAnalogData_Async(AnalogPortsList, GlobalCancelToken).ConfigureAwait(false),
                    //    GlobalCancelToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
               }
               catch (AggregateException ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onAsyncError != null)
                              onAsyncError(this, new AsyncErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " +
                                  ex.Flatten().InnerExceptions.Select(er => er.Message));
                    }
               }
               catch (Exception ex)
               {
                    if (Utilities.IsRealError(ex))
                    {
                         if (onError != null)
                              onError(this, new ErrorEventArgs(ex));
                         else
                              Debug.Print("Unhandled Error: " + ex.Message);
                    }
               }

          }

          private async Task CheckForNewDigitalData_Async(object state, CancellationToken ct)
          {
               List<EthernetBoardPort> lstBrd = (List<EthernetBoardPort>)state;

               string getResult = null;
               string setResult = null;
               //ObjectIdentifier[] _OidVals = null; //new[] { lstBrd.SelectMany(port => port.ChannelsList };
               int NewChannelValue;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to CurrentDigitalChannelValues list. Update list if new values are found //
               //*************************************************************************************//           

               foreach (EthernetBoardPort pt in lstBrd)
               {
                    // Array of ObjectIdentifiers for the OIDs
                    //_OidVals = pt.ChannelsList.Select(c => c.OID).ToArray();

                    foreach (Channel ch in pt.ChannelsList)
                    {
                         ct.ThrowIfCancellationRequested();

                         getResult = null;
                         setResult = null;

                         try
                         {
                              //getResult = await Task.Run(() => SnmpGet_Async(ch));
                              getResult = await TaskTools.RetryOnFault(() => SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct), 2).ConfigureAwait(false);

                              if (getResult != null && getResult != string.Empty)
                              {
                                   if (int.TryParse(getResult, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light is on/off!!
                                        {
                                             Debug.Print("NEW DIG LIGHT VALUE!!!!");

                                             ct.ThrowIfCancellationRequested();

                                             lock (pt) // Lock Port
                                             {
                                                  //setResult = await Task.Run(() => SnmpSet_Async(ch, NewChannelValue));
                                                  //setResult = TaskTools.RetryOnFault(() => SnmpSet_Async(ch, NewChannelValue), 3);
                                                  setResult = SnmpTools.SnmpSet(ch.OID, IP_Address, NewChannelValue, ct);

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
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " +
                                            ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (ex is TaskCanceledException)
                              {
                                   Debug.Print("DTASK CANCELLED");
                                   return;
                              }

                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    }
               }

               DigitalTimer.Enabled = true;
          }

          private async Task CheckForNewAnalogData_Async(object state, CancellationToken ct)
          {
               string getResult = null;
               string setResult = null;
               int NewChannelValue;

               List<EthernetBoardPort> lstBrd = (List<EthernetBoardPort>)state;

               //******************************* SCAN PORT *******************************************//
               // check each channel in the port for updated data (button values)                     //
               // Compare it to AnalogPortsList list. Update list if new values are found              //
               //*************************************************************************************//

               foreach (EthernetBoardPort pt in lstBrd)
               {
                    // Array of ObjectIdentifiers for the OIDs
                    //_OidVals = pt.ChannelsList.Select(c => c.OID).ToArray();

                    ct.ThrowIfCancellationRequested();

                    foreach (Channel ch in pt.ChannelsList)
                    {
                         getResult = null;
                         setResult = null;

                         try
                         {
                              ct.ThrowIfCancellationRequested();

                              //getResult = await TaskTools.RetryOnFault(() => SnmpGet_Async(_OidVals[ch.Id - 1], ct), 2).ConfigureAwait(false);
                              getResult = await Task.Run(() => SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct)).ConfigureAwait(false);

                              if (getResult != null && getResult != string.Empty)
                              {
                                   if (int.TryParse(getResult, out NewChannelValue))
                                   {
                                        if (NewChannelValue != ch.Value) // Light has new value
                                        {
                                             Debug.Print("NEW ANALOG LIGHT VALUE!!!!");

                                             lock (this)
                                             {
                                                  setResult = SnmpTools.SnmpSet(ch.OID, IP_Address, NewChannelValue, ct);

                                                  if (setResult != null && setResult != string.Empty)
                                                  {
                                                       ch.State = (NewChannelValue == 0 ? ChannelState.OFF : ChannelState.ON);

                                                       if (onAnalogInput != null)
                                                            onAnalogInput(BoardID, pt.IoPortNo, ch);
                                                  }
                                             }
                                        }
                                   }
                              }
                         }
                         catch (AggregateException ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " +
                                            ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (ex is TaskCanceledException)
                              {
                                   Debug.Print("ATASK CANCELLED");
                                   return;
                              }

                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    }
               }

               AnalogTimer.Enabled = true;
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



          // 8888888888888 BEYONF DHD HERE TEST FUNCTIONS

          // just async - shouldnt block

          /// <summary>
          /// Parallel no inner task - Async 
          /// </summary>
          /// <returns></returns>
          public string GetInitialValues_Parallel(CancellationToken ct)
          {
               System.Text.StringBuilder sb = new System.Text.StringBuilder();
               string result = null;
               int retValue;

               sw4.Start();
               if (DigitalPortsList != null)
               {
                    try
                    {
                         // Run all tasks parallel wait for them to finish
                         Parallel.ForEach(DigitalPortsList[0].ChannelsList, async ch =>
                         {
                              result = null;
                              result = await SnmpTools.SnmpGet_Async(ch.OID, IP_Address, GlobalCancelToken).ConfigureAwait(false);

                              if (result != null && int.TryParse(result, out retValue))
                              {
                                   Debug.Print(sb.Append("Channel Io1 ID: ").Append(ch.Id).Append(" Value: ").AppendLine(result).ToString());

                                   if (onDigitalInput != null)
                                        onDigitalInput(BoardID, DigitalPortsList[0].IoPortNo, ch);
                              }
                         });
                    }
                    catch (AggregateException ex)
                    {
                         if (Utilities.IsRealError(ex))
                         {
                              if (onAsyncError != null)
                                   onAsyncError(this, new AsyncErrorEventArgs(ex));
                              else
                                   Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                         }
                    }
                    catch (Exception ex)
                    {
                         if (Utilities.IsRealError(ex))
                         {
                              if (onError != null)
                                   onError(this, new ErrorEventArgs(ex));
                              else
                                   Debug.Print("Unhandled Error: " + ex.Message);
                         }
                    }

                    if (DigitalPortsList != null)
                    {
                         try
                         {
                              // Run all tasks parallel wait for them to finish
                              Parallel.ForEach(DigitalPortsList[1].ChannelsList, async ch =>
                              {
                                   result = await SnmpTools.SnmpGet_Async(ch.OID, IP_Address, GlobalCancelToken).ConfigureAwait(false);

                                   if (result != null && int.TryParse(result, out retValue))
                                   {
                                        Debug.Print(sb.Append("Channel Io2 ID: ").Append(ch.Id).Append(" VAlue: ").AppendLine(result).ToString());

                                        if (onDigitalInput != null)
                                             onDigitalInput(BoardID, DigitalPortsList[1].IoPortNo, ch);
                                   }
                              });
                         }
                         catch (AggregateException ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " +
                                            ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    }

                    if (AnalogPortsList != null)
                    {
                         try
                         {
                              // Run all tasks parallel wait for them to finish
                              Parallel.ForEach(AnalogPortsList[0].ChannelsList, async ch =>
                              {
                                   result = await SnmpTools.SnmpGet_Async(ch.OID, IP_Address, ct).ConfigureAwait(false);

                                   if (result != null && int.TryParse(result, out retValue))
                                   {
                                        Debug.Print(sb.Append("Channel Adc1 ID: ").Append(ch.Id).Append(" Value: ").AppendLine(result).ToString());

                                        if (onAnalogInput != null)
                                             onAnalogInput(BoardID, AnalogPortsList[0].IoPortNo, ch);
                                   }
                              });
                         }
                         catch (AggregateException ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onAsyncError != null)
                                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " +
                                            ex.Flatten().InnerExceptions.Select(er => er.Message));
                              }
                         }
                         catch (Exception ex)
                         {
                              if (Utilities.IsRealError(ex))
                              {
                                   if (onError != null)
                                        onError(this, new ErrorEventArgs(ex));
                                   else
                                        Debug.Print("Unhandled Error: " + ex.Message);
                              }
                         }
                    }
               }

               return sb.ToString();
          }



     }
}