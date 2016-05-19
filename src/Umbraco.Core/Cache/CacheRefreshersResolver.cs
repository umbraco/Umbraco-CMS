using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Cache
{
	

	/// <summary>
	/// A resolver to return all ICacheRefresher objects
	/// </summary>
	internal sealed class CacheRefreshersResolver : ContainerLazyManyObjectsResolver<CacheRefreshersResolver, ICacheRefresher>
	{
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="serviceContainer"></param>
	    /// <param name="logger"></param>
	    /// <param name="refreshers"></param>		
	    internal CacheRefreshersResolver(IServiceContainer serviceContainer, ILogger logger, Func<IEnumerable<Type>> refreshers)
            : base(serviceContainer, logger, refreshers)
		{
			
		}
        
		/// <summary>
		/// Gets the <see cref="ICacheRefresher"/> implementations.
		/// </summary>
		public IEnumerable<ICacheRefresher> CacheRefreshers => Values;

	    /// <summary>
	    /// Returns an instance for the type identified by its unique type identifier.
	    /// </summary>
	    /// <param name="id">The type identifier.</param>
	    /// <returns>The value of the type uniquely identified by <paramref name="id"/>.</returns>
	    public ICacheRefresher GetById(Guid id)
	    {
	        return Values.FirstOrDefault(x => x.UniqueIdentifier == id);
	    }
        
	}
}
