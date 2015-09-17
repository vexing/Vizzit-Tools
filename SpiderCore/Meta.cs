using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class Meta
    {

        public int totalLinks { get; set; }
        public int externalLinks { get; set; }
        public int fileLinks { get; set; }
        public int internalLinks { get; set; }

        public Meta()
        {
            totalLinks = 0;
            externalLinks = 0;
            fileLinks = 0;
            internalLinks = 0;
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
    }
}
