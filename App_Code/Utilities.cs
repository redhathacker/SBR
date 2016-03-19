using System;
using System.Collections.Generic;
using System.Text;
using Lextm.SharpSnmpLib.Messaging;

namespace snmpd
{
     public enum BoardState { DISCONNECTED, CONNECTED }
     public enum EthernetBoardPortNo { DIGITAL_PORT_1, DIGITAL_PORT_2, ADC_PORT_1 }
     public enum PortMode { INPUT, OUTPUT }
     public enum ChannelState { OFF, ON }

     public class Utilities
     {

     }

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
