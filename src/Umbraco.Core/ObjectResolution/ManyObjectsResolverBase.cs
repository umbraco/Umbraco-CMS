using System;
using System.Collections.Generic;
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
		
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects.
		/// </summary>
		/// <param name="scope">The lifetime scope of instantiated objects, default is per Application</param>
		protected ManyObjectsResolverBase(ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
		{
			if (scope == ObjectLifetimeScope.HttpRequest)
			{
				if (HttpContext.Current == null)
				{
					throw new InvalidOperationException("Use alternative constructor accepting a HttpContextBase object in order to set the lifetime scope to HttpRequest when HttpContext.Current is null");		
				}
				CurrentHttpContext = new HttpContextWrapper(HttpContext.Current);
			}

			LifetimeScope = scope;
			InstanceTypes = new List<Type>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects.
		/// with creation of objects based on an HttpRequest lifetime scope.
		/// </summary>
		/// <param name="httpContext"></param>
		protected ManyObjectsResolverBase(HttpContextBase httpContext)
		{
			LifetimeScope = ObjectLifetimeScope.HttpRequest;
			CurrentHttpContext = httpContext;
			InstanceTypes = new List<Type>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an initial list of objects.
		/// </summary>
		/// <param name="value">The list of objects.</param>
		/// <param name="scope">If set to true will resolve singleton objects which will be created once for the lifetime of the application</param>
		protected ManyObjectsResolverBase(IEnumerable<Type> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
			: this(scope)
		{			
			InstanceTypes = new List<Type>(value);
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
			InstanceTypes = new List<Type>(value);
		} 
		#endregion

		/// <summary>
		/// Returns the list of Types registered that instances will be created from
		/// </summary>
		protected List<Type> InstanceTypes { get; private set; }

		/// <summary>
		/// Returns the Current HttpContextBase used to construct this object if one exists. 
		/// If one exists then the LifetimeScope will be ObjectLifetimeScope.HttpRequest
		/// </summary>
		protected HttpContextBase CurrentHttpContext { get; private set; }

		/// <summary>
		/// Returns the ObjectLifetimeScope for created objects
		/// </summary>
		protected ObjectLifetimeScope LifetimeScope { get; private set; }

		/// <summary>
		/// Returns the list of new object instances.
		/// </summary>
		protected IEnumerable<TResolved> Values
		{
			get
			{				
				//We cannot return values unless resolution is locked
				if (!Resolution.IsFrozen)
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
		public void RemoveType(Type value)
		{	
			EnsureCorrectType(value);
			using (new WriteLock(_lock))
			{
				InstanceTypes.Remove(value);
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
		/// Adds a Type to the end of the list.
		/// </summary>
		/// <param name="value">The object to be added.</param>
		public void AddType(Type value)
		{
			EnsureCorrectType(value);
			using (var l = new UpgradeableReadLock(_lock))
			{
				if (InstanceTypes.Contains(value))
				{
					throw new InvalidOperationException("The Type " + value + " already exists in the collection");
				};

				l.UpgradeToWriteLock();
				InstanceTypes.Add(value);
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
		public void Clear()
		{
			using (new WriteLock(_lock))
			{
				InstanceTypes.Clear();
			}
		}

		/// <summary>
		/// Inserts a Type at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the object should be inserted.</param>
		/// <param name="value">The object to insert.</param>
		public void InsertType(int index, Type value)
		{
			EnsureCorrectType(value);
			using (var l = new UpgradeableReadLock(_lock))
			{
				if (InstanceTypes.Contains(value))
				{
					throw new InvalidOperationException("The Type " + value + " already exists in the collection");
				};

				l.UpgradeToWriteLock();
				InstanceTypes.Insert(index, value);
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

		private void EnsureCorrectType(Type t)
		{
			if (!TypeHelper.IsTypeAssignableFrom<TResolved>(t))
				throw new InvalidOperationException("The resolver " + this.GetType() + " can only accept types of " + typeof(TResolved) + ". The Type passed in to this method is " + t);
		}

	}
}