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

namespace SpiderCore
{
    //TODO: Move to external namcespace
    public class Output
    {
        private List<PageData> pageDataList;
        private Meta metaData;
        private Dictionary<string, InternalLink> visitedLinks;
        public string jsonFileName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        /// <param name="visitedLinks">list of all InternalLinks</param>
        public Output(List<PageData> pageDataList, Dictionary<string, InternalLink> visitedLinks)
        {
            this.pageDataList = pageDataList;
            this.visitedLinks = visitedLinks;
            createJson(1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        /// <param name="visitedLinks">list of all InternalLinks</param>
        public Output(List<PageData> pageDataList, Dictionary<string, InternalLink> visitedLinks, int custNo)
        {            
            this.pageDataList = pageDataList;
            this.visitedLinks = visitedLinks;
            createJson(custNo);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        /// <param name="visitedLinks">list of all InternalLinks</param>
        /// <param name="customer_id">vizzit customer_id</param>
        /// <param name="metaData">meta data object</param>
        public Output(List<PageData> pageDataList, Dictionary<string, InternalLink> visitedLinks, string customer_id, Meta metaData)
        {
            this.metaData = metaData;
            this.visitedLinks = visitedLinks;

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

            string metaFileName = String.Format(@"{0}_{1}.meta", customer_id, date.ToString("yyMMdd_HHmmss"));
            using (StreamWriter sw = new StreamWriter(metaFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, metaData);
            }

            string zipFile = StringCompressor.CreateZipFile(jsonFileName, metaFileName);
            FileHandler.FileSend.SendFile(zipFile, customer_id);            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        /// <param name="visitedLinks">list of all InternalLinks</param>
        public Output(List<PageData> pageDataList, Dictionary<string, InternalLink> visitedLinks, string customer_id)
        {
            this.visitedLinks = visitedLinks;

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
            FileHandler.FileSend.SendFile(zipFile, customer_id);
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
