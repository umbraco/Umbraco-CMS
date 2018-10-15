/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  �2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using umbraco.DataLayer.Utility;
using Umbraco.Core.Logging;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Abstract base class for SQL helpers that use parameters implementing IDataParameter.
    /// </summary>
    /// <typeparam name="P">SQL parameter data type.</typeparam>
    public abstract class SqlHelper<P> : ISqlHelper where P : IDataParameter
    {
        #region Private Fields

        /// <summary>The connection string that provides access to the SQL database.</summary>
        private readonly string m_ConnectionString;

        #endregion

        #region Protected Fields

        /// <summary>Utility that provides access to complex database actions.</summary>
        protected IUtilitySet m_Utility;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the connection string that provides access to the SQL database.
        /// </summary>
        /// <value>The connection string that provides access to the SQL database.</value>
        public string ConnectionString
        {
            get { return m_ConnectionString; }
        }

        /// <summary>
        /// Gets the Umbraco utility associated with this SQL helper.
        /// </summary>
        /// <value>The utilities.</value>
        public IUtilitySet Utility
        {
            get { return m_Utility; }
        }

        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelper&lt;P&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlHelper(string connectionString) : this(connectionString, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelper&lt;P&gt;"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string, cannot be <c>null</c>.</param>
        /// <param name="utility">The utility, a default utility will be used if <c>null</c>.</param>
        public SqlHelper(string connectionString, IUtilitySet utility)
        {
            // Check arguments
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (utility == null)
                utility = new DefaultUtility<SqlHelper<P>>(this);

            // Initialize member fields
            m_ConnectionString = connectionString;
            m_Utility = utility;

            #if DEBUG && DebugDataLayer
                // Log creation
                Trace.TraceInformation(GetType().Name + " created.");
            #endif
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SqlHelper&lt;P&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~SqlHelper()
        {
            Dispose();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Performs necessary conversion on the SQL command prior to its execution.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>The original command text. Inheriting classes can change this behavior.</returns>
        protected virtual string ConvertCommand(string commandText)
        {
            if (commandText == null)
                throw new ArgumentNullException("commandText");
            return commandText;
        }

        /// <summary>
        /// Performs necessary conversion on the parameters prior to its execution.
        /// </summary>
        /// <param name="parameters">The parameters. Expected to be of type IParameterContainer.</param>
        /// <returns>The parameters, converted to the raw types of their containers.</returns>
        protected virtual P[] ConvertParameters(IParameter[] parameters)
        {
            // Check arguments
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            // Add raw types of parameters to an array
            var newParams = new P[parameters.Length];
            for (var index = 0; index < parameters.Length; index++)
            {
                // Get raw type out of container
                var parameter = parameters[index] as IParameterContainer<P>;
                if (parameter == null)
                    throw new ArgumentException(String.Format("Element {0} of parameters has the wrong type. Expected: IParameterContainer<{1}>. Received: null.", index, typeof(P).Name), "parameters");
                newParams[index] = parameter.RawParameter;
            }
            return newParams;
        }

        /// <summary>Converts the scalar value to the given type.</summary>
        /// <typeparam name="T">Desired type of the value.</typeparam>
        /// <param name="scalar">A scalar returned by ExecuteScalar.</param>
        /// <returns>The scalar, converted to type T.</returns>
        protected virtual T ConvertScalar<T>(object scalar)
        {
            if (scalar == null || (scalar == DBNull.Value && !typeof(T).IsAssignableFrom(typeof(DBNull))))
                return default(T);

            switch(typeof(T).FullName)
            {
                case "System.Int32": return (T)(object)int.Parse(scalar.ToString());
                case "System.Int64": return (T)(object)long.Parse(scalar.ToString());
                default: return (T)scalar;
            }
        }
        #endregion

        #region ISqlHelper Members

		/// <summary>
		/// Creates a concatenation fragment for use in an SQL query.
		/// </summary>
		/// <param name="values">The values that need to be concatenated</param>
		/// <returns>The SQL query fragment.</returns>
		/// <remarks>SQL Server uses a+b, MySql uses concat(a,b), Oracle uses a||b...</remarks>
		public virtual string Concat(params string[] values)
		{
			// default is SQL Server syntax
			return "(" + string.Join("+", values) + ")";
		}

        /// <summary>
        /// Escapes a string for use in an SQL query.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>You should use SQL parameters instead.</remarks>
        public virtual string EscapeString(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");
        }

        /// <summary>
        /// Creates a new parameter for use with this specific implementation of ISqlHelper.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>A new parameter of the correct type.</returns>
        /// <remarks>Abstract factory pattern</remarks>
        public abstract IParameter CreateParameter(string parameterName, object value);

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
            string commandConverted = ConvertCommand(commandText);
            P[] parametersConverted = ConvertParameters(parameters);
            try
            {
                return ConvertScalar<T>(ExecuteScalar(commandConverted.TrimToOneLine(), parametersConverted));
            }
            catch (Exception e)
            {
                LogHelper.Error<SqlHelper<P>>(string.Format("Error executing query {0}", commandText), e);
                throw new SqlHelperException("ExecuteScalar", commandText, parameters, e);
            }
        }

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The number of rows affected by the command.
        /// </returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        public int ExecuteNonQuery(string commandText, params IParameter[] parameters)
        {
			string commandConverted = ConvertCommand(commandText);
            P[] parametersConverted = ConvertParameters(parameters);
            try
            {
                return ExecuteNonQuery(commandConverted.TrimToOneLine(), parametersConverted);
            }
            catch (Exception e)
            {
                LogHelper.Error<SqlHelper<P>>(string.Format("Error executing query {0}", commandText), e);
                throw new SqlHelperException("ExecuteNonQuery", commandText, parameters, e);
            }
        }
        
        /// <summary>
        /// Executes a command and returns a records reader containing the results.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A data reader containing the results of the command.
        /// </returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        public IRecordsReader ExecuteReader(string commandText, params IParameter[] parameters)
        {
			string commandConverted = ConvertCommand(commandText);
            P[] parametersConverted = ConvertParameters(parameters);
            try
            {
                return ExecuteReader(commandConverted.TrimToOneLine(), parametersConverted);
            }
            catch (Exception e)
            {
                LogHelper.Error<SqlHelper<P>>(string.Format("Error executing query {0}", commandText), e);
                throw new SqlHelperException("ExecuteReader", commandText, parameters, e);
            }
        }
        
        /// <summary>
        /// Executes a command that returns an XML value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// An XML reader containing the return value.
        /// </returns>
        /// <exception cref="umbraco.DataLayer.SqlHelperException">If a data source error occurs.</exception>
        public XmlReader ExecuteXmlReader(string commandText, params IParameter[] parameters)
        {
			string commandConverted = ConvertCommand(commandText);
            P[] parametersConverted = ConvertParameters(parameters);
            try
            {
                return ExecuteXmlReader(commandConverted.TrimToOneLine(), parametersConverted);
            }
            catch (Exception e)
            {
                LogHelper.Error<SqlHelper<P>>(string.Format("Error executing query {0}", commandText), e);
                throw new SqlHelperException("ExecuteXmlReader", commandText, parameters, e);
            }
        }
        
        #endregion

        #region Abstract Query Methods

        /// <summary>
        /// Executes a command that returns a single value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return value of the command.</returns>
        protected abstract object ExecuteScalar(string commandText, P[] parameters);

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The number of rows affected by the command.
        /// </returns>
        protected abstract int ExecuteNonQuery(string commandText, params P[] parameters);

        /// <summary>
        /// Executes a command and returns a records reader containing the results.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A data reader containing the results of the command.
        /// </returns>
        protected abstract IRecordsReader ExecuteReader(string commandText, params P[] parameters);

        /// <summary>
        /// Executes a command that returns an XML value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// An XML reader containing the return value.
        /// </returns>
        protected virtual XmlReader ExecuteXmlReader(string commandText, params P[] parameters)
        {
            var xmlString = (string)ExecuteScalar(commandText, parameters);
            if (xmlString == null)
                throw new ArgumentException("The query didn't return a value.");

            return XmlReader.Create(new StringReader(xmlString));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        { }

        #endregion

        #region Evil Reflection

        internal CurrentConnectionUsing UseCurrentConnection
        {
            get { return new CurrentConnectionUsing(); }
        }

        #endregion
    }

    #region Evil Reflection

    internal class CurrentConnectionUsing : IDisposable
    {
        private static MethodInfo OpenMethod { get; set; }
        //private static MethodInfo CloseMethod { get; set; }

        private static readonly object ScopeProvider;
        private static readonly MethodInfo ScopeProviderAmbientOrNoScopeMethod;
        private static readonly PropertyInfo ScopeDatabaseProperty;
        private static readonly PropertyInfo ConnectionProperty;
        private static readonly FieldInfo TransactionField;
        private static readonly PropertyInfo InnerConnectionProperty;
        private static readonly PropertyInfo InnerTransactionProperty;
        private static readonly object[] NoArgs = new object[0];

        private readonly object _database;

        static CurrentConnectionUsing()
        {
            var coreAssembly = Assembly.Load("Umbraco.Core");

            var applicationContextType = coreAssembly.GetType("Umbraco.Core.ApplicationContext");
            var databaseContextType = coreAssembly.GetType("Umbraco.Core.DatabaseContext");
            var umbracoDatabaseType = coreAssembly.GetType("Umbraco.Core.Persistence.UmbracoDatabase");
            var databaseType = coreAssembly.GetType("Umbraco.Core.Persistence.Database");
            var scopeProviderType = coreAssembly.GetType("Umbraco.Core.Scoping.IScopeProviderInternal");
            var scopeType = coreAssembly.GetType("Umbraco.Core.Scoping.IScope");

            var applicationContextCurrentProperty = applicationContextType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
            if (applicationContextCurrentProperty == null) throw new Exception("oops: applicationContextCurrentProperty.");
            var applicationContext = applicationContextCurrentProperty.GetValue(null, NoArgs);

            var applicationContextDatabaseContextProperty = applicationContextType.GetProperty("DatabaseContext", BindingFlags.Instance | BindingFlags.Public);
            if (applicationContextDatabaseContextProperty == null) throw new Exception("oops: applicationContextDatabaseContextProperty.");
            var databaseContext = applicationContextDatabaseContextProperty.GetValue(applicationContext, NoArgs);

            var databaseContextScopeProviderField = databaseContextType.GetField("ScopeProvider", BindingFlags.Instance | BindingFlags.NonPublic);
            if (databaseContextScopeProviderField == null) throw new Exception("oops: databaseContextScopeProviderField.");
            ScopeProvider = databaseContextScopeProviderField.GetValue(databaseContext);

            ScopeProviderAmbientOrNoScopeMethod = scopeProviderType.GetMethod("GetAmbientOrNoScope", BindingFlags.Instance | BindingFlags.Public);
            if (ScopeProviderAmbientOrNoScopeMethod == null) throw new Exception("oops: ScopeProviderAmbientOrNoScopeMethod.");
            ScopeDatabaseProperty = scopeType.GetProperty("Database", BindingFlags.Instance | BindingFlags.Public);
            if (ScopeDatabaseProperty == null) throw new Exception("oops: ScopeDatabaseProperty.");

            OpenMethod = databaseType.GetMethod("OpenSharedConnection", BindingFlags.Instance | BindingFlags.Public);
            //CloseMethod = databaseType.GetMethod("CloseSharedConnection", BindingFlags.Instance | BindingFlags.Public);

            ConnectionProperty = umbracoDatabaseType.GetProperty("Connection", BindingFlags.Instance | BindingFlags.Public);
            TransactionField = databaseType.GetField("_transaction", BindingFlags.Instance | BindingFlags.NonPublic);

            var profilerAssembly = Assembly.Load("MiniProfiler");
            var profiledDbConnectionType = profilerAssembly.GetType("StackExchange.Profiling.Data.ProfiledDbConnection");
            InnerConnectionProperty = profiledDbConnectionType.GetProperty("InnerConnection", BindingFlags.Instance | BindingFlags.Public);
            var profiledDbTransactionType = profilerAssembly.GetType("StackExchange.Profiling.Data.ProfiledDbTransaction");
            InnerTransactionProperty = profiledDbTransactionType.GetProperty("WrappedTransaction", BindingFlags.Instance | BindingFlags.Public);
        }

        public CurrentConnectionUsing()
        {
            var scope = ScopeProviderAmbientOrNoScopeMethod.Invoke(ScopeProvider, NoArgs);
            _database = ScopeDatabaseProperty.GetValue(scope);

            var connection = ConnectionProperty.GetValue(_database, NoArgs);
            // we have to open to make sure that we *do* have a connection
            if (connection == null)
                OpenMethod.Invoke(_database, NoArgs);
        }

        public IDbConnection Connection
        {
            get
            {
                var connection = ConnectionProperty.GetValue(_database, NoArgs);
                if (connection == null) return null;
                var inner = InnerConnectionProperty.GetValue(connection, NoArgs);
                return inner as IDbConnection;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                var transaction = TransactionField.GetValue(_database);
                if (transaction == null) return null;
                var inner = InnerTransactionProperty.GetValue(transaction, NoArgs);
                return inner as IDbTransaction;
            }
        }

        public void Dispose()
        {
            // this was required when we *always* opened the connection
            // but now we open it only if it's not opened already and then,
            // we assume that Core will dispose of it somehow - since it
            // hasn't been opened twice.

            // we have to close since we have opened
            //CloseMethod.Invoke(_database, NoArgs);
        }
    }
    
    #endregion
}
