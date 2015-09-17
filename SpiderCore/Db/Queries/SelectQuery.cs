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

        public static List<string> GetStartPage()
        {
            Connector con = new Connector();
            string sqlQuery = @"
                SELECT 
                    first_page
                FROM
                    v2_sundsvallskommun_se.settings";

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
    }
}
