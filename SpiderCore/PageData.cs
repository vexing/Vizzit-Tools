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
        private int id;
        private List<string> linkDataList;
        private List<string> mailLinkList;

        public PageData(string url)
        {
            this.url = url;
            linkDataList = new List<string>();
            mailLinkList = new List<string>();
        }

        public PageData(string url, int id)
        {
            this.url = url;
            this.id = id;
        }

        public void insertLink(string linkData)
        {
            linkDataList.Add(linkData);
        }

        public void insertMailLink(string linkData)
        {
            mailLinkList.Add(linkData);
        }

        #region GetSet
        public List<string> LinkDataList
        {
            get
            {
                return linkDataList;
            }
        }

        public List<string> MailLinkList
        {
            get
            {
                return mailLinkList;
            }
            set
            {
                mailLinkList = value;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }
        }
        #endregion
    }
}
