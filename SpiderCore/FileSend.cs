using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class FileSend
    {
        private string zippedJson;

        public FileSend(string zippedJson)
        {
            this.zippedJson = zippedJson;
        }

        public void SendFile()
        {
            HttpWebRequest req = (HttpWebRequest)
            WebRequest.Create("http://www.vizzit.se/files/uploadSpider.php");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            string postData = "zipFile=" + zippedJson;
            req.ContentLength = postData.Length;

            StreamWriter stOut = new
            StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII);
            stOut.Write(postData);
            stOut.Close();
        }
    }
}
