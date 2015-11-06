using SpiderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;
using Spider;
using FileHandler;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace SpiderCore
{
    //TODO: Move to FileHandler?
    public class Output
    {
        private List<PageData> pageDataList;
        private Meta metaData;
        public string jsonFileName;
        bool disposed = false;
        SafeHandle handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        public Output(List<PageData> pageDataList)
        {
            this.pageDataList = pageDataList;
            createJson(1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        public Output(List<PageData> pageDataList, int custNo)
        {            
            this.pageDataList = pageDataList;
            createJson(custNo);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        /// <param name="customer_id">vizzit customer_id</param>
        /// <param name="metaData">meta data object</param>
        public Output(ref List<PageData> pageDataList, string customer_id, Meta metaData, bool sendFile)
        {
            this.metaData = metaData;

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            DateTime date = DateTime.Now;

            try
            {
                string path = String.Format(@"files/{0}/", customer_id);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                jsonFileName = String.Format(@"files/{0}/{0}_{1}.json", customer_id, date.ToString("yyMMdd_HHmmss"));
                using (StreamWriter sw = new StreamWriter(@jsonFileName))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, pageDataList);
                }

                string metaFileName = String.Format(@"files/{0}/{0}_{1}.meta", customer_id, date.ToString("yyMMdd_HHmmss"));
                using (StreamWriter sw = new StreamWriter(@metaFileName))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, metaData);
                }

                string zipFile = StringCompressor.CreateZipFile(jsonFileName, metaFileName);
                if (sendFile)
                    FileHandler.FileSend.SendFile(zipFile, customer_id, date.ToString("yyyy-MM-dd"));

            }
            catch(Exception ex)
            {
                string em = ex.Message;
            }
            pageDataList.Clear();

            Thread.Sleep(4000);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        public Output(List<PageData> pageDataList, string customer_id, bool sendFile)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            DateTime date = DateTime.Now;

            jsonFileName = String.Format(@"{0}_{1}.json", customer_id, date.ToString("yyMMdd_HHmmss"));
            using (StreamWriter sw = new StreamWriter(jsonFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pageDataList);
            }

            string zipFile = StringCompressor.CreateZipFile(jsonFileName);
            if(sendFile)
                FileHandler.FileSend.SendFile(zipFile, customer_id, date.ToString("yyyy-MM-dd"));
        }

        /// <summary>
        /// Used for testing for memory issues, should probably be removed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                handle.Dispose();

            disposed = true;
        }

        /// <summary>
        /// Create the actual JSON with Newtonsoft
        /// </summary>
        private void createJson(string customer_id)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            jsonFileName = String.Format(@"{0}.json", customer_id);
            using (StreamWriter sw = new StreamWriter(jsonFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pageDataList);
            }

            StringCompressor.CreateZipFile(jsonFileName);
        }

        /// <summary>
        /// Create the actual JSON with Newtonsoft
        /// </summary>
        private void createJson(int custNo)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            jsonFileName = String.Format(@"Spiderdata{0}.json", custNo);
            using (StreamWriter sw = new StreamWriter(jsonFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pageDataList);
            }

            StringCompressor.CreateZipFile(jsonFileName);
        }

        #region GetSet
        #endregion
    }
}
