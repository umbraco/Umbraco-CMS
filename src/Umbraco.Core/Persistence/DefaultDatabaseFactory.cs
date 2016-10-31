using System;
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
            // no http context, create the call context object
            // NOTHING is going to track the object and it is the responsibility of the caller to release it!
            // using the ReleaseDatabase method.
            if (HttpContext.Current == null)
            {
                LogHelper.Debug<DefaultDatabaseFactory>("Get NON http [T" + Environment.CurrentManagedThreadId + "]");
                var value = NonContextValue;
                if (value != null) return value;
                lock (Locker)
                {
                    value = NonContextValue;
                    if (value != null) return value;

                    LogHelper.Debug<DefaultDatabaseFactory>("Create NON http [T" + Environment.CurrentManagedThreadId + "]");
                    NonContextValue = value = string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                                            ? new UmbracoDatabase(ConnectionString, ProviderName, _logger)
                                            : new UmbracoDatabase(_connectionStringName, _logger);

                    return value;
                }
            }

            // we have an http context, so only create one per request.
            // UmbracoDatabase is marked IDisposeOnRequestEnd and therefore will be disposed when
            // UmbracoModule attempts to dispose the relevant HttpContext items. so we DO dispose
            // connections at the end of each request. no need to call ReleaseDatabase.
            LogHelper.Debug<DefaultDatabaseFactory>("Get http [T" + Environment.CurrentManagedThreadId + "]");
            if (HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)) == false)
            {
                LogHelper.Debug<DefaultDatabaseFactory>("Create http [T" + Environment.CurrentManagedThreadId + "]");
                HttpContext.Current.Items.Add(typeof(DefaultDatabaseFactory),
                                              string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                                                  ? new UmbracoDatabase(ConnectionString, ProviderName, _logger)
                                                  : new UmbracoDatabase(_connectionStringName, _logger));
            }
            return (UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
        }

        // releases the "context" database
        public void ReleaseDatabase()
        {
            if (HttpContext.Current == null)
            {
                var value = NonContextValue;
                if (value != null) value.Dispose();
                NonContextValue = null;
            }
            else
            {
                var db = (UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
                if (db != null) db.Dispose();
                HttpContext.Current.Items.Remove(typeof(DefaultDatabaseFactory));
            }
        }

        protected override void DisposeResources()
		{
            ReleaseDatabase();
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
            SafeCallContext.Register(DetachDatabase, AttachDatabase);
        }

        // detaches the current database
        // ie returns the database and remove it from whatever is "context"
        private static UmbracoDatabase DetachDatabase()
        {
            if (HttpContext.Current == null)
            {
                var db = NonContextValue;
                NonContextValue = null;
                return db;
            }
            else
            {
                var db = (UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
                HttpContext.Current.Items.Remove(typeof(DefaultDatabaseFactory));
                return db;
            }
        }

        // attach a current database
        // ie assign it to whatever is "context"
        // throws if there already is a database
        private static void AttachDatabase(object o)
        {
            var database = o as UmbracoDatabase;
            if (o != null && database == null) throw new ArgumentException("Not an UmbracoDatabase.", "o");

            if (HttpContext.Current == null)
            {
                if (NonContextValue != null) throw new InvalidOperationException();
                if (database != null) NonContextValue = database;
            }
            else
            {
                if (HttpContext.Current.Items[typeof(DefaultDatabaseFactory)] != null) throw new InvalidOperationException();
                if (database != null) HttpContext.Current.Items[typeof(DefaultDatabaseFactory)] = database;
            }
        }

        #endregion
    }
}