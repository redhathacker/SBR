using System;
using System.Collections.Generic;
using System.Text;
using Lextm.SharpSnmpLib.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace snmpd
{
    public enum BoardState { DISCONNECTED, CONNECTED }
    public enum EthernetBoardPortNo { DIGITAL_PORT_1, DIGITAL_PORT_2, ADC_PORT_1 }
    public enum PortMode { INPUT, OUTPUT }
    public enum ChannelState { OFF, ON }

    public static class Util
    {
        public const int LISTEN_PORT = 161;
        public const int RECEIVE_PORT = 162;
        public const int DIG_CHAN_START_NO = 1;
        public const int DIG_CHAN_PER_PORT = 8;
        public const int ANLG_CHAN_START_NO = 1;
        public const int ANLG_CHAN_PER_PORT = 4;
        public const int SNMP_GET_TIMEOUT = 1;
        public const int SNMP_SET_TIMEOUT = 1;
        public const int NUMBER_OF_DIGITAL_PORTS = 2;
        public const int NUMBER_OF_ANALOG_PORTS = 1;
        public const string COMMUNITY = "private";
        public const string SnmpGetPath = @"C:\SNMPTools\SnmpGet\SnmpGet";
        public const string ENTERPRISE_OID = "19865";

        public const string Board1_IP = "162.198.1.40";
        public const string Board2_IP = "162.198.1.41";
        public const string Board3_IP = "162.198.1.42";
        public const string Board4_IP = "162.198.1.43";
        public const string Board5_IP = "162.198.1.44";
        public const string Board6_IP = "162.198.1.45";
        public const string Board7_IP = "162.198.1.46";
        public const string Board8_IP = "162.198.1.47";


        public static bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                    return true;
            }

            return false;
        }

        public static bool KillProcess(string name)
        {
            Process[] asio = Process.GetProcessesByName(name);

            if (asio.Length > 0)
            {
                asio[0].Kill();
                return true;
            }
            return false;
        }

        public static bool IsRealError(Exception ex)
        {
            bool validError = false;

            if (!(ex is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                    && !(ex is System.Net.Sockets.SocketException)
                    && !(ex is OperationCanceledException)
                    && !(ex is TaskCanceledException))
            {
                validError = true;
            }
            return validError;
        }

        public static bool IsRealError(AggregateException ex)
        {
            bool validError = false;

            foreach (Exception e in ex.InnerExceptions)
            {
                if (!(e is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                    && !(e is System.Net.Sockets.SocketException)
                    && !(e is OperationCanceledException)
                    && !(e is TaskCanceledException))
                {
                    validError = true;
                }
            }

            return validError;
        }

        /// <summary>
        /// Converts a byte to a binary number sequence. 
        /// </summary>
        /// <param name="iVal">Value to convert</param>
        /// <param name="iLen">Length of string</param>
        /// <returns>string</returns>
        public static string FromIntToBinaryString(int iVal, int iLen)
        {
            return (iLen > 1 ? FromIntToBinaryString(iVal >> 1, iLen - 1) : null) + "01"[iVal & 1];
        }

        /// <summary>
        /// 0 - 255
        /// </summary>
        /// <param name="pChVals">Byte value of all channels.</param>
        /// <param name="pNoOfChars">Number of channels in port.</param>
        /// <returns></returns>
        public static char[] FromIntToChars(int pChVals, int pNoOfChars)
        {
            string sVals = FromIntToBinaryString(pChVals, pNoOfChars);

            return sVals.ToCharArray();
        }


        /// <summary>
        /// Runs SnmpGet.exe and returns the value of all 8 channels in an array. 
        /// ex. SnmpGet.exe -q -r:192.168.1.41 -t:30 -c:"private" -o:.1.3.6.1.4.1.19865.1.2.2.1.0
        /// </summary>
        /// <param name="pIp">IP of device to read</param>
        /// <param name="pOid">OID of value to read</param>
        /// <param name="pTimeout">Timeout in seconds</param>
        /// <returns></returns>
        public static async Task<int> SnmpGetDigitalValues(string pIp, string pOid, string pTimeout)
        {
            string output = String.Empty;
            int AllChannelsByteValue = 0;

            string[] argValues = new string[] { pIp, pTimeout, Util.COMMUNITY, pOid };

            string argString = String.Format(" -q -r:{0} -t:{1} -c:{2} -o:{3}", argValues);

            // Declare SnmpGet.exe and its arguments
            ProcessStartInfo psi = new ProcessStartInfo(Util.SnmpGetPath, argString);
            Process GetProc = new Process();

            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            // RunGet snmpGet.exe
            GetProc.StartInfo = psi;
            GetProc.Start();

            // Read the value SnmpGet.exe returned 
            // output is 0-255, the byte value of all 8 channels
            output = await GetProc.StandardOutput.ReadToEndAsync();

            // make sure we got a number
            if (!int.TryParse(output.Trim(), out AllChannelsByteValue))
            {
                Debug.Print("SnmpGetDigitalValues: " + output + " IP: " + pIp + " OID: " + pOid);
                AllChannelsByteValue = -1;
            }

            return AllChannelsByteValue;
        }

        /// <summary>
        /// snmpget (snmpget -c:"private" -d -t:1 192.168.1.41 1.3.6.1.4.1.19865.1.2.3.33.0)
        /// </summary>
        /// <param name="pIp"></param>
        /// <param name="pOid"></param>
        /// <param name="pTimeout"></param>
        /// <returns>int</returns>
        public static async Task<int> SnmpGetAnalogValue(string pIp, string pOid, string pTimeout)
        {
            StringBuilder sb = new StringBuilder();
            int ChanValue = 0;
            string args;

            args = sb.Append(" -q -t").Append(pTimeout).Append(" -c\"private\" ")
                .Append(pIp).Append(" ").AppendLine(pOid).ToString();

            Debug.Print(args);

            ProcessStartInfo psi = new ProcessStartInfo(Util.SnmpGetPath, args);
            Process GetProc = new Process();

            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            GetProc.StartInfo = psi;
            GetProc.Start();

            while (!GetProc.StandardOutput.EndOfStream)
            {
                string output = await GetProc.StandardOutput.ReadToEndAsync();

                if (output.Length > 0)
                {
                    if (!int.TryParse(output, out ChanValue))
                    {
                        ChanValue = 0;
                    }
                }
            }

            return ChanValue;
        }


        public static void SetupTrapsAndShit()
        {
            //  cfgTrapServerIP - .1.3.6.1.4.1.19865.1.1.18
            //  DAEnetIP2
            // 
        }

    } // End Class

    public sealed class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public Exception InnerException { get; private set; }
        public string Source { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public DateTime DateOccurred { get; private set; }

        public ErrorEventArgs(Exception ex)
        {
            Exception = ex;
            InnerException = ex.InnerException;
            Source = ex.Source;
            Message = ex.Message;
            StackTrace = ex.StackTrace;
            DateOccurred = DateTime.Now;
        }

        public override string ToString()
        {
            return "Error: " + Message + "Source:" + Source;
        }
    }

    public sealed class DigitalInputEventArgs : EventArgs
    {//int boardID, int _ioPort, Channel channel
        public int BoardID { get; private set; }
        public string BoardIP { get; private set; }
        public int PortNo { get; private set; }
        public Channel Chan { get; private set; }

        public string Message { get; set; }

        public DigitalInputEventArgs(int pBoardID, int pPortNo, Channel pCh)
        {
            BoardIP = "192.168.1." + pBoardID.ToString();
            BoardID = pBoardID;
            PortNo = pPortNo;
            Chan = pCh;
        }
    }

    public sealed class AnalogInputEventArgs : EventArgs
    {//int boardID, int _ioPort, Channel channel
        public int BoardID { get; private set; }
        public int PortNo { get; private set; }
        public Channel Chan { get; private set; }

        public AnalogInputEventArgs(int pBoardID, int pPortNo, Channel pCh)
        {
            BoardID = pBoardID;
            PortNo = pPortNo;
            Chan = pCh;
        }
    }

    public sealed class BoardInitEventArgs : EventArgs
    {//int boardID, int _ioPort, Channel channel
        public int BoardID { get; private set; }
        public List<EthernetBoardPort> DigitalPorts { get; private set; }
        public List<EthernetBoardPort> AnalogPorts { get; private set; }

        public BoardInitEventArgs(int pBoardID, List<EthernetBoardPort> pIoPorts, List<EthernetBoardPort> pAdcPorts = null)
        {
            BoardID = pBoardID;
            DigitalPorts = pIoPorts;
            AnalogPorts = pAdcPorts;
        }
    }

    public sealed class AsyncErrorEventArgs : EventArgs
    {
        private AggregateException AggException;
        public List<ErrorEventArgs> Exceptions { get; private set; }

        public AsyncErrorEventArgs(AggregateException ex)
        {
            AggException = ex;
            Exceptions = new List<ErrorEventArgs>();

            foreach (Exception e in ex.InnerExceptions)
            {
                Exceptions.Add(new ErrorEventArgs(e));
            }
        }

        public override string ToString()
        {
            return AggException.Flatten().ToString();
        }
    }

    //for use with channel callbacks
    public class RequestState
    {
        // This class stores the State of the request.
        // IE the parameters it needs
        public StringBuilder Request_Data;
        public ResponseMessage Respose_Message { get; set; }
        public SetRequestMessage Set_Request_Message { get; set; }
        public GetRequestMessage Get_Request_Message { get; set; }
        public ISnmpMessage Get_Response_Message { get; set; }
        public IAsyncResult Set_Response_Message { get; set; }

        public RequestState()
        {
            Request_Data = new StringBuilder("");
            Respose_Message = null;
            Set_Request_Message = null;
            Get_Request_Message = null;
            Set_Response_Message = null;
            Get_Response_Message = null;
        }
    }



}
