using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using umbraco.interfaces;

namespace Umbraco.Core
{
	

	/// <summary>
	/// A resolver to return all ICacheRefresher objects
	/// </summary>
	internal sealed class CacheRefreshersResolver : LegacyTransientObjectsResolver<CacheRefreshersResolver, ICacheRefresher>
	{
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <param name="logger"></param>
	    /// <param name="refreshers"></param>		
	    internal CacheRefreshersResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> refreshers)
            : base(serviceProvider, logger, refreshers)
		{
			
		}


		/// <summary>
		/// Gets the <see cref="ICacheRefresher"/> implementations.
		/// </summary>
		public IEnumerable<ICacheRefresher> CacheRefreshers
		{
			get
			{
				EnsureIsInitialized();
				return Values;
			}
		}

		protected override Guid GetUniqueIdentifier(ICacheRefresher obj)
		{
			return obj.UniqueIdentifier;
		}
	}
}
