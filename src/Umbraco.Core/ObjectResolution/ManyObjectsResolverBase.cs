using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Web;
using Umbraco.Core.Logging;

namespace Umbraco.Core.ObjectResolution
{
    /// <summary>
	/// The base class for all many-objects resolvers.
	/// </summary>
	/// <typeparam name="TResolver">The type of the concrete resolver class.</typeparam>
	/// <typeparam name="TResolved">The type of the resolved objects.</typeparam>
	public abstract class ManyObjectsResolverBase<TResolver, TResolved> : ResolverBase<TResolver>
		where TResolved : class
        where TResolver : ResolverBase
	{
		private Lazy<IEnumerable<TResolved>> _applicationInstances;
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly string _httpContextKey;
		private readonly List<Type> _instanceTypes = new List<Type>();
	    private IEnumerable<TResolved> _sortedValues;

		private int _defaultPluginWeight = 10;

		#region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects,
        /// and an optional lifetime scope.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="scope">The lifetime scope of instantiated objects, default is per Application.</param>
        /// <remarks>If <paramref name="scope"/> is per HttpRequest then there must be a current HttpContext.</remarks>
        /// <exception cref="InvalidOperationException"><paramref name="scope"/> is per HttpRequest but the current HttpContext is null.</exception>
        protected ManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (logger == null) throw new ArgumentNullException("logger");
            CanResolveBeforeFrozen = false;
            if (scope == ObjectLifetimeScope.HttpRequest)
            {
                if (HttpContext.Current == null)
                    throw new InvalidOperationException("Use alternative constructor accepting a HttpContextBase object in order to set the lifetime scope to HttpRequest when HttpContext.Current is null");

                CurrentHttpContext = new HttpContextWrapper(HttpContext.Current);
            }

            ServiceProvider = serviceProvider;
            Logger = logger;
            LifetimeScope = scope;
            if (scope == ObjectLifetimeScope.HttpRequest)
                _httpContextKey = GetType().FullName;
            _instanceTypes = new List<Type>();

            InitializeAppInstances();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
		protected ManyObjectsResolverBase(ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, scope)
		{
			
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects,
        /// with creation of objects based on an HttpRequest lifetime scope.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="httpContext">The HttpContextBase corresponding to the HttpRequest.</param>
        /// <exception cref="ArgumentNullException"><paramref name="httpContext"/> is <c>null</c>.</exception>
        protected ManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, HttpContextBase httpContext)
		{
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            CanResolveBeforeFrozen = false;
            Logger = logger;
			LifetimeScope = ObjectLifetimeScope.HttpRequest;
			_httpContextKey = GetType().FullName;
            ServiceProvider = serviceProvider;
            CurrentHttpContext = httpContext;
			_instanceTypes = new List<Type>();

            InitializeAppInstances();
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected ManyObjectsResolverBase(HttpContextBase httpContext)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, httpContext)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an initial list of object types,
        /// and an optional lifetime scope.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="value">The list of object types.</param>
        /// <param name="scope">The lifetime scope of instantiated objects, default is per Application.</param>
        /// <remarks>If <paramref name="scope"/> is per HttpRequest then there must be a current HttpContext.</remarks>
        /// <exception cref="InvalidOperationException"><paramref name="scope"/> is per HttpRequest but the current HttpContext is null.</exception>
        protected ManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(serviceProvider, logger, scope)
		{
			_instanceTypes = value.ToList();
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected ManyObjectsResolverBase(IEnumerable<Type> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, value, scope)
        {
            
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an initial list of objects,
		/// with creation of objects based on an HttpRequest lifetime scope.
		/// </summary>
		/// <param name="httpContext">The HttpContextBase corresponding to the HttpRequest.</param>
		/// <param name="value">The list of object types.</param>
		/// <exception cref="ArgumentNullException"><paramref name="httpContext"/> is <c>null</c>.</exception>
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected ManyObjectsResolverBase(HttpContextBase httpContext, IEnumerable<Type> value)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, httpContext)
		{
			_instanceTypes = value.ToList();
		} 
		#endregion

        private void InitializeAppInstances()
        {
            _applicationInstances = new Lazy<IEnumerable<TResolved>>(() => CreateInstances().ToArray());
        }

		/// <summary>
		/// Gets or sets a value indicating whether the resolver can resolve objects before resolution is frozen.
		/// </summary>
		/// <remarks>This is false by default and is used for some special internal resolvers.</remarks>
		internal bool CanResolveBeforeFrozen { get; set; }

		/// <summary>
		/// Gets the list of types to create instances from.
		/// </summary>
		protected virtual IEnumerable<Type> InstanceTypes
		{
			get { return _instanceTypes; }
		}

		/// <summary>
		/// Gets or sets the <see cref="HttpContextBase"/> used to initialize this object, if any.
		/// </summary>
		/// <remarks>If not null, then <c>LifetimeScope</c> will be <c>ObjectLifetimeScope.HttpRequest</c>.</remarks>
		protected HttpContextBase CurrentHttpContext { get; private set; }

        /// <summary>
        /// Returns the service provider used to instantiate objects
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        public ILogger Logger { get; private set; }

        /// <summary>
		/// Gets or sets the lifetime scope of resolved objects.
		/// </summary>
		protected ObjectLifetimeScope LifetimeScope { get; private set; }

		/// <summary>
		/// Gets the resolved object instances, sorted by weight.
		/// </summary>
		/// <returns>The sorted resolved object instances.</returns>
		/// <remarks>
		/// <para>The order is based upon the <c>WeightedPluginAttribute</c> and <c>DefaultPluginWeight</c>.</para>
		/// <para>Weights are sorted ascendingly (lowest weights come first).</para>
		/// </remarks>
		protected IEnumerable<TResolved> GetSortedValues()
		{
            if (_sortedValues == null)
            {
                var values = Values.ToList();
                values.Sort((f1, f2) => GetObjectWeight(f1).CompareTo(GetObjectWeight(f2)));
                _sortedValues = values;
            }
            return _sortedValues;
		}

		/// <summary>
		/// Gets or sets the default type weight.
		/// </summary>
		/// <remarks>Determines the weight of types that do not have a <c>WeightedPluginAttribute</c> set on 
		/// them, when calling <c>GetSortedValues</c>.</remarks>
		protected virtual int DefaultPluginWeight
		{
			get { return _defaultPluginWeight; }
			set { _defaultPluginWeight = value; }
		}

        /// <summary>
        /// Returns the weight of an object for user with GetSortedValues
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
		protected virtual int GetObjectWeight(object o)
		{
			var type = o.GetType();
			var attr = type.GetCustomAttribute<WeightedPluginAttribute>(true);
			return attr == null ? DefaultPluginWeight : attr.Weight;
		}

		/// <summary>
		/// Gets the resolved object instances.
		/// </summary>
		/// <exception cref="InvalidOperationException"><c>CanResolveBeforeFrozen</c> is false, and resolution is not frozen.</exception>
		protected IEnumerable<TResolved> Values
		{
			get
			{
			    using (Resolution.Reader(CanResolveBeforeFrozen))
			    {
                    // note: we apply .ToArray() to the output of CreateInstance() because that is an IEnumerable that
                    // comes from the PluginManager we want to be _sure_ that it's not a Linq of some sort, but the
                    // instances have actually been instanciated when we return.

                    switch (LifetimeScope)
                    {
                        case ObjectLifetimeScope.HttpRequest:

                            // create new instances per HttpContext
                            if (CurrentHttpContext.Items[_httpContextKey] == null)
                            {
                                var instances = CreateInstances().ToArray();
                                var disposableInstances = instances.OfType<IDisposable>();
                                //Ensure anything resolved that is IDisposable is disposed when the request termintates
                                foreach (var disposable in disposableInstances)
                                {
                                    CurrentHttpContext.DisposeOnPipelineCompleted(disposable);
                                }
                                CurrentHttpContext.Items[_httpContextKey] = instances;
                            }
                            return (TResolved[])CurrentHttpContext.Items[_httpContextKey];
                            
                        case ObjectLifetimeScope.Application:

                            return _applicationInstances.Value;

                        case ObjectLifetimeScope.Transient:
                        default:
                            // create new instances each time
                            return CreateInstances().ToArray();
                    }
                }
			}
		}

		/// <summary>
		/// Creates the object instances for the types contained in the types collection.
		/// </summary>
		/// <returns>A list of objects of type <typeparamref name="TResolved"/>.</returns>
		protected virtual IEnumerable<TResolved> CreateInstances()
		{
			return ServiceProvider.CreateInstances<TResolved>(InstanceTypes, Logger);
		}

		#region Types collection manipulation

		/// <summary>
		/// Removes a type.
		/// </summary>
		/// <param name="value">The type to remove.</param>
		/// <exception cref="InvalidOperationException">the resolver does not support removing types, or 
		/// the type is not a valid type for the resolver.</exception>
		public virtual void RemoveType(Type value)
		{
			EnsureSupportsRemove();

			using (Resolution.Configuration)
			using (var l = new UpgradeableReadLock(_lock))
			{
				EnsureCorrectType(value);

				l.UpgradeToWriteLock();
				_instanceTypes.Remove(value);
			}
		}

		/// <summary>
		/// Removes a type.
		/// </summary>
		/// <typeparam name="T">The type to remove.</typeparam>
		/// <exception cref="InvalidOperationException">the resolver does not support removing types, or 
		/// the type is not a valid type for the resolver.</exception>
		public void RemoveType<T>()
            where T : TResolved
		{
			RemoveType(typeof(T));
		}

		/// <summary>
		/// Adds types.
		/// </summary>
		/// <param name="types">The types to add.</param>
		/// <remarks>The types are appended at the end of the list.</remarks>
		/// <exception cref="InvalidOperationException">the resolver does not support adding types, or 
		/// a type is not a valid type for the resolver, or a type is already in the collection of types.</exception>
		protected void AddTypes(IEnumerable<Type> types)
		{
			EnsureSupportsAdd();

			using (Resolution.Configuration)
			using (new WriteLock(_lock))
			{
				foreach(var t in types)
				{
					EnsureCorrectType(t);
                    if (_instanceTypes.Contains(t))
					{
						throw new InvalidOperationException(string.Format(
							"Type {0} is already in the collection of types.", t.FullName));
					}
					_instanceTypes.Add(t);	
				}				
			}
		}

		/// <summary>
		/// Adds a type.
		/// </summary>
		/// <param name="value">The type to add.</param>
		/// <remarks>The type is appended at the end of the list.</remarks>
		/// <exception cref="InvalidOperationException">the resolver does not support adding types, or 
		/// the type is not a valid type for the resolver, or the type is already in the collection of types.</exception>
		public virtual void AddType(Type value)
		{
			EnsureSupportsAdd();

			using (Resolution.Configuration)
			using (var l = new UpgradeableReadLock(_lock))
			{
				EnsureCorrectType(value);
                if (_instanceTypes.Contains(value))
				{
					throw new InvalidOperationException(string.Format(
						"Type {0} is already in the collection of types.", value.FullName));
				}

				l.UpgradeToWriteLock();
				_instanceTypes.Add(value);
			}
		}

		/// <summary>
		/// Adds a type.
		/// </summary>
		/// <typeparam name="T">The type to add.</typeparam>
		/// <remarks>The type is appended at the end of the list.</remarks>
		/// <exception cref="InvalidOperationException">the resolver does not support adding types, or 
		/// the type is not a valid type for the resolver, or the type is already in the collection of types.</exception>
		public void AddType<T>()
            where T : TResolved
		{
			AddType(typeof(T));
		}

		/// <summary>
		/// Clears the list of types
		/// </summary>
		/// <exception cref="InvalidOperationException">the resolver does not support clearing types.</exception>
		public virtual void Clear()
		{
			EnsureSupportsClear();

			using (Resolution.Configuration)
			using (new WriteLock(_lock))
			{
				_instanceTypes.Clear();
			}
		}

        /// <summary>
        /// WARNING! Do not use this unless you know what you are doing, clear all types registered and instances
        /// created. Typically only used if a resolver is no longer used in an application and memory is to be GC'd
        /// </summary>
        internal void ResetCollections()
        {
            using (new WriteLock(_lock))
            {
                _instanceTypes.Clear();
                _sortedValues = null;
                _applicationInstances = null;
            }
        }

		/// <summary>
		/// Inserts a type at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the type should be inserted.</param>
		/// <param name="value">The type to insert.</param>
		/// <exception cref="InvalidOperationException">the resolver does not support inserting types, or 
		/// the type is not a valid type for the resolver, or the type is already in the collection of types.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
		public virtual void InsertType(int index, Type value)
		{			
			EnsureSupportsInsert();

			using (Resolution.Configuration)
			using (var l = new UpgradeableReadLock(_lock))
			{
				EnsureCorrectType(value);
                if (_instanceTypes.Contains(value))
				{
					throw new InvalidOperationException(string.Format(
						"Type {0} is already in the collection of types.", value.FullName));
				}

				l.UpgradeToWriteLock();
				_instanceTypes.Insert(index, value);
			}
		}

        /// <summary>
        /// Inserts a type at the beginning of the list.
        /// </summary>
        /// <param name="value">The type to insert.</param>
        /// <exception cref="InvalidOperationException">the resolver does not support inserting types, or 
        /// the type is not a valid type for the resolver, or the type is already in the collection of types.</exception>
        public virtual void InsertType(Type value)
        {
            InsertType(0, value);
        }

        /// <summary>
		/// Inserts a type at the specified index.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="index">The zero-based index at which the type should be inserted.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
		public void InsertType<T>(int index)
            where T : TResolved
		{
			InsertType(index, typeof(T));
		}

        /// <summary>
        /// Inserts a type at the beginning of the list.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        public void InsertType<T>()
            where T : TResolved
        {
            InsertType(0, typeof(T));
        }
        
        /// <summary>
		/// Inserts a type before a specified, already existing type.
		/// </summary>
		/// <param name="existingType">The existing type before which to insert.</param>
		/// <param name="value">The type to insert.</param>
		/// <exception cref="InvalidOperationException">the resolver does not support inserting types, or 
		/// one of the types is not a valid type for the resolver, or the existing type is not in the collection,
		/// or the new type is already in the collection of types.</exception>
		public virtual void InsertTypeBefore(Type existingType, Type value)
		{
			EnsureSupportsInsert();

			using (Resolution.Configuration)
			using (var l = new UpgradeableReadLock(_lock))
			{
				EnsureCorrectType(existingType);
				EnsureCorrectType(value);
                if (_instanceTypes.Contains(existingType) == false)
				{
					throw new InvalidOperationException(string.Format(
						"Type {0} is not in the collection of types.", existingType.FullName));
				}
                if (_instanceTypes.Contains(value))
				{
					throw new InvalidOperationException(string.Format(
						"Type {0} is already in the collection of types.", value.FullName));
				}
                int index = _instanceTypes.IndexOf(existingType);

				l.UpgradeToWriteLock();
				_instanceTypes.Insert(index, value);
			}
		}

		/// <summary>
		/// Inserts a type before a specified, already existing type.
		/// </summary>
		/// <typeparam name="TExisting">The existing type before which to insert.</typeparam>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <exception cref="InvalidOperationException">the resolver does not support inserting types, or 
		/// one of the types is not a valid type for the resolver, or the existing type is not in the collection,
		/// or the new type is already in the collection of types.</exception>
		public void InsertTypeBefore<TExisting, T>()
            where TExisting : TResolved
            where T : TResolved
		{
			InsertTypeBefore(typeof(TExisting), typeof(T));
		}

		/// <summary>
		/// Returns a value indicating whether the specified type is already in the collection of types.
		/// </summary>
		/// <param name="value">The type to look for.</param>
		/// <returns>A value indicating whether the type is already in the collection of types.</returns>
		public virtual bool ContainsType(Type value)
		{
			using (new ReadLock(_lock))
			{
				return _instanceTypes.Contains(value);
			}
		}

        /// <summary>
        /// Gets the types in the collection of types.
        /// </summary>
        /// <returns>The types in the collection of types.</returns>
        /// <remarks>Returns an enumeration, the list cannot be modified.</remarks>
        public virtual IEnumerable<Type> GetTypes()
        {
            Type[] types;
            using (new ReadLock(_lock))
            {
                types = _instanceTypes.ToArray();
            }
            return types;
        }

		/// <summary>
		/// Returns a value indicating whether the specified type is already in the collection of types.
		/// </summary>
		/// <typeparam name="T">The type to look for.</typeparam>
		/// <returns>A value indicating whether the type is already in the collection of types.</returns>
		public bool ContainsType<T>()
            where T : TResolved
		{
			return ContainsType(typeof(T));
		}

		#endregion

		/// <summary>
		/// Returns a WriteLock to use when modifying collections
		/// </summary>
		/// <returns></returns>
		protected WriteLock GetWriteLock()
		{
			return new WriteLock(_lock);
		}

        #region Type utilities

        /// <summary>
        /// Ensures that a type is a valid type for the resolver.
        /// </summary>
        /// <param name="value">The type to test.</param>
        /// <exception cref="InvalidOperationException">the type is not a valid type for the resolver.</exception>
        protected virtual void EnsureCorrectType(Type value)
        {
            if (TypeHelper.IsTypeAssignableFrom<TResolved>(value) == false)
                throw new InvalidOperationException(string.Format(
                    "Type {0} is not an acceptable type for resolver {1}.", value.FullName, GetType().FullName));
        }

        #endregion

        #region Types collection manipulation support

        /// <summary>
        /// Ensures that the resolver supports removing types.
        /// </summary>
        /// <exception cref="InvalidOperationException">The resolver does not support removing types.</exception>
        protected void EnsureSupportsRemove()
		{
			if (SupportsRemove == false)
                throw new InvalidOperationException("This resolver does not support removing types");
		}

        /// <summary>
        /// Ensures that the resolver supports clearing types.
        /// </summary>
        /// <exception cref="InvalidOperationException">The resolver does not support clearing types.</exception>
        protected void EnsureSupportsClear()		{
			if (SupportsClear == false)
                throw new InvalidOperationException("This resolver does not support clearing types");
		}

        /// <summary>
        /// Ensures that the resolver supports adding types.
        /// </summary>
        /// <exception cref="InvalidOperationException">The resolver does not support adding types.</exception>
        protected void EnsureSupportsAdd()
		{
			if (SupportsAdd == false)
                throw new InvalidOperationException("This resolver does not support adding new types");
		}

        /// <summary>
        /// Ensures that the resolver supports inserting types.
        /// </summary>
        /// <exception cref="InvalidOperationException">The resolver does not support inserting types.</exception>
        protected void EnsureSupportsInsert()
		{
			if (SupportsInsert == false)
                throw new InvalidOperationException("This resolver does not support inserting new types");
		}

        /// <summary>
        /// Gets a value indicating whether the resolver supports adding types.
        /// </summary>
		protected virtual bool SupportsAdd
		{
			get { return true; }
		}

        /// <summary>
        /// Gets a value indicating whether the resolver supports inserting types.
        /// </summary>
        protected virtual bool SupportsInsert
		{
			get { return true; }
		}

        /// <summary>
        /// Gets a value indicating whether the resolver supports clearing types.
        /// </summary>
        protected virtual bool SupportsClear
		{
			get { return true; }
		}

        /// <summary>
        /// Gets a value indicating whether the resolver supports removing types.
        /// </summary>
        protected virtual bool SupportsRemove
		{
			get { return true; }
        }

        #endregion
    }
}