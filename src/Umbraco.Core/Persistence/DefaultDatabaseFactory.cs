using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// The default implementation for the IDatabaseFactory
	/// </summary>
	/// <remarks>
	/// If we are running in an http context
	/// it will create on per context, otherwise it will a global singleton object which is NOT thread safe
	/// since we need (at least) a new instance of the database object per thread.
	/// </remarks>
	internal class DefaultDatabaseFactory : DisposableObject, IDatabaseFactory
	{
		private static volatile UmbracoDatabase _globalInstance = null;
		private static readonly object Locker = new object();

		public UmbracoDatabase CreateDatabase()
		{
			//no http context, create the singleton global object
			if (HttpContext.Current == null)
			{
				if (_globalInstance == null)
				{
					lock (Locker)
					{
						//double check
						if (_globalInstance == null)
						{
							_globalInstance = new UmbracoDatabase(GlobalSettings.UmbracoConnectionName);
						}
					}
				}
				return _globalInstance;
			}

			//we have an http context, so only create one per request
			if (!HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)))
			{
				HttpContext.Current.Items.Add(typeof(DefaultDatabaseFactory), new UmbracoDatabase(GlobalSettings.UmbracoConnectionName));
			}
			return (UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
		}

		protected override void DisposeResources()
		{
			if (HttpContext.Current == null)
			{
				_globalInstance.Dispose();
			}
			else
			{
				if (HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)))
				{
					((UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)]).Dispose();
				}
			}
		}
	}
}