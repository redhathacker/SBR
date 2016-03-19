using System;
using System.Threading.Tasks;

namespace snmpd
{
     public class TaskTools
     {
          public static async Task<T> RetryOnFault<T>(Func<Task<T>> function, int maxTries)
          {
               for (int index = 0; index < maxTries; index++)
               {
                    try
                    {
                         return await function().ConfigureAwait(false);
                    }
                    catch
                    {
                         if (index == maxTries - 1) throw;
                    }
               }

               return default(T);
          }

          public static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
          {
               AsyncErrorEventArgs Errors = new AsyncErrorEventArgs(e.Exception);
               e.SetObserved();
          }
     }
}
