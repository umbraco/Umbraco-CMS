using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Umbraco.Core.Resolving;
using umbraco.interfaces;

namespace Umbraco.Core
{
	internal sealed class CacheRefreshersResolver : ManyObjectResolverBase<ICacheRefresher>
	{

		#region Singleton

		private static readonly CacheRefreshersResolver Instance = new CacheRefreshersResolver(PluginTypeResolver.Current.ResolveCacheRefreshers());

		public static CacheRefreshersResolver Current
		{
			get { return Instance; }
		}
		#endregion

		#region Constructors
		static CacheRefreshersResolver() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="refreshers"></param>
		/// <remarks>
		/// We are creating Transient instances (new instances each time) because this is how the legacy code worked and
		/// I don't want to muck anything up by changing them to application based instances. 
		/// TODO: However, it would make much more sense to do this and would speed up the application plus this would make the GetById method much easier.
		/// </remarks>
		internal CacheRefreshersResolver(IEnumerable<Type> refreshers)
			: base(true)
		{
			foreach (var l in refreshers)
			{
				this.Add(l);
			}
		}
		#endregion

		/// <summary>
		/// Maintains a list of Ids and their types when first call to CacheResolvers or GetById occurs, this is used
		/// in order to return a single object by id without instantiating the entire type stack.
		/// </summary>
		private static ConcurrentDictionary<Guid, Type> _refreshers;
		private readonly ReaderWriterLockSlim _lock= new ReaderWriterLockSlim();

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

		/// <summary>
		/// Returns a new ICacheRefresher instance by id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ICacheRefresher GetById(Guid id)
		{
			EnsureRefreshersList();
			return !_refreshers.ContainsKey(id)
			       	? null
			       	: PluginTypeResolver.Current.CreateInstance<ICacheRefresher>(_refreshers[id]);
		}

		/// <summary>
		/// Populates the refreshers dictionary to allow us to instantiate a type by Id since the ICacheRefresher type doesn't contain any metadata
		/// </summary>
		private void EnsureRefreshersList()
		{
			using (var l = new UpgradeableReadLock(_lock))
			{
				if (_refreshers == null)
				{
					l.UpgradeToWriteLock();
					_refreshers = new ConcurrentDictionary<Guid, Type>();
					foreach(var v in Values)
					{
						_refreshers.TryAdd(v.UniqueIdentifier, v.GetType());
					}
				}
			}
		}
		
	}
}
