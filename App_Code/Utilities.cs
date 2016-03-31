using System;
using System.Collections.Generic;
using System.Text;
using Lextm.SharpSnmpLib.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace snmpd
{
     public enum BoardState { DISCONNECTED, CONNECTED }
     public enum EthernetBoardPortNo { DIGITAL_PORT_1, DIGITAL_PORT_2, ADC_PORT_1 }
     public enum PortMode { INPUT, OUTPUT }
     public enum ChannelState { OFF, ON }

     public static class Utilities
     {
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

    } // End Class

     public sealed class ErrorEventArgs : EventArgs
     {
          public Exception Exception { get; set; }
          public Exception InnerException { get; set; }
          public string Source { get; set; }
          public string Message { get; set; }
          public string StackTrace { get; set; }
          public DateTime DateOccurred { get; set; }

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

     public sealed class AsyncErrorEventArgs : EventArgs
     {
          private AggregateException AggException;
          public List<ErrorEventArgs> Exceptions { get; set; }

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
