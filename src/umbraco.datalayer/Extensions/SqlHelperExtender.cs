/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using umbraco.DataLayer.Utility;

namespace umbraco.DataLayer.Extensions
{
    /// <summary>
    /// Class that adds extensions to an existing SQL helper.
    /// </summary>
    /// <remarks>Decorator design pattern.</remarks>
    public class SqlHelperExtender : ISqlHelper
    {
        #region Private Fields
        
        /// <summary>SQL helper this SqlHelperExtender extends.</summary>
        private readonly ISqlHelper m_SqlHelper;

        /// <summary>Extensions that are currently in use.</summary>
        private readonly List<ISqlHelperExtension> m_Extensions = new List<ISqlHelperExtension>();

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return m_SqlHelper.ConnectionString; }
        }

        /// <summary>
        /// Gets the Umbraco utility associated with this SQL helper.
        /// </summary>
        /// <value>The utilities.</value>
        public IUtilitySet Utility
        {
            get { return m_SqlHelper.Utility; }
        } 

        #endregion

        #region Public Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelperExtender"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public SqlHelperExtender(ISqlHelper sqlHelper)
        {
            if (sqlHelper == null)
                throw new ArgumentNullException("sqlHelper");
            m_SqlHelper = sqlHelper;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelperExtender"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="extensions">The extensions.</param>
        public SqlHelperExtender(ISqlHelper sqlHelper, params ISqlHelperExtension[] extensions)
            : this(sqlHelper)
        {
            foreach (ISqlHelperExtension extension in extensions)
                AddExtension(extension);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the extension to the extension chain.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void AddExtension(ISqlHelperExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");
            m_Extensions.Add(extension);
        }

        /// <summary>
        /// Removes an extension from the extension chain.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void RemoveExtension(ISqlHelperExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");
            m_Extensions.Remove(extension);
        }

        #endregion

        #region ISqlHelper Members
        
        /// <summary>
        /// Creates a new parameter for use with this specific implementation of ISqlHelper.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>A new parameter of the correct type.</returns>
        /// <remarks>Abstract factory pattern</remarks>
        public IParameter CreateParameter(string parameterName, object value)
        {
            return m_SqlHelper.CreateParameter(parameterName, value);
        }

		/// <summary>
		/// Creates a concatenation fragment for use in an SQL query.
		/// </summary>
		/// <param name="values">The values that need to be concatenated</param>
		/// <returns>The SQL query fragment.</returns>
		/// <remarks>SQL Server uses a+b, MySql uses concat(a,b), Oracle uses a||b...</remarks>
		public virtual string Concat(params string[] values)
		{
			return m_SqlHelper.Concat(values);
		}
		
		/// <summary>
        /// Escapes a string for use in an SQL query.
        /// </summary>
        /// <param name="text">The text to be escaped.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>You should use SQL parameters instead.</remarks>
        public string EscapeString(string text)
        {
            // Although EscapeString is obsolete, we still need to implement it
            #pragma warning disable 618
            return m_SqlHelper.EscapeString(text);
            #pragma warning restore 618
        }

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The number of rows affected by the command.
        /// </returns>
        public int ExecuteNonQuery(string commandText, params IParameter[] parameters)
        {
            foreach (ISqlHelperExtension extension in m_Extensions)
                extension.OnExecuteNonQuery(m_SqlHelper, ref commandText, ref parameters);
            return m_SqlHelper.ExecuteNonQuery(commandText, parameters);
        }

        /// <summary>
        /// Executes a command and returns a records reader containing the results.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A data reader containing the results of the command.
        /// </returns>
        public IRecordsReader ExecuteReader(string commandText, params IParameter[] parameters)
        {
            foreach (ISqlHelperExtension extension in m_Extensions)
                extension.OnExecuteReader(m_SqlHelper, ref commandText, ref parameters);
            return m_SqlHelper.ExecuteReader(commandText, parameters);
        }

        /// <summary>
        /// Executes a command that returns a single value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return value of the command.</returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        public T ExecuteScalar<T>(string commandText, params IParameter[] parameters)
        {
            foreach (ISqlHelperExtension extension in m_Extensions)
                extension.OnExecuteScalar(m_SqlHelper, ref commandText, ref parameters);
            return m_SqlHelper.ExecuteScalar<T>(commandText, parameters);
        }

        /// <summary>
        /// Executes a command that returns an XML value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// An XML reader containing the return value.
        /// </returns>
        public XmlReader ExecuteXmlReader(string commandText, params IParameter[] parameters)
        {
            foreach (ISqlHelperExtension extension in m_Extensions)
                extension.OnExecuteXmlReader(m_SqlHelper, ref commandText, ref parameters);
            return m_SqlHelper.ExecuteXmlReader(commandText, parameters);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            m_SqlHelper.Dispose();
        }

        #endregion
    }
}
