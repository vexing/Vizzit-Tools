using SpiderCore;
using SpiderCore.Db;
using SpiderCore.Db.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetDatabase();
        }

        private void GetDatabase()
        {
            List<string> dbList = new List<string>();
            List<Customer> customerList = new List<Customer>();

            try
            {
                Connector connector = new Connector();
                dbList = SelectQuery.GetCustomerDbList();
            }
            catch (Exception ex)
            {
                debugLabel.Text += ex.Message;
            }

            foreach (string customerDb in dbList)
            {
                Customer customer = new Customer();
                try
                {
                    customer.Domain = createFullurl(SelectQuery.GetDomain(customerDb)[0]);
                    customer.Id = SelectQuery.GetCustomerId(customerDb)[0];
                    customer.Startpage = SelectQuery.GetStartPage(customerDb)[0];
                    customerList.Add(customer);
                }
                catch (Exception ex)
                {
                    if(!ex.Message.Contains("Index was out of range"))
                        LogLine(customer.Id + " " + ex.Message);
                }
            }

            foreach(Customer c in customerList)
            {
                string url = c.Domain + c.Startpage;
                if(!isWorking(url))
                using (StreamWriter w = File.AppendText("notWorking.txt"))
                {
                    w.WriteLine("{0} {1} is not working", c.Id, url);
                }
            }
        }

        private bool isWorking(string currentUrl)
        {
            currentUrl = fixUri(currentUrl);

            try
            {
                Uri pageUri = new Uri(currentUrl, UriKind.Absolute);
                HttpWebResponse webResponse;
                WebRequest.DefaultWebProxy = null;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(pageUri);
                webRequest.Proxy = null;
                webRequest.AllowAutoRedirect = true;
                webRequest.Timeout = 5000;

                webResponse = (HttpWebResponse)webRequest.GetResponse();

                if (webResponse.StatusCode == HttpStatusCode.OK)
                    return true;
                else
                {
                    LogLine(webResponse.StatusCode + " " + pageUri);
                    return false;
                }
            }
            catch(Exception ex)
            {
                LogLine(ex.Message + " " + currentUrl);
                return false;
            }

        }

        private static void LogLine(string logLine)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log(logLine, w);
            }
        }

        private static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
        }

        private string fixUri(string uri)
        {
            uri = uri.Trim();
            uri = uri.Replace(" ", "%20");
            uri = uri.Replace("_", "%5F");
            return uri;
        }

        private string createFullurl(string url)
        {
            if (url.EndsWith(@"/"))
                url = url.Remove(url.Length - 1);

            if (!url.StartsWith(@"http://") || !url.StartsWith(@"https://"))
                url = @"http://" + url;
            return url;
        }
    }
}
