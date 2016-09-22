using System;
using System.Threading.Tasks;

namespace snmpd
{
    /// <summary>
    /// Tool that help with Tasks
    /// </summary>
     public class TaskTools
     {
        /// <summary>
        /// Retry a method if it doesnt work
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <param name="maxTries"></param>
        /// <returns></returns>
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

        /// <summary>
        /// A method used for unhandled CLR errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
          public static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
          {
               AsyncErrorEventArgs Errors = new AsyncErrorEventArgs(e.Exception);
               e.SetObserved();
          }
     }
}
