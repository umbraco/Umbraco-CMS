using System;
using System.Collections.Generic;
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
            try
            {
                using (var conn = SqlCeContextGuardian.Open(connectionString))
                {
                    return ExecuteScalarTry(conn, null, commandText, commandParameters);
                }
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running Scalar: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee);
            }
        }

        public static object ExecuteScalar(
            SqlCeConnection conn, SqlCeTransaction trx,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters)
        {
            try
            {
                return ExecuteScalarTry(conn, trx, commandText, commandParameters);
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running Scalar: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee);
            }
        }

        public static object ExecuteScalar(
            SqlCeConnection conn,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters)
        {
            return ExecuteScalar(conn, null, commandType, commandText, commandParameters);
        }

        private static object ExecuteScalarTry(
            SqlCeConnection conn, SqlCeTransaction trx,
            string commandText,
            params SqlCeParameter[] commandParameters)
        {
            object retVal;
            using (var cmd = trx == null ? new SqlCeCommand(commandText, conn) : new SqlCeCommand(commandText, conn, trx))
            {
                AttachParameters(cmd, commandParameters);
                Debug.WriteLine("---------------------------------SCALAR-------------------------------------");
                Debug.WriteLine(commandText);
                Debug.WriteLine("----------------------------------------------------------------------------");
                retVal = cmd.ExecuteScalar();
            }
            return retVal;
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
                using (var conn = SqlCeContextGuardian.Open(connectionString))
                {
                    return ExecuteNonQueryTry(conn, null, commandText, commandParameters);
                }
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running NonQuery: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        public static int ExecuteNonQuery(
            SqlCeConnection conn,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
        )
        {
            return ExecuteNonQuery(conn, null, commandType, commandText, commandParameters);
        }

        public static int ExecuteNonQuery(
            SqlCeConnection conn, SqlCeTransaction trx,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
            )
        {
            try
            {
                return ExecuteNonQueryTry(conn, trx, commandText, commandParameters);
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running NonQuery: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        private static int ExecuteNonQueryTry(
            SqlCeConnection conn, SqlCeTransaction trx,
            string commandText,
            params SqlCeParameter[] commandParameters)
        {
            // this is for multiple queries in the installer
            if (commandText.Trim().StartsWith("!!!"))
            {
                commandText = commandText.Trim().Trim('!');
                var commands = commandText.Split('|');
                var currentCmd = string.Empty;

                foreach (var command in commands)
                {
                    try
                    {
                        currentCmd = command;
                        if (string.IsNullOrWhiteSpace(command)) continue;
                        var c = trx == null ? new SqlCeCommand(command, conn) : new SqlCeCommand(command, conn, trx);
                        c.ExecuteNonQuery();
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

            Debug.WriteLine("----------------------------------------------------------------------------");
            Debug.WriteLine(commandText);
            Debug.WriteLine("----------------------------------------------------------------------------");
            var cmd = new SqlCeCommand(commandText, conn);
            AttachParameters(cmd, commandParameters);
            var rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected;
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
                var conn = SqlCeContextGuardian.Open(connectionString);
                return ExecuteReaderTry(conn, null, commandText, commandParameters);
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running Reader: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        public static SqlCeDataReader ExecuteReader(
            SqlCeConnection conn,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
        )
        {
            return ExecuteReader(conn, commandType, commandText, commandParameters);
        }

        public static SqlCeDataReader ExecuteReader(
            SqlCeConnection conn, SqlCeTransaction trx,
            CommandType commandType,
            string commandText,
            params SqlCeParameter[] commandParameters
            )
        {
            try
            {
                return ExecuteReaderTry(conn, trx, commandText, commandParameters);
            }
            catch (Exception ee)
            {
                throw new SqlCeProviderException("Error running Reader: \nSQL Statement:\n" + commandText + "\n\nException:\n" + ee.ToString());
            }
        }

        private static SqlCeDataReader ExecuteReaderTry(
            SqlCeConnection conn, SqlCeTransaction trx,
            string commandText,
            params SqlCeParameter[] commandParameters)
        {
            Debug.WriteLine("---------------------------------READER-------------------------------------");
            Debug.WriteLine(commandText);
            Debug.WriteLine("----------------------------------------------------------------------------");

            try
            {
                var cmd = trx == null ? new SqlCeCommand(commandText, conn) : new SqlCeCommand(commandText, conn, trx);
                AttachParameters(cmd, commandParameters);
                return cmd.ExecuteReader();
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        public static bool VerifyConnection(string connectionString)
        {
            using (var conn = SqlCeContextGuardian.Open(connectionString))
            {
                return conn.State == ConnectionState.Open;
            }
        }

        private static void AttachParameters(SqlCeCommand command, IEnumerable<SqlCeParameter> commandParameters)
        {
            foreach (var parameter in commandParameters)
            {
                if ((parameter.Direction == ParameterDirection.InputOutput) && (parameter.Value == null))
                    parameter.Value = DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }
    }
}
