using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Umbraco.Core.ObjectResolution
{
	internal abstract class ManyObjectsResolverBase<TResolver, TResolved> : ResolverBase<TResolver>
		where TResolved : class 
		where TResolver : class
	{
		private List<TResolved> _applicationInstances = null;
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly List<Type> _instanceTypes = new List<Type>(); 

		#region Constructors
	
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects.
		/// </summary>
		/// <param name="scope">The lifetime scope of instantiated objects, default is per Application</param>
		protected ManyObjectsResolverBase(ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
		{
			CanResolveBeforeFrozen = false;
			if (scope == ObjectLifetimeScope.HttpRequest)
			{
				if (HttpContext.Current == null)
				{
					throw new InvalidOperationException("Use alternative constructor accepting a HttpContextBase object in order to set the lifetime scope to HttpRequest when HttpContext.Current is null");		
				}
				CurrentHttpContext = new HttpContextWrapper(HttpContext.Current);
			}

			LifetimeScope = scope;
			_instanceTypes = new List<Type>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects.
		/// with creation of objects based on an HttpRequest lifetime scope.
		/// </summary>
		/// <param name="httpContext"></param>
		protected ManyObjectsResolverBase(HttpContextBase httpContext)
		{
			CanResolveBeforeFrozen = false;
			if (httpContext == null) throw new ArgumentNullException("httpContext");
			LifetimeScope = ObjectLifetimeScope.HttpRequest;
			CurrentHttpContext = httpContext;
			_instanceTypes = new List<Type>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an initial list of objects.
		/// </summary>
		/// <param name="value">The list of objects.</param>
		/// <param name="scope">If set to true will resolve singleton objects which will be created once for the lifetime of the application</param>
		protected ManyObjectsResolverBase(IEnumerable<Type> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
			: this(scope)
		{
			_instanceTypes = new List<Type>(value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an initial list of objects
		/// with creation of objects based on an HttpRequest lifetime scope.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="value"></param>
		protected ManyObjectsResolverBase(HttpContextBase httpContext, IEnumerable<Type> value)
			: this(httpContext)
		{
			_instanceTypes = new List<Type>(value);
		} 
		#endregion

		/// <summary>
		/// used internally for special resolvers to be able to resolve objects before resolution is frozen.
		/// </summary>
		internal bool CanResolveBeforeFrozen { get; set; }

		/// <summary>
		/// Returns the list of Types registered that instances will be created from
		/// </summary>
		protected virtual IEnumerable<Type> InstanceTypes
		{
			get { return _instanceTypes; }
		}

		/// <summary>
		/// Returns the Current HttpContextBase used to construct this object if one exists. 
		/// If one exists then the LifetimeScope will be ObjectLifetimeScope.HttpRequest
		/// </summary>
		protected HttpContextBase CurrentHttpContext { get; private set; }

		/// <summary>
		/// Returns the ObjectLifetimeScope for created objects
		/// </summary>
		protected ObjectLifetimeScope LifetimeScope { get; private set; }

		private int _defaultPluginWeight = 10;
		
		/// <summary>
		/// Used in conjunction with GetSortedValues and WeightedPluginAttribute, if any of the objects
		/// being resolved do not contain the WeightedPluginAttribute then this will be the default weight applied
		/// to the object.
		/// </summary>
		protected virtual int DefaultPluginWeight
		{
			get { return _defaultPluginWeight; }
			set { _defaultPluginWeight = value; }
		}

		/// <summary>
		/// If a resolver requries that objects are resolved with a specific order using the WeightedPluginAttribute
		/// then this method should be used instead of the Values property.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable<TResolved> GetSortedValues()
		{
			var vals = Values.ToList();
			//ensure they are sorted
			vals.Sort((f1, f2) =>
				{
					Func<object, int> getWeight = o =>
						{
							var weightAttribute = f1.GetType().GetCustomAttribute<WeightedPluginAttribute>(true);
							return weightAttribute != null ? weightAttribute.Weight : DefaultPluginWeight;
						};
					return getWeight(f1).CompareTo(getWeight(f2));
				});
			return vals;
		}  

		/// <summary>
		/// Returns the list of new object instances.
		/// </summary>
		protected IEnumerable<TResolved> Values
		{
			get
			{				
				//We cannot return values unless resolution is locked
				if (!CanResolveBeforeFrozen && !Resolution.IsFrozen)
					throw new InvalidOperationException("Values cannot be returned until Resolution is frozen");

				switch (LifetimeScope)
				{
					case ObjectLifetimeScope.HttpRequest:
						//create new instances per HttpContext, this means we'll lazily create them and once created, cache them in the HttpContext
						//create new instances per application, this means we'll lazily create them and once created, cache them
						using (var l = new UpgradeableReadLock(_lock))
						{
							//check if the items contain the key (based on the full type name)
							if (CurrentHttpContext.Items[this.GetType().FullName] == null)
							{
								l.UpgradeToWriteLock();
								//add the items to the context items (based on full type name)
								CurrentHttpContext.Items[this.GetType().FullName] = new List<TResolved>(CreateInstances());
							}
							return (List<TResolved>)CurrentHttpContext.Items[this.GetType().FullName];
						}
					case ObjectLifetimeScope.Application:
						//create new instances per application, this means we'll lazily create them and once created, cache them
						using(var l = new UpgradeableReadLock(_lock))
						{
							if (_applicationInstances == null)
							{
								l.UpgradeToWriteLock();
								_applicationInstances = new List<TResolved>(CreateInstances());
							}
							return _applicationInstances;
						}
					case ObjectLifetimeScope.Transient:
					default:
						//create new instances each time
						return CreateInstances();
				}				
			}
		}

		protected virtual IEnumerable<TResolved> CreateInstances()
		{
			return PluginManager.Current.CreateInstances<TResolved>(InstanceTypes);
		} 

		/// <summary>
		/// Removes a type.
		/// </summary>
		/// <param name="value">The type to remove.</param>
		public virtual void RemoveType(Type value)
		{
			EnsureRemoveSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				EnsureCorrectType(value);
				_instanceTypes.Remove(value);
			}
		}

		/// <summary>
		/// Removes a type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void RemoveType<T>()
		{
			RemoveType(typeof (T));
		}

		/// <summary>
		/// protected method allow the inheritor to add many types at once
		/// </summary>
		/// <param name="types"></param>
		protected void AddTypes(IEnumerable<Type> types)
		{
			EnsureAddSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				foreach(var t in types)
				{
					EnsureCorrectType(t);
					if (InstanceTypes.Contains(t))
					{
						throw new InvalidOperationException("The Type " + t + " already exists in the collection");
					};
					_instanceTypes.Add(t);	
				}				
			}
		}

		/// <summary>
		/// Adds a Type to the end of the list.
		/// </summary>
		/// <param name="value">The object to be added.</param>
		public virtual void AddType(Type value)
		{
			EnsureAddSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				EnsureCorrectType(value);
				if (InstanceTypes.Contains(value))
				{
					throw new InvalidOperationException("The Type " + value + " already exists in the collection");
				};
				_instanceTypes.Add(value);
			}
		}

		/// <summary>
		/// Adds a Type to the end of the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void AddType<T>()
		{
			AddType(typeof (T));
		}

		/// <summary>
		/// Clears the list.
		/// </summary>
		public virtual void Clear()
		{
			EnsureClearSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				_instanceTypes.Clear();
			}
		}

		/// <summary>
		/// Inserts a Type at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the object should be inserted.</param>
		/// <param name="value">The object to insert.</param>
		public virtual void InsertType(int index, Type value)
		{			
			EnsureInsertSupport();

			EnsureResolutionNotFrozen();

			using (var l = GetWriteLock())
			{
				EnsureCorrectType(value);
				if (InstanceTypes.Contains(value))
				{
					throw new InvalidOperationException("The Type " + value + " already exists in the collection");
				};

				_instanceTypes.Insert(index, value);
			}
		}

		/// <summary>
		/// Inserts a Type at the specified index.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="index"></param>
		public void InsertType<T>(int index)
		{
			InsertType(index, typeof (T));
		}

		/// <summary>
		/// Returns a WriteLock to use when modifying collections
		/// </summary>
		/// <returns></returns>
		protected WriteLock GetWriteLock()
		{
			return new WriteLock(_lock);
		}

		/// <summary>
		/// Returns an upgradeable read lock for use when reading/modifying collections
		/// </summary>
		/// <returns></returns>
		protected UpgradeableReadLock GetUpgradeableReadLock()
		{
			return new UpgradeableReadLock(_lock);
		}

		/// <summary>
		/// Throws an exception if resolution is frozen
		/// </summary>
		protected void EnsureResolutionNotFrozen()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("The type list cannot be modified after resolution has been frozen");
		}

		/// <summary>
		/// Throws an exception if this does not support Remove
		/// </summary>
		protected void EnsureRemoveSupport()
		{
			if (!SupportsRemove)
				throw new InvalidOperationException("This resolver does not support Removing types");
		}

		/// <summary>
		/// Throws an exception if this does not support Clear
		/// </summary>
		protected void EnsureClearSupport()
		{
			if (!SupportsClear)
				throw new InvalidOperationException("This resolver does not support Clearing types");
		}

		/// <summary>
		/// Throws an exception if this does not support Add
		/// </summary>
		protected void EnsureAddSupport()
		{
			if (!SupportsAdd)
				throw new InvalidOperationException("This resolver does not support Adding new types");
		}

		/// <summary>
		/// Throws an exception if this does not support insert
		/// </summary>
		protected void EnsureInsertSupport()
		{
			if (!SupportsInsert)
				throw new InvalidOperationException("This resolver does not support Inserting new types");
		}

		/// <summary>
		/// Throws an exception if the type is not of the TResolved type
		/// </summary>
		/// <param name="t"></param>
		protected void EnsureCorrectType(Type t)
		{
			if (!TypeHelper.IsTypeAssignableFrom<TResolved>(t))
				throw new InvalidOperationException("The resolver " + this.GetType() + " can only accept types of " + typeof(TResolved) + ". The Type passed in to this method is " + t);
		}

		protected virtual bool SupportsAdd
		{
			get { return true; }
		}

		protected virtual bool SupportsInsert
		{
			get { return true; }
		}

		protected virtual bool SupportsClear
		{
			get { return true; }
		}

		protected virtual bool SupportsRemove
		{
			get { return true; }
		}
	}
}