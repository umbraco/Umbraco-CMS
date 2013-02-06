using System;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;

namespace umbraco
{
	
    [Obsolete("This class is no longer used, use Umbraco.Web.Cache.LibraryCacheRefresher instead")]
    public class LibraryCacheRefresher : Umbraco.Web.Cache.CacheRefresherEventHandler
	{
		
	}
}