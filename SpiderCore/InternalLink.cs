using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    /// <summary>
    /// In the end this should probably be done in LinkData since we didn't need to separate different links
    /// </summary>
    public class InternalLink : LinkData
    {
        private HttpStatusCode status;
        private string customStatus;
        private bool relative;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="linkUrl">Url to the link</param>
        /// <param name="status">status code</param>
        /// <param name="relative">Relative link or not</param>
        public InternalLink(string linkUrl, HttpStatusCode status, bool relative) : base(linkUrl)
        {
            this.status = status;
            this.relative = relative;
            this.customStatus = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="linkUrl">Url to the link</param>
        /// <param name="status">status code</param>
        /// <param name="relative">Relative link or not</param>
        public InternalLink(string linkUrl, string customStatus, bool relative)
            : base(linkUrl)
        {
            this.relative = relative;
            this.customStatus = customStatus;
        }   

        #region GetSet
        public HttpStatusCode Status
        {
            get
            {
                return status;
            }
        }
        public string CustomStatus
        {
            get
            {
                return customStatus;
            }
        }
        public bool Relative
        {
            get
            {
                return relative;
            }
        }     
        #endregion
    }
}
