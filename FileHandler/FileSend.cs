using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileHandler
{
    public static class FileSend
    {
        /// <summary>
        /// Sends the file to Vizzit
        /// </summary>
        /// <param name="zippedJson">Filepath</param>
        /// <returns></returns>
        public static void SendFile(string zippedJson)
        {
            using (WebClient client = new WebClient())
            {
                client.UploadFile("http://www.vizzit.se/files/uploadSpider.php", zippedJson);
            }
        }

        public static void SendFile(string fileName, string customerId, string date)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("FileName", fileName);
            postParameters.Add("CustomerId", customerId);
            postParameters.Add("Date", date);
            postParameters.Add("UploadFile", new FormUpload.FileParameter(data, fileName, "application/msword"));

            // Create request and receive response
            string postURL = "http://www.vizzit.se/files/uploadSpider.php";
            string userAgent = "VizzitSpider";
            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, userAgent, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();
        }
    }
}
