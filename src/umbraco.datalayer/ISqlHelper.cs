/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;
using System.Xml;
using umbraco.DataLayer.Utility;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Interface of a module that interacts with a certain type of SQL database.
    /// </summary>
    public interface ISqlHelper : IDisposable
    {
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the Umbraco utility associated with this SQL helper.
        /// </summary>
        /// <value>The utilities.</value>
        IUtilitySet Utility { get; }

        /// <summary>
        /// Creates a new parameter for use with this specific implementation of ISqlHelper.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>A new parameter of the correct type.</returns>
        /// <remarks>Abstract factory pattern</remarks>
        IParameter CreateParameter(string parameterName, object value);

        /// <summary>
        /// Escapes a string for use in an SQL query.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>You should use parameters instead.</remarks>
        [Obsolete("You should use parameters instead. (see CreateParameter)", false)]
        string EscapeString(string value);

		/// <summary>
		/// Creates a concatenation fragment for use in an SQL query.
		/// </summary>
		/// <param name="values">The values that need to be concatenated</param>
		/// <returns>The SQL query fragment.</returns>
		/// <remarks>SQL Server uses a+b, MySql uses concat(a,b), Oracle uses a||b...</remarks>
		string Concat(params string[] values);

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The number of rows affected by the command.</returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        int ExecuteNonQuery(string commandText, params IParameter[] parameters);

        /// <summary>
        /// Executes a command and returns a records reader containing the results.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A data reader containing the results of the command.</returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        IRecordsReader ExecuteReader(string commandText, params IParameter[] parameters);

        /// <summary>
        /// Executes a command that returns a single value.
        /// </summary>
        /// <typeparam name="ScalarType">The type of the value.</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return value of the command.</returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        ScalarType ExecuteScalar<ScalarType>(string commandText, params IParameter[] parameters);

        /// <summary>
        /// Executes a command that returns an XML value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An XML reader containing the return value.</returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        XmlReader ExecuteXmlReader(string commandText, params IParameter[] parameters);
    }
}
