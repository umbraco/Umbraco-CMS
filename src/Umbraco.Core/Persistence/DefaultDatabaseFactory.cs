using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// The default implementation for the IDatabaseFactory
	/// </summary>
	/// <remarks>
	/// If we are running in an http context
	/// it will create one per context, otherwise it will be a global singleton object which is NOT thread safe
	/// since we need (at least) a new instance of the database object per thread.
	/// </remarks>
	internal class DefaultDatabaseFactory : DisposableObject, IDatabaseFactory
	{
	    private readonly string _connectionStringName;
	    private readonly ILogger _logger;
	    public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }

        // NO! see notes in v8 HybridAccessorBase
        //[ThreadStatic]
        //private static volatile UmbracoDatabase _nonHttpInstance;

        private const string ItemKey = "Umbraco.Core.Persistence.DefaultDatabaseFactory";

        private static UmbracoDatabase NonContextValue
        {
            get { return (UmbracoDatabase) CallContext.LogicalGetData(ItemKey); }
            set
            {
                if (value == null) CallContext.FreeNamedDataSlot(ItemKey);
                else CallContext.LogicalSetData(ItemKey, value);
            }
        }

        private static readonly object Locker = new object();

	    /// <summary>
	    /// Constructor accepting custom connection string
	    /// </summary>
	    /// <param name="connectionStringName">Name of the connection string in web.config</param>
	    /// <param name="logger"></param>
	    public DefaultDatabaseFactory(string connectionStringName, ILogger logger)
		{
            if (logger == null) throw new ArgumentNullException("logger");
	        Mandate.ParameterNotNullOrEmpty(connectionStringName, "connectionStringName");

            //if (NonContextValue != null) throw new Exception("NonContextValue is not null.");

            _connectionStringName = connectionStringName;
	        _logger = logger;
		}

	    /// <summary>
	    /// Constructor accepting custom connectino string and provider name
	    /// </summary>
	    /// <param name="connectionString">Connection String to use with Database</param>
	    /// <param name="providerName">Database Provider for the Connection String</param>
	    /// <param name="logger"></param>
	    public DefaultDatabaseFactory(string connectionString, string providerName, ILogger logger)
		{
            if (logger == null) throw new ArgumentNullException("logger");
	        Mandate.ParameterNotNullOrEmpty(connectionString, "connectionString");
			Mandate.ParameterNotNullOrEmpty(providerName, "providerName");

            //if (NonContextValue != null) throw new Exception("NonContextValue is not null.");

            ConnectionString = connectionString;
			ProviderName = providerName;
            _logger = logger;
		}

		public UmbracoDatabase CreateDatabase()
		{
            UmbracoDatabase database;

            // gets or creates a database, using either the call context (if no http context) or
            // the current request context (http context) to store it. once done using the database,
            // it should be disposed - which will remove it from whatever context it is currently
            // stored in. this is automatic with http context because UmbracoDatabase implements
            // IDisposeOnRequestEnd, but NOT with call context.

            if (HttpContext.Current == null)
            {
                database = NonContextValue;
                if (database == null)
                {
                    lock (Locker)
                    {
                        database = NonContextValue;
                        if (database == null)
                        {
                            database = CreateDatabaseInstance(ContextOwner.CallContext);
                            NonContextValue = database;
                        }
#if DEBUG_DATABASES
                        else
                        {
                            Log("Get lcc", database);
                        }
#endif
                    }
                }
#if DEBUG_DATABASES
                else
                {
                    Log("Get lcc", database);
                }
#endif
                return database;
            }

		    if (HttpContext.Current.Items.Contains(typeof (DefaultDatabaseFactory)) == false)
		    {
		        database = CreateDatabaseInstance(ContextOwner.HttpContext);
                HttpContext.Current.Items.Add(typeof (DefaultDatabaseFactory), database);
		    }
		    else
		    {
		        database = (UmbracoDatabase) HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
#if DEBUG_DATABASES
                Log("Get ctx", database);
#endif
            }

            return database;
		}

        // called by UmbracoDatabase when disposed, so that the factory can de-list it from context
	    internal void OnDispose(UmbracoDatabase disposing)
	    {
	        var value = disposing;
	        switch (disposing.ContextOwner)
	        {
                case ContextOwner.CallContext:
                    value = NonContextValue;
                    break;
                case ContextOwner.HttpContext:
                    value = (UmbracoDatabase) HttpContext.Current.Items[typeof (DefaultDatabaseFactory)];
                    break;
	        }

            if (value != null && value.InstanceId != disposing.InstanceId) throw new Exception("panic: wrong db.");

            switch (disposing.ContextOwner)
            {
                case ContextOwner.CallContext:
                    NonContextValue = null;
#if DEBUG_DATABASES
                    Log("Clr lcc", disposing);
#endif
                    break;
                case ContextOwner.HttpContext:
                    HttpContext.Current.Items.Remove(typeof(DefaultDatabaseFactory));
#if DEBUG_DATABASES
                    Log("Clr ctx", disposing);
#endif
                    break;
            }

            disposing.ContextOwner = ContextOwner.None;

#if DEBUG_DATABASES
            _databases.Remove(value);
#endif
        }

#if DEBUG_DATABASES
        // helps identifying when non-httpContext databases are created by logging the stack trace
        private void LogCallContextStack()
        {
            var trace = Environment.StackTrace;
            if (trace.IndexOf("ScheduledPublishing") > 0)
                LogHelper.Debug<DefaultDatabaseFactory>("CallContext: Scheduled Publishing");
            else if (trace.IndexOf("TouchServerTask") > 0)
                LogHelper.Debug<DefaultDatabaseFactory>("CallContext: Server Registration");
            else if (trace.IndexOf("LogScrubber") > 0)
                LogHelper.Debug<DefaultDatabaseFactory>("CallContext: Log Scrubber");
            else
                LogHelper.Debug<DefaultDatabaseFactory>("CallContext: " + Environment.StackTrace);
        }

        private readonly List<UmbracoDatabase> _databases = new List<UmbracoDatabase>();

        // helps identifying database leaks by keeping track of all instances
        public List<UmbracoDatabase> Databases { get { return _databases; } }

        private static void Log(string message, UmbracoDatabase database)
        {
            LogHelper.Debug<DefaultDatabaseFactory>(message + " (" + (database == null ? "" : database.InstanceSid) + ").");
        }
#endif

        internal enum ContextOwner
        {
            None,
            HttpContext,
            CallContext
        }

        internal UmbracoDatabase CreateDatabaseInstance(ContextOwner contextOwner)
	    {
            var database = string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                    ? new UmbracoDatabase(ConnectionString, ProviderName, _logger)
                    : new UmbracoDatabase(_connectionStringName, _logger);
	        database.ContextOwner = contextOwner;
	        database.DatabaseFactory = this;
            //database.EnableSqlTrace = true;
#if DEBUG_DATABASES
            Log("Create " + contextOwner, database);
            if (contextOwner == ContextOwner.CallContext)
                LogCallContextStack();
            _databases.Add(database);
#endif
            return database;
	    }

        protected override void DisposeResources()
		{
            UmbracoDatabase database;

            if (HttpContext.Current == null)
            {
                database = NonContextValue;
#if DEBUG_DATABASES
                Log("Release lcc", database);
#endif
            }
            else
            {
                database = (UmbracoDatabase) HttpContext.Current.Items[typeof (DefaultDatabaseFactory)];
#if DEBUG_DATABASES
                Log("Release ctx", database);
#endif
            }

            if (database != null) database.Dispose(); // removes it from call context
        }

        // during tests, the thread static var can leak between tests
        // this method provides a way to force-reset the variable
        internal void ResetForTests()
	    {
            var value = NonContextValue;
            if (value != null) value.Dispose();
            NonContextValue = null;
        }

        #region SafeCallContext

        // see notes in SafeCallContext - need to do this since we are using
        // the logical call context...

        static DefaultDatabaseFactory()
        {
            SafeCallContext.Register(DetachAmbientDatabase, AttachAmbientDatabase);
        }

        // gets a value indicating whether there is an ambient database
	    internal static bool HasAmbientDatabase
	    {
	        get
	        {
	            return HttpContext.Current == null
	                ? NonContextValue != null
	                : HttpContext.Current.Items[typeof (DefaultDatabaseFactory)] != null;
	        }
	    }

        // detaches the current database
        // ie returns the database and remove it from whatever is "context"
        internal static UmbracoDatabase DetachAmbientDatabase()
        {
            UmbracoDatabase database;

            if (HttpContext.Current == null)
            {
                database = NonContextValue;
                NonContextValue = null;
            }
            else
            {
                database = (UmbracoDatabase) HttpContext.Current.Items[typeof (DefaultDatabaseFactory)];
                HttpContext.Current.Items.Remove(typeof (DefaultDatabaseFactory));
            }

            if (database != null) database.ContextOwner = ContextOwner.None;
            return database;
        }

        // attach a current database
        // ie assign it to whatever is "context"
        // throws if there already is a database
        internal static void AttachAmbientDatabase(object o)
        {
            var database = o as UmbracoDatabase;
            if (o != null && database == null) throw new ArgumentException("Not an UmbracoDatabase.", "o");

            if (HttpContext.Current == null)
            {
                if (NonContextValue != null) throw new InvalidOperationException();
                if (database == null) return;

                NonContextValue = database;
                database.ContextOwner = ContextOwner.CallContext;
            }
            else
            {
                if (HttpContext.Current.Items[typeof (DefaultDatabaseFactory)] != null) throw new InvalidOperationException();
                if (database == null) return;

                HttpContext.Current.Items[typeof (DefaultDatabaseFactory)] = database;
                database.ContextOwner = ContextOwner.HttpContext;
            }
        }

        #endregion
    }
}