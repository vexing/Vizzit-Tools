using Newtonsoft.Json;
using SpiderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    //TODO: Move to external namcespace
    public class Output
    {
        private List<PageData> pageDataList;
        private Dictionary<string, InternalLink> visitedLinks;
        private string jsonString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageDataList">List on all PageData objects</param>
        /// <param name="visitedLinks">list of all InternalLinks</param>
        public Output(List<PageData> pageDataList, Dictionary<string, InternalLink> visitedLinks)
        {
            this.pageDataList = pageDataList;
            this.visitedLinks = visitedLinks;
            jsonString = createJson();
        }

        /// <summary>
        /// Create the actual JSON with Newtonsoft
        /// </summary>
        private string createJson()
        {
            return JsonConvert.SerializeObject(pageDataList, Formatting.Indented);
        }  

        #region GetSet
        public string JsonString
        {
            get
            {
                return jsonString;
            }
        }
        #endregion
    }
}
