﻿using HtmlAgilityPack;
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
        private string domain;
        private List<string> unvistedLinks;
        private Dictionary<string, InternalLink> visitedLinks;
        private List<PageData> pageDataList;
        private Output output;
        private FileSend fileSend;
        public string errorMsg;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domain">Domain we want to crawl as a string</param>
        public Core(string domain)
        {
            this.domain = domain;
            unvistedLinks = new List<string>();
            visitedLinks = new Dictionary<string, InternalLink>();
            pageDataList = new List<PageData>();
        }

        /// <summary>
        /// Starts the spider with specifik startpage,
        /// important for non friendly customers
        /// </summary>
        public void StartSpider(string firstPage)
        {
            string url = this.domain + firstPage;
            
            unvistedLinks.Add(url);
            crawlSite();

            output = new Output(pageDataList, visitedLinks);
            var zippedJson = StringCompressor.CompressString(output.JsonString);

            fileSend = new FileSend(zippedJson);
            fileSend.SendFile();
        }

        /// <summary>
        /// Crawl the entire site
        /// </summary>
        private void crawlSite()
        {
            //TODO: Need to handle external links

            //As long as there is an unvisted link we continue
            while (unvistedLinks.Count > 0)
            {
                InternalLink linkData;
                //Check if it is a mail link
                bool isMail = mailLinkCheck(unvistedLinks.First<string>());

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


                if (!ownDomain || unvistedLinks.First<string>().Contains("javascript") || unvistedLinks.First<string>().StartsWith("#"))
                {
                    unvistedLinks.RemoveAt(0);
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

                var pageData = new PageData(unvistedLinks.First<string>());
                bool linkRelative = relativeCheck(pageData.Url);

                try
                {
                    doc = page.Load(currentPage);
                    linkData = new InternalLink(pageData.Url, page.StatusCode, linkRelative);
                    visitedLinks.Add(pageData.Url, linkData);

                    //Extract data from currentPage if status 200
                    if (page.StatusCode == HttpStatusCode.OK && ownDomain)
                    {
                        var extractor = new Extractor(doc);
                        Dictionary<string, string> meta = extractor.GetMeta();
                        string title = extractor.GetTitle();
                        List<string> hrefTags = extractor.ExtractAllAHrefTags();

                        foreach (string hrefTag in hrefTags)
                        {
                            if (!pageData.LinkDataList.Contains(hrefTag))
                            {
                                if (mailLinkCheck(hrefTag))
                                    pageData.insertMailLink(hrefTag);
                                else
                                    pageData.insertLink(hrefTag);
                            }
                        }   
                        //Insert the whole range of new links
                        insertNewLinks(hrefTags);
                    }

                    //Add the finished page
                    pageDataList.Add(pageData);
                    //Remove the link since we are done with it
                    unvistedLinks.RemoveAt(0);
                }
                catch(Exception ex)
                {
                    //Removed the faulty link, needs a better handling
                    unvistedLinks.RemoveAt(0);
                    errorMsg = ex.Message;
                }
            }
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

        /// <summary>
        /// Inserts new unvisted links
        /// </summary>
        /// <param name="hrefTags">List of new links we want to check</param>
        private void insertNewLinks(List<string> hrefTags)
        {
            for(int i = 0;i < hrefTags.Count;i++)
            {
                if (hrefTags[i].Contains("javascript") || hrefTags[i].StartsWith("#") || hrefTags[i].StartsWith("mailto"))
                {
                    hrefTags.Remove(hrefTags[i]);
                    i--;
                }

                if (!relativeCheck(hrefTags[i]))
                    if (!ownDomainCheck(hrefTags[i]))
                    {
                        hrefTags.Remove(hrefTags[i]);
                        i--;
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