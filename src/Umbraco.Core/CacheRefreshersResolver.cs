using System;
using System.Collections.Generic;
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
		/// <param name="refreshers"></param>		
		internal CacheRefreshersResolver(Func<IEnumerable<Type>> refreshers)
			: base(refreshers)
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
