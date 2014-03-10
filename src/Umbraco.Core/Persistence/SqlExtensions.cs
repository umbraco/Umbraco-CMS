using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence
{
    //TODO: check if any of this works and for what databse types it works for: 
    // ref: http://stackoverflow.com/questions/16171144/how-to-check-for-database-availability

    internal static class SqlExtensions
    {
        public static bool IsConnectionAvailable(string connString)
        {
            using (var connection = new SqlConnection(connString))
            {
                return connection.IsAvailable();
            }
        }

        public static bool IsAvailable(this SqlConnection connection)
        {
            try
            {
                connection.Open();
                connection.Close();
            }
            catch (SqlException)
            {
                return false;
            }

            return true;
        }
    }
}
