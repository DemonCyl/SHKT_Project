using SHKT_Project.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHKT_Project.Common
{
    public class SQLHelper
    {

        public class DbHelperSQL
        {
            //private string connectionString = ConfigurationManager.ConnectionStrings["connJAS"].ConnectionString;
            private string connStr = null;

            public DbHelperSQL(ConfigData configData)
            {
                //this.configData = data;

                this.connStr = new StringBuilder("server=" + configData.DataIpAddress +
                ";database=" + configData.DataBaseName + "; uid=" + configData.Uid + ";pwd=" + configData.Pwd + "").ToString();
            }

            public SqlConnection GetConnection()
            {
                return new SqlConnection(connStr);
            }


        }
    }
}
