using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    /// <summary>
    /// Logs to a textfile
    /// </summary>
    public class Log
    {
        private string customerId;
        private string logName;
        private string logFile;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="customerId"></param>
        public Log(string customerId)
        {
            this.customerId = customerId;
            createLogFile();
        }
        
        /// <summary>
        /// Creates the logfile for the specific customer
        /// </summary>
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
                string em = ex.Message;
            }
        }

        /// <summary>
        /// Should be overloaded but isn't, I use this function for all log inputs. It's messy but doesn't really matter
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <param name="url"></param>
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
                string em = ex.Message;
            }
            
        }

        /// <summary>
        /// Appends the finished logline to the textfile
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="w"></param>
        private void writeLine(string logMessage, TextWriter w)
        {
            try
            {
                w.WriteLine("{0} {1}: {2}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString(), logMessage);
            }
            catch (Exception ex)
            {
                string em = ex.Message;
            }
        }
    }
}
