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
    public static class GuiLogger
    {
        private static List<string> log = new List<string>();

        public static event EventHandler LogAdded;

        public static void Log(string message)
        {
            log.Add(message);

            if (LogAdded != null)
                LogAdded(null, EventArgs.Empty);
        }

        public static string GetLastLog()
        {
            if (log.Count > 0)
                return log[log.Count - 1];
            else
                return null;
        }
    }
}
