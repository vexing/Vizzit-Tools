using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore
{
    public class Output
    {
        private List<PageData> pageDataList;
        private Dictionary<string, InternalLink> visitedLinks;
        private string jsonString;

        public Output(List<PageData> pageDataList, Dictionary<string, InternalLink> visitedLinks)
        {
            this.pageDataList = pageDataList;
            this.visitedLinks = visitedLinks;
            jsonString = createJson();
        }

        private string createJson()
        {
            string json =  "{ ";
            foreach(PageData pd in pageDataList)
            {
                json += "\"" + pd.Url + "\" : [ ";

                foreach (string il in pd.LinkDataList)
                {
                    if (visitedLinks.ContainsKey(il))
                    {
                        json += "{ \"url\" : \"" + visitedLinks[il].LinkUrl + "\",";
                        json += "\"status\" : \"" + visitedLinks[il].Status + "\",";
                        json += "\"relative\" : \"" + visitedLinks[il].Relative + "\" },";
                    }
                    else
                    {
                        json += "{ \"url\" : \"" + il + "\",";
                        json += "\"status\" : \"notVisited\",";
                        json += "\"relative\" : \"notVisited\"},";
                    }
                }

                json = json.Remove(json.Length - 1, 1);
                json += " ],";                    
            }
            json = json.Remove(json.Length - 1, 1);
            json += " }";

            return json;
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
