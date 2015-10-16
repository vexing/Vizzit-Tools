using HtmlAgilityPack;
using SpiderCore.Db;
using SpiderCore.Db.Queries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SpiderCore
{
    public class Core
    {
        [ThreadStaticAttribute] private static string domain;
        const int BUFFER_SIZE = 1024;
        const int DefaultTimeout = 10000; // 2 minutes timeout
        private List<string> unvistedLinks;
        private List<string> dailyCheckPages;
        private Dictionary<string, InternalLink> visitedLinks;
        private List<PageData> pageDataList;
        private Output output;
        private Log newLog;
        private Meta meta;
        private List<string> structure;
        private string database;
        private bool structureUpdateDone;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domain">Domain we want to crawl as a string</param>
        /// <param name="customer_id"></param>
        public Core(string domain, string customer_id, string database)
        {
            structureUpdateDone = false;
            meta = new Meta(customer_id);
            Core.domain = domain;
            this.database = database;
            unvistedLinks = new List<string>();
            visitedLinks = new Dictionary<string, InternalLink>();
            pageDataList = new List<PageData>();
            newLog = new Log(customer_id);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="customer_id"></param>
        public Core(string customer_id)
        {
            meta = new Meta(customer_id);
            unvistedLinks = new List<string>();
            visitedLinks = new Dictionary<string, InternalLink>();
            pageDataList = new List<PageData>();
            newLog = new Log(customer_id);
        }

        /// <summary>
        /// Starts the spider with specifik startpage,
        /// important for non friendly customers
        /// </summary>
        public Output StartSpider(string firstPage)
        {
            string url = Core.domain + firstPage;
            
            unvistedLinks.Add(url);
            crawlSite();

            output = new Output(pageDataList);

            return output;
        }

        public Output StartSpider(int custNo, string firstPage)
        {
            string url = Core.domain + firstPage;

            unvistedLinks.Add(url);
            crawlSite();

            output = new Output(pageDataList, custNo);

            return output;
        }

        public Output StartSpider(List<string> urlsToParse)
        {
            unvistedLinks = urlsToParse;
            crawlSiteDaily();

            output = new Output(pageDataList);

            return output;
        }

        public Output StartSpider(int custNo, string firstPage, string customer_id, bool sendFile, bool dailyCheck)
        {
            meta.setDaily(dailyCheck);
            if (!dailyCheck)
            {
                string url = Core.domain + firstPage;

                unvistedLinks.Add(url);
                crawlSite();
            }
            else
                crawlSiteDaily();

            output = new Output(ref pageDataList, customer_id, meta, sendFile);

            return output;
        }

        /// <summary>
        /// Crawl the entire site
        /// </summary>
        private void crawlSite()
        {
            Process currentProc = Process.GetCurrentProcess();
            GuiLogger.Log("Using " + currentProc.PrivateMemorySize64.ToString() + " at begining of crawlSite");

            newLog.logLine("Starting ", Core.domain);
            GuiLogger.Log(String.Format("Crawling {0}", Core.domain));
            //As long as there is an unvisted link we continue
            while (unvistedLinks.Count > 0)
            {
                DateTime startTime = DateTime.Now;
                InternalLink linkData;
                //Check if it is a mail link
                bool isMail = mailLinkCheck(unvistedLinks.First<string>());
                bool isFile = isFileCheck(unvistedLinks.First<string>());

                if (isMail)
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone)
                        compareStructure();
                    continue;
                }

                bool ownDomain = true;

                //Visit the first link in the list
                bool relative = relativeCheck(unvistedLinks.First<string>());
                if(!relative)
                    ownDomain = ownDomainCheck(unvistedLinks.First<string>());

                if (unvistedLinks.First<string>().Contains("javascript") ||
                    unvistedLinks.First<string>().StartsWith("#"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone)
                        compareStructure();
                    continue;
                }
                //Remove shit for SCB
                if (unvistedLinks.First<string>().Contains("type=terms") || unvistedLinks.First<string>().Contains("query=lua") ||
                    unvistedLinks.First<string>().Contains("publobjid="))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone)
                        compareStructure();
                    continue;
                }
                //Remove googletranslate, umea for example have this
                if (unvistedLinks.First<string>().StartsWith(@"//translate.google.com/translate?sl=sv"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone)
                        compareStructure();
                    continue;
                }

                if (!unvistedLinks.First<string>().StartsWith("/") &&
                    !unvistedLinks.First<string>().StartsWith("http"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone)
                        compareStructure();
                    continue;
                }

                string currentPage;
                var page = new HtmlWeb();

                page.UserAgent = "VizzitSpider";
                var doc = new HtmlDocument();
               
                //Add domain if the link is relative
                if (relative && ownDomain)
                    currentPage = domain + unvistedLinks.First<string>();
                else
                    currentPage = unvistedLinks.First<string>();

                PageData pageData;
                //If it isn't friendly we need to extract the id
                if(unvistedLinks.First<string>().Contains("id="))
                    pageData = new PageData(unvistedLinks.First<string>(), "no id");
                else
                    pageData = new PageData(unvistedLinks.First<string>());

                bool linkRelative = relativeCheck(pageData.Url);
                //bool responseOk = pageResponseCheck(currentPage);
                HttpWebResponse webResponse;
                currentPage = fixUri(currentPage);

                //Folkuni needs this or they get alot of null references
                if (currentPage.EndsWith(@".pdf/") || currentPage.EndsWith(@".doc/") || currentPage.EndsWith(@".docx/") || currentPage.EndsWith(@".bmp/"))
                {
                    isFile = true;
                    currentPage = currentPage.Remove(currentPage.Length - 1);
                }

                //Try to visit the page and extract links
                try
                {                    
                    Uri pageUri = new Uri(currentPage, UriKind.Absolute);
                    pageUri = escapeUri(pageUri);

                    WebRequest.DefaultWebProxy = null;

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(pageUri);
                    webRequest.Proxy = null;
                    webRequest.AllowAutoRedirect = true;
                    webRequest.MaximumAutomaticRedirections = 10;
                    if(ownDomain)
                        webRequest.Timeout = 10000;
                    else
                        webRequest.Timeout = 5000;

                    using (webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        if (isFile)
                        {
                            linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                            visitedLinks.Add(pageData.Url, linkData);

                            if (isFile)
                                meta.addFileLink();
                            else if (!ownDomain && !webResponse.ResponseUri.IsFile)
                                meta.addExternalLink();
                            else
                                meta.addInternalLink();
                        }
                        else if (!ownDomain)
                        {

                            linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                            visitedLinks.Add(pageData.Url, linkData);
                            meta.addExternalLink();
                        }
                        else
                        {
                            Stream stream = webResponse.GetResponseStream();
                            string responseString = ((TextReader)new StreamReader(stream)).ReadToEnd();

                            doc.LoadHtml(responseString);
                            linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                            visitedLinks.Add(pageData.Url, linkData);


                            //Extract data from currentPage if status 200
                            if (webResponse.StatusCode == HttpStatusCode.OK && ownDomain)
                            {
                                var extractor = new Extractor(doc);
                                Dictionary<string, string> metaList = extractor.GetMeta();
                                string title = extractor.GetTitle();
                                List<string> hrefTags = extractor.ExtractAllAHrefTags();

                                //Iterate the links
                                foreach (string hrefTag in hrefTags)
                                {
                                    //Insert the link to the correct List
                                    if (!pageData.checkIfLinkExistInLinkString(hrefTag))
                                    {
                                        if (mailLinkCheck(hrefTag))
                                            pageData.insertMailLink(hrefTag);
                                        else
                                            if (!hrefTag.StartsWith("#"))
                                                pageData.insertLink(hrefTag);
                                    }
                                }
                                //Insert the whole range of new links
                                insertNewLinks(hrefTags);
                            }
                            meta.addInternalLink();
                            stream.Close();
                        }
                    }
                    //Add the finished page
                    if (!isFile && ownDomain)
                        pageDataList.Add(pageData);

                    //Remove the link since we are done with it
                    unvistedLinks.RemoveAt(0);
                    webResponse.Close();
                }
                catch (WebException ex)
                {
                    handleWebException(ex, pageData.Url, linkRelative, ref visitedLinks);

                    //Removed the faulty link, needs a better handling
                    meta.totalLinks++;
                    unvistedLinks.RemoveAt(0);
                }
                catch (UriFormatException ex)
                {
                    newLog.logLine("Uri exception: " + ex.GetType() + " " + ex.StackTrace + " " + ex.Message, currentPage);

                    string customStatus = "invalidUri";
                    linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                    visitedLinks.Add(pageData.Url, linkData);
                    unvistedLinks.RemoveAt(0);
                }
                catch(NullReferenceException ex)
                {
                    newLog.logLine("NullReferenceException " + ex.Message, currentPage);
                    unvistedLinks.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    newLog.logLine("General exception: " + ex.GetType() + " " + ex.Message, currentPage);
                    unvistedLinks.RemoveAt(0);
                }

                TimeSpan parseTime = DateTime.Now.Subtract(startTime);
                if (parseTime > new TimeSpan(0, 0, 10))
                    newLog.logLine("Parse took " + parseTime.Seconds +" seconds ", pageData.Url);

                if(unvistedLinks.Count <= 0 && !structureUpdateDone && visitedLinks.Count > 2)
                    compareStructure();

                if (unvistedLinks.Count == visitedLinks.Count)
                {
                    currentProc = Process.GetCurrentProcess();
                    GuiLogger.Log("Using " + currentProc.PrivateMemorySize64.ToString() + " about halfway");
                }
            }
            buildLinkLists();
            visitedLinks.Clear();

            currentProc = Process.GetCurrentProcess();
            GuiLogger.Log("Using " + currentProc.PrivateMemorySize64.ToString() + " after crawling is done");
            Thread.Sleep(5000);

            GuiLogger.Log(String.Format("Finished crawling {0}", domain));
            newLog.logLine("Spider", "done");
        }

        private void compareStructure()
        {
            try
            {
                structure = SelectQuery.GetStructure(database, DateTime.Now.ToString("yyyy-MM-dd"));
            }
            catch(Exception ex)
            {
                GuiLogger.Log(ex.Message);
            }

            unvistedLinks.AddRange(structure.Except(visitedLinks.Keys));
            meta.setFromStructure(unvistedLinks.Count);
            structureUpdateDone = true;
            structure.Clear();
        }

        private Uri escapeUri(Uri pageUri)
        {
            if (pageUri.Host.Contains("å") || pageUri.Host.Contains("ä") || pageUri.Host.Contains("ö") ||
                pageUri.Host.Contains("Å") || pageUri.Host.Contains("Ä") || pageUri.Host.Contains("Ö"))
            {
                IdnMapping idn = new IdnMapping();
                string newUri = idn.GetAscii(pageUri.ToString());

                return new Uri(newUri, UriKind.Absolute);
            }
            else
                return pageUri;
        }

        private void crawlSiteDaily()
        {
            try
            {
                Connector connector = new Connector();
                unvistedLinks = SelectQuery.GetPagesWithBrokenLinkgs(database);

                dailyCheckPages = new List<string>(unvistedLinks);
            }
            catch (Exception ex)
            {
                GuiLogger.Log(ex.Message);
            }

            newLog.logLine("Starting daily ", Core.domain);
            GuiLogger.Log(String.Format("Crawling {0}", Core.domain));
            //As long as there is an unvisted link we continue
            while (unvistedLinks.Count > 0)
            {
                DateTime startTime = DateTime.Now;
                InternalLink linkData;
                //Check if it is a mail link
                bool isMail = mailLinkCheck(unvistedLinks.First<string>());
                bool isFile = isFileCheck(unvistedLinks.First<string>());

                if (isMail)
                {
                    unvistedLinks.RemoveAt(0);
                    continue;
                }

                bool ownDomain = true;

                //Visit the first link in the list
                bool relative = relativeCheck(unvistedLinks.First<string>());
                if (!relative)
                    ownDomain = ownDomainCheck(unvistedLinks.First<string>());

                if (unvistedLinks.First<string>().Contains("javascript") ||
                    unvistedLinks.First<string>().StartsWith("#"))
                {
                    unvistedLinks.RemoveAt(0);
                    continue;
                }
                //Remove shit for SCB
                if (unvistedLinks.First<string>().Contains("type=terms") || unvistedLinks.First<string>().Contains("query=lua") ||
                    unvistedLinks.First<string>().Contains("publobjid="))
                {
                    unvistedLinks.RemoveAt(0);
                    continue;
                }
                //Remove googletranslate, umea for example have this
                if (unvistedLinks.First<string>().StartsWith(@"//translate.google.com/translate?sl=sv"))
                {
                    unvistedLinks.RemoveAt(0);
                    continue;
                }

                if (!unvistedLinks.First<string>().StartsWith("/") &&
                    !unvistedLinks.First<string>().StartsWith("http"))
                {
                    unvistedLinks.RemoveAt(0);
                    continue;
                }

                string currentPage;
                var page = new HtmlWeb();
                page.PreRequest = delegate(HttpWebRequest webRequest)
                {
                    webRequest.AllowAutoRedirect = true;
                    webRequest.Timeout = 5000;
                    return true;
                };

                page.UserAgent = "VizzitSpider";
                var doc = new HtmlDocument();

                //Add domain if the link is relative
                if (relative && ownDomain)
                    currentPage = domain + unvistedLinks.First<string>();
                else
                    currentPage = unvistedLinks.First<string>();

                PageData pageData;
                //If it isn't friendly we need to extract the id
                if (unvistedLinks.First<string>().Contains("id="))
                    pageData = new PageData(unvistedLinks.First<string>(), "no id");
                else
                    pageData = new PageData(unvistedLinks.First<string>());

                bool linkRelative = relativeCheck(pageData.Url);
                //bool responseOk = pageResponseCheck(currentPage);
                HttpWebResponse webResponse;
                currentPage = fixUri(currentPage);

                //Folkuni needs this or they get alot of null references
                if (currentPage.EndsWith(@".pdf/") || currentPage.EndsWith(@".doc/") || currentPage.EndsWith(@".docx/") || currentPage.EndsWith(@".bmp/"))
                {
                    isFile = true;
                    currentPage = currentPage.Remove(currentPage.Length - 1);
                }

                //Try to visit the page and extract links
                try
                {
                    Uri pageUri = new Uri(currentPage, UriKind.Absolute);
                    pageUri = escapeUri(pageUri);

                    WebRequest.DefaultWebProxy = null;

                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(pageUri);
                    webRequest.Proxy = null;
                    webRequest.AllowAutoRedirect = true;
                    if (ownDomain)
                        webRequest.Timeout = 10000;
                    else
                        webRequest.Timeout = 5000;

                    using (webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        if (isFile)
                        {
                            linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                            visitedLinks.Add(pageData.Url, linkData);

                            if (isFile)
                                meta.addFileLink();
                            else if (!ownDomain && !webResponse.ResponseUri.IsFile)
                                meta.addExternalLink();
                            else
                                meta.addInternalLink();
                        }
                        else if (!ownDomain)
                        {

                            linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                            visitedLinks.Add(pageData.Url, linkData);
                            meta.addExternalLink();
                        }
                        else
                        {
                            Stream stream = webResponse.GetResponseStream();
                            string responseString = ((TextReader)new StreamReader(stream)).ReadToEnd();

                            doc.LoadHtml(responseString);
                            linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                            visitedLinks.Add(pageData.Url, linkData);


                            //Extract data from currentPage if status 200
                            if (webResponse.StatusCode == HttpStatusCode.OK && ownDomain)
                            {
                                var extractor = new Extractor(doc);
                                Dictionary<string, string> metaList = extractor.GetMeta();
                                string title = extractor.GetTitle();
                                List<string> hrefTags = extractor.ExtractAllAHrefTags();

                                //Iterate the links
                                foreach (string hrefTag in hrefTags)
                                {
                                    //Insert the link to the correct List
                                    if (!pageData.checkIfLinkExistInLinkString(hrefTag))
                                    {
                                        if (mailLinkCheck(hrefTag))
                                            pageData.insertMailLink(hrefTag);
                                        else
                                            if (!hrefTag.StartsWith("#"))
                                                pageData.insertLink(hrefTag);
                                    }
                                }
                                if(dailyCheckPages.Contains(pageData.Url))
                                    insertNewLinks(hrefTags);
                            }
                            meta.addInternalLink();
                            stream.Close();
                        }
                    }
                    //Add the finished page
                    if (!isFile && ownDomain)
                        pageDataList.Add(pageData);

                    //Remove the link since we are done with it
                    unvistedLinks.RemoveAt(0);
                    webResponse.Close();
                }
                catch (WebException ex)
                {
                    handleWebException(ex, pageData.Url, linkRelative, ref visitedLinks);

                    //Removed the faulty link, needs a better handling
                    meta.totalLinks++;
                    unvistedLinks.RemoveAt(0);
                }
                catch (UriFormatException ex)
                {
                    newLog.logLine("Uri exception: " + ex.GetType() + " " + ex.StackTrace + " " + ex.Message, currentPage);

                    string customStatus = "invalidUri";
                    linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                    visitedLinks.Add(pageData.Url, linkData);
                    unvistedLinks.RemoveAt(0);
                }
                catch (NullReferenceException ex)
                {
                    newLog.logLine("NullReferenceException " + ex.Message, currentPage);
                    unvistedLinks.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    newLog.logLine("General exception: " + ex.GetType() + " " + ex.Message, currentPage);
                    unvistedLinks.RemoveAt(0);
                }

                TimeSpan parseTime = DateTime.Now.Subtract(startTime);
                if (parseTime > new TimeSpan(0, 0, 10))
                    newLog.logLine("Parse took " + parseTime.Seconds + " seconds ", pageData.Url);
            }
            buildLinkLists();
            visitedLinks.Clear();

            GuiLogger.Log(String.Format("Finished crawling {0}", domain));
            newLog.logLine("Spider", "done");
        }

        private void handleWebException(Exception ex, string url, bool linkRelative, ref Dictionary<string, InternalLink> visitedLinks)
        {
            InternalLink linkData;

            if (ex.Message.Contains("(404) Not Found"))
            {                        
                linkData = new InternalLink(url, HttpStatusCode.NotFound, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(403) Forbidden"))
            {
                linkData = new InternalLink(url, HttpStatusCode.Forbidden, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(500) Internal Server Error"))
            {
                linkData = new InternalLink(url, HttpStatusCode.InternalServerError, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(401) Unauthorized"))
            {
                linkData = new InternalLink(url, HttpStatusCode.Unauthorized, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(502) Bad Gateway"))
            {
                linkData = new InternalLink(url, HttpStatusCode.BadGateway, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(503) Server Unavailable"))
            {
                linkData = new InternalLink(url, HttpStatusCode.ServiceUnavailable, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(504) Gateway Timeout"))
            {
                linkData = new InternalLink(url, HttpStatusCode.GatewayTimeout, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(410) Gone"))
            {
                linkData = new InternalLink(url, HttpStatusCode.Gone, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(412) Not logged in"))
            {
                linkData = new InternalLink(url, HttpStatusCode.PreconditionFailed, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(416) Requested Range Not Satisfiable"))
            {
                linkData = new InternalLink(url, HttpStatusCode.RequestedRangeNotSatisfiable, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains("(400) Bad Request"))
            {
                linkData = new InternalLink(url, HttpStatusCode.BadRequest, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if(ex.Message.Contains(@"The underlying connection was closed"))
            {
                //usualy timeout because of low IIS treshold.
                string customStatus = "connectionClosed";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if(ex.Message.Contains(@"The remote name could not be resolved"))
            {
                string customStatus = "remoteNameNotResolved";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"Too many automatic redirections were attempted"))
            {
                string customStatus = "tooManyRedirects";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"Unable to connect to the remote server"))
            {
                string customStatus = "unableToConnect";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"Could not create SSL/TLS secure channel"))
            {
                string customStatus = "SSL";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"Invalid URI"))
            {
                string customStatus = "invalidURI";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"The operation has timed out"))
            {
                string customStatus = "timeOut";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"The connection was closed unexpectedly"))
            {
                string customStatus = "connectionClosedUnexpectedly";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"Cannot handle redirect from HTTP/HTTPS protocols to other dissimilar ones"))
            {
                string customStatus = "redirectFromProtocols";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else if (ex.Message.Contains(@"The server committed a protocol violation"))
            {
                string customStatus = "serverProtocolViolation";
                linkData = new InternalLink(url, customStatus, linkRelative);
                visitedLinks.Add(url, linkData);
            }
            else
                newLog.logLine("General webexception " + ex.Message, unvistedLinks.First<string>());
        }

        private string fixUri(string uri)
        {
            uri = uri.Trim();
            uri = HttpUtility.UrlDecode(uri);
            uri = uri.Replace(" ", "+");
            return uri;
        }

        /// <summary>
        /// Builds the links list in each PageData object
        /// </summary>
        /// <param name="url"></param>
        private void buildLinkLists()
        {
            foreach(PageData pd in pageDataList)
                 pd.insertLinksToLinks(visitedLinks);
        }       

        /// <summary>
        /// Simple check to check if the link is absolute or relative
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool relativeCheck(string url)
        {
            if (url.StartsWith("http") || url.StartsWith("www."))
                return false;
            else
                return true;
        }

        private bool isFileCheck(string url)
        {
            if (url.EndsWith(".aps") || url.EndsWith("/") || url.EndsWith(".aspx") || url.EndsWith(".htm") ||
                        url.EndsWith(".html") || url.EndsWith(".xhtml") || url.EndsWith(".jhtml"))
                return false;
            else if (url.Length >= 5 && url.Substring(url.Length - 5).Contains("."))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if the link is a mailto: link
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool mailLinkCheck(string url)
        {
            if (url.Contains("mailto"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if the link is within the own domain
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ownDomainCheck(string url)
        {
            if (url.StartsWith(domain) || url.StartsWith(@"http://" + domain) || url.StartsWith(@"https://" + domain) || url.StartsWith(@"\"))
                return true;
            else
                return false;
        }

        private bool pageResponseCheck(string url)
        {
            WebRequest webRequest = WebRequest.Create(url);

            // Set the 'Timeout' property in Milliseconds.
            webRequest.Timeout = 10000;

            try
            {
                // This request will throw a WebException if it reaches the timeout limit before it is able to fetch the resource.
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                Stream stream = webResponse.GetResponseStream();
                string responseString = ((TextReader)new StreamReader(stream)).ReadToEnd();
                webResponse.Close();
                return true;
            }
            catch (WebException ex)
            {
                if(ex.Message.Contains("Timeout"))
                {
                    newLog.logLine(ex.Message + " from pageResponseCheck", unvistedLinks.First<string>());
                    return false;
                }
                else
                    return true;
            }
        }

        /// <summary>
        /// Inserts new unvisted links
        /// </summary>
        /// <param name="hrefTags">List of new links we want to check</param>
        private void insertNewLinks(List<string> hrefTags)
        {
            bool resetI = false;

            for(int i = 0;i < hrefTags.Count;i++)
            {
                if(resetI)
                {
                    resetI = false;
                    i = 0;
                }

                if (hrefTags[i].Contains("javascript") || hrefTags[i].StartsWith("#") || hrefTags[i].StartsWith("mailto"))
                {
                    hrefTags.Remove(hrefTags[i]);
                    if(i != 0)
                        i--;
                    else
                        resetI = true;
                }
            }

            var modifiedList = hrefTags;

            //Check if the link allready is in the list
            IEnumerable<string> differenceQuery = modifiedList.Except(unvistedLinks);
            //Check if the link allready was visited
            differenceQuery = differenceQuery.Except(visitedLinks.Keys);

            unvistedLinks.AddRange(differenceQuery);
        }
    }
}