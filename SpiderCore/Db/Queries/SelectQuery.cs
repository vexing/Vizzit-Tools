using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore.Db.Queries
{
    /// <summary>
    /// Lacking alot. We probably want to make sure all database functions are in here.
    /// </summary>
    public class SelectQuery
    {
        /// <summary>
        /// Get structure
        /// </summary>
        /// <param name="db"></param>
        /// <param name="date"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static List<string> GetStructure(string db, string date, string domain)
        {
            List<string> structure = SelectQueryModel.GetStructure(db, date);

            //Add domain to the links
            List<string> structureWithDomain = new List<string>();

            foreach (string url in structure)
            {
                if(!url.Contains(domain))
                    structureWithDomain.Add(domain + url);
            }

            return structureWithDomain;
        }
    }
}
