using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class LinkData
    {
        protected string linkUrl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="linkUrl"></param>
        public LinkData(string linkUrl)
        {
            this.linkUrl = linkUrl;
        }

        #region GetSet
        public string LinkUrl
        {
            get
            {
                return linkUrl;
            }
        }        
        #endregion
    }
}
