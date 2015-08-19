﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class InternalLink : LinkData
    {
        private HttpStatusCode status;
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
        }             

        #region GetSet
        public HttpStatusCode Status
        {
            get
            {
                return status;
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
