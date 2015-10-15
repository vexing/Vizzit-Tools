using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
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

        private string setDate()
        {
            DateTime now = DateTime.Now;

            return now.ToString("yyyy-MM-dd");
        }

        public void addFileLink()
        {
            fileLinks++;
            totalLinks++;
        }


        public void addInternalLink()
        {
            internalLinks++;
            totalLinks++;
        }

        public void addExternalLink()
        {
            externalLinks++;
            totalLinks++;
        }

        public void setFromStructure(int i)
        {
            structurePages = i;
        }
        
        public void setDaily(bool isDaily)
        {
            daily = isDaily;
        }
    }
}
