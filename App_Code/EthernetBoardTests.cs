using System.Net;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using Lextm.SharpSnmpLib;
using System.Threading.Tasks;

namespace snmpd
{
     class EthernetBoardTests
     {

          private IPAddress _myIP;

          private CancellationTokenSource TokenFactory = new CancellationTokenSource();
          private CancellationToken MainCancelToken;

          public static EthernetBoard EthernetBoard_1 = new EthernetBoard(0, "192.168.1.40");
          public static EthernetBoard EthernetBoard_2 = new EthernetBoard(1, "192.168.1.41");
          public static EthernetBoard EthernetBoard_3 = new EthernetBoard(2, "192.168.1.42");
          public static EthernetBoard EthernetBoard_4 = new EthernetBoard(3, "192.168.1.43");
          public static EthernetBoard EthernetBoard_5 = new EthernetBoard(4, "192.168.1.44");
          public static EthernetBoard EthernetBoard_6 = new EthernetBoard(5, "192.168.1.45");
          public static EthernetBoard EthernetBoard_7 = new EthernetBoard(6, "192.168.1.46");
          public static EthernetBoard EthernetBoard_8 = new EthernetBoard(7, "192.168.1.47");


          public EthernetBoardTests(string pIpAddress)
          {
               MainCancelToken = TokenFactory.Token;
          }

          public void StartListening_STP(object state)
          {
               //DigPort1_Listen_1(state);   // SmartThreadingLib
               //DigPort2_Listen_1(state);   // SmartThreadingLib
               //AdcPort1_Listen_1(state);   // SmartThreadingLib
          }

          // Uses the SmartThreadPool Library - Port 1 - NOT DONE
          //private void DigPort1_Listen_1(object state)
          //{



          //    // Create 8 diff threads, wait for them all to complete
          //    try
          //    {
          //        IWorkItemResult<string> wir0 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[0]);

          //        IWorkItemResult<string> wir1 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[1]);

          //        IWorkItemResult<string> wir2 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>(SnmpGet), this.DigitalPortsList[0].channels[2]);

          //        IWorkItemResult<string> wir3 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[3]);

          //        IWorkItemResult<string> wir4 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[4]);

          //        IWorkItemResult<string> wir5 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[5]);

          //        IWorkItemResult<string> wir6 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[6]);

          //        IWorkItemResult<string> wir7 = _DigPort1_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, Task<string>>(SnmpGet), this.DigitalPortsList[0].channels[7]);

          //        bool success = SmartThreadPool.WaitAll(new IWorkItemResult<string>[]
          //            { wir0, wir1, wir2, wir3, wir4, wir5, wir6, wir7 });

          //        if (success)
          //        {
          //            this.DigitalPortsList[0].channels[0].Value = Convert.ToInt32(wir0.Result);
          //            this.DigitalPortsList[0].channels[1].Value = Convert.ToInt32(wir1.Result);
          //            this.DigitalPortsList[0].channels[2].Value = Convert.ToInt32(wir2.Result);
          //            this.DigitalPortsList[0].channels[3].Value = Convert.ToInt32(wir3.Result);
          //            this.DigitalPortsList[0].channels[4].Value = Convert.ToInt32(wir4.Result);
          //            this.DigitalPortsList[0].channels[5].Value = Convert.ToInt32(wir5.Result);
          //            this.DigitalPortsList[0].channels[6].Value = Convert.ToInt32(wir6.Result);
          //            this.DigitalPortsList[0].channels[7].Value = Convert.ToInt32(wir7.Result);
          //        }

          //    }
          //    catch (Exception ex)
          //    {
          //        Debug.Print("DigPort1_Listen_1 Error: " + ex.Message + " Type: " + ex.GetType().ToString());
          //    }
          //}

          // Uses the SmartThreadPool Library - Port 2 - DONE
          //private void DigPort2_Listen_1(object state)
          //{

          //    // Create 8 diff threads, call snmpGET with each of them
          //    try
          //    {

          //        IWorkItemResult<string> wir0 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[0]);

          //        IWorkItemResult<string> wir1 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[1]);

          //        IWorkItemResult<string> wir2 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[2]);

          //        IWorkItemResult<string> wir3 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[3]);

          //        IWorkItemResult<string> wir4 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[4]);

          //        IWorkItemResult<string> wir5 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[5]);

          //        IWorkItemResult<string> wir6 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[6]);

          //        IWorkItemResult<string> wir7 = _DigPort2_Pool.QueueWorkItem(
          //                  new Amib.Threading.Func<Channel, string>(SnmpGet), this.DigitalPortsList[1].channels[7]);

          //        // wait for them all to complete
          //        bool success = SmartThreadPool.WaitAll(new IWorkItemResult<string>[] { wir0, wir1, wir2, wir3, wir4, wir5, wir6, wir7 });

          //        if (success)
          //        {
          //            List<int> lstValues = new List<int>();
          //            // copy the values returned into list
          //            lstValues.Add(Convert.ToInt32(wir0.Result));
          //            lstValues.Add(Convert.ToInt32(wir1.Result));
          //            lstValues.Add(Convert.ToInt32(wir2.Result));
          //            lstValues.Add(Convert.ToInt32(wir3.Result));
          //            lstValues.Add(Convert.ToInt32(wir4.Result));
          //            lstValues.Add(Convert.ToInt32(wir5.Result));
          //            lstValues.Add(Convert.ToInt32(wir6.Result));
          //            lstValues.Add(Convert.ToInt32(wir7.Result));

          //            // now call SmnpSET
          //            IWorkItemResult<string> wirA = _DigPort2_Pool.QueueWorkItem(
          //                           new Amib.Threading.Func<Channel, int, string>
          //                                (SnmpSet), this.DigitalPortsList[1].channels[0], lstValues.IndexOf(0));

          //            IWorkItemResult<string> wirB = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[1], lstValues.IndexOf(1));

          //            IWorkItemResult<string> wirC = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[2], lstValues.IndexOf(2));

          //            IWorkItemResult<string> wirD = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[3], lstValues.IndexOf(3));

          //            IWorkItemResult<string> wirE = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[4], lstValues.IndexOf(4));

          //            IWorkItemResult<string> wirF = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[5], lstValues.IndexOf(5));

          //            IWorkItemResult<string> wirG = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[6], lstValues.IndexOf(6));

          //            IWorkItemResult<string> wirH = _DigPort2_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.DigitalPortsList[1].channels[7], lstValues.IndexOf(7));
          //        }

          //        sw.Stop();
          //        Debug.Print("Time Elapsed in DigPort2_Listen_1: " + sw.Elapsed);
          //    }
          //    catch (Exception ex)
          //    {
          //        Debug.Print("DigPort2_Listen_1 Error: " + ex.Message + " Type: " + ex.GetType().ToString());
          //    }
          //}

          //Uses the SmartThreadPool Library - ADcPort 1 - DONE
          //private void AdcPort1_Listen_1(object state)
          //{
          //    // Have to fix Timeout errors

          //    try
          //    {
          //        IWorkItemResult<string> wir0 = _AlgPort1_Pool.QueueWorkItem(
          //             new Amib.Threading.Func<Channel, string>(SnmpGet),
          //             this.AnalogPortsList[0].channels[0]);

          //        IWorkItemResult<string> wir1 = _AlgPort1_Pool.QueueWorkItem(
          //             new Amib.Threading.Func<Channel, string>(SnmpGet),
          //             this.AnalogPortsList[0].channels[1]);

          //        IWorkItemResult<string> wir2 = _AlgPort1_Pool.QueueWorkItem(
          //             new Amib.Threading.Func<Channel, string>(SnmpGet),
          //             this.AnalogPortsList[0].channels[2]);

          //        IWorkItemResult<string> wir3 = _AlgPort1_Pool.QueueWorkItem(
          //             new Amib.Threading.Func<Channel, string>(SnmpGet),
          //             this.AnalogPortsList[0].channels[3]);

          //        bool success = SmartThreadPool.WaitAll(new IWorkItemResult<string>[] { wir0, wir1, wir2, wir3 });

          //        if (success)
          //        {
          //            int[] results = { Convert.ToInt32(wir0.Result), Convert.ToInt32(wir1.Result),
          //            Convert.ToInt32(wir2.Result), Convert.ToInt32(wir3.Result) };

          //            this.AnalogPortsList[0].channels[0].Value = results[0];
          //            this.AnalogPortsList[0].channels[1].Value = results[1];
          //            this.AnalogPortsList[0].channels[2].Value = results[2];
          //            this.AnalogPortsList[0].channels[3].Value = results[3];

          //            IWorkItemResult<string> wi0 = _AlgPort1_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.AnalogPortsList[0].channels[0], results[0]);

          //            IWorkItemResult<string> wi1 = _AlgPort1_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.AnalogPortsList[0].channels[1], results[1]);

          //            IWorkItemResult<string> wi2 = _AlgPort1_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.AnalogPortsList[0].channels[2], results[2]);

          //            IWorkItemResult<string> wi3 = _AlgPort1_Pool.QueueWorkItem(
          //                      new Amib.Threading.Func<Channel, int, string>
          //                           (SnmpSet), this.AnalogPortsList[0].channels[3], results[3]);
          //        }

          //        sw.Stop();
          //        Debug.Print("Time Elapsed in AdcPort1_Listen_1: " + sw.Elapsed);
          //    }
          //    catch (Exception ex)
          //    {
          //        Debug.Print("AdcPort1_Listen_1 Error: " + ex.Message + " Type: " + ex.GetType().ToString());
          //    }
          //}


          // Uses Task Parallel Library




          /// <summary>
          /// USes async Linq Selec good for async IO
          /// </summary>
          /// <param name="_chLst"></param>
          /// <returns></returns>
          //public async Task<IEnumerable<string>> GetValsReturnChannelList_Async(EthernetBoardPort _chLst, CancellationToken ct)
          //{
          //    List<Exception> Exceptions = null;
          //    IEnumerable<Channel>[] chValues = null;

          //    try
          //    {
          //            IEnumerable<string> _chValues = _chLst.channels.ToArray().;
          //                //(ch => ch.Value < 8);
          //                //_chLst.channels.FindAll(ch => await SnmpGet_Async(ch, ct));
          //                //TaskTools.RetryOnFault(() => SnmpGet_Async(ch, ct), 3));

          //           // chValues = await Task.WhenAll(_channels);
          //    }
          //    catch (Exception ex)
          //    {
          //        if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
          //            && !(ex is System.Net.Sockets.SocketException))
          //        {
          //            if (Exceptions == null)
          //                Exceptions = new List<Exception>();

          //            Exceptions.Add(ex);
          //        }
          //    }

          //    if (Exceptions != null)
          //        throw new AggregateException(Exceptions);

          //        return _channels;
          //}




          /// <summary>
          /// Task.StartNew regular foreach - retryOnFault
          /// </summary>
          //public async Task InitGetValues_AsyncRetry(CancellationToken ct)
          //{
          //     Task<string> getResult = null;
          //     //Task<string> setResult = null;

          //     Debug.Print("InitGetValues_AsyncRetry BEGIN--------");

          //     foreach (EthernetBoardPort pt in DigitalPortsList)
          //          foreach (Channel ch in pt.ChannelsList)
          //          {
          //               getResult = null;

          //               try
          //               {
          //                    // Works
          //                    getResult = await Task.Factory.StartNew(() => TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3));
          //                    //setResult = await getResult.ContinueWith(tc => TaskTools.RetryOnFault(() =>
          //                    //    SnmpSet_Async(ch, Convert.ToInt32(tc.Result)), 3), TaskContinuationOptions.OnlyOnRanToCompletion);

          //                    // works
          //                    //getResult = await Task.Factory.StartNew(() => SnmpGet_Async(ch));
          //                    //setResult = await getResult.ContinueWith(t1 => SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)),
          //                    //    TaskContinuationOptions.OnlyOnRanToCompletion);
          //                    if (getResult != null && getResult.IsCompleted)
          //                    {
          //                         Debug.Print("Digital Success! Channel ID: " + ch.Id + " Get Value: " + getResult.Result); // + " Set Value: " + setResult.Result);
          //                    }

          //               }
          //               catch (AggregateException ex)
          //               {
          //                    HandleAggregateErrors(ex);
          //               }
          //               catch (Exception ex)
          //               {
          //                    HandleErrors(ex);
          //               }
          //          }

          //     foreach (EthernetBoardPort pt in AnalogPortsList)
          //          foreach (Channel ch in pt.ChannelsList)
          //          {
          //               getResult = null;

          //               try
          //               {
          //                    // works
          //                    getResult = await Task.Factory.StartNew(() => TaskTools.RetryOnFault(() => SnmpGet_Async(ch.OID, ct), 3));
          //                    //setResult = await getResult.ContinueWith(tc => TaskTools.RetryOnFault(() =>
          //                    //    SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)), 3), TaskContinuationOptions.OnlyOnRanToCompletion);

          //                    // works
          //                    //getResult = await Task.Factory.StartNew(() => SnmpGet_Async(ch));
          //                    //setResult = await getResult.ContinueWith(t1 => SnmpSet_Async(ch, Convert.ToInt32(getResult.Result)),
          //                    //   TaskContinuationOptions.OnlyOnRanToCompletion);
          //                    if (getResult != null && getResult.IsCompleted)
          //                    {
          //                         Debug.Print("Analog Success! Channel ID: " + ch.Id + " Get Value: " + getResult.Result); // + " Set Value: " + setResult.Result);
          //                    }
          //               }
          //               catch (AggregateException ex)
          //               {
          //                    HandleAggregateErrors(ex);
          //               }
          //               catch (Exception ex)
          //               {
          //                    HandleErrors(ex);
          //               }
          //          }
          //}



     }
}


// set
//List<Variable> lstVar = new List<Variable>();
//Variable VarOid = new Variable(pOid, new Integer32(pNewChannelValue));
//lstVar.Add(VarOid);

// get
//result = Messenger.Get(VersionCode.V1,
//     EndPoint, new OctetString(COMMUNITY),
//     new List<Variable>( { new Variable(pOid) }, SNMP_GET_TIMEOUT));
