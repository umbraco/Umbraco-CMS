using System;
using Umbraco.Web.Cache;

namespace umbraco.presentation.cache
{

    /// <summary>
    /// pageRefresher is the standard CacheRefresher used by Load-Balancing in Umbraco.
    /// 
    /// If Load balancing is enabled (by default disabled, is set in umbracoSettings.config) pageRefresher will be called
    /// everytime content is added/updated/removed to ensure that the content cache is identical on all load balanced servers
    /// 
    /// pageRefresger inherits from interfaces.ICacheRefresher.
    /// </summary>
    [Obsolete("This class is no longer in use, use Umbraco.Web.Cache.PageCacheRefresher instead")]
    public class pageRefresher : PageCacheRefresher
	{        		
	}
}
