/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using MSC = MySql.Data.MySqlClient;

namespace umbraco.DataLayer.SqlHelpers.MySql
{
    /// <summary>
    /// Sql Helper for a MySQL 5.0 database.
    /// </summary>
    public class MySqlHelper : SqlHelper<MSC.MySqlParameter>
    {
        /// <summary>SQL parser that replaces the SQL-Server specific tokens by their MySQL equivalent.</summary>
        private MySqlParser m_SqlParser = new MySqlParser();

        /// <summary>Initializes a new instance of the <see cref="MySqlHelper"/> class.</summary>
        /// <param name="connectionString">The connection string.</param>
        public MySqlHelper(string connectionString)
            : base(connectionString)
        {
            m_Utility = new MySqlUtility(this);
        }

        /// <summary>
        /// Creates a new parameter for use with this specific implementation of ISqlHelper.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>A new parameter of the correct type.</returns>
        /// <remarks>Abstract factory pattern</remarks>
        public override IParameter CreateParameter(string parameterName, object value)
        {
            return new MySqlParameter(parameterName, value);
        }

        /// <summary>Converts a the command before executing.</summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>The original command text.</returns>
        protected override string ConvertCommand(string commandText)
        {
            return m_SqlParser.Parse(commandText);
        }

        /// <summary>Executes a command that returns a single value.</summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return value of the command.</returns>
        protected override object ExecuteScalar(string commandText, MSC.MySqlParameter[] parameters)
        {
            using (var cc = UseCurrentConnection)
            {
                return ExecuteScalar((MSC.MySqlConnection) cc.Connection, (MSC.MySqlTransaction) cc.Transaction, commandText, parameters);
            }
        }

        // copied & adapted from MySqlHelper
        private static object ExecuteScalar(MSC.MySqlConnection connection, MSC.MySqlTransaction trx, string commandText, params MSC.MySqlParameter[] commandParameters)
        {
            var mySqlCommand = new MSC.MySqlCommand();
            mySqlCommand.Connection = connection;
            if (trx != null) mySqlCommand.Transaction = trx;
            mySqlCommand.CommandText = commandText;
            mySqlCommand.CommandType = CommandType.Text;
            if (commandParameters != null)
            {
                foreach (var commandParameter in commandParameters)
                    mySqlCommand.Parameters.Add(commandParameter);
            }
            var obj = mySqlCommand.ExecuteScalar();
            mySqlCommand.Parameters.Clear();
            return obj;
        }

        /// <summary>Executes a command and returns the number of rows affected.</summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The number of rows affected by the command.
        /// </returns>
        protected override int ExecuteNonQuery(string commandText, MSC.MySqlParameter[] parameters)
        {
            using (var cc = UseCurrentConnection)
            {
                return ExecuteNonQuery((MSC.MySqlConnection) cc.Connection, (MSC.MySqlTransaction)cc.Transaction, commandText, parameters);
            }
        }

        // copied & adapted from MySqlHelper
        public static int ExecuteNonQuery(MSC.MySqlConnection connection, MSC.MySqlTransaction trx, string commandText, params MSC.MySqlParameter[] commandParameters)
        {
            var mySqlCommand = new MSC.MySqlCommand();
            mySqlCommand.Connection = connection;
            if (trx != null) mySqlCommand.Transaction = trx;
            mySqlCommand.CommandText = commandText;
            mySqlCommand.CommandType = CommandType.Text;
            if (commandParameters != null)
            {
                foreach (var commandParameter in commandParameters)
                    mySqlCommand.Parameters.Add(commandParameter);
            }
            var num = mySqlCommand.ExecuteNonQuery();
            mySqlCommand.Parameters.Clear();
            return num;
        }

        /// <summary>Executes a command and returns a records reader containing the results.</summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A data reader containing the results of the command.
        /// </returns>
        protected override IRecordsReader ExecuteReader(string commandText, MSC.MySqlParameter[] parameters)
        {
            using (var cc = UseCurrentConnection)
            {
                return new MySqlDataReader(ExecuteReader((MSC.MySqlConnection) cc.Connection, (MSC.MySqlTransaction) cc.Transaction, commandText, parameters));
            }
        }

        // copied & adapted from MySqlHelper
        private static MSC.MySqlDataReader ExecuteReader(MSC.MySqlConnection connection, MSC.MySqlTransaction trx, string commandText, MSC.MySqlParameter[] commandParameters)
        {
            MSC.MySqlCommand mySqlCommand = new MSC.MySqlCommand();
            mySqlCommand.Connection = connection;
            if (trx != null) mySqlCommand.Transaction = trx;
            mySqlCommand.CommandText = commandText;
            mySqlCommand.CommandType = CommandType.Text;
            if (commandParameters != null)
            {
                foreach (var commandParameter in commandParameters)
                    mySqlCommand.Parameters.Add(commandParameter);
            }
            MSC.MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
            mySqlCommand.Parameters.Clear();
            return mySqlDataReader;
        }

        /// <summary>Converts the scalar value to the given type.</summary>
        /// <typeparam name="T">Desired type of the value.</typeparam>
        /// <param name="scalar">A scalar returned by ExecuteScalar.</param>
        /// <returns>The scalar, converted to type T.</returns>
        protected override T ConvertScalar<T>(object scalar)
        {
            if (scalar == null)
                return default(T);

            switch (typeof(T).FullName)
            {
                case "System.Boolean": return (T)(object)((1).Equals(scalar));
                case "System.Guid": return (T)(object)(Guid.Parse(scalar.ToString()));
                default: return base.ConvertScalar<T>(scalar);
            }
        }

		/// <summary>
		/// Creates a concatenation fragment for use in an SQL query.
		/// </summary>
		/// <param name="values">The values that need to be concatenated</param>
		/// <returns>The SQL query fragment.</returns>
		/// <remarks>SQL Server uses a+b, MySql uses concat(a,b), Oracle uses a||b...</remarks>
		public override string Concat(params string[] values)
		{
			// mysql has a concat function
			return "concat(" + string.Join(",", values) + ")";
		}
	}
}
