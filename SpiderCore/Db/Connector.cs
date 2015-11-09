using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore.Db
{
    public class Connector
    {
        private String str = @"server=80.72.9.34;userid=v2;password=xh.N3p47,HZzw4JN;";
        private MySqlConnection connection = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public Connector()
        {
            connection = new MySqlConnection(str);
        }

        /// <summary>
        /// open a connection
        /// </summary>
        public void OpenCon()
        {
            connection.Open();
        }

        /// <summary>
        /// Close a connection
        /// </summary>
        public void CloseCon()
        {
            connection.Close();
        }

        /// <summary>
        /// Select query template for List return
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<string> Query(string query)
        {
            List<string> r = new List<string>();
            MySqlDataReader reader = null;

            try
            {
                OpenCon();

                MySqlCommand cmd = new MySqlCommand(query, connection);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    r.Add(reader.GetString(0));
                }
            }
            catch (MySqlException e)
            {
                string errorMsg = e.Message;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection != null)
                    CloseCon();
            }

            return r;
        }

        #region GetSet
        #endregion
    }
}
