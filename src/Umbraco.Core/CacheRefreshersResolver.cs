using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace Umbraco.Core
{
	///// <summary>
	///// A resolver to return all ICacheRefresher objects
	///// </summary>
	//internal sealed class CacheRefreshersResolver : LegacyTransientObjectsResolver<ICacheRefresher>
	//{

	//    #region Singleton

	//    private static readonly CacheRefreshersResolver Instance = new CacheRefreshersResolver(PluginTypeResolver.Current.ResolveCacheRefreshers());

	//    public static CacheRefreshersResolver Current
	//    {
	//        get { return Instance; }
	//    }
	//    #endregion

	//    #region Constructors
	//    static CacheRefreshersResolver() { }

	//    /// <summary>
	//    /// Constructor
	//    /// </summary>
	//    /// <param name="refreshers"></param>		
	//    internal CacheRefreshersResolver(IEnumerable<Type> refreshers)
	//        : base(refreshers)
	//    {

	//    }
	//    #endregion

	//    /// <summary>
	//    /// Gets the <see cref="ICacheRefresher"/> implementations.
	//    /// </summary>
	//    public IEnumerable<ICacheRefresher> CacheResolvers
	//    {
	//        get
	//        {
	//            EnsureRefreshersList();
	//            return Values;
	//        }
	//    }

	//    protected override Guid GetUniqueIdentifier(ICacheRefresher obj)
	//    {
	//        return obj.UniqueIdentifier;
	//    }
	//}

	/// <summary>
	/// A resolver to return all ICacheRefresher objects
	/// </summary>
	internal sealed class CacheRefreshersResolver : LegacyTransientObjectsResolver<CacheRefreshersResolver, ICacheRefresher>
	{
	
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="refreshers"></param>		
		internal CacheRefreshersResolver(IEnumerable<Type> refreshers)
			: base(refreshers)
		{
			
		}


		/// <summary>
		/// Gets the <see cref="ICacheRefresher"/> implementations.
		/// </summary>
		public IEnumerable<ICacheRefresher> CacheResolvers
		{
			get
			{
				EnsureRefreshersList();
				return Values;
			}
		}

		protected override Guid GetUniqueIdentifier(ICacheRefresher obj)
		{
			return obj.UniqueIdentifier;
		}
	}
}
