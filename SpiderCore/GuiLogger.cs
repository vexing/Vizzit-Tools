using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    /// <summary>
    /// Tool for writing to the gui from Spider
    /// </summary>
    public static class GuiLogger
    {
        private static List<string> log = new List<string>();
        private static Dictionary<DateTime, string> runningCustomers = new Dictionary<DateTime, string>();
        private static Dictionary<DateTime, string> finishedCustomers = new Dictionary<DateTime, string>();

        public static event EventHandler LogAdded;
        public static event EventHandler RunningCustomerChanged;

        /// <summary>
        /// Input into the log
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            log.Add(message);

            if (LogAdded != null)
                LogAdded(null, EventArgs.Empty);
        }

        /// <summary>
        /// Used by GUI event to fetch latest log msg
        /// </summary>
        /// <returns></returns>
        public static string GetLastLog()
        {
            if (log.Count > 0)
                return log[log.Count - 1];
            else
                return null;
        }

        /// <summary>
        /// Adds a customer to runningCustomers 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="customer"></param>
        public static void addRunningCustomer(DateTime startTime, string customer)
        {
            runningCustomers.Add(startTime, customer);

            if (RunningCustomerChanged != null)
                RunningCustomerChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the whole list
        /// </summary>
        /// <returns></returns>
        public static Dictionary<DateTime, string> getRunningCustomersList()
        {
            return runningCustomers;
        }
    }
}
