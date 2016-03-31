using System;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;

namespace snmpd
{
    static class SnmpTools
    {

        /// <summary>
        /// A syncronous call to Snmp Get method.
        /// </summary>
        /// <param name="OID">The Object Identifier object associated with the channel to read.</param>
        /// <returns>The value read from the channel.</returns>
        public static string SnmpGet(ObjectIdentifier OID, string pIp)
        {
            IList<Variable> result = null;
            IList<Variable> argsIn = new List<Variable> { new Variable(OID) };
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);
            string retVal;

            try
            {
                result = Messenger.Get(VersionCode.V1, Ep,
                     new OctetString(Utilities.COMMUNITY),
                     argsIn, Utilities.SNMP_GET_TIMEOUT);
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
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

        /// <summary>
        /// Snmp GET for all channels in specified port. 
        /// </summary>
        /// <param name="pPortToScan">The Port to try to read.</param>
        /// <returns></returns>
        public static async Task<IList<Variable>> SnmpGetAll(EthernetBoardPortNo pPortToScan, string pIp)
        {
            IList<Variable> result = null;
            ObjectIdentifier _oid;
            List<Variable> lstVar = new List<Variable>();
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);

            switch (pPortToScan)
            {
                case EthernetBoardPortNo.DIGITAL_PORT_1:
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch1));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch2));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch3));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch4));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch5));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch6));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch7));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch8));

                    break;
                case EthernetBoardPortNo.DIGITAL_PORT_2:
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch1));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch2));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch3));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch4));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch5));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch6));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch7));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch8));

                    break;
                case EthernetBoardPortNo.ADC_PORT_1:
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch1));
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch2));
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch3));
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch4));

                    break;
                default:
                    throw new InvalidOperationException("SnmpGetAll: The port number to scan was incorrect.");
            }

            try
            {
                result = await Task.Run(() => Messenger.Get(VersionCode.V1, Ep,
                     new OctetString(Utilities.COMMUNITY), lstVar, 1000));

                Debug.Print("GETALL: Result Id: " + result.Select(v => v.Id) + " Data: " + result.Select(v => v.Id));
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
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
        public static async Task<string> SnmpGet_Async(ObjectIdentifier pOid, string pIp, CancellationToken ct)
        {
            IList<Variable> result = null;
            string retVal;
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);

            ct.ThrowIfCancellationRequested();

            // This wrapper is super IMPORTANT recheck convention
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    result = Messenger.Get(VersionCode.V1,
                        Ep, new OctetString(Utilities.COMMUNITY),
                        new List<Variable> { new Variable(pOid) }, Utilities.SNMP_GET_TIMEOUT);

                }, ct, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
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

  
        /// <summary>
        /// Retrieves values from an EndPoint (Ip, Port) 
        /// </summary>
        /// <returns>(List) of type Variable (octet dictionary). The new value of the channel</returns>
        public static async Task<string> SnmpGet_AsyncCallback(Channel ch, string pIp, CancellationToken ct)
        {
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);

            IList<Variable> result = null;
            RequestState MyGetStateObject = new RequestState();

            GetRequestMessage GetReqMessage;
            IAsyncResult asyncGetResults;

            GetReqMessage = new GetRequestMessage(111, VersionCode.V1,
                    new OctetString(Utilities.COMMUNITY), new List<Variable> { new Variable(ch.OID) });

            MyGetStateObject.Get_Request_Message = GetReqMessage;

            //// IAsyncResult
            //asyncGetResults = GetReqMessage.BeginGetResponse(EndPoint, new UserRegistry(), EndPoint.GetSocket(), 
            //    new AsyncCallback(GetResponse_Callback), MyGetStateObject);

            asyncGetResults = GetReqMessage.BeginGetResponse(Ep, new UserRegistry(), Ep.GetSocket(), async ar =>
            {
                MyGetStateObject.Get_Response_Message = await Task.Factory.StartNew(() => 
                    GetReqMessage.EndGetResponse(ar));

                if (MyGetStateObject.Get_Response_Message.Pdu().ErrorStatus.ToInt32() != 0) // Error?
                {
                    result = MyGetStateObject.Get_Response_Message.Variables();
                    Debug.Print(MyGetStateObject.Get_Response_Message.ToString());
                }

            }, null);

            return asyncGetResults.ToString();
        }

        // uses the ONLY async calls in  snmpLibrary
        public static void GetResponse_Callback(IAsyncResult asynchronousResult)
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

        public static string SnmpSet_AsyncCallback(Channel ch, string pIp)
        {
            List<Variable> lstVar = new List<Variable>();
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);
            RequestState MySetStateObject = new RequestState();
            SetRequestMessage SetReqMsg;
            IAsyncResult asyncResponse;

            int _NewChannelValue = ch.Value;
            lstVar.Add(new Variable(ch.OID, new Integer32(_NewChannelValue)));

            // build the SET message
            SetReqMsg = new SetRequestMessage(211, VersionCode.V1, new OctetString("private"), lstVar);

            MySetStateObject.Set_Request_Message = SetReqMsg;

            // Start the SET Request - is already async 
            asyncResponse = SetReqMsg.BeginGetResponse(Ep, new UserRegistry(),
                Ep.GetSocket(), new AsyncCallback(SetResponse_Callback), MySetStateObject);

            return asyncResponse.ToString();
        }

        public static void SetResponse_Callback(IAsyncResult asynchronousResult)
        {
            RequestState setRequestState = (RequestState)asynchronousResult.AsyncState;
            SetRequestMessage SetRequestMsg = setRequestState.Set_Request_Message;

            // ISnmpMessage
            setRequestState.Get_Response_Message = SetRequestMsg.EndGetResponse(asynchronousResult);

            // Read the response into a Stream object.
            //asyncResponse = myRequestState.Respose_Message.BeginGetResponse()
            return;
        }

        /// uses Linq and .Select - Returns an array of string  (channel values)
        public static async Task<String[]> GetVals_Async(EthernetBoardPort _chLst, string pIp, CancellationToken ct)
        {
            string[] chValues = null;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                // best for IO Ports??!?!?      
                IEnumerable<Task<string>> _channels = _chLst.ChannelsList.Select(ch =>
                    TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, pIp, ct), 3));

                chValues = await Task.WhenAll(_channels);
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                    throw;
                }
            }

            return chValues;
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
                        Utilities.SNMP_GET_TIMEOUT), ct).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                    throw;
                }
            }

            if (result != null && result.Count > 0)
                return result[0].Data.ToString();
            else
                return string.Empty;
        }

        public static async Task<IEnumerable<Channel>> GetChannels_Async(EthernetBoardPort pt)
        {
            IEnumerable<Channel> tsk = await Task.Factory.StartNew(() => 
                pt.ChannelsList.Select(ch => ch));

            return tsk;
        }

        // async and parallel - doesnt block
        public static async Task<IEnumerable<Channel>> AllChannelsInParallelNonBlockingAsync(EthernetBoardPort ebPort, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            IEnumerable<Task<IEnumerable<Channel>>> allTasks =
                 ChannelHelper.AllChannels.Select(ch => GetChannels_Async(ebPort));

            IEnumerable<Channel>[] allResults = await Task.WhenAll(allTasks);

            return allResults.SelectMany(ch => ch);
        }


        #region SET METHODS

        // ************ SETS ****************************
        public static async Task<string> SnmpSetAll_ParallelAsync(int pNewValue, string pIp, EthernetBoardPort ptList, CancellationToken ct)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            List<Task<string>> taskList = new List<Task<string>>();

            ct.ThrowIfCancellationRequested();

            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[0].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[1].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[2].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[3].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[4].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[5].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[6].OID, pNewValue, pIp, ct)));
            taskList.Add(await Task.Factory.StartNew(() => SnmpSet_Async(ptList.ChannelsList[7].OID, pNewValue, pIp, ct)));

            ct.ThrowIfCancellationRequested();

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


            return sb.ToString();
        }

        public static async Task<string> SnmpSetAll_Async(int pNewValue, CancellationToken ct)
        {
            int _port = 161;
            string sIp = "192.168.1.40";
            string res = null;

            List<Task<Task<string>>> taskList = new List<Task<Task<string>>>();
            StringBuilder sb = new StringBuilder();
            EthernetBoard brd = new EthernetBoard(0, "192.168.1.40");

            if (await Task.Run(() => brd.init("Board 0", _port, "private", 50, 100, true, true)))
            {
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[0].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[1].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[2].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[3].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[4].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[5].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[6].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[0].ChannelsList[7].OID, pNewValue, sIp, ct)));

                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[0].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[1].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[2].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[3].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[4].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[5].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[6].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.DigitalPortsList[1].ChannelsList[7].OID, pNewValue, sIp, ct)));

                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[0].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[1].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[2].OID, pNewValue, sIp, ct)));
                taskList.Add(Task.Factory.StartNew(() => SnmpSet_Async(brd.AnalogPortsList[0].ChannelsList[3].OID, pNewValue, sIp, ct)));

                Task tsk = Task.WhenAll(taskList.ToArray());
 
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
            return sb.ToString();
        }

        public static async Task<string> SnmpSet_StaticAsync(ObjectIdentifier pOid, string sIPAddress, int pPortNo, int pNewChannelValue, CancellationToken ct)
        {
            IList<Variable> result = null;
            List<Variable> lstVar = new List<Variable>();

            lstVar.Add(new Variable(pOid, new Integer32(pNewChannelValue)));

            IPEndPoint MyEp = new IPEndPoint(IPAddress.Parse(sIPAddress), pPortNo);

            try
            {
                result = await Task.Run(() => Messenger.Set(VersionCode.V1, MyEp,
                        new OctetString("private"), lstVar, Utilities.SNMP_SET_TIMEOUT), ct).ConfigureAwait(false);

                if (result != null && result.Count > 0)
                {
                    Debug.Print("Result is: " + result[0].Data.ToString());
                }
                else
                {
                    Debug.Print("Result is: BROKEN");
                }
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                    throw;
                }
            }

            if (result != null && result.Count > 0)
                return result[0].Data.ToString();
            else
                return string.Empty;
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
        public static string SnmpSet(ObjectIdentifier _Oid, string pIp, int _iChannelValue, CancellationToken ct)
        {
            IList<Variable> result = null;
            List<Variable> lstVar = new List<Variable>();
            lstVar.Add(new Variable(_Oid, new Integer32(_iChannelValue)));
            string retVal = null;
            string _sOid = _Oid.ToString();
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);

            ct.ThrowIfCancellationRequested();

            try
            {
                Task.Factory.StartNew(() =>
                {
                    result = Messenger.Set(VersionCode.V1, Ep,
                            new OctetString(Utilities.COMMUNITY), lstVar, Utilities.SNMP_SET_TIMEOUT);
                },
                ct, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
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

        public static async Task<IList<Variable>> SnmpSetAll(EthernetBoardPortNo _portToScan, int _iChannelValue, string pIp, CancellationToken ct)
        {
            IList<Variable> result = null;
            List<Variable> lstVar = new List<Variable>();
            int ON = 1;
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);

            ct.ThrowIfCancellationRequested();

            switch (_portToScan)
            {
                case EthernetBoardPortNo.DIGITAL_PORT_1:
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch1, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch2, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch3, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch4, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch5, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch6, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch7, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io1_Ch8, new Integer32(ON)));

                    break;
                case EthernetBoardPortNo.DIGITAL_PORT_2:
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch1, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch2, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch3, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch4, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch5, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch6, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch7, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Io2_Ch8, new Integer32(ON)));
                    break;
                case EthernetBoardPortNo.ADC_PORT_1:
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch1, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch2, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch3, new Integer32(ON)));
                    lstVar.Add(new Variable(ChannelHelper.Ad1_Ch4, new Integer32(ON)));

                    break;
                default:
                    throw new InvalidOperationException("SnmpSetAll: The port number to scan was incorrect.");
            }

            try

            {
                result = await Task.Run(() => Messenger.Set(VersionCode.V1, Ep,
                    new OctetString(Utilities.COMMUNITY), lstVar, Utilities.SNMP_SET_TIMEOUT));

                Debug.Print("SETALL: Result Id: " + result.Select(v => v.Id) + " Data: " + result.Select(v => v.Id));
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Message);
                    throw;
                }
            }
            //if (result != null && result.Count > 0)
            //    retVal = result[0].Data.ToString();
            //else
            //    retVal = string.Empty;

            return result;
        }

        public static string SnmpSet(ObjectIdentifier pOid, int pNewVale, string pIp)
        {
            IList<Variable> result = null;
            string retVal = null;
            List<Variable> lstVar = new List<Variable>();
            lstVar.Add(new Variable(pOid, new Integer32(pNewVale)));
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);

            try
            {
                result = Messenger.Set(VersionCode.V1, Ep,
                    new OctetString(Utilities.COMMUNITY), lstVar, Utilities.SNMP_SET_TIMEOUT);
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
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

        /// <summary>Updates the EthernetBoard.Port.Channel value of a device.</summary>
        /// <returns>(List) of type Variable (octet dictionary) </returns>
        public static async Task<string> SnmpSet_Async(ObjectIdentifier pOid, int pNewChannelValue, string pIp, CancellationToken ct)
        {
            // use TPL 
            string retVal = null;
            IPEndPoint Ep = new IPEndPoint(IPAddress.Parse(pIp), Utilities.LISTEN_PORT);
            IList<Variable> result = null;
            List<Variable> lstVar = new List<Variable>();
            lstVar.Add(new Variable(pOid, new Integer32(pNewChannelValue)));

            try
            {
                ct.ThrowIfCancellationRequested();

                // This wrapper is super IMPORTANT recheck convention
                await Task.Run(() =>
                {
                    result = null;

                    result = Messenger.Set(VersionCode.V1, Ep,
                        new OctetString(Utilities.COMMUNITY), lstVar, Utilities.SNMP_SET_TIMEOUT);

                }).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                if (Utilities.IsRealError(ex))
                {
                    Debug.Print("Unhandled Error: " + ex.Flatten().InnerExceptions.Select(er => er.Message));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Utilities.IsRealError(ex))
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


        #endregion SET METHODS
    }
}
