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

        /// <summary>
        /// Sends the specified (zipped) file via HTTP.
        /// </summary>
        /// <param name="target">The upload target, i.e. the URL of the server</param>
        /// <param name="custID">The customer ID</param>
        /// <param name="files">The path to the file to be sent</param>
        public static void SendByHttp(string target, string custID, string zippedFile)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.Method = "POST";

            string sBoundary = "---------------------------$$VizzitBoundary_1.0.0.0$$";
            request.ContentType = "multipart/form-data; boundary=" + sBoundary;
            request.Timeout = Timeout.Infinite;

            // open a write stream to the specified Target using "POST"
            StreamWriter st = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);

            FileInfo f = new FileInfo(zippedFile);
            FileStream fStream = f.OpenRead();

            Byte[] fileData = new Byte[f.Length];
            fStream.Read(fileData, 0, (int)f.Length);
            fStream.Close();

            // build formpost-body
            AddContentPart(st, sBoundary, "UploadFile", zippedFile, "application/octet-stream", fileData);
            AddContentPart(st, sBoundary, "CustomerId", null, null, custID);

            st.Write("--" + sBoundary + "--\r\n");
            st.Flush();
            st.Close();
        }

        private static void AddContentPart(StreamWriter stream, string boundary, string name, string filename, string contenttype, byte[] data)
        {
            stream.Write("--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + name + "\"; ");

            if (null != filename)
                stream.Write(" filename=\"" + filename + "\"");

            stream.Write("\r\nContent-Type: " + contenttype + "\r\n\r\n");
            stream.Flush();

            stream.BaseStream.Write(data, 0, data.Length);
            stream.Flush();

            stream.Write("\r\n");
            stream.Flush();
        }

        private static void AddContentPart(StreamWriter stream, string boundary, string name, string filename, string contenttype, string data)
        {
            stream.Write("--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + name + "\"");

            if (null != filename)
                stream.Write(" ;filename=\"" + filename + "\"");

            if (null != contenttype)
                stream.Write("\r\nContent-Type: " + contenttype);

            stream.Write("\r\n\r\n");
            stream.Write(data);
            stream.Write("\r\n");
            stream.Flush();
        }
    }
}
