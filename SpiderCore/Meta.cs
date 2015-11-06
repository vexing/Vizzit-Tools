using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    /// <summary>
    /// Is pretty much a container class for meta data but with some funcionallity
    /// </summary>
    public class Meta
    {
        public string date { get; set; }
        public string customerId { get; set; }
        public int totalLinks { get; set; }
        public int externalLinks { get; set; }
        public int fileLinks { get; set; }
        public int internalLinks { get; set; }
        public int structurePages { get; set; }
        public bool daily { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="customerId"></param>
        public Meta(string customerId)
        {
            date = setDate();
            this.customerId = customerId;
            totalLinks = 0;
            externalLinks = 0;
            fileLinks = 0;
            internalLinks = 0;
            structurePages = 0;
        }

        /// <summary>
        /// Set crawl date
        /// </summary>
        /// <returns></returns>
        private string setDate()
        {
            DateTime now = DateTime.Now;

            return now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// adds a file link to the meta
        /// </summary>
        public void addFileLink()
        {
            fileLinks++;
            totalLinks++;
        }


        /// <summary>
        /// Adds ownDomain link to the meta
        /// </summary>
        public void addInternalLink()
        {
            internalLinks++;
            totalLinks++;
        }

        /// <summary>
        /// Adds an external link to the meta
        /// </summary>
        public void addExternalLink()
        {
            externalLinks++;
            totalLinks++;
        }

        /// <summary>
        /// Sets how many pages was parsed through structure
        /// </summary>
        /// <param name="i"></param>
        public void setFromStructure(int i)
        {
            structurePages = i;
        }
        
        /// <summary>
        /// Sets if its a daily crawl or not
        /// </summary>
        /// <param name="isDaily"></param>
        public void setDaily(bool isDaily)
        {
            daily = isDaily;
        }
    }
}
