using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class Core
    {
        [ThreadStaticAttribute] private static string domain;
        private List<string> unvistedLinks;
        private Dictionary<string, InternalLink> visitedLinks;
        private List<PageData> pageDataList;
        private Output output;
        private Log newLog;
        private Meta meta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domain">Domain we want to crawl as a string</param>
        public Core(string domain, string customer_id)
        {
            meta = new Meta();
            Core.domain = domain;
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

            output = new Output(pageDataList, visitedLinks);

            return output;
        }

        public Output StartSpider(int custNo, string firstPage)
        {
            string url = Core.domain + firstPage;

            unvistedLinks.Add(url);
            crawlSite();

            output = new Output(pageDataList, visitedLinks, custNo);

            return output;
        }

        public Output StartSpider(int custNo, string firstPage, string customer_id)
        {
            string url = Core.domain + firstPage;

            unvistedLinks.Add(url);
            crawlSite();

            output = new Output(pageDataList, visitedLinks, customer_id, meta);

            return output;
        }

        /// <summary>
        /// Crawl the entire site
        /// </summary>
        private void crawlSite()
        {
            newLog.logLine("Starting ", Core.domain);
            GuiLogger.Log(String.Format("Crawling {0}", Core.domain));
            //As long as there is an unvisted link we continue
            while (unvistedLinks.Count > 0)
            {
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
                if(!relative)
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
                if(unvistedLinks.First<string>().Contains("id="))
                {
                    pageData = new PageData(unvistedLinks.First<string>(), "no id");
                }
                else
                    pageData = new PageData(unvistedLinks.First<string>());

                bool linkRelative = relativeCheck(pageData.Url);
                //bool responseOk = pageResponseCheck(currentPage);

                //Try to visit the page and extract links
                try
                {
                    if (isFile)
                    {
                        //if (currentPage.Contains(domain))
                        //  currentPage = currentPage.Replace(domain, "");

                        Uri fileUrl = new Uri(currentPage);
                        // Create a 'FileWebrequest' object with the specified Uri. 
                        HttpWebRequest myFileWebRequest = (HttpWebRequest)WebRequest.Create(fileUrl);
                        // Send the 'FileWebRequest' object and wait for response. 
                        HttpWebResponse myFileWebResponse = (HttpWebResponse)myFileWebRequest.GetResponse();

                        linkData = new InternalLink(pageData.Url, myFileWebResponse.StatusCode, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);

                        if (myFileWebResponse.ResponseUri.IsFile)
                            meta.addFileLink();
                        else if (!ownDomain && !myFileWebResponse.ResponseUri.IsFile)
                            meta.addExternalLink();
                        else
                            meta.addInternalLink();   

                        myFileWebResponse.Close();
                        
                        
                    }
                    else if (!ownDomain)
                    {
                        if (!currentPage.StartsWith("http"))
                           currentPage = @"http://" + currentPage;

                        WebRequest webRequest = WebRequest.Create(currentPage);
                        HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                        Stream stream = webResponse.GetResponseStream();

                        linkData = new InternalLink(pageData.Url, webResponse.StatusCode, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                        meta.addExternalLink();
                    }
                    else
                    {
                        WebRequest webRequest = WebRequest.Create(currentPage);
                        HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
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
                            webResponse.Close();
                        }
                        meta.addInternalLink();
                    }
                    //Add the finished page
                    if (!isFile && ownDomain)
                        pageDataList.Add(pageData);

                    //Remove the link since we are done with it
                    unvistedLinks.RemoveAt(0);
                }
                catch (WebException ex)
                {
                    if (ex.Message.Contains("(404) Not Found"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.NotFound, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains("(403) Forbidden"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.Forbidden, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains("(500) Internal Server Error"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.InternalServerError, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains("(401) Unauthorized"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.Unauthorized, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains("(502) Bad Gateway"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.BadGateway, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains("(410) Gone"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.Gone, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains("(412) Not logged in"))
                    {
                        linkData = new InternalLink(pageData.Url, HttpStatusCode.PreconditionFailed, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if(ex.Message.Contains(@"The underlying connection was closed"))
                    {
                        //usualy timeout because of low IIS treshold.
                        string customStatus = "connectionClosed";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if(ex.Message.Contains(@"The remote name could not be resolved"))
                    {
                        string customStatus = "remoteNameNotResolved";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains(@"Too many automatic redirections were attempted"))
                    {
                        string customStatus = "tooManyRedirects";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains(@"Unable to connect to the remote server"))
                    {
                        string customStatus = "unableToConnect";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains(@"Could not create SSL/TLS secure channel"))
                    {
                        string customStatus = "SSL";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains(@"Unable to connect to the remote server"))
                    {
                        string customStatus = "unableToConnect";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains(@"Invalid URI: The hostname could not be parsed"))
                    {
                        string customStatus = "invalidURI";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else if (ex.Message.Contains(@"The operation has timed out"))
                    {
                        string customStatus = "timeOut";
                        linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                        visitedLinks.Add(pageData.Url, linkData);
                    }
                    else
                        newLog.logLine("General webexception " + ex.Message, unvistedLinks.First<string>());

                    //Removed the faulty link, needs a better handling
                    meta.totalLinks++;
                    unvistedLinks.RemoveAt(0);
                }
                catch (UriFormatException ex)
                {
                    newLog.logLine("Uri exception: " + ex.GetType() + " " + ex.Message, unvistedLinks.First<string>());

                    string customStatus = "invalidUri";
                    linkData = new InternalLink(pageData.Url, customStatus, linkRelative);
                    visitedLinks.Add(pageData.Url, linkData);
                    unvistedLinks.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    newLog.logLine("General exception: " + ex.GetType() +" " + ex.Message, unvistedLinks.First<string>());
                    unvistedLinks.RemoveAt(0);
                }
            }
            buildLinkLists();

            GuiLogger.Log(String.Format("Finished crawling {0}", domain));
            newLog.logLine("Spider", "done");
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
            if (url.Contains("http:") || url.Contains("www.") || url.Contains("https:"))
                return false;
            else
                return true;
        }

        private bool isFileCheck(string url)
        {
            if (url.EndsWith(".aps") || url.EndsWith("/") || url.EndsWith(".aspx") || url.EndsWith(".htm") ||
                        url.EndsWith(".html") || url.EndsWith(".xhtml") || url.EndsWith(".jhtml"))
                return false;
            else
                return true;
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
            if (!url.StartsWith(domain) && (!url.StartsWith("http://" + domain) || !url.StartsWith("https://" + domain)))
                return false;
            else
                return true;
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

                /*if (!relativeCheck(hrefTags[i]))
                    if (!ownDomainCheck(hrefTags[i]))
                    {
                        hrefTags.Remove(hrefTags[i]);
                        if (i != 0)
                            i--;
                        else
                           resetI = true;
                    }*/
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