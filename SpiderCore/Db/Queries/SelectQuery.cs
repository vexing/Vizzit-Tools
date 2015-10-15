using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore.Db.Queries
{
    public static class SelectQuery
    {
        public static List<string> GetCustomerDbList()
        {
            Connector con = new Connector();
            string sqlQuery = @"
                SELECT 
                    `value`
                FROM
                    `vizzit_login`.`company_parameters` cp
                INNER JOIN
                    `vizzit_login`.`companies` c ON cp.`company_id` = c.`company_id`
                WHERE
                    c.`name` NOT LIKE '~%'
                        AND c.`name` NOT LIKE '!%'
                        AND c.`name` NOT LIKE '~^%'
                        AND cp.`name` = 'database_name'";

            return con.Query(sqlQuery);
        }

        public static List<string> GetStartPage(string db)
        {
            Connector con = new Connector();
            string sqlQuery = String.Format(@"
                SELECT 
                    first_page
                FROM
                    {0}.settings", db);

             return con.Query(sqlQuery);
        }

        public static List<string> GetDomain(string db)
        {
            Connector con = new Connector();
            string sqlQuery = String.Format(@"
                SELECT 
                    c.server_domain
                FROM
                    vizzit_login.companies c
                        LEFT JOIN
                    vizzit_login.company_parameters p ON p.company_id = c.company_id
                WHERE
                    p.name = 'database_name'
                        AND p.value = '{0}'", db);

            return con.Query(sqlQuery);
        }

        public static List<string> GetCustomerId(string db)
        {
            Connector con = new Connector();
            string sqlQuery = String.Format(
                "SELECT s.customer_id FROM vizzit_login.sites s JOIN vizzit_login.company_parameters cp ON cp.company_id = s.company_id WHERE cp.name = 'database_name' AND cp.value = '{0}'", db);

            return con.Query(sqlQuery);
        }

        public static List<string> GetPagesWithBrokenLinkgs(string db)
        {
            Connector con = new Connector();
            string sqlQuery = String.Format(@"
                SELECT 
                    p2.url as pageUrl
                FROM
                    {0}.SpiderPageVisited v
                        INNER JOIN
                    {0}.SpiderPageLink l USING (spiderPageVisitedId)
                        INNER JOIN
                    {0}.SpiderPage p2 ON v.spiderPageId = p2.spiderPageId
                        INNER JOIN
                    {0}.SpiderPageLinkData d USING (spiderPageLinkId)
                WHERE
	                broken = 1", db);

            return con.Query(sqlQuery);
        }


        public static List<string> GetDatabase(string customerId)
        {
            Connector con = new Connector();
            string sqlQuery = String.Format(@"
                SELECT distinct
                    (value)
                FROM
                    vizzit_login.company_parameters cp
                        JOIN
                    vizzit_login.sites s USING (company_id)
                WHERE
                    cp.name = 'database_name'
                        AND s.customer_id = '{0}'", customerId);

            return con.Query(sqlQuery);
        }

        public static List<string> GetStructure(string db, string date)
        {
            Connector con = new Connector();
            string sqlQuery = String.Format(@"
                SELECT 
                    url
                FROM
                    {0}.structure
                WHERE
                    (`end` > '{1}'
                        OR `end` = '0000-00-00')
                        AND start <= '{1}'
                        AND type != 'file'", db, date);

            return con.Query(sqlQuery);
        }
    }
}
