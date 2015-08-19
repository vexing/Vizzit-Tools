using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class PageData
    {
        private string url;
        private string id;
        private List<string> linkStrings;
        private List<string> mailLinks;
        private List<string> notVisitedLinks;

        private List<InternalLink> links;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">Url to the page</param>
        public PageData(string url)
        {
            this.url = url;
            this.id = "0";
            linkStrings = new List<string>();
            mailLinks = new List<string>();
            notVisitedLinks = new List<string>();
            links = new List<InternalLink>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">Url to the page</param>
        /// <param name="id">Id for the page</param>
        public PageData(string url, string id)
        {
            this.url = url;
            this.id = id;
        }

        /// <summary>
        /// Inserts a link into the list
        /// </summary>
        /// <param name="linkData">The linkData for the link to insert</param>
        public void insertLink(string linkData)
        {
            linkStrings.Add(linkData);
        }

        /// <summary>
        /// Inserts a link to the links list
        /// </summary>
        /// <param name="il">InternalLink object that is to be inserted</param>
        public void insertToLinks(InternalLink il)
        {
            links.Add(il);
        }

        /// <summary>
        /// Inserts a maillink into the list
        /// </summary>
        /// <param name="linkData">The linkData for the link to insert</param>
        public void insertMailLink(string linkData)
        {
            mailLinks.Add(linkData);
        }

        /// <summary>
        /// Inserts a link to the list for links that haven't been visited
        /// </summary>
        /// <param name="link"></param>
        public void insertToNotVisitedLinks(string link)
        {
            notVisitedLinks.Add(link);
        }

        /// <summary>
        /// Clears the LinkStrings list
        /// </summary>
        public void clearLinkStrings()
        {
            linkStrings.Clear();
        }

        /// <summary>
        /// Checks if link allready exist in linkStrings
        /// </summary>
        /// <param name="link"></param>
        /// <returns>true or false</returns>
        public bool checkIfLinkExistInLinkString(string link)
        {
            return linkStrings.Contains(link);
        }

        /// <summary>
        /// Insert all that to the links list. 
        /// </summary>
        /// <param name="visitedLinks">Dictionary with a string as key and a list of InternalLinks</param>
        public void insertLinksToLinks(Dictionary<string, InternalLink> visitedLinks)
        {
            foreach (string link in linkStrings)
            {
                if (visitedLinks.ContainsKey(link))
                {
                    insertToLinks(visitedLinks[link]);
                }
                else
                    insertToNotVisitedLinks(link);
            }
        }

        #region GetSet
        public List<InternalLink> Links
        {
            get
            {
                return links;
            }
        }

        public List<string> MailLinks
        {
            get
            {
                return mailLinks;
            }
            set
            {
                mailLinks = value;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
        }
        #endregion
    }
}
