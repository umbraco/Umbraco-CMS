using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;
using System.Diagnostics;
using SQLCE4Umbraco;

namespace SqlCE4Umbraco
{
    public class SqlCeApplicationBlock
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(
            string connectionString,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
            )
        {
            object retVal;

            try
            {
                using (SqlCeConnection conn = SqlCeContextGuardian.Open(connectionString))
                {
                    using (SqlCeCommand cmd = new SqlCeCommand(commandText, conn))
                    {
                        AttachParameters(cmd, commandParameters);
                        Debug.WriteLine("---------------------------------SCALAR-------------------------------------");
                        Debug.WriteLine(commandText);
                        Debug.WriteLine("----------------------------------------------------------------------------");
                        retVal = cmd.ExecuteScalar();
                    }
                }

                return retVal;
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running Scalar: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        public static int ExecuteNonQuery(
            string connectionString,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
            )
        {
            try
            {
                int rowsAffected;
                using (SqlCeConnection conn = SqlCeContextGuardian.Open(connectionString))
                {
                    // this is for multiple queries in the installer
                    if (commandText.Trim().StartsWith("!!!"))
                    {
                        commandText = commandText.Trim().Trim('!');
                        string[] commands = commandText.Split('|');
                        string currentCmd = String.Empty;

                        foreach (string cmd in commands)
                        {
                            try
                            {
                                currentCmd = cmd;
                                if (!String.IsNullOrWhiteSpace(cmd))
                                {
                                    SqlCeCommand c = new SqlCeCommand(cmd, conn);
                                    c.ExecuteNonQuery();
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("*******************************************************************");
                                Debug.WriteLine(currentCmd);
                                Debug.WriteLine(e);
                                Debug.WriteLine("*******************************************************************");
                            }
                        }
                        return 1;
                    }
                    else
                    {
                        Debug.WriteLine("----------------------------------------------------------------------------");
                        Debug.WriteLine(commandText);
                        Debug.WriteLine("----------------------------------------------------------------------------");
                        conn.ConnectionString = connectionString;
                        conn.Open();
                        SqlCeCommand cmd = new SqlCeCommand(commandText, conn);
                        AttachParameters(cmd, commandParameters);
                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                }

                return rowsAffected;
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running NonQuery: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static SqlCeDataReader ExecuteReader(
            string connectionString,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
            )
        {
            try
            {
                Debug.WriteLine("---------------------------------READER-------------------------------------");
                Debug.WriteLine(commandText);
                Debug.WriteLine("----------------------------------------------------------------------------");
                SqlCeDataReader reader;
                SqlCeConnection conn = SqlCeContextGuardian.Open(connectionString);

                try
                {
                    SqlCeCommand cmd = new SqlCeCommand(commandText, conn);
                    AttachParameters(cmd, commandParameters);
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    conn.Close();
                    throw;
                }

                return reader;
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running Reader: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        public static bool VerifyConnection(string connectionString)
        {
            bool isConnected = false;
            using (SqlCeConnection conn = SqlCeContextGuardian.Open(connectionString))
            {
                isConnected = conn.State == ConnectionState.Open;
            }

            return isConnected;
        }

        private static void AttachParameters(SqlCeCommand command, SqlCeParameter[] commandParameters)
        {
            foreach (SqlCeParameter parameter in commandParameters)
            {
                if ((parameter.Direction == ParameterDirection.InputOutput) && (parameter.Value == null))
                {
                    parameter.Value = DBNull.Value;
                }
                command.Parameters.Add(parameter);
            }
        }



    }
}
