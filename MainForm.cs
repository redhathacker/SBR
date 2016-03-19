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

using Dart.Snmp;

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
          private CancellationTokenSource MainCancelTokenSource = null;
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

          // ********************* to here is temporary

          // Configuration Declarations
          private Dictionary<string, List<int>> valuelist = new Dictionary<string, List<int>>();

          public MainForm()
          {
               valuelist = new Dictionary<string, List<int>>();

               InitializeComponent();

               /******************************************************/
               // This is to handle any unhandled errors that occur in an async method. It will get fired when the 
               // task thread is garbage collected. Make sure to eventually LOG these somewhere
               TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>
                    (TaskTools.TaskScheduler_UnobservedTaskException);
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

          private async Task<string> InitBoardsChannelsPorts()
          {
               StringBuilder sb = new StringBuilder();

               //_ActiveBoards.Add(MainBoardsController.GetBoard(0));

               ActiveBoardsList.Add(EthernetBoard_1);
               ActiveBoardsList.Add(EthernetBoard_2);
               ActiveBoardsList.Add(EthernetBoard_3);
               ActiveBoardsList.Add(EthernetBoard_4);
               ActiveBoardsList.Add(EthernetBoard_5);
               ActiveBoardsList.Add(EthernetBoard_6);
               ActiveBoardsList.Add(EthernetBoard_7);
               ActiveBoardsList.Add(EthernetBoard_8);

               //onEthernetBoardError_Async

               EthernetBoard_1.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_2.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_3.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_4.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_5.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_6.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_7.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);
               EthernetBoard_8.onAsyncError += new EthernetBoard.AsyncErrorHandlerDelegate(onEthernetBoardError_Async);

               //EthernetBoard_1.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_2.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_3.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_4.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_5.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_6.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_7.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);
               //EthernetBoard_8.onError += new EthernetBoard.ErrorHandlerDelegate(onEthernetBoardError);

               //Task Tsk1 = Task.Run(() => EthernetBoard_1.init(0, "1st board .40", "192.168.1.40", 161, "private", 50, 100, false, false));
               //Task Tsk2 = Task.Run(() => EthernetBoard_2.init(1, "2nd board .41", "192.168.1.41", 161, "private", 50, 100, false, false));
               //Task Tsk3 = Task.Run(() => EthernetBoard_3.init(2, "3rd board .42", "192.168.1.42", 161, "private", 50, 100, false, false));
               //Task Tsk4 = Task.Run(() => EthernetBoard_4.init(3, "4th board .43", "192.168.1.43", 161, "private", 50, 100, false, false));
               //Task Tsk5 = Task.Run(() => EthernetBoard_5.init(4, "5th board .44", "192.168.1.44", 161, "private", 50, 100, false, false));
               //Task Tsk6 = Task.Run(() => EthernetBoard_6.init(5, "6th board .45", "192.168.1.45", 161, "private", 50, 100, false, false));
               //Task Tsk7 = Task.Run(() => EthernetBoard_7.init(6, "7th board .46", "192.168.1.46", 161, "private", 50, 100, false, false));
               //Task Tsk8 = Task.Run(() => EthernetBoard_8.init(7, "8th board .47", "192.168.1.47", 161, "private", 50, 100, false, false));

               await Task.Run(() => EthernetBoard_8.init(7, "8th board .47", "192.168.1.47", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_1.init(0, "1st board .40", "192.168.1.40", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_2.init(1, "2nd board .41", "192.168.1.41", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_3.init(2, "3rd board .42", "192.168.1.42", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_4.init(3, "4th board .43", "192.168.1.43", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_5.init(4, "5th board .44", "192.168.1.44", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_6.init(5, "6th board .45", "192.168.1.45", 161, "private", 50, 100, false, false), MainCancelToken);
               await Task.Run(() => EthernetBoard_7.init(6, "7th board .46", "192.168.1.46", 161, "private", 50, 100, false, false), MainCancelToken);

               //List<Task> lstTasks = new List<Task>() { { Tsk1 }, { Tsk2 }, { Tsk3 }, { Tsk4 }, { Tsk5 }, { Tsk6 }, { Tsk7 }, { Tsk8 } };

               //try
               //{
               //     Task.WaitAll(lstTasks.ToArray());
               //}
               //catch (Exception ex)
               //{
               //     onEthernetBoardError(this, new ErrorEventArgs(ex));
               //}

               Parallel.ForEach(ActiveBoardsList, Brd =>
               {
                    if (Brd.StartTimers())
                         sb.AppendLine("Timer Started: " + Brd.BoardName);
               });

               SetOnOffLabel(true);

               return sb.ToString();
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
               string getResults = null;
               string oldVal = null;

               EthernetBoard Board1 = new EthernetBoard(0, txtToIP.Text);
               Board1.onError += onEthernetBoardError;
               Board1.onAsyncError += onEthernetBoardError_Async;

               if (await Task.Run(() => Board1.init(0, "Board 0", "192.168.1.40", 161, "private", 50, 100, true, false)))
               {
                    while (true)
                    {
                         foreach (EthernetBoardPort p in Board1.DigitalPortsList)
                         {
                              foreach (Channel ch in p.ChannelsList)
                              {
                                   oldVal = ch.Value.ToString();
                                   getResults = null;

                                   try
                                   {
                                        getResults = await Task.Run(() => Board1.SnmpGet_Async(ch.OID, MainCancelToken));

                                        if (getResults != null && oldVal == getResults)
                                             txtOutput.Text += "Results Channel" + ch.Id + " New Val: " + getResults + Environment.NewLine;
                                        else
                                             txtOutput.Text += "Results Channel" + ch.Id + ": " + getResults + Environment.NewLine;
                                   }
                                   catch (AggregateException ex)
                                   {
                                        txtOutput.Text += "TIMEOUT " + Environment.NewLine;
                                   }
                              }
                         }
                    }
               }
               // now I need the board to start scanning 161 forever
               // Open socket, Listen for 2 seconds, return value or null, 
               // if value updated then update channel. Send new value to the hardware.
               // Listen again.
               // else
               //Value null just rerun listen
          }

          private async Task<string> DoSomeSets_Async(int pNewValue, CancellationToken ct)
          {
               int _port = 161;
               string sIp = txtToIP.Text;
               string res = null;

               object LockHelper = new object();

               List<Task<Task<string>>> taskList = new List<Task<Task<string>>>();
               StringBuilder sb = new StringBuilder();
               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               if (await Task.Run(() => brd.init(0, "Board 0", "192.168.1.40", _port, "private", 50, 100, true, true)))
               {
                    sw.Restart();

                    lock (LockHelper)
                    {
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[0].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[1].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[2].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[3].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[4].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[5].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[6].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[7].OID, pNewValue, ct), ct));

                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[0].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[1].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[2].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[3].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[4].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[5].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[6].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[7].OID, pNewValue, ct), ct));

                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[0].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[1].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[2].OID, pNewValue, ct), ct));
                         taskList.Add(Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[3].OID, pNewValue, ct), ct));

                         sb.AppendLine("----------------- DoSomeSets_Async ---------------------");

                         if (pNewValue == 1)
                              sb.AppendLine("--------------All Boards ON --------------");
                         else
                              sb.AppendLine("---------------All Boards OFF --------------");

                         Task tsk = Task.WhenAll(taskList.ToArray());
                    } // end lock
                    foreach (Task<Task<string>> ts in taskList)
                    {
                         if (ts.IsFaulted)
                         {
                              sb.AppendLine("Task Id: " + ts.Id + " TASK IS FAULTED");
                         }
                         else if (ts.IsCompleted)
                         {
                              sb.AppendLine("Task Id: " + ts.Id + " Task complete - Result: " + res + " Status: " + ts.Status);
                         }
                         else
                         {
                              sb.AppendLine("Task Id: " + ts.Id + " Task ??? - Result: " + res + " Status: " + ts.Status);
                         }
                    }
               }
               sw.Stop();
               Debug.Print("Do Some Sets Parallel DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
               sb.AppendLine("Do Some Sets Parallel DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
               return sb.ToString();
          }


          private async Task<string> DoSomeSets_ParallelAsync(int pNewValue, CancellationToken ct)
          {
               int _port = 161;
               string sIp = txtToIP.Text;
               List<Task<string>> taskList = new List<Task<string>>();
               //List<Task> taskList = new List<Task>();
               StringBuilder sb = new StringBuilder();
               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               if (await Task.Run(() => brd.init(0, "Board 0", "192.168.1.40", _port, "private", 50, 100, true, true)))
               {

                    sw.Restart();

                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[0].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[1].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[2].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[3].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[4].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[5].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[6].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[7].OID, pNewValue, ct), ct));

                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[0].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[1].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[2].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[3].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[4].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[5].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[6].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[7].OID, pNewValue, ct), ct));

                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[0].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[1].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[2].OID, pNewValue, ct), ct));
                    taskList.Add(await Task.Factory.StartNew(() => brd.SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[3].OID, pNewValue, ct), ct));

                    sb.AppendLine("----------------- DoSomeSets_Async ---------------------");

                    if (pNewValue == 1)
                         sb.AppendLine("--------------All Boards ON --------------");
                    else
                         sb.AppendLine("---------------All Boards OFF --------------");

                    Parallel.ForEach(taskList, async t =>
                    {
                         await t;

                         if (t.IsFaulted)
                         {
                              sb.AppendLine("Task Id: " + t.Id + " TASK IS FAULTED");
                         }
                         else if (t.IsCompleted)
                         {
                              sb.AppendLine("Task Id: " + t.Id + " Task complete - Result: " + t.Result + " Status: " + t.Status);
                         }
                         else
                         {
                              sb.AppendLine("Task Id: " + t.Id + " Task ??? - Result: " + t.Result + " Status: " + t.Status);
                         }
                    });

                    //foreach (Task t in taskList)
                    //{
                    //    if (t.IsCompleted)
                    //        sb.AppendLine("Result: " + t.ToString() + " Status: " + t.Status);
                    //}
               }

               sw.Stop();
               Debug.Print("Do Some Sets Parallel DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds + Environment.NewLine);
               sb.AppendLine("Do Some Sets Parallel DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
               return sb.ToString();
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

          private async void btnGetTpl2_Click(object sender, EventArgs e)
          {
               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               if (await Task.Run(() => brd.init(0, "Board 0", "192.168.1.40", 161, "private", 50, 100, true, true)))
               {
                    sw.Restart();
                    string res = brd.TPLGet_2(MainCancelToken);

                    WriteNewToOutputWindow(res);
                    AppendToOutputWindow("----------------------- Channel Values -------------------------");

                    foreach (EthernetBoardPort p in brd.DigitalPortsList)
                    {
                         AppendToOutputWindow("---------------- PORT " + p.IoPortNo + " ---------------------------");

                         foreach (Channel ch in p.ChannelsList)
                         {
                              AppendToOutputWindow("channel: " + ch.OID + " has a value of: " + ch.Value);
                         }
                    }
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
               string res;
               sw.Restart();
               res = await DoSomeSets_ParallelAsync(1, MainCancelToken);
               sw.Stop();
               WriteNewToOutputWindow(res);
               AppendToOutputWindow("Lights On DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }

          private async void btnLightsOff_Async_Click(object sender, EventArgs e)
          {
               string res;
               sw.Restart();
               res = await DoSomeSets_ParallelAsync(0, MainCancelToken);
               sw.Stop();
               WriteNewToOutputWindow(res);
               AppendToOutputWindow("Lights Off DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
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
               Task.Run(() => Board1.init(0, "Board 0", "192.168.1.40", 161, "private", 50, 100, true, true));
          }

          private async void btnEthernetBoard_1_TPL_Click(object sender, EventArgs e)
          {
               string res;
               string _ip = txtToIP.Text.Trim();

               EthernetBoard Board1_T_C = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => Board1_T_C.init(0, "Board 40", _ip, 161, "private", 50, 100, true, false));

               sw.Restart();

               res = await Board1_T_C.TPLGet_1(MainCancelToken);

               WriteNewToOutputWindow(res);
               AppendToOutputWindow("----------------------- Channel Values -------------------------");

               foreach (EthernetBoardPort p in Board1_T_C.DigitalPortsList)
               {
                    AppendToOutputWindow("---------------- Board " + p.IoPortNo + " ---------------------------");

                    foreach (Channel ch in p.ChannelsList)
                    {
                         AppendToOutputWindow("channel: " + ch.OID + " has a value of: " + ch.Value);
                    }
               }

               sw.Stop();
               AppendToOutputWindow("TPLGet_1 DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }

          private async void btnLoad_Click(object sender, EventArgs e)
          {
               sw.Restart();

               try
               {
                    string res = await InitBoardsChannelsPorts();
                    WriteNewToOutputWindow(res);
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

          private void btnGetNonAsync_Click(object sender, EventArgs e)
          {

          }

          private async void btnInitTpl_Click(object sender, EventArgs e)
          {
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => brd.init(0, "Board 40", _ip, 161, "private", 50, 100, true, false));

               sw.Restart();

               try
               {
                    await brd.GetInitialValues_Parallel(MainCancelToken);
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(sender, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException btnInitTpl_Click");
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(sender, new ErrorEventArgs(ex));
                    Debug.Print("Exception btnInitTpl_Click");
               }

               sw.Stop();
               AppendToOutputWindow("GetInitialValues_Parallel DONE ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }

          private async void btnInitGetSet_Click(object sender, EventArgs e)
          {
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, _ip);

               await Task.Run(() => brd.init(0, "Board 40", _ip, 161, "private", 50, 100, true, false));

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
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               sw.Restart();
               await Task.Run(() => brd.init(0, "Board 40", _ip, 161, "private", 50, 100, true, false));
               sw.Stop();
               AppendToOutputWindow("*****************************init ----- MilliSeconds: " + sw.ElapsedMilliseconds);


               try
               {

                    sw.Restart();
                    await Task.Run(() => brd.GetVals_Async(brd.DigitalPortsList[0], MainCancelToken));
                    await Task.Run(() => brd.GetVals_Async(brd.DigitalPortsList[1], MainCancelToken));
                    await Task.Run(() => brd.GetVals_Async(brd.AnalogPortsList[0], MainCancelToken));
                    sw.Stop();
                    AppendToOutputWindow("*****************************GetVals_Async ----- MilliSeconds: " + sw.ElapsedMilliseconds);

                    sw.Restart();
                    await Task.Run(() => brd.InitGetValues_AsyncRetry(MainCancelToken));
                    sw.Stop();
                    AppendToOutputWindow("*****************************InitGetValues_AsyncRetry ----- MilliSeconds: " + sw.ElapsedMilliseconds);

                    sw.Restart();
                    await Task.Run(() => brd.GetInitialValues_Async(MainCancelToken));
                    sw.Stop();
                    AppendToOutputWindow("*****************************GetInitialValues_Async ----- MilliSeconds: " + sw.ElapsedMilliseconds);

                    sw.Restart();

                    IEnumerable<Channel> DigLst1 = new List<Channel>();
                    IEnumerable<Channel> DigLst2 = new List<Channel>();
                    IEnumerable<Channel> AlgLst1 = new List<Channel>();

                    DigLst1 = await Task.Run(() => brd.AllChannelsInParallelNonBlockingAsync(brd.DigitalPortsList[0], MainCancelToken));
                    DigLst2 = await Task.Run(() => brd.AllChannelsInParallelNonBlockingAsync(brd.DigitalPortsList[1], MainCancelToken));
                    AlgLst1 = await Task.Run(() => brd.AllChannelsInParallelNonBlockingAsync(brd.AnalogPortsList[0], MainCancelToken));
                    sw.Stop();

                    AppendToOutputWindow("*****************************AllChannelsInParallelNonBlockingAsync ----- MilliSeconds: " + sw.ElapsedMilliseconds);

                    sw.Restart();
                    await Task.Run(() => brd.GetInitialValues_Parallel(MainCancelToken));
                    sw.Stop();
                    AppendToOutputWindow("*****************************GetInitialValues_Parallel ----- MilliSeconds: " + sw.ElapsedMilliseconds);
               }
               catch (AggregateException ae)
               {
                    onEthernetBoardError_Async(sender, new AsyncErrorEventArgs(ae));
                    Debug.Print("AggregateException " + sender.ToString());
               }
               catch (Exception ex)
               {
                    onEthernetBoardError(sender, new ErrorEventArgs(ex));
                    Debug.Print("Exception " + sender.ToString());
               }
          }

          private void btnCancelAllTasks_Click(object sender, EventArgs e)
          {

          }

          private async void btnDoGetAll_Click(object sender, EventArgs e)
          {
               string _ip = txtToIP.Text.Trim();

               EthernetBoard brd = new EthernetBoard(0, txtToIP.Text);

               await Task.Run(() => brd.init(0, "Board 40", _ip, 161, "private", 50, 100, true, false));

               sw.Restart();

               await Task.Run(() => brd.GetInitialValues_Async(MainCancelToken));

               IList<Variable> l1 = brd.SnmpGetAll(EthernetBoardPortNo.DIGITAL_PORT_1);
               IList<Variable> l2 = brd.SnmpGetAll(EthernetBoardPortNo.DIGITAL_PORT_2);
               IList<Variable> l3 = brd.SnmpGetAll(EthernetBoardPortNo.ADC_PORT_1);

               sw.Stop();
               
               foreach(Variable v in l1)
               {
                    AppendToOutputWindow("Dig 1 - Channel Result : " + v.Data.ToString());
               }

               foreach (Variable v in l2)
               {
                    AppendToOutputWindow("Dig 2 - Channel Result : " + v.Data.ToString());
               }

               foreach (Variable v in l3)
               {
                    AppendToOutputWindow("Alg 1 - Channel Result : " + v.Data.ToString());
               }

               AppendToOutputWindow("*****************************SnmpGetAll ----- MilliSeconds: " + sw.ElapsedMilliseconds);
          }

          private void btnGetTpl1_Click(object sender, EventArgs e)
          {

          }
     }
}
