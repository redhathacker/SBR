using System;
using System.Threading;
using System.Diagnostics;



namespace snmpd
{    
     // ToDo: This getBoard, GetAllBoards, 
     
     // this class ensures that none my threads are running on the UI thread
     // Tasks are tricky and they dnt always do what you think think will
     // or run where you think ther will run
     public class EthernetBoardController : IDisposable 
     {
          Stopwatch sw = new Stopwatch();

          private CancellationTokenSource MainCancelTokenFactory = new CancellationTokenSource();
          private CancellationToken MainCancelToken;

          private EthernetBoard EthernetBoard_1 = new EthernetBoard(0, "192.168.1.40");
          private EthernetBoard EthernetBoard_2 = new EthernetBoard(1, "192.168.1.41");
          private EthernetBoard EthernetBoard_3 = new EthernetBoard(2, "192.168.1.42");
          private EthernetBoard EthernetBoard_4 = new EthernetBoard(3, "192.168.1.43");
          private EthernetBoard EthernetBoard_5 = new EthernetBoard(4, "192.168.1.44");
          private EthernetBoard EthernetBoard_6 = new EthernetBoard(5, "192.168.1.45");
          private EthernetBoard EthernetBoard_7 = new EthernetBoard(6, "192.168.1.46");
          private EthernetBoard EthernetBoard_8 = new EthernetBoard(7, "192.168.1.47");

          // the Ethernet boards
         public EthernetBoard GetBoard(int _boardId)
          {
               EthernetBoard brd = null;

               switch (_boardId)
                    {
                    case 0:
                         brd = EthernetBoard_1;
                         break;
                    case 1:
                         brd = EthernetBoard_2;
                         break;
                    case 2:
                         brd = EthernetBoard_3;
                         break;
                    case 3:
                         brd = EthernetBoard_4;
                         break;
                    case 4:
                         brd = EthernetBoard_5;
                         break;
                    case 5:
                         brd = EthernetBoard_6;
                         break;
                    case 6:
                         brd = EthernetBoard_7;
                         break;
                    case 7:
                         brd = EthernetBoard_8;
                         break;
                    default:
                         brd = null;
                         break;
               }

               return brd;
          }

          // cancel all tasks
          public void StopAllTasks()
          {
               MainCancelTokenFactory.Cancel();
          }

          /// <summary>
          /// Stops the Task on the MainForm
          /// </summary>
          public void StopMainTask()
          {

          }

          /// <summary>
          /// Stops all tasks on all running boards
          /// </summary>
          public void StopAllEthernetBoardTasks()
          {

          }

          /// <summary>
          /// This stops all tasks on the specified board
          /// </summary>
          public void StopEthernetBoard(int boardID)
          {
               MainCancelToken = MainCancelTokenFactory.Token;
          }

          void IDisposable.Dispose()
          {
               throw new NotImplementedException();
          }
     }
}