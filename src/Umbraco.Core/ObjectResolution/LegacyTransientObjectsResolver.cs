using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.Logging;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// The base class for old legacy factories such as the DataTypeFactory or CacheResolverFactory.
	/// </summary>
	/// <typeparam name="TResolver">The type of the concrete resolver class.</typeparam>
	/// <typeparam name="TResolved">The type of the resolved objects.</typeparam>
	/// <remarks>
	/// This class contains basic functionality to mimic the functionality in these old factories since they all return 
	/// transient objects (though this should be changed) and the method GetById needs to lookup a type to an ID and since 
	/// these old classes don't contain metadata, the objects need to be instantiated first to get their metadata, we then store this
	/// for use in the GetById method.
	/// </remarks>
	internal abstract class LegacyTransientObjectsResolver<TResolver, TResolved> : LazyManyObjectsResolverBase<TResolver, TResolved>
		where TResolved : class
        where TResolver : ResolverBase
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private ConcurrentDictionary<Guid, Type> _id2Type;

		#region Constructors

	    /// <summary>
	    /// Initializes a new instance of the <see cref="LegacyTransientObjectsResolver{TResolver, TResolved}"/> class with an initial list of object types.
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <param name="logger"></param>
	    /// <param name="value">A function returning the list of object types.</param>
	    /// <remarks>
	    /// We are creating Transient instances (new instances each time) because this is how the legacy code worked and
	    /// I don't want to muck anything up by changing them to application based instances. 
	    /// TODO: However, it would make much more sense to do this and would speed up the application plus this would make the GetById method much easier.
	    /// </remarks>
	    protected LegacyTransientObjectsResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> value)
            : base(serviceProvider, logger, value, ObjectLifetimeScope.Transient) //  new objects every time
		{ }
		#endregion

		/// <summary>
		/// Returns the unique identifier of the type of a specified object.
		/// </summary>
		/// <param name="value">The object.</param>
		/// <returns>The unique identifier of the type of <paramref name="value"/>.</returns>
		protected abstract Guid GetUniqueIdentifier(TResolved value); 

		/// <summary>
		/// Returns a new instance for the type identified by its unique type identifier.
		/// </summary>
		/// <param name="id">The type identifier.</param>
		/// <returns>The value of the type uniquely identified by <paramref name="id"/>.</returns>
		public TResolved GetById(Guid id)
		{
			EnsureIsInitialized();
			return _id2Type.ContainsKey(id) == false
			       	? null
                    : ServiceProvider.GetService(_id2Type[id]) as TResolved;
		}

		/// <summary>
		/// Populates the identifiers-to-types dictionnary.
		/// </summary>
		/// <remarks>
		/// <para>This allow us to instantiate a type by ID since these legacy types doesn't contain any metadata.</para>
		/// <para>We instanciate all types once to get their unique identifier, then build the dictionary so that
		/// when GetById is called, we can instanciate a single object.</para>
		/// </remarks>
		protected void EnsureIsInitialized()
		{
			using (var l = new UpgradeableReadLock(_lock))
			{
				if (_id2Type == null)
				{
					l.UpgradeToWriteLock();

					_id2Type = new ConcurrentDictionary<Guid, Type>();
					foreach (var value in Values)
					{
						_id2Type.TryAdd(GetUniqueIdentifier(value), value.GetType());
					}
				}
			}
		}

	}
}