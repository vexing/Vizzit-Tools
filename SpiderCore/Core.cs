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
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SpiderCore
{
    public class Core
    {
        //Hard threadsafing to make sure this doesn't get shared between threads(probably not needed)
        [ThreadStaticAttribute] private static string domain;
        const int DefaultTimeout = 10000; // 2 minutes timeout
        private List<string> unvistedLinks;
        private List<string> dailyCheckPages;
        private Dictionary<string, InternalLink> visitedLinks;
        private Dictionary<string, int> visitedUriCount;
        private List<PageData> pageDataList;
        private Output output;
        private Log newLog;
        private Meta meta;
        private List<string> structure;
        private string database;
        private bool structureUpdateDone;
        private bool dailyCheck;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domain">Domain we want to crawl as a string</param>
        /// <param name="customer_id"></param>
        /// <param name="database"></param>
        public Core(string domain, string customer_id, string database)
        {
            structureUpdateDone = false;
            meta = new Meta(customer_id);
            Core.domain = domain;
            this.database = database;
            unvistedLinks = new List<string>();
            visitedLinks = new Dictionary<string, InternalLink>();
            visitedUriCount = new Dictionary<string, int>();
            pageDataList = new List<PageData>();
            newLog = new Log(customer_id);
        }

        /// <summary>
        /// Starts the spider, and returns an Output object ready to be zipped and sent to Vizzit
        /// </summary>
        /// <param name="firstPage"></param>
        /// <param name="customer_id"></param>
        /// <param name="sendFile">Send files to vizzit, true or false</param>
        /// <param name="dailyCheck">Daily check, true or false</param>
        public Output StartSpider(int custNo, string firstPage, string customer_id, bool sendFile, bool dailyCheck)
        {
            this.dailyCheck = dailyCheck;
            meta.setDaily(dailyCheck);
            if (!dailyCheck)
            {
                string url = Core.domain + firstPage;

                unvistedLinks.Add(url);
                crawlSite();
            }
            else
            {
                dailyCheckPages = SelectQueryModel.GetPagesWithBrokenLinkgs(database);
                unvistedLinks.AddRange(dailyCheckPages);
                if (unvistedLinks.Count > 0)
                    crawlSite();
            }

            output = new Output(ref pageDataList, customer_id, meta, sendFile);

            return output;
        }

        /// <summary>
        /// Crawl the entire site, very long. Should probably be broken into smaller functions
        /// </summary>
        private void crawlSite()
        {
            GuiLogger.addRunningCustomer(DateTime.Now, Core.domain);
            //Dirty fix for region skåne....We need that firstPage back:/
            string firstPage = unvistedLinks.First<string>().Remove(0, domain.Count());

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
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }

                bool ownDomain = true;

                //Visit the first link in the list
                bool relative = relativeCheck(unvistedLinks.First<string>());

                //Skane needs to be in here because their firstpage is used as a part of the domain to check if its ownDomain
                if (!relative || domain.StartsWith(@"http://www.skane.se"))
                    ownDomain = ownDomainCheck(unvistedLinks.First<string>(), firstPage);

                //Remove links with javascript and # in them
                if (unvistedLinks.First<string>().Contains("javascript") ||
                    unvistedLinks.First<string>().StartsWith("#"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }
                //Remove shit for SCB, the crawl goes bananas on theese pages
                if (unvistedLinks.First<string>().Contains("type=terms") || unvistedLinks.First<string>().Contains("query=lua") ||
                    unvistedLinks.First<string>().Contains("publobjid="))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }
                //Remove googletranslate, umea for example have this
                if (unvistedLinks.First<string>().StartsWith(@"//translate.google.com/translate?sl=sv"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }

                //Remove siteseeker pages, might be other variations of siteseeker pages that I haven't found yet
                if (unvistedLinks.First<string>().Contains("h.siteseeker"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }

                //MAke sure we don't visit a link that won't work anyway
                if (!unvistedLinks.First<string>().StartsWith("/") &&
                    !unvistedLinks.First<string>().StartsWith("http"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
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

                //Tillväxtverket mfl jsessionId fix
                if (currentPage.Contains("jsessionid"))
                    currentPage = removeJsession(currentPage);

                PageData pageData = new PageData(unvistedLinks.First<string>());

                //This is left in because we might want to use it at some point.
                bool linkRelative = relativeCheck(pageData.Url);
                //Only used for testing
                //bool responseOk = pageResponseCheck(currentPage);

                HttpWebResponse webResponse;
                currentPage = fixUri(currentPage);

                //Folkuni needs this or they get alot of null references(I think there might be more sites that has this problem)
                if (currentPage.EndsWith(@".pdf/") || currentPage.EndsWith(@".doc/") || currentPage.EndsWith(@".docx/") || currentPage.EndsWith(@".bmp/"))
                {
                    isFile = true;
                    currentPage = currentPage.Remove(currentPage.Length - 1);
                }

                //For now, takes forever...Not needed with the page block function?
                if (domain.Contains("www.sll.se") && currentPage.ToLower().Contains("/kalender/"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();

                    continue;
                }

                //Remove emailencoded pages, alot of customers use this function to make it harder for bots to parse emails, we get problems when crawling thoose pages
                if(currentPage.Contains("EmailEncoderEmbed"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }

                //Slu had problems with this link, not sure what it is
                if (domain.Contains("www.slu.se") && currentPage.ToLower().Contains("app.readspeaker.com"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }

                //Ekobrott phonenumbers screw things up
                if (domain.Contains("www.ekobrottsmyndigheten.se") && currentPage.Contains("tel:"))
                {
                    unvistedLinks.RemoveAt(0);
                    if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                        compareStructure();
                    continue;
                }

                //Try to visit the page and extract links
                try
                {
                    Uri pageUri = new Uri(currentPage, UriKind.Absolute);

                    //IDN encoding for åöä in domain, the if check is needed otherwise we destroy urls without åäö
                    if (pageUri.Host.Contains("å") || pageUri.Host.Contains("ä") || pageUri.Host.Contains("ö") ||
                        pageUri.Host.Contains("Å") || pageUri.Host.Contains("Ä") || pageUri.Host.Contains("Ö"))
                    {
                        pageUri = idnUriEncode(pageUri);
                    }

                    //Seems to be needed to bypass timeouts because of a bug in .net
                    WebRequest.DefaultWebProxy = null;

                    //We need to use AbsoluteUri to get the encoded url
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(pageUri.AbsoluteUri);
                    //Same as above, only one should be needed?
                    webRequest.Proxy = null;
                    //This makes the spider able to follow redirects, we set the limit to 10 redirects before we throw an exception
                    webRequest.AllowAutoRedirect = true;
                    webRequest.MaximumAutomaticRedirections = 10;

                    //We have different timeout times depending if its the own domain or not
                    if (ownDomain)
                        webRequest.Timeout = 10000;
                    else
                        webRequest.Timeout = 5000;

                    //This part keeps track if we have visited the url allready, not considering queries and if we have more then 100 visits we will skip this page.
                    if (visitedUriCount.ContainsKey(pageUri.AbsolutePath))
                        visitedUriCount[pageUri.AbsolutePath]++;
                    else
                        visitedUriCount.Add(pageUri.AbsolutePath, 1);

                    if (visitedUriCount[pageUri.AbsolutePath] >= 100)
                    {
                        newLog.logLine(pageUri.AbsoluteUri, "reached 100 visits. Blocking the page");
                        unvistedLinks.RemoveAt(0);
                        if (unvistedLinks.Count <= 0 && !structureUpdateDone && !dailyCheck)
                            compareStructure();
                        continue;
                    }

                    //Now its time to make the request to the url
                    using (webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        //We have to do it alittle bit different depending on if its a file, external or internal link or if it is a dailyheck. It is very important that the order isn't
                        //changed. This should definently be rewriten because its some copy paste in here wich is really bad...
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
                            using (Stream stream = webResponse.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(stream))
                                {
                                    string responseString = ((TextReader)sr).ReadToEnd();

                                    doc.LoadHtml(responseString);

                                    linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                                    visitedLinks.Add(pageData.Url, linkData);
                                    sr.Close();
                                }

                                //Extract data from currentPage if status 200
                                if (webResponse.StatusCode == HttpStatusCode.OK && ownDomain)
                                {
                                    var extractor = new Extractor(doc);
                                    Dictionary<string, string> metaList = extractor.GetMeta();
                                    string title = extractor.GetTitle();
                                    List<string> hrefTags = extractor.ExtractAllAHrefTags();
                                    List<string> linkList = new List<string>();

                                    //Iterate the links
                                    foreach (string hrefTag in hrefTags)
                                    {
                                        //Make sure relative links is saved as absolute so nothing bad happens
                                        string linkUrl = hrefTag;
                                        if (relativeCheck(hrefTag))
                                        {
                                            if (linkUrl.StartsWith("/"))
                                                linkUrl = domain + hrefTag;
                                            else
                                            {
                                                int index = currentPage.IndexOf("?");
                                                string currentPageNoQuery;

                                                if (index > 0)
                                                    currentPageNoQuery = currentPage.Substring(0, index);
                                                else
                                                    currentPageNoQuery = currentPage;

                                                if (currentPageNoQuery.Contains(domain.Replace(@"http://", "").Replace(@"https://", "")))
                                                    linkUrl = currentPageNoQuery + hrefTag;
                                                else
                                                    linkUrl = domain + currentPageNoQuery + hrefTag;
                                            }
                                        }

                                        //Insert the link to the correct List
                                        if (!pageData.checkIfLinkExistInLinkString(linkUrl))
                                        {
                                            if (mailLinkCheck(hrefTag))
                                                pageData.insertMailLink(hrefTag);
                                            else
                                                if (!hrefTag.StartsWith("#"))
                                                {
                                                    pageData.insertLink(linkUrl);
                                                    linkList.Add(linkUrl);
                                                }
                                        }
                                    }
                                    //We have to check if the page we are visiting are a page from the dailycheck List.
                                    //If it is we add the links we find on this page.
                                    if ((dailyCheck && dailyCheckPages.Contains(pageData.Url)) || !dailyCheck)
                                        insertNewLinks(ref linkList);
                                }
                                meta.addInternalLink();
                                stream.Close();
                            }
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

                if (unvistedLinks.Count <= 0 && !structureUpdateDone && visitedLinks.Count > 2  && !dailyCheck)
                    compareStructure();
            }

            buildLinkLists();
            
            newLog.logLine("Spider", "done");
        }

        /// <summary>
        /// Get the complete structure from the database and compares it to the visited pages list, adds the difference to the unvisitedLinks
        /// </summary>
        private void compareStructure()
        {
            structure = SelectQuery.GetStructure(database, DateTime.Now.ToString("yyyy-MM-dd"), domain);

            //There might be a better way to do this but I haven't found it
            unvistedLinks.AddRange(structure.Except(visitedLinks.Keys));
            meta.setFromStructure(unvistedLinks.Count);
            structureUpdateDone = true;
        }

        /// <summary>
        /// This is only needed for domains with åäö.
        /// </summary>
        /// <param name="pageUri"></param>
        /// <returns></returns>
        private Uri idnUriEncode(Uri pageUri)
        {
            IdnMapping idn = new IdnMapping();
            string host = pageUri.Scheme + @"://" +  idn.GetAscii(pageUri.Host);
            string fullUrl = host + pageUri.PathAndQuery;

            return new Uri(fullUrl, UriKind.Absolute);
        }        

        /// <summary>
        /// Should probably be a switch but I didn't realize how many errors we would pick up when i wrote this
        /// Might also want to rewrite the whole function since the comapre is kinda messy
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="url"></param>
        /// <param name="linkRelative"></param>
        /// <param name="visitedLinks"></param>
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

        /// <summary>
        /// Very hacky. It is kinda needed for now. Especially for sll...There is probably some encoding out there that can do it better. Just haven't looked for it.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private string fixUri(string uri)
        {
            uri = uri.Trim();
            uri = uri.Replace("&amp;", "&");
            if(domain.Contains("www.sll.se"))
                uri = uri.Replace("amp;", "");
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

        /// <summary>
        /// Check if it is a file. There is a variable in WepRequest that will do this. I have seen it fail tho so this might still be needed.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
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
        /// Another hacky function, alot of SiteVision customers have this crap in their url.
        /// This function will remove it!
        /// </summary>
        /// <param name="url"></param>§
        /// <returns></returns>
        private string removeJsession(string url)
        {
            string removeString = "";
            bool started = false;

            foreach(char c in url)
            {
                if (c.ToString().Equals(";") && !started)
                {
                    started = true;
                    removeString += c.ToString();
                }
                else if (c.ToString().Equals("?"))
                    break;
                else if (started)
                    removeString += c.ToString();
            }

            if (String.IsNullOrEmpty(removeString))
                return url;
            else
                return url.Replace(removeString, "");
        }

        /// <summary>
        /// Check if the link is within the own domain
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ownDomainCheck(string url, string firstPage)
        {
            if ((domain.StartsWith(@"http://www.skane.se") || domain.StartsWith("www.skane.se")) && url.Contains(firstPage))
                return true;
            else if ((url.StartsWith(domain) || url.StartsWith(@"http://" + domain) || url.StartsWith(@"https://" + domain) || url.StartsWith(@"\")) && 
                (!url.StartsWith(@"http://www.skane.se") && !url.StartsWith("www.skane.se")))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if the url contains the domain.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ownDomainCheck(string url)
        {
            string pureDomain = domain.Replace(@"http://", "").Replace(@"https://", "");
            if (url.Contains(pureDomain))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Response check to see if the page is responding. This is only for testing, should not be used otherwise.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
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
        private void insertNewLinks(ref List<string> hrefTags)
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

            unvistedLinks.AddRange(new List<string>(hrefTags.Except(unvistedLinks).Except(visitedLinks.Keys)));
        }
    }
}