// The manager receives notifications (traps) on port 162

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Collections.Concurrent;

namespace snmpd
{
     /// <summary>
     /// Description of MainForm.
     /// </summary>
     public partial class MainForm : Form
     {
          //**************** from here to 
          EthernetBoardController MainBoardsController;

          Stopwatch sw = new Stopwatch();
          Stopwatch sw2 = new Stopwatch();
          Stopwatch sw3 = new Stopwatch();
          Stopwatch sw4 = new Stopwatch();
          Stopwatch sw5 = new Stopwatch();
          Stopwatch sw6 = new Stopwatch();
          Stopwatch sw7 = new Stopwatch();

          private CancellationTokenSource MainCancelTokenSource = new CancellationTokenSource();
          private CancellationToken MainCancelToken;

          private EthernetBoard EthernetBoard_1 = new EthernetBoard(0, "192.168.1.40");
          private EthernetBoard EthernetBoard_2 = new EthernetBoard(1, "192.168.1.41");
          private EthernetBoard EthernetBoard_3 = new EthernetBoard(2, "192.168.1.42");
          private EthernetBoard EthernetBoard_4 = new EthernetBoard(3, "192.168.1.43");
          private EthernetBoard EthernetBoard_5 = new EthernetBoard(4, "192.168.1.44");
          private EthernetBoard EthernetBoard_6 = new EthernetBoard(5, "192.168.1.45");
          private EthernetBoard EthernetBoard_7 = new EthernetBoard(6, "192.168.1.46");
          private EthernetBoard EthernetBoard_8 = new EthernetBoard(7, "192.168.1.47");

          private List<EthernetBoard> ActiveBoardsList = new List<EthernetBoard>();
          private BlockingCollection<EthernetBoard> bcBoards = new BlockingCollection<EthernetBoard>();

          // ********************* to here is temporary

          // Configuration Declarations
          private Dictionary<string, List<int>> valuelist = new Dictionary<string, List<int>>();

          public MainForm()
          {
               valuelist = new Dictionary<string, List<int>>();

               InitializeComponent();

               MainCancelToken = MainCancelTokenSource.Token;

               /******************************************************/
               // This is to handle any unhandled errors that occur in an async method. It will get fired when the 
               // task thread is garbage collected. Make sure to eventually LOG these somewhere
               TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>
                   (TaskTools.TaskScheduler_UnobservedTaskException);

               //onEthernetBoardError_Async
               EthernetBoard_1.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_2.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_3.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_4.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_5.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_6.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_7.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_8.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);

               EthernetBoard_1.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_2.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_3.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_4.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_5.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_6.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_7.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               EthernetBoard_8.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);

               bcBoards.Add(EthernetBoard_1);
               bcBoards.Add(EthernetBoard_2);
               bcBoards.Add(EthernetBoard_3);
               bcBoards.Add(EthernetBoard_4);
               bcBoards.Add(EthernetBoard_5);
               bcBoards.Add(EthernetBoard_6);
               bcBoards.Add(EthernetBoard_7);
               bcBoards.Add(EthernetBoard_8);

               //Task.Run(() => InitBoardsChannelsPorts());
          }

          #region ThreadingStuff

          #endregion

          private void btnDiscover_Click(object sender, EventArgs e)
          {
               IPEndPoint targetEP;
               Discoverer discoverer = new Discoverer();

               discoverer.AgentFound += DiscovererAgentFound;

               if (chkSpecificIP.Checked)
                    targetEP = new IPEndPoint(IPAddress.Parse(txtSpecificIP.Text), 161);
               else
                    targetEP = new IPEndPoint(IPAddress.Broadcast, 161);

               Console.WriteLine("v1 discovery...");
               discoverer.Discover(VersionCode.V1, targetEP,
                   new OctetString("private"), 6000);
               Console.WriteLine("v2 discovery...");
               discoverer.Discover(VersionCode.V2, targetEP,
                   new OctetString("private"), 6000);
          }

          private void chkSpecificIP_CheckedChanged(object sender, EventArgs e)
          {
               txtSpecificIP.Visible = chkSpecificIP.Checked;
          }

          private async Task InitBoardsChannelsPorts()
          {
               StringBuilder sb = new StringBuilder();
               await bcBoards.ElementAt(0).init("1st board .40", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(1).init("2nd board .41", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(2).init("3rd board .42", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(3).init("4th board .43", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(4).init("5th board .44", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(5).init("6th board .45", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(6).init("7th board .46", 161, "private", 50, 100, false, false);
               await bcBoards.ElementAt(7).init("8th board .47", 161, "private", 50, 100, false, false);

               //await Task.Run(() => EthernetBoard_8.init("8th board .47", 161, "private", 50, 100, false, false))
          }

          private void StartBoardListeners()
          {
               Parallel.ForEach(bcBoards, Brd =>
               {
                    if (!(Brd.StartTimers()))
                    {
                         Debug.Print("Timer NOT Started: " + Brd.BoardName);
                    }
               });

               SetOnOffLabel(true);
          }

          //********************* Random Functions **********************
          private void SetOnOffLabel(bool boardsAreOn)
          {
               if (boardsAreOn)
               {
                    lblOnOff.Text = "ON";
                    lblOnOff.BackColor = System.Drawing.Color.LightGreen;
               }
               else
               {
                    lblOnOff.Text = "OFF";
                    lblOnOff.BackColor = System.Drawing.Color.Red;
               }
          }

          // ************************* EVENT HANDLERS *****************************
          private void DiscovererAgentFound(object sender, AgentFoundEventArgs e)
          {
               string msg = e.Agent + "has discovered " + e.Variable.Data.ToString();
               Debug.Print(msg);
          }

          private void onEthernetBoardError(object sender, ErrorEventArgs e)
          {
               if (!(e.Exception is Lextm.SharpSnmpLib.Messaging.TimeoutException) &&
                   !(e.Exception is SocketException))
                    Debug.Print(e.ToString());
          }

          private void onEthernetBoardError_Async(object sender, AsyncErrorEventArgs e)
          {
               Debug.Print(e.ToString());
          }

          private void MainForm_Load(object sender, EventArgs e)
          {
               MainBoardsController = new EthernetBoardController();
          }

          private async void btnRunInLoop_Click(object sender, EventArgs e)
          {
               await Task.WhenAll();

               string getResults = null;
               int oldVal = -1;

               try
               {
                    while (true)
                    {
                         MainCancelToken.ThrowIfCancellationRequested();

                         foreach (EthernetBoard bd in bcBoards)
                         {
                              MainCancelToken.ThrowIfCancellationRequested();

                              foreach (EthernetBoardPort p in bd.DigitalPortsList)
                              {
                                   MainCancelToken.ThrowIfCancellationRequested();

                                   foreach (Channel ch in p.ChannelsList)
                                   {
                                        oldVal = ch.Value;
                                        getResults = null;

                                        MainCancelToken.ThrowIfCancellationRequested();

                                        getResults = await Task.Run(() =>
                                            SnmpTools.SnmpGet_Async(ch.OID, bd.IP_Address, MainCancelToken));

                                        if (getResults != null && getResults != oldVal.ToString())
                                             Debug.Print("Results Channel " + ch.Id + " New Val: " + getResults);
                                        // async write value here
                                        else
                                             Debug.Print("Results Channel " + ch.Id + ": " + getResults);
                                   }
                              }
                         }
                    }
               }
               catch (AggregateException ex)
               {
                    onEthernetBoardError_Async(this, new AsyncErrorEventArgs(ex));
               }
               catch (Exception ez)
               {
                    onEthernetBoardError(this, new ErrorEventArgs(ez));
               }

          }

          private void WriteNewToOutputWindow(string ln)
          {
               txtOutput.Text = string.Empty;
               txtOutput.Text += ln + Environment.NewLine;
          }

          private void AppendToOutputWindow(string ln)
          {
               txtOutput.Text += ln + Environment.NewLine;
          }

          private void ClearOutputWindow()
          {
               txtOutput.Text = string.Empty;
          }

          private EthernetBoard GetMeABoard(int _boardId, string _ip)
          {
               return new EthernetBoard(_boardId, _ip);
          }

          #region Form Events

          private async void btnTPLGetSet_Click(object sender, EventArgs e)
          {
               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               if (await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true)))
               {
                    sw.Restart();



                    sw.Stop();
                    AppendToOutputWindow("TPLGet_2 DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
                    Debug.Print("TPLGet_2 DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
               }

          }

          private void btnBoardAsTask_Click(object sender, EventArgs e)
          {
               WriteNewToOutputWindow("Attempting to create EthernetBoard in its own thread.");
               Task<EthernetBoard> tskBoard1, tskBoard2, tskBoard3, tskBoard4;

               List<Task<EthernetBoard>> listOfTasks = new List<Task<EthernetBoard>>();
               Task<EthernetBoard>[] ebArry;

               try
               {
                    tskBoard1 = new Task<EthernetBoard>(() => GetMeABoard(0, "192.168.1.40"), CancellationToken.None, TaskCreationOptions.LongRunning);
                    tskBoard2 = new Task<EthernetBoard>(() => GetMeABoard(1, "192.168.1.41"), CancellationToken.None, TaskCreationOptions.LongRunning);
                    tskBoard3 = new Task<EthernetBoard>(() => GetMeABoard(2, "192.168.1.42"), CancellationToken.None, TaskCreationOptions.LongRunning);
                    tskBoard4 = new Task<EthernetBoard>(() => GetMeABoard(3, "192.168.1.43"), CancellationToken.None, TaskCreationOptions.LongRunning);

                    listOfTasks.Add(tskBoard1);
                    listOfTasks.Add(tskBoard2);
                    listOfTasks.Add(tskBoard3);
                    listOfTasks.Add(tskBoard4);

                    ebArry = listOfTasks.ToArray();//<EthernetBoard>;
                    Parallel.ForEach(ebArry, t =>
                    {
                         t.Start();
                    });
               }
               catch
               {
                    AppendToOutputWindow("---------- FAIL GODDAMNIT!!!!! ------------");
               }
          }

          private async void btnLightsOn_Async_Click(object sender, EventArgs e)
          {
               StringBuilder sb = new StringBuilder();
               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);
               EthernetBoard brd2 = new EthernetBoard(1, "192.168.1.41");
               string res;

               if (await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true)))
               {
                    sw3.Restart();
                    res = await SnmpTools.SnmpSetAll_ParallelAsync(1, "192.168.1.40", brd.DigitalPortsList[0], MainCancelToken);
                    sw3.Stop();
                    WriteNewToOutputWindow(res);
                    AppendToOutputWindow("Lights On DONE ----- MilliSeconds: " + sw3.ElapsedMilliseconds);
               }

               if (await Task.Run(() => brd2.init("Board 1", 161, "private", 50, 100, true, true)))
               {
                    sw5.Restart();
                    res = await SnmpTools.SnmpSetAll_Async(1, MainCancelToken);
                    sw5.Stop();
                    WriteNewToOutputWindow(res);
                    AppendToOutputWindow("Lights On DONE ----- MilliSeconds: " + sw5.ElapsedMilliseconds);
               }
          }

          private async void btnLightsOff_Async_Click(object sender, EventArgs e)
          {
               StringBuilder sb = new StringBuilder();
               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);
               EthernetBoard brd2 = new EthernetBoard(1, "192.168.1.41");
               string res;

               if (await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true)))
               {
                    sw4.Restart();
                    res = await SnmpTools.SnmpSetAll_ParallelAsync(0, "192.168.1.40", brd.DigitalPortsList[0], MainCancelToken);
                    sw4.Stop();
                    WriteNewToOutputWindow(res);
                    AppendToOutputWindow("Lights OFF DONE ----- MilliSeconds: " + sw4.ElapsedMilliseconds);
               }

               if (await Task.Run(() => brd2.init("Board 1", 161, "private", 50, 100, true, true)))
               {
                    sw7.Restart();
                    res = await SnmpTools.SnmpSetAll_Async(0, MainCancelToken);
                    sw7.Stop();
                    WriteNewToOutputWindow(res);
                    AppendToOutputWindow("Lights OFF DONE ----- MilliSeconds: " + sw7.ElapsedMilliseconds);
               }
          }

          private void btnClearText_Click(object sender, EventArgs e)
          {
               txtOutput.Text = String.Empty;
          }

          private void mnuFile_ButtonsForm_Click(object sender, EventArgs e)
          {
               frmPushButtons frm = new frmPushButtons();
               frm.Show();
          }

          private void btnEthernetBoard_1_Threads_Click(object sender, EventArgs e)
          {
               EthernetBoard Board1 = new EthernetBoard(0, txtToIP.Text);
               Task.Run(() => Board1.init("Board 0", 161, "private", 50, 100, true, true));
          }   

          private async void btnLoad_Click(object sender, EventArgs e)
          {
               // turn on all boards
               sw.Restart();

               try
               {
                    await InitBoardsChannelsPorts();
                    StartBoardListeners();
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(sender, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException btnLoad_Click");
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(sender, new ErrorEventArgs(ex));
                    Debug.Print("Exception btnLoad_Click");
               }

               sw.Stop();
               AppendToOutputWindow("ALL BOARDS ON DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }
          #endregion


          private async void btnInitTpl_Click(object sender, EventArgs e)
          {
               string windowText = await TestInits();
               AppendToOutputWindow(windowText);
          }

          private async Task<string> TestInits()
          {
               StringBuilder sb = new StringBuilder();

               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true));

               try
               {
                    sw.Restart();
                    await Task.Run(() => brd.GetInitialValues_Async(MainCancelToken)).ConfigureAwait(false);
                    sw.Stop();
                    sb.AppendLine("GetInitialValues_Async DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);

                    sw3.Restart();
                    await Task.Run(() => brd.GetInitialValues_Parallel(MainCancelToken)).ConfigureAwait(false);
                    sw3.Stop();
                    sb.AppendLine("GetInitialValues_Parallel DONE ----- MilliSeconds: " + sw3.ElapsedMilliseconds);
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(this, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException btnInitTpl_Click");
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(this, new ErrorEventArgs(ex));
                    Debug.Print("Exception btnInitTpl_Click");
               }

               return sb.ToString();
          }

          private async void btnInitGetSet_Click(object sender, EventArgs e)
          {
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, _ip);

               await Task.Run(() => brd.init("Board 40", 161, "private", 50, 100, true, false));

               sw.Restart();

               try
               {
                    await Task.Run(() => brd.InitGetValues_AsyncRetry(MainCancelToken));
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(sender, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException btnInitGetSet_Click");
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(sender, new ErrorEventArgs(ex));
                    Debug.Print("Exception btnInitGetSet_Click");
               }

               sw.Stop();
               AppendToOutputWindow("InitGetValues_AsyncRetry ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }

          private async void btnCompareGets_Click(object sender, EventArgs e)
          {
               string windowText = await CompareGets();
               AppendToOutputWindow(windowText);
          }

          private async Task<string> CompareGets()
          {

               StringBuilder sb = new StringBuilder();

               IEnumerable<Channel> DigLst1 = new List<Channel>();
               IEnumerable<Channel> DigLst2 = new List<Channel>();
               IEnumerable<Channel> AlgLst1 = new List<Channel>();

               string[] arry1;
               string[] arry2;
               string[] arry3;
               string res = null;
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true));

               try
               {

                    sb.AppendLine("START *************GetVals_Async ***************");

                    sw.Restart();
                    arry1 = await Task.Run(() => SnmpTools.GetVals_Async(brd.DigitalPortsList[0], _ip, MainCancelToken));

                    foreach (string str in arry1)
                    {
                         sb.AppendLine("Digi Port1 1 Value: " + str);
                    }

                    arry2 = await Task.Run(() => SnmpTools.GetVals_Async(brd.DigitalPortsList[1], _ip, MainCancelToken));

                    foreach (string str in arry2)
                    {
                         sb.AppendLine("Digi Port1 2 Value: " + str);
                    }
                    arry3 = await Task.Run(() => SnmpTools.GetVals_Async(brd.AnalogPortsList[0], _ip, MainCancelToken));

                    foreach (string str in arry3)
                    {
                         sb.AppendLine("Analog Port1 1 Value: " + str);
                    }

                    sw.Stop();
                    sb.AppendLine("*************GetVals_Async ----- MilliSeconds: " + sw.ElapsedMilliseconds);


                    sb.AppendLine("START *************InitGetValues_AsyncRetry ***************");
                    sw3.Restart();
                    await Task.Run(() => brd.InitGetValues_AsyncRetry(MainCancelToken));
                    sw3.Stop();
                    sb.AppendLine("************************InitGetValues_AsyncRetry ----- MilliSeconds: " + sw3.ElapsedMilliseconds);


                    sb.AppendLine("START *************GetInitialValues_Async ***************");
                    sw4.Restart();
                    await Task.Run(() => brd.GetInitialValues_Async(MainCancelToken));
                    sw4.Stop();
                    sb.AppendLine("***********************GetInitialValues_Async ----- MilliSeconds: " + sw4.ElapsedMilliseconds);


                    sb.AppendLine("START *************AllChannelsInParallelNonBlockingAsync ***************");
                    sw5.Restart();

                    DigLst1 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.DigitalPortsList[0], MainCancelToken));
                    foreach (Channel ch in DigLst1)
                         sb.AppendLine("Digital List1 : Value " + ch.Value);

                    DigLst2 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.DigitalPortsList[1], MainCancelToken));

                    foreach (Channel ch in DigLst2)
                         sb.AppendLine("Digital List2 : Value " + ch.Value);

                    AlgLst1 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.AnalogPortsList[0], MainCancelToken));

                    foreach (Channel ch in AlgLst1)
                         sb.AppendLine("Analog List1 : Value " + ch.Value);

                    sw5.Stop();

                    sb.AppendLine("************************AllChannelsInParallelNonBlockingAsync ----- MilliSeconds: " + sw5.ElapsedMilliseconds);

                    sw6.Restart();
                    string eres = await Task.Run(() => brd.GetInitialValues_Parallel(MainCancelToken));
                    sb.AppendLine(eres);
                    sw6.Stop();
                    sb.AppendLine("*****************************GetInitialValues_Parallel ----- MilliSeconds: " + sw6.ElapsedMilliseconds);

                    sb.AppendLine("START*************************** SnmpGet_AsyncCallback -------------------");
                    sw7.Restart();

                    DigLst1 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.DigitalPortsList[0], MainCancelToken));
                    foreach (Channel ch in DigLst1)
                    {
                         res = await SnmpTools.SnmpGet_AsyncCallback(ch, _ip, MainCancelToken);
                         sb.AppendLine("Digital List1 : " + res);
                    }

                    DigLst2 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.DigitalPortsList[1], MainCancelToken));

                    foreach (Channel ch in DigLst2)
                    {
                         res = await SnmpTools.SnmpGet_AsyncCallback(ch, _ip, MainCancelToken);
                         sb.AppendLine("Digital List2 : " + res);
                    }

                    AlgLst1 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.AnalogPortsList[0], MainCancelToken));

                    foreach (Channel ch in AlgLst1)
                    {
                         res = await SnmpTools.SnmpGet_AsyncCallback(ch, _ip, MainCancelToken);
                         sb.AppendLine("Analog List1 : Value " + ch.Value);
                    }

                    sw7.Stop();
                    sb.AppendLine("SnmpGet_AsyncCallback DONE ----- MilliSeconds: " + sw7.ElapsedMilliseconds);
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(this, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException " + ae.Flatten().ToString());
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(this, new ErrorEventArgs(ex));
                    Debug.Print("Exception " + ex.ToString());
               }

               return sb.ToString();
          }

          private void btnCancelAllTasks_Click(object sender, EventArgs e)
          {
               MainCancelTokenSource.Cancel();
          }

          private async void btnDoGetAll_Click(object sender, EventArgs e)
          {
               await DoGetAll();
          }


          private async Task DoGetAll()
          {
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true));

               sw.Restart();

               IList<Variable> l1 = await SnmpTools.SnmpGetAll(EthernetBoardPortNo.DIGITAL_PORT_1, _ip);
               Debug.Print("1. Get All Results: " + l1.Select(c => c.Data.ToString()));

               IList<Variable> l2 = await SnmpTools.SnmpGetAll(EthernetBoardPortNo.DIGITAL_PORT_2, _ip);
               Debug.Print("2. Get All Results: " + l2.Select(c => c.Data.ToString()));

               IList<Variable> l3 = await SnmpTools.SnmpGetAll(EthernetBoardPortNo.ADC_PORT_1, _ip);
               Debug.Print("3. Get All Results: " + l3.Select(c => c.Data.ToString()));

               sw.Stop();

               AppendToOutputWindow("*****************************SnmpGetAll ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }
          private void btnDoSetAll_Click(object sender, EventArgs e)
          {
               DoSetAll();
          }

          private async Task DoSetAll()
          {
               int ON = 1;
               int OFF = 0;
               string pIp = "192.168.1.40";
               IList<Variable> lstRes = null;

               try
               {
                    lstRes = await Task.Run(() =>
                            SnmpTools.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_1, ON, pIp, MainCancelToken));
                    Debug.Print("Set All Results: " + lstRes.Select(c => c.Data.ToString()));
                    lstRes = await Task.Run(() =>
                            SnmpTools.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_2, ON, pIp, MainCancelToken));
                    Debug.Print("Results: " + lstRes.Select(c => c.Data.ToString()));
                    lstRes = await Task.Run(() =>
                            SnmpTools.SnmpSetAll(EthernetBoardPortNo.ADC_PORT_1, ON, pIp, MainCancelToken));
                    Debug.Print("Results: " + lstRes.Select(c => c.Data.ToString()));

                    //lstRes = await Task.Run(() =>
                    //        EthernetBoard_2.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_1,
                    //        ON, MainCancelToken));
                    //Debug.Print("Results: " + lstRes.Select(c => c.Data.ToString()));
                    //lstRes = await Task.Run(() =>
                    //        EthernetBoard_3.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_1,
                    //        ON, MainCancelToken));
                    //Debug.Print("Results: " + lstRes.Select(c => c.Data.ToString()));
                    //lstRes = await Task.Run(() =>
                    //        EthernetBoard_4.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_1,
                    //        ON, MainCancelToken));
                    //Debug.Print("Results: " + lstRes.Select(c => c.Data.ToString()));
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(this, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException" + ae.Flatten().Message);
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(this, new ErrorEventArgs(ex));
                    Debug.Print("Exception " + ex.Message);
               }

          }
          private async void btnCompareSets_Click(object sender, EventArgs e)
          {
               string windowText = await CompareSets();
               AppendToOutputWindow(windowText);
          }

          private async Task<string> CompareSets()
          {
               StringBuilder sb = new StringBuilder();

               IEnumerable<Channel> DigLst1 = new List<Channel>();
               IEnumerable<Channel> DigLst2 = new List<Channel>();
               IEnumerable<Channel> AlgLst1 = new List<Channel>();
               List<string> getRes1 = new List<string>();
               int newVal;
               IList<Variable> lstV;

               string st = null;
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => brd.init("Board 0", 161, "private", 50, 100, true, true));

               try

               {
                    sb.AppendLine("*********SnmpSet_Async ********************************");

                    sw.Restart();
                    foreach (Channel ch in brd.DigitalPortsList[0].ChannelsList)
                    {
                         newVal = (ch.Value == 0 ? 1 : 0);
                         st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                         sb.AppendLine(st);
                    }

                    foreach (Channel ch in brd.DigitalPortsList[1].ChannelsList)
                    {
                         newVal = (ch.Value == 0 ? 1 : 0);
                         st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                         sb.AppendLine(st);
                    }

                    foreach (Channel ch in brd.AnalogPortsList[0].ChannelsList)
                    {
                         newVal = (ch.Value == 0 ? 1 : 0);
                         st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                         sb.AppendLine(st);
                    }
                    sw.Stop();
                    sb.AppendLine("*********SnmpSet_Async ----- MilliSeconds: " + sw.ElapsedMilliseconds);

                    sb.AppendLine("START *********DoSomeSets_ParallelAsync ********************************");
                    sw2.Restart();

                    st = await SnmpTools.SnmpSetAll_ParallelAsync(1, "192.168.1.40", brd.DigitalPortsList[0], MainCancelToken);
                    sw2.Stop();
                    sb.AppendLine(st);
                    sb.AppendLine("*********DoSomeSets_ParallelAsync ----- MilliSeconds: " + sw2.ElapsedMilliseconds);

                    sb.AppendLine("START *********SnmpSet ********************************");
                    sw3.Restart();

                    foreach (Channel ch in brd.DigitalPortsList[0].ChannelsList)
                    {
                         newVal = (ch.Value == 0 ? 1 : 0);
                         st = SnmpTools.SnmpSet(ch.OID, _ip, newVal, MainCancelToken);
                         sb.AppendLine(st);
                    }

                    foreach (Channel ch in brd.DigitalPortsList[1].ChannelsList)
                    {
                         newVal = (ch.Value == 0 ? 1 : 0);
                         st = SnmpTools.SnmpSet(ch.OID, _ip, newVal, MainCancelToken);
                         sb.AppendLine(st);
                    }

                    foreach (Channel ch in brd.AnalogPortsList[0].ChannelsList)
                    {
                         newVal = (ch.Value == 0 ? 1 : 0);
                         st = SnmpTools.SnmpSet(ch.OID, _ip, newVal, MainCancelToken);
                         sb.AppendLine(st);
                    }

                    sw3.Stop();
                    sb.AppendLine(st);
                    sb.AppendLine("*********SnmpSet ----- MilliSeconds: " + sw3.ElapsedMilliseconds);

                    sb.AppendLine("START *********SnmpSetAll ********************************");
                    sw4.Restart();
                    newVal = 1;
                    lstV = await SnmpTools.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_1, newVal, _ip, MainCancelToken);
                    foreach (Variable v in lstV)
                    {
                         sb.AppendLine("BOARD 1 - ID: " + v.Id + " Value : " + v.Data);
                    }

                    lstV = await SnmpTools.SnmpSetAll(EthernetBoardPortNo.DIGITAL_PORT_2, newVal, _ip, MainCancelToken);
                    foreach (Variable v in lstV)
                    {
                         sb.AppendLine("BOARD 2 - ID: " + v.Id + " Value : " + v.Data);
                    }

                    lstV = await SnmpTools.SnmpSetAll(EthernetBoardPortNo.ADC_PORT_1, newVal, _ip, MainCancelToken);
                    foreach (Variable v in lstV)
                    {
                         sb.AppendLine("BOARD ANALOG 1 - ID: " + v.Id + " Value : " + v.Data);
                    }
                    sw4.Stop();
                    sb.AppendLine("*********SnmpSetAll ----- MilliSeconds: " + sw4.ElapsedMilliseconds);
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(this, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException " + ae.Flatten().ToString());
               }
               catch (Exception ex)
               {
                    if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                        && !(ex is SocketException)
                        && !(ex is OperationCanceledException)
                        && !(ex is TaskCanceledException))
                    {
                         onEthernetBoardError(this, new ErrorEventArgs(ex));
                         Debug.Print("Exception " + ex.ToString());
                    }
               }

               return sb.ToString();
          }

          private void toolStripMenuItem1_Click(object sender, EventArgs e)
          {
               PowerSnmpAgent frm = new PowerSnmpAgent();
               frm.Show();
          }
     }
}
