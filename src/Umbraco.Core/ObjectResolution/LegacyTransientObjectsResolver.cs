using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// A base resolver used for old legacy factories such as the DataTypeFactory or CacheResolverFactory.
	/// </summary>
	/// <typeparam name="TResolver"></typeparam>
	/// <typeparam name="TResolved"> </typeparam>
	/// <remarks>
	/// This class contains basic functionality to mimic the functionality in these old factories since they all return 
	/// transient objects (though this should be changed) and the method GetById needs to lookup a type to an ID and since 
	/// these old classes don't contain metadata, the objects need to be instantiated first to get their metadata, we then store this
	/// for use in the GetById method.
	/// </remarks>
	internal abstract class LegacyTransientObjectsResolver<TResolver, TResolved> : ManyObjectsResolverBase<TResolver, TResolved>
		where TResolved : class
		where TResolver : class
	{

		#region Constructors
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="refreshers"></param>
		/// <remarks>
		/// We are creating Transient instances (new instances each time) because this is how the legacy code worked and
		/// I don't want to muck anything up by changing them to application based instances. 
		/// TODO: However, it would make much more sense to do this and would speed up the application plus this would make the GetById method much easier.
		/// </remarks>
		protected LegacyTransientObjectsResolver(IEnumerable<Type> refreshers)
			: base(ObjectLifetimeScope.Transient) // false = new objects every time
		{
			foreach (var l in refreshers)
			{
				this.AddType(l);
			}
		}
		#endregion

		/// <summary>
		/// Maintains a list of Ids and their types when first call to CacheResolvers or GetById occurs, this is used
		/// in order to return a single object by id without instantiating the entire type stack.
		/// </summary>
		private ConcurrentDictionary<Guid, Type> _trackIdToType;
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		/// <summary>
		/// method to return the unique id for type T
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		protected abstract Guid GetUniqueIdentifier(TResolved obj); 

		/// <summary>
		/// Returns a new ICacheRefresher instance by id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public TResolved GetById(Guid id)
		{
			EnsureRefreshersList();
			return !_trackIdToType.ContainsKey(id)
			       	? null
					: PluginManager.Current.CreateInstance<TResolved>(_trackIdToType[id]);
		}

		/// <summary>
		/// Populates the refreshers dictionary to allow us to instantiate a type by Id since the ICacheRefresher type doesn't contain any metadata
		/// </summary>
		protected void EnsureRefreshersList()
		{
			using (var l = new UpgradeableReadLock(_lock))
			{
				if (_trackIdToType == null)
				{
					l.UpgradeToWriteLock();
					_trackIdToType = new ConcurrentDictionary<Guid, Type>();
					foreach (var v in Values)
					{
						_trackIdToType.TryAdd(GetUniqueIdentifier(v), v.GetType());
					}
				}
			}
		}

	}
}