using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class Log
    {
        private string customerId;
        private string logName;
        private string logFile;

        public Log(string customerId)
        {
            this.customerId = customerId;
            createLogFile();
        }
        
        private void createLogFile()
        {
            DateTime startTime = DateTime.Now;

            logName = startTime.ToString("yyMMddHHmms") + customerId + ".log";
            string currentDir = Directory.GetCurrentDirectory();
            string logDir = currentDir + "/log";

            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(@logDir);

                string customerLogDir = logDir + "/" + customerId;

                if (!Directory.Exists(customerLogDir))
                    Directory.CreateDirectory(@customerLogDir);

                logFile = customerLogDir + "/" + logName;
                File.Create(@logFile).Close();
            }
            catch (Exception ex)
            {
                GuiLogger.Log(ex.Message);
            }
        }

        public void logLine(string errorMsg, string url)
        {
            try
            {
                string logMessage = errorMsg + " " + url;

                using (StreamWriter w = File.AppendText(logFile))
                {
                    writeLine(logMessage, w);
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                GuiLogger.Log(ex.Message);
            }
            
        }

        private void writeLine(string logMessage, TextWriter w)
        {
            try
            {
                w.WriteLine("{0} {1}: {2}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString(), logMessage);
            }
            catch (Exception ex)
            {
                GuiLogger.Log(ex.Message);
            }
        }
    }
}
