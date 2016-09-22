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

namespace snmpd
{

    /// <summary>
    /// A class that represents a Denkovi Relay Board
    /// </summary>
    public sealed class EthernetBoard : IDisposable
    {
        #region VariableAndDelegateDeclarations

        /// <summary>
        /// Delegate for access to error handling functions outside of the class
        /// </summary>
        public delegate void ErrorHandlerDelegate(object sender, ErrorEventArgs e);

        /// <summary>
        /// error event for the client to subscribe to
        /// </summary>
        public event ErrorHandlerDelegate onError;

        public delegate void InitializeHandlerDelegate(object sender, BoardInitEventArgs e);
        public event InitializeHandlerDelegate onInit;

        public delegate void AsyncErrorHandlerDelegate(object sender, AsyncErrorEventArgs e);
        public event AsyncErrorHandlerDelegate onAsyncError;

        public delegate void LoggingRaisedHandlerDelegate(string message, string color);
        public event LoggingRaisedHandlerDelegate onLoggingRaised;

        public delegate void DigitalInputHandlerDelegate(object sender, DigitalInputEventArgs e);
        public event DigitalInputHandlerDelegate onDigitalInput;

        public delegate void AnalogInputHandlerDelegate(object sender, AnalogInputEventArgs e);
        public event AnalogInputHandlerDelegate onAnalogInput;



        // public properties
        public string BoardName { get; private set; }
        public int BoardID { get; private set; }
        public int TcpPort { get; private set; }
        public string IP_Address { get; private set; }
        public bool IsActive { get; private set; }
        public bool AnalogListening { get; private set; }
        public bool Cancel
        {
            get { return CancelTokenSource.IsCancellationRequested; }
            set { if (value == true) CancelTokenSource.Cancel(); }
        }

        private IPEndPoint EndPoint = null;
        private IPAddress IP = null;

        public List<EthernetBoardPort> DigitalPorts { get; private set; }
        public List<EthernetBoardPort> AnalogPorts { get; private set; }

        private CancellationTokenSource CancelTokenSource = null;
        private CancellationToken GlobalCancelToken;

        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

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
            int thisID = 0;

            BoardID = _boardId;
            IP_Address = _IpAddress;

            string sOid = null;
            string sNm = null;

            DigitalPorts = new List<EthernetBoardPort>();

            // These loops assign the channels names, numbers, and other values to
            // the List of Channel objects in the EthernetBoardPort objects
            tempPort = new EthernetBoardPort(EthernetBoardPortNo.DIGITAL_PORT_1,
                false, PortMode.INPUT, _IpAddress, _boardId);

            for (int index = 0; index < Util.DIG_CHAN_PER_PORT; index++)
            {
                thisID = index + 1;
                sb.Clear();
                sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.1.").Append(thisID).Append(".0").ToString();
                sNm = "Digital Port # 2 Channel # " + thisID;

                tempPort.DigitalChannels.Add(new Channel(thisID, new ObjectIdentifier(sOid), sNm, ChannelState.OFF));
            }

            DigitalPorts.Add(tempPort);

            tempPort = new EthernetBoardPort(EthernetBoardPortNo.DIGITAL_PORT_2,
                false, PortMode.INPUT, _IpAddress, _boardId);

            for (int index2 = 0; index2 < Util.DIG_CHAN_PER_PORT; index2++)
            {
                thisID = index2 + 1;
                sb.Clear();
                sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.2.").Append(thisID).Append(".0").ToString();
                sNm = "Digital Port # 2 Channel # " + thisID;

                tempPort.DigitalChannels.Add(new Channel
                    (thisID, new ObjectIdentifier(sOid), sNm, ChannelState.OFF));
            }

            DigitalPorts.Add(tempPort);

            if (BoardID == 0 || BoardID == 4)
            {
                AnalogPorts = new List<EthernetBoardPort>();
                tempPort = new EthernetBoardPort(EthernetBoardPortNo.ADC_PORT_1,
                    true, PortMode.INPUT, _IpAddress, _boardId);

                // 1 analog port with 4 channels
                for (int index3 = 0; index3 < Util.ANLG_CHAN_PER_PORT; index3++)
                {
                    thisID = index3 + 1;
                    sb.Clear();
                    sOid = sb.Append(".1.3.6.1.4.1.19865.1.2.3.").Append(thisID).Append(".0").ToString();
                    sNm = "Analog Port # 1 Channel #" + thisID;

                    tempPort.AnalogChannels.Add(new AnalogChannel
                        (thisID, new ObjectIdentifier(sOid), sNm, 0));
                }

                AnalogPorts.Add(tempPort);
            }        
        }

        /// <summary>
        /// This method sets up global EthernetBoard values, retrieves and assigns all 
        /// EthernetBoardPorts and Channel values.
        /// </summary>
        /// <param name="pBoardName">The name of the Board.</param>
        /// <returns>Boolean success</returns>
        public async Task<bool> Init(string pBoardName)
        {
            bool success = false;
            IsActive = false;
            AnalogListening = false;

            CancelTokenSource = new CancellationTokenSource();
            GlobalCancelToken = CancelTokenSource.Token;

            TcpPort = Util.LISTEN_PORT;
            BoardName = pBoardName;
            IP = IPAddress.Parse(IP_Address);
            EndPoint = new IPEndPoint(IP, TcpPort);

            await Task.Run(() => GetInitialValues());

            success = true;

            onInit?.Invoke(this, new BoardInitEventArgs(BoardID, DigitalPorts, AnalogPorts));

            //if (onLoggingRaised != null)
            //    onLoggingRaised("Initialized GPIO Ethernet Board with ID : " + _BoardID.ToString()
            //        + " and Name : " + _BoardName + " completed successfully", "#0000FF");

            return success;
        }

        #endregion

        #region ChannelMethods

        public async Task AllOn()
        {
            foreach (Channel ch in DigitalPorts[1].DigitalChannels)
            {
                ch.SetState(ChannelState.ON);

                onDigitalInput?.Invoke(this, new DigitalInputEventArgs(this.BoardID, DigitalPorts[1].IoPortNo, ch));
            }
        }

        public async Task AllOff()
        {
            foreach (Channel ch in DigitalPorts[1].DigitalChannels)
            {
                ch.SetState(ChannelState.OFF);

                onDigitalInput?.Invoke(this, new DigitalInputEventArgs(this.BoardID, DigitalPorts[1].IoPortNo, ch));
            }
        }

        /// <summary>
        /// For debugging purposes. Shows the value of every channel.
        /// </summary>
        /// <returns></returns>
        public string PrintAllChannels()
        {
            StringBuilder sb = new StringBuilder();
            string sChVal;

            sb.Clear();
            sb.Append("All Channel Values - Board: ").AppendLine(IP_Address);
            sb.AppendLine("DIGITAL PORT 1 ");

            foreach (Channel ch in DigitalPorts[0].DigitalChannels)
            {
                sChVal = (ch.State == ChannelState.ON ? "ON" : "OFF");

                sb.Append("Channel #").Append(ch.Id).Append(" - Value: ").Append(sChVal).AppendLine(".");
            }

            sb.AppendLine("DIGITAL PORT 2 ");

            foreach (Channel ch in DigitalPorts[1].DigitalChannels)
            {
                sChVal = (ch.State == ChannelState.ON ? "ON" : "OFF");

                sb.Append("Channel #").Append(ch.Id).Append(" - Value: ").Append(sChVal).AppendLine(".");
            }

            if (AnalogPorts.Count > 0)
            {
                sb.AppendLine("ANALOG PORT 1 ");
                foreach (AnalogChannel ch in AnalogPorts[0].AnalogChannels)
                {
                    sb.Append("Channel #").Append(ch.Id).Append(" - Value: ").Append(ch.Value).AppendLine(".");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Set a trap to be informed when a channel value has changed
        /// </summary>
        /// <param name="pIP">The IP of the device to monitor</param>
        /// <param name="pEOid">The Enterprise OID</param>
        /// <returns></returns>
        public async Task CreateTrap(IProgress<string> pProgress, CancellationToken CanTok)
        {
            List<Variable> lstVars = new List<Variable>();

            foreach (Channel ch in ChannelHelper.AllDigitalBoard1Channels)
            {
                lstVars.Add(new Variable(ch.OID, new Integer32(1)));
            }

            foreach (Channel ch in ChannelHelper.AllDigitalBoard2Channels)
            {
                lstVars.Add(new Variable(ch.OID, new Integer32(1)));
            }

            // ???? encode the communitystring. Once I set the communitystring to base64 for the discovered SNMP class objec????
            try
            {
                CanTok.ThrowIfCancellationRequested();
                ObjectIdentifier oid = new ObjectIdentifier(Util.ENTERPRISE_OID);

                // sendtrapv1(Where its going, Who its from, community string, Enterprise OID, TrapType, 
                // some code, timestamp, List Trap values)
                await Task.Factory.StartNew(() => Messenger.SendTrapV1(new IPEndPoint(this.IP, Util.RECEIVE_PORT),
                    this.IP, new OctetString("private"), oid, GenericCode.ColdStart, 0, 0, lstVars)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                    && !(ex is System.Net.Sockets.SocketException))
                {
                    throw;
                }
            }


            // -----------------------------------------------------------------------
            //using SnmpSharpNet() { 
            //    SnmpSharpNet.TrapAgent agent = new SnmpSharpNet.TrapAgent();

            //    // Variable Binding collection to send with the trap
            //    SnmpSharpNet.VbCollection col = new SnmpSharpNet.VbCollection();

            //    col.Add(new Oid("1.3.6.1.2.1.1.1.0"), new OctetString("Test string"));
            //    col.Add(new Oid("1.3.6.1.2.1.1.2.0"), new Oid("1.3.6.1.9.1.1.0"));
            //    col.Add(new Oid("1.3.6.1.2.1.1.3.0"), new TimeTicks(2324));
            //    col.Add(new Oid("1.3.6.1.2.1.1.4.0"), new OctetString("Milan"));

            //    // Send the trap to the localhost port 162
            //    agent.SendV1Trap(new IpAddress("localhost"), 162, "public",
            //                     new Oid("1.3.6.1.2.1.1.1.0"), new IpAddress("127.0.0.1"),
            //                     SnmpConstants.LinkUp, 0, 13432, col);
            //}
            // ------------------------------------------------------------------------

        }


        /// <summary>
        /// Toggle the State of a channel (On/Off).
        /// </summary>
        /// <param name="ch">The Channel to toggle.</param>
        public void ToggleRelay(Channel ch)
        {
            int newValue = (ch.State == ChannelState.ON ? 0 : 1); // Set it to the opposite

            Task.Run(() => SnmpSet_Async(ch.OID, newValue,
                IP_Address, GlobalCancelToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// Read and copy all channel values from hardware. 
        /// </summary>
        /// <returns>Async Task</returns>
        private async Task GetInitialValues()
        {
            string sTo = "1";
            string sIP = IP_Address;
            string sOID, s;
            int AnalogChanVal, PreviewByteValue;

            try
            {
                for (int PortIndex = 0; PortIndex < Util.NUMBER_OF_DIGITAL_PORTS; PortIndex++)
                {
                    if (PortIndex == 0)
                        sOID = ChannelHelper.Io1_All.ToString();
                    else if (PortIndex == 1)
                        sOID = ChannelHelper.Io2_All.ToString();
                    else
                        sOID = ChannelHelper.Io2_All.ToString();

                    // Read all channels simultaneously
                    PreviewByteValue = await Util.SnmpGetDigitalValues(sIP, sOID, sTo);

                    if (PreviewByteValue > -1)
                    {
                        // Compare current and just read channel values - if values have changed
                        if ((DigitalPorts[PortIndex].ChannelsByteValue != PreviewByteValue))
                        {
                            // value has changed
                            char[] newVals = Util.FromIntToChars(PreviewByteValue, 8);

                            for (int ChannelIndex = 0; ChannelIndex < DigitalPorts[PortIndex].DigitalChannels.Count; ChannelIndex++)
                            {
                                if (newVals[ChannelIndex] != '0')
                                {
                                    // update object value
                                    DigitalPorts[PortIndex].DigitalChannels[ChannelIndex].SetState(ChannelState.ON);

                                    onDigitalInput?.Invoke(this, new DigitalInputEventArgs(BoardID,
                                        DigitalPorts[PortIndex].IoPortNo, DigitalPorts[PortIndex].DigitalChannels[ChannelIndex]));
                                }
                            }
                        }
                    }
                }

                // only supports 1 analog list
                if (AnalogPorts.Count > 0)
                {
                    sOID = ChannelHelper.Ad1_Ch1.ToString();
                    AnalogChanVal = await Util.SnmpGetAnalogValue(sIP, sOID, sTo);
                    AnalogPorts[0].AnalogChannels[0].SetValue(AnalogChanVal);
                    Debug.Print("GetAnaVal(1) - " + AnalogChanVal);

                    sOID = ChannelHelper.Ad1_Ch2.ToString();
                    AnalogChanVal = await Util.SnmpGetAnalogValue(sIP, sOID, sTo);
                    AnalogPorts[0].AnalogChannels[1].SetValue(AnalogChanVal);
                    Debug.Print("GetAnaVal(2) - " + AnalogChanVal);

                    sOID = ChannelHelper.Ad1_Ch3.ToString();
                    AnalogChanVal = await Util.SnmpGetAnalogValue(sIP, sOID, sTo);
                    AnalogPorts[0].AnalogChannels[2].SetValue(AnalogChanVal);
                    Debug.Print("GetAnaVal(3) - " + AnalogChanVal);

                    sOID = ChannelHelper.Ad1_Ch4.ToString();
                    AnalogChanVal = await Util.SnmpGetAnalogValue(sIP, sOID, sTo);
                    AnalogPorts[0].AnalogChannels[3].SetValue(AnalogChanVal);
                    Debug.Print("GetAnaVal(4) - " + AnalogChanVal);
                }

            }
            catch (AggregateException ex)
            {
                if (Util.IsRealError(ex))
                {
                    if (onAsyncError != null)
                        onAsyncError(this, new AsyncErrorEventArgs(ex));
                    else
                        Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions
                            .Select(er => er.Message));
                }
            }
            catch (Exception ex)
            {
                if (Util.IsRealError(ex))
                {
                    if (onError != null)
                        onError(this, new ErrorEventArgs(ex));
                    else
                        Debug.Print("Unhandled Error: " + ex.Message);
                }
            }
        }

        private async Task CheckForNewDigitalData()
        {
            // only reading port 2
            char[] PreviewChanVals = null;
            int PreviewByteValue;
            //EthernetBoardPort port1 = DigitalPorts[0];
            EthernetBoardPort port2 = DigitalPorts[1];
            string sIp = IP_Address;
            string sTo = Util.SNMP_GET_TIMEOUT.ToString();
            string sOid, s;

            //******************************* SCAN PORT *******************************************//
            // check each channel in the port for updated data (button values)                     //
            // Compare it to DigitalPorts list. Update list if new values are found            //
            //*************************************************************************************//
            try
            {

                sOid = ChannelHelper.Io2_All.ToString();

                // Read all channels simultaneously
                PreviewByteValue = await Util.SnmpGetDigitalValues(sIp, sOid, sTo);

                if (PreviewByteValue > -1)
                {
                    // Debug stuff
                    PreviewChanVals = Util.FromIntToChars(PreviewByteValue, 8);
                    s = new string(PreviewChanVals);
                    Debug.Print("CheckForNewDigitalData. PreviewByteValue: " + PreviewByteValue + " - Binary String: " + s);

                    // Compare current and just read channel values - if values have changed
                    if (port2.ChannelsByteValue != PreviewByteValue)
                    {
                        // value has changed
                        char[] oldVals = Util.FromIntToChars(port2.ChannelsByteValue, 8);
                        char[] newVals = Util.FromIntToChars(PreviewByteValue, 8);
                        port2.ChannelsByteValue = PreviewByteValue;

                        for (int ChannelIndex = 0; ChannelIndex < PreviewChanVals.Length; ChannelIndex++)
                        {
                            if (oldVals[ChannelIndex] != newVals[ChannelIndex])
                            {
                                // Debug stuff
                                PreviewChanVals = Util.FromIntToChars(PreviewByteValue, 8);
                                s = new string(PreviewChanVals);
                                Debug.Print("New Value! " + IP_Address + " " + sOid + " value: "
                                    + newVals[ChannelIndex]);

                                // update object value
                                port2.DigitalChannels[ChannelIndex].ToggleState();

                                onDigitalInput?.Invoke(this, new DigitalInputEventArgs(BoardID, port2.IoPortNo,
                                        port2.DigitalChannels[ChannelIndex]));

                                // update interface
                                //if (onDigitalInput != null)
                                //{
                                //    App.Current.Dispatcher.Invoke(onDigitalInput,
                                //        new DigitalInputEventArgs(BoardID, DigitalPorts[PortIndex].IoPortNo,
                                //        DigitalPorts[PortIndex].DigitalChannels[ChannelIndex]));
                                //}
                            }
                        }
                    }
                }

                PreviewChanVals = null;
            }
            catch (AggregateException ex)
            {
                if (Util.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " +
                        ex.Flatten().InnerExceptions.Select(er => er.Message));
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    Debug.Print("ATASK CANCELLED");
                }

                if (Util.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                }
            }
        }

        private async Task CheckForNewAnalogData()
        {
            int NewChannelValue;
            EthernetBoardPort algPort = AnalogPorts[0];
            string sIp = algPort.ParentIP;
            string sOid = ChannelHelper.Ad1_All.ToString();
            string sTo = Util.SNMP_GET_TIMEOUT.ToString();
            AnalogChannel Chan;

            //******************************* SCAN PORT *******************************************//
            // check each channel in the port for updated data (button values)                     //
            // Compare it to AnalogPorts list. Update list if new values are found              //
            //*************************************************************************************//
            try
            {
                NewChannelValue = await Util.SnmpGetAnalogValue(ChannelHelper.Ad1_Ch1.ToString(), sOid, sTo);

                if (NewChannelValue != algPort.AnalogChannels.ElementAt(0).Value) // Light has new value
                {
                    Chan = algPort.AnalogChannels.ElementAt(0);
                    Chan.SetValue(NewChannelValue);

                    if (onAnalogInput != null)
                    {
                        onAnalogInput?.Invoke(onAnalogInput,
                            new AnalogInputEventArgs(algPort.ParentID, algPort.IoPortNo, Chan));
                    }
                }

                NewChannelValue = await Util.SnmpGetAnalogValue(ChannelHelper.Ad1_Ch2.ToString(), sOid, sTo);

                if (NewChannelValue != algPort.AnalogChannels.ElementAt(1).Value) // Light has new value
                {
                    Chan = algPort.AnalogChannels.ElementAt(1);
                    Chan.SetValue(NewChannelValue);

                    if (onAnalogInput != null)
                    {
                        onAnalogInput?.Invoke(onAnalogInput,
                            new AnalogInputEventArgs(algPort.ParentID, algPort.IoPortNo, Chan));
                    }
                }

                NewChannelValue = await Util.SnmpGetAnalogValue(ChannelHelper.Ad1_Ch3.ToString(), sOid, sTo);

                if (NewChannelValue != algPort.AnalogChannels.ElementAt(2).Value) // Light has new value
                {
                    Chan = algPort.AnalogChannels.ElementAt(2);
                    Chan.SetValue(NewChannelValue);

                    if (onAnalogInput != null)
                    {
                        onAnalogInput?.Invoke(onAnalogInput,
                            new AnalogInputEventArgs(algPort.ParentID, algPort.IoPortNo, Chan));
                    }
                }

                NewChannelValue = await Util.SnmpGetAnalogValue(ChannelHelper.Ad1_Ch4.ToString(), sOid, sTo);

                if (NewChannelValue != algPort.AnalogChannels.ElementAt(3).Value) // Light has new value
                {
                    Chan = algPort.AnalogChannels.ElementAt(3);
                    Chan.SetValue(NewChannelValue);

                    if (onAnalogInput != null)
                    {
                        onAnalogInput?.Invoke(onAnalogInput,
                            new AnalogInputEventArgs(algPort.ParentID, algPort.IoPortNo, Chan));
                    }
                }
            }
            catch (AggregateException ex)
            {
                if (Util.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " +
                        ex.Flatten().InnerExceptions.Select(er => er.Message));
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    Debug.Print("ATASK CANCELLED");
                }

                if (Util.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                }
            }
        }

        public void ToggleDigitalPolling()
        {
            if (IsActive)
            {
                Cancel = true;
            }
            else
            {
                StartDigitalPolling();
            }
        }

        public void ToggleAnalogPolling()
        {
            if (IsActive)
            {
                Cancel = true;
            }
            else
            {
                StartAnalogPolling();
            }
        }

        public async Task StartDigitalPolling()
        {
            // run a loop reading the channels each iteration
            // until a cancel is received.

            while (!GlobalCancelToken.IsCancellationRequested)
            {
                //Task myTask = new Task(Utilities.SnmpGetDigital(;
                // read live channel values
                await CheckForNewDigitalData();
                // compare these channels to the current values
            }
        }

        public void StartAnalogPolling()
        {
            // run a loop reading the channels each iteration
            // until a cancel is received.

            while (!GlobalCancelToken.IsCancellationRequested)
            {
                //Task myTask = new Task(Utilities.SnmpGetDigital(;
                // read live channel values
                CheckForNewAnalogData();
                // compare these channels to the current values
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
            GC.ReRegisterForFinalize(this);
        }

        #endregion

        #region Snmp Methods

        /// <summary>Updates the EthernetBoard.Port.Channel value of a device.</summary>
        /// <returns>(List) of type Variable (octet dictionary) </returns>
        public static async Task<string> SnmpSet_Async(ObjectIdentifier pOid, int pNewChannelValue, string pIp, CancellationToken ct)
        {
            // use TPL 
            string retVal = null;
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Util.LISTEN_PORT);
            IList<Variable> result = null;
            List<Variable> lstVar = new List<Variable>();
            lstVar.Add(new Variable(pOid, new Integer32(pNewChannelValue)));

            try
            {
                ct.ThrowIfCancellationRequested();
                result = null;

                // This wrapper is super IMPORTANT recheck convention
                await Task.Run(() =>
                {
                    result = Messenger.Set(VersionCode.V1, Ep,
                         new OctetString(Util.COMMUNITY), lstVar, Util.SNMP_SET_TIMEOUT);
                }, ct).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (Util.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Util.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                    throw;
                }
            }
            if (result != null && result.Count > 0)
                retVal = result[0].Data.ToString();
            else
                retVal = string.Empty;

            return retVal;
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
    }
}
