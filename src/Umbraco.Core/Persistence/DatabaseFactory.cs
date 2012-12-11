using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Creates a database object for PetaPoco depending on the context. If we are running in an http context
    /// it will create on per context, otherwise it will create a global singleton
    /// </summary>
    /// <remarks>
    /// Because the Database is created static, the configuration has to be checked and set in 
    /// another class, which is where the DatabaseContext comes in.
    /// </remarks>
    internal sealed class DatabaseFactory
    {

        #region Singleton

		private static readonly Lazy<DatabaseFactory> lazy = new Lazy<DatabaseFactory>(() => new DatabaseFactory());
	    private static volatile Database _globalInstance = null;
	    private static readonly object Locker = new object();
        public static DatabaseFactory Current { get { return lazy.Value; } }

        private DatabaseFactory()
        {
        }

        #endregion

	    /// <summary>
	    /// Returns an instance of the PetaPoco database
	    /// </summary>
	    /// <remarks>
		/// If we are running in an http context
		/// it will create on per context, otherwise it will create transient objects (new instance each time)    
	    /// </remarks>
	    public Database Database
	    {
		    get
		    {
			    //no http context, create the singleton global object
			    if (HttpContext.Current == null)
			    {
					if (_globalInstance == null)
					{
						lock(Locker)
						{
							//double check
							if (_globalInstance == null)
							{
								_globalInstance = new Database(GlobalSettings.UmbracoConnectionName);
							}
						}
					}
				    return _globalInstance;
			    }

			    //we have an http context, so only create one per request
			    if (!HttpContext.Current.Items.Contains(typeof (DatabaseFactory)))
			    {
				    HttpContext.Current.Items.Add(typeof (DatabaseFactory), new Database(GlobalSettings.UmbracoConnectionName));
			    }
			    return (Database) HttpContext.Current.Items[typeof (DatabaseFactory)];
		    }
	    }
    }
}