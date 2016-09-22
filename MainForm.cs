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
using System.Windows;
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
        private TaskScheduler uiScheduler;
        private TaskFactory uiTaskFactory;

        public delegate void UpdateTextWindow(string sVal, DigitalInputEventArgs e);
        public event UpdateTextWindow onTextWindowUpdate;

        public frmPushButtons buttonsWindow;

        public Task tskBrd1;
        public Task tskBrd2;
        public Task tskBrd3;
        public Task tskBrd4;
        public Task tskBrd5;
        public Task tskBrd6;
        public Task tskBrd7;
        public Task tskBrd8;

        Stopwatch sw1 = new Stopwatch();
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
        private Dictionary<Variable, int> ChannelValues = new Dictionary<Variable, int>();

        // Configuration Declarations
        private ConcurrentBag<Task> valuelist = new ConcurrentBag<Task>();
        private ConcurrentBag<EthernetBoard> RelayBoards = new ConcurrentBag<EthernetBoard>();

        public MainForm()
        {
            InitializeComponent();

            MainCancelToken = MainCancelTokenSource.Token;

            /******************************************************/
            // This is to handle any unhandled errors that occur in an async method. It will get fired when the 
            // task thread is garbage collected. Make sure to eventually LOG these somewhere
            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>
                (TaskTools.TaskScheduler_UnobservedTaskException);

            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            uiTaskFactory = new TaskFactory(uiScheduler);

            buttonsWindow = new frmPushButtons();

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

            EthernetBoard_1.onInit += new EthernetBoard.InitializeHandlerDelegate(onEthernetBoardInit);
            EthernetBoard_2.onInit += new EthernetBoard.InitializeHandlerDelegate(onEthernetBoardInit);
            EthernetBoard_3.onInit += new EthernetBoard.InitializeHandlerDelegate(onEthernetBoardInit);
            EthernetBoard_4.onInit += new EthernetBoard.InitializeHandlerDelegate(onEthernetBoardInit);

            EthernetBoard_1.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_2.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_3.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_4.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_5.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_6.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_7.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);
            EthernetBoard_8.onDigitalInput += new EthernetBoard.DigitalInputHandlerDelegate(onRelayBoardDigitalInput);

            RelayBoards.Add(EthernetBoard_1);
            RelayBoards.Add(EthernetBoard_2);
            RelayBoards.Add(EthernetBoard_3);
            RelayBoards.Add(EthernetBoard_4);
            RelayBoards.Add(EthernetBoard_5);
            RelayBoards.Add(EthernetBoard_6);
            RelayBoards.Add(EthernetBoard_7);
            RelayBoards.Add(EthernetBoard_8);

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

        private void InitAllBoards()
        {
            StringBuilder sb = new StringBuilder();

            Parallel.ForEach(RelayBoards, brd =>
            {
                int indx = brd.BoardID;
                string nam = "Board ID: " + brd.BoardID;
                Task.Run(async () => await brd.Init(nam));
                Debug.Print("Board init: " + nam);
            });
        }

        private void StartBoardListeners()
        {
            tskBrd1 = Task.Factory.StartNew(() => RelayBoards.ElementAt(0).StartDigitalPolling(), 
                MainCancelToken, TaskCreationOptions.LongRunning,
                TaskScheduler.Current);

  
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

        public void onEthernetBoardInit(object sender, BoardInitEventArgs e)
        {
            lblData.Text = "INIT-ID = " + e.BoardID;
            lblData.BackColor = System.Drawing.Color.Salmon;

            foreach (Channel ch in e.DigitalPorts[0].DigitalChannels)
            {
                if (ch.State == ChannelState.ON)
                    buttonsWindow.ToggleButtonColor(ch.Id);
            }

            foreach (Channel ch in e.DigitalPorts[1].DigitalChannels)
            {
                if (ch.State == ChannelState.ON)
                    buttonsWindow.ToggleButtonColor(ch.Id);
            }            
        }

        public void onRelayBoardDigitalInput(object sender, DigitalInputEventArgs e)
        {
            AppendToOutputWindow("New Digital Value Detected! IP " + e.BoardIP + " PortNo: "
                + e.PortNo + " Channel: " + e.Chan.OID + " New Value: " + e.Chan.State);

            lblData.Text = "Digi Input!!!";
            lblData.BackColor = System.Drawing.Color.Red;

            buttonsWindow.ToggleButtonColor(e.Chan.Id);
        }

        private async void btnRunInLoop_Click(object sender, EventArgs e)
        {
            string getResults = null;
            ChannelState oldVal;

            try
            {
                while (true)
                {
                    MainCancelToken.ThrowIfCancellationRequested();

                    foreach (EthernetBoard bd in ActiveBoardsList)
                    {
                        MainCancelToken.ThrowIfCancellationRequested();

                        foreach (EthernetBoardPort p in bd.DigitalPorts)
                        {
                            MainCancelToken.ThrowIfCancellationRequested();

                            foreach (Channel ch in p.DigitalChannels)
                            {
                                oldVal = ch.State;
                                getResults = null;

                                MainCancelToken.ThrowIfCancellationRequested();

                                getResults = await Task.Run(() =>
                                    SnmpTools.SnmpGet_Async(ch.OID, bd.IP_Address, MainCancelToken));

                                if (getResults != null && getResults.Length > 0)
                                    if (getResults != oldVal.ToString())
                                        Debug.Print("Board: " + bd.BoardID + " Port: " + p.IoPortNo + " Channel " + ch.Id + " New Val: " + getResults);
                                    // async write value here
                                    else
                                        Debug.Print("Board: " + bd.BoardID + " Port: " + p.IoPortNo + " Channel " + ch.Id + ": " + getResults);
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
            uiTaskFactory.StartNew(() => txtOutput.Text += ln + Environment.NewLine);
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

        private void btnBoardAsTask_Click(object sender, EventArgs e)
        {
            WriteNewToOutputWindow("Attempting to create EthernetBoard in its own thread.");
            Task<EthernetBoard> tskBoard1, tskBoard2, tskBoard3, tskBoard4, tskBoard5,
                tskBoard6, tskBoard7, tskBoard8;

            List<Task<EthernetBoard>> listOfTasks = new List<Task<EthernetBoard>>();
            Task<EthernetBoard>[] ebArry = new Task<EthernetBoard>[8]; //[8];

            try
            {

            }
            catch
            {
                AppendToOutputWindow("---------- FAIL GODDAMNIT!!!!! ------------");
            }
        }

        private async void btnLightsOn_Click(object sender, EventArgs e)
        {
            bool weGood = false;
            bool weGoodAgain = false;

            StringBuilder sb = new StringBuilder();
            EthernetBoard brd = new EthernetBoard(0, "192.168.1.40");
            EthernetBoard brd2 = new EthernetBoard(1, "192.168.1.41");
            string res1, res2;

            weGood = await Task.Run(() => brd.Init("Test Board 1"));
            weGoodAgain = await Task.Run(() => brd2.Init("Test Board 2"));

            sw3.Restart();

            await brd.AllOn();

            //Parallel.ForEach(RelayBoards, EthBrd =>
            //{
            //    for (int ct = 0; ct < EthBrd.DigitalPorts[1].DigitalChannels.Count; ct++)
            //    {
            //        if (EthBrd.DigitalPorts[1].DigitalChannels[ct].State == ChannelState.ON)
            //        {
            //            buttonsWindow.ToggleButtonColor(brd.BoardID);
            //        }
            //    }
            //});

            sw3.Stop();

     
            AppendToOutputWindow("Lights On DONE ----- MilliSeconds: " + sw3.ElapsedMilliseconds);

            if (await Task.Run(() => brd2.Init("Board 1")))
            {
                sw5.Restart();
                res1 = await SnmpTools.SnmpSetAll_Async(brd2, 1, MainCancelToken);

                sw5.Stop();
                AppendToOutputWindow(res1);
                AppendToOutputWindow("Lights On DONE ----- MilliSeconds: " + sw5.ElapsedMilliseconds);
            }
        }

        private async void btnLightsOff_Async_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            EthernetBoard brd = new EthernetBoard(0, cmbIPs.SelectedText);
            EthernetBoard brd2 = new EthernetBoard(1, "192.168.1.41");
            string res1, res2;
            bool weGood = false;
            bool weGoodAgain = false;

            weGood = await Task.Run(() => brd.Init("Test Board 1"));
            weGoodAgain = await Task.Run(() => brd2.Init("Test Board 2"));

            sw4.Restart();
            res1 = await SnmpTools.SnmpSetAll_ParallelAsync(0, cmbIPs.SelectedText, brd.DigitalPorts[0], MainCancelToken);
            res2 = await SnmpTools.SnmpSetAll_ParallelAsync(0, cmbIPs.SelectedText, brd.DigitalPorts[1], MainCancelToken);
            sw4.Stop();

            WriteNewToOutputWindow(res1);
            AppendToOutputWindow(res2);
            AppendToOutputWindow("Lights OFF DONE ----- MilliSeconds: " + sw4.ElapsedMilliseconds);

            sw7.Restart();
            res1 = await SnmpTools.SnmpSetAll_Async(brd2, 0, MainCancelToken);
            sw7.Stop();

            AppendToOutputWindow(res1);
            AppendToOutputWindow("Lights OFF DONE ----- MilliSeconds: " + sw7.ElapsedMilliseconds);
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

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            // turn on all boards
            //sw1.Restart();
            await EthernetBoard_1.Init("TestBoard");

            try
            {
                // Inits all Boards
                InitAllBoards();

                // Starts All Boards
                //StartBoardListeners();

                Task tsk1 = Task.Factory.StartNew(() => EthernetBoard_1.StartDigitalPolling(),
                    MainCancelToken, TaskCreationOptions.LongRunning, 
                    TaskScheduler.FromCurrentSynchronizationContext());

            }
            catch (Exception ex)
            {
                onEthernetBoardError(sender, new ErrorEventArgs(ex));
                Debug.Print("Exception btnLoad_Click");
            }

            sw1.Stop();
            AppendToOutputWindow("ALL BOARDS ON DONE ----- MilliSeconds: " + sw1.ElapsedMilliseconds);
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

            string _ip = cmbIPs.SelectedText.Trim();

            EthernetBoard brd = new EthernetBoard(0, cmbIPs.SelectedText);

            await Task.Run(() => brd.Init("Board 0"));

            try
            {
                sw1.Restart();
                //await Task.Run(() => brd.GetInitialValues_Async(MainCancelToken)).ConfigureAwait(false);
                sw1.Stop();
                sb.AppendLine("GetInitialValues_Async DONE ----- MilliSeconds: " + sw1.ElapsedMilliseconds);

                sw3.Restart();
                //await Task.Run(() => brd.GetInitialValues_Parallel(MainCancelToken)).ConfigureAwait(false);
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
            string _ip = cmbIPs.SelectedText.Trim();

            EthernetBoard brd = new EthernetBoard(0, _ip);

            await Task.Run(() => brd.Init("Board 40"));

            sw1.Restart();

            try
            {
                // await Task.Run(() => SnmpTools.(MainCancelToken));
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

            sw1.Stop();
            AppendToOutputWindow("InitGetValues_AsyncRetry ----- MilliSeconds: " + sw1.ElapsedMilliseconds);
        }

        private async void btnCompareGets_Click(object sender, EventArgs e)
        {
            string windowText = await CompareGets();
            WriteNewToOutputWindow(windowText);
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

            string[] chValues1;
            string[] chValues2;
            string[] chValues3;

            string res = null;
            string _ip = cmbIPs.SelectedText.Trim();

            EthernetBoard brd = new EthernetBoard(0, cmbIPs.SelectedText);

            await Task.Run(() => brd.Init("Test Board 1"));

            try
            {

                sb.AppendLine("START *************GetVals_Async ***************");

                sw1.Restart();
                arry1 = await Task.Run(() => SnmpTools.GetVals_Async(brd.DigitalPorts[0], _ip, MainCancelToken));

                foreach (string str in arry1)
                {
                    sb.AppendLine("1. Digi Port1 Value: " + str);
                }

                arry2 = await Task.Run(() => SnmpTools.GetVals_Async(brd.DigitalPorts[1], _ip, MainCancelToken));

                foreach (string str in arry2)
                {
                    sb.AppendLine("1. Digi Port2 Value: " + str);
                }


                sw1.Stop();
                sb.AppendLine("*************GetVals_Async ----- MilliSeconds: " + sw1.ElapsedMilliseconds);


                sb.AppendLine("START *************GetChannels_Async ***************");
                sw3.Restart();

                DigLst1 = await Task.Run(() => SnmpTools.GetChannels_Async(brd.DigitalPorts[0]));
                DigLst2 = await Task.Run(() => SnmpTools.GetChannels_Async(brd.DigitalPorts[1]));

                sw3.Stop();

                foreach (Channel ch in DigLst1)
                {
                    sb.AppendLine("2. Digi Port1 Value: " + ch.State);
                }

                foreach (Channel ch in DigLst2)
                {
                    sb.AppendLine("2. Digi Port2 Value: " + ch.State);
                }

                sb.AppendLine("************************GetChannels_Async ----- MilliSeconds: " + sw3.ElapsedMilliseconds);


                sb.AppendLine("START *************GetAllChannels_ParallelAsync ***************");

                sw4.Restart();



                sw4.Stop();

                sb.AppendLine("***********************GetAllChannels_ParallelAsync ----- MilliSeconds: " + sw4.ElapsedMilliseconds);

                sb.AppendLine("START *************AllChannelsInParallelNonBlockingAsync ***************");
                sw5.Restart();

                DigLst1 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.DigitalPorts[0], MainCancelToken));

                foreach (Channel ch in DigLst1)
                    sb.AppendLine("4. Digital List1 : Value " + ch.State);

                DigLst2 = await Task.Run(() => SnmpTools.AllChannelsInParallelNonBlockingAsync(brd.DigitalPorts[1], MainCancelToken));

                foreach (Channel ch in DigLst2)
                    sb.AppendLine("4. Digital List2 : Value " + ch.State);

                sw5.Stop();

                sb.AppendLine("************************AllChannelsInParallelNonBlockingAsync ----- MilliSeconds: " + sw5.ElapsedMilliseconds);

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
            string _ip = cmbIPs.SelectedText.Trim();

            EthernetBoard brd = new EthernetBoard(0, cmbIPs.SelectedText);

            await Task.Run(() => brd.Init("Board 0"));

            sw1.Restart();

            IList<Variable> l1 = await SnmpTools.SnmpGetAll(EthernetBoardPortNo.DIGITAL_PORT_1, _ip);
            Debug.Print("1. Get All Results: " + l1.Select(c => c.Data.ToString()));

            IList<Variable> l2 = await SnmpTools.SnmpGetAll(EthernetBoardPortNo.DIGITAL_PORT_2, _ip);
            Debug.Print("2. Get All Results: " + l2.Select(c => c.Data.ToString()));

            IList<Variable> l3 = await SnmpTools.SnmpGetAll(EthernetBoardPortNo.ADC_PORT_1, _ip);
            Debug.Print("3. Get All Results: " + l3.Select(c => c.Data.ToString()));

            sw1.Stop();

            AppendToOutputWindow("*****************************SnmpGetAll ----- MilliSeconds: " + sw1.ElapsedMilliseconds);
        }
        private async void btnDoSetAll_Click(object sender, EventArgs e)
        {
            await DoSetAll();
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
            string _ip = cmbIPs.SelectedText.Trim();

            EthernetBoard brd = new EthernetBoard(0, cmbIPs.SelectedText);

            await Task.Run(() => brd.Init("Board 0"));

            try

            {
                sb.AppendLine("*********SnmpSet_Async ********************************");

                sw1.Restart();
                foreach (Channel ch in brd.DigitalPorts[0].DigitalChannels)
                {
                    newVal = (ch.State == 0 ? 1 : 0);
                    st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                    sb.AppendLine(st);
                }

                foreach (Channel ch in brd.DigitalPorts[1].DigitalChannels)
                {
                    newVal = (ch.State == 0 ? 1 : 0);
                    st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                    sb.AppendLine(st);
                }

                foreach (Channel ch in brd.AnalogPorts[0].DigitalChannels)
                {
                    newVal = (ch.State == 0 ? 1 : 0);
                    st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                    sb.AppendLine(st);
                }

                sw1.Stop();
                sb.AppendLine("*********SnmpSet_Async ----- MilliSeconds: " + sw1.ElapsedMilliseconds);

                sb.AppendLine("START *********SnmpSetAll_ParallelAsync ********************************");
                sw2.Restart();

                st = await SnmpTools.SnmpSetAll_ParallelAsync(1, _ip, brd.DigitalPorts[0], MainCancelToken);
                sw2.Stop();
                sb.AppendLine(st);
                sb.AppendLine("*********SnmpSetAll_ParallelAsync ----- MilliSeconds: " + sw2.ElapsedMilliseconds);

                sb.AppendLine("START *********SnmpSet ********************************");
                sw3.Restart();

                foreach (Channel ch in brd.DigitalPorts[0].DigitalChannels)
                {
                    newVal = (ch.State == 0 ? 1 : 0);
                    st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                    sb.AppendLine(st);
                }

                foreach (Channel ch in brd.DigitalPorts[1].DigitalChannels)
                {
                    newVal = (ch.State == 0 ? 1 : 0);
                    st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                    sb.AppendLine(st);
                }

                if (brd.BoardID == 0 || brd.BoardID == 4)
                {
                    foreach (AnalogChannel ch in brd.AnalogPorts[0].DigitalChannels)
                    {
                        newVal = ch.Value;
                        st = await SnmpTools.SnmpSet_Async(ch.OID, newVal, _ip, MainCancelToken);
                        sb.AppendLine(st);
                    }
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
           // PowerSnmpAgent frm = new PowerSnmpAgent();
            //frm.Show();
        }

        private void btnSendTrap_Click(object sender, EventArgs e)
        {
            var progress1 = new Progress<string>(s => txtOutput.Text = s);
            //var progress2 = new Progress<string>(s => txtOutput.Text = s);
            //var progress3 = new Progress<string>(s => txtOutput.Text = s);
            //var progress4 = new Progress<string>(s => txtOutput.Text = s);
            //var progress5 = new Progress<string>(s => txtOutput.Text = s);
            //var progress6 = new Progress<string>(s => txtOutput.Text = s);
            //var progress7 = new Progress<string>(s => txtOutput.Text = s);
            //var progress8 = new Progress<string>(s => txtOutput.Text = s);

            Task tsk1 = Task.Factory.StartNew(() => EthernetBoard_1.CreateTrap(progress1, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk2 = Task.Factory.StartNew(() => EthernetBoard_2.CreateTrap(progress2, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk3 = Task.Factory.StartNew(() => EthernetBoard_3.CreateTrap(progress3, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk4 = Task.Factory.StartNew(() => EthernetBoard_4.CreateTrap(progress4, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk5 = Task.Factory.StartNew(() => EthernetBoard_5.CreateTrap(progress5, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk6 = Task.Factory.StartNew(() => EthernetBoard_6.CreateTrap(progress6, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk7 = Task.Factory.StartNew(() => EthernetBoard_7.CreateTrap(progress7, MainCancelToken), TaskCreationOptions.LongRunning);
            //Task tsk8 = Task.Factory.StartNew(() => EthernetBoard_8.CreateTrap(progress8, MainCancelToken), TaskCreationOptions.LongRunning);

            valuelist.Add(tsk1);
            //valuelist.Add(tsk2);
            //valuelist.Add(tsk3);
            //valuelist.Add(tsk4);
            //valuelist.Add(tsk5);
            //valuelist.Add(tsk6);
            //valuelist.Add(tsk7);
            //valuelist.Add(tsk8);
        }
    }
}
