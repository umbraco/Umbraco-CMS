using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace Umbraco.Core.Resolving
{
	internal abstract class ManyObjectResolverBase<TResolved>
		where TResolved : class
	{
		private readonly bool _instancePerApplication = false;		
		private List<TResolved> _applicationInstances = null;
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectResolverBase{TResolved}"/> class with an empty list of objects.
		/// </summary>
		/// <param name="instancePerApplication">If set to true will resolve singleton objects which will be created once for the lifetime of the application</param>
		protected ManyObjectResolverBase(bool instancePerApplication = true)
		{
			_instancePerApplication = instancePerApplication;
			InstanceTypes = new List<Type>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectResolverBase{TResolved}"/> class with an initial list of objects
		/// with creation of objects based on an HttpRequest lifetime scope.
		/// </summary>
		/// <param name="httpContext"></param>
		protected ManyObjectResolverBase(HttpContextBase httpContext)
			: this(false)
		{
			CurrentHttpContext = httpContext;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectResolverBase{TResolved}"/> class with an initial list of objects.
		/// </summary>
		/// <param name="value">The list of objects.</param>
		/// <param name="instancePerApplication">If set to true will resolve singleton objects which will be created once for the lifetime of the application</param>
		protected ManyObjectResolverBase(IEnumerable<Type> value, bool instancePerApplication = true)
		{
			_instancePerApplication = instancePerApplication;
			InstanceTypes = new List<Type>(value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectResolverBase{TResolved}"/> class with an empty list of objects
		/// with creation of objects based on an HttpRequest lifetime scope.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="value"></param>
		protected ManyObjectResolverBase(HttpContextBase httpContext, IEnumerable<Type> value)
			: this(value, false)
		{
			CurrentHttpContext = httpContext;
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
		protected ObjectLifetimeScope LifetimeScope
		{
			get
			{
				if (_instancePerApplication)
					return ObjectLifetimeScope.Application;
				if (CurrentHttpContext != null)
					return ObjectLifetimeScope.HttpRequest;
				return ObjectLifetimeScope.Transient;
			}
		}

		/// <summary>
		/// Returns the list of new object instances.
		/// </summary>
		protected IEnumerable<TResolved> Values
		{
			get
			{
				//we should not allow the returning/creating of objects if resolution is not yet frozen!
				if (Resolution.IsFrozen)
					throw new InvalidOperationException("Resolution is not frozen. It is not possible to instantiate and returng objects until resolution is frozen.");

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
								CurrentHttpContext.Items[this.GetType().FullName] = new List<TResolved>(
									PluginTypeResolver.Current.CreateInstances<TResolved>(InstanceTypes));
							}
							return _applicationInstances;
						}
					case ObjectLifetimeScope.Application:
						//create new instances per application, this means we'll lazily create them and once created, cache them
						using(var l = new UpgradeableReadLock(_lock))
						{
							if (_applicationInstances == null)
							{
								l.UpgradeToWriteLock();
								_applicationInstances = new List<TResolved>(
									PluginTypeResolver.Current.CreateInstances<TResolved>(InstanceTypes));
							}
							return _applicationInstances;
						}
					case ObjectLifetimeScope.Transient:
					default:
						//create new instances each time
						return PluginTypeResolver.Current.CreateInstances<TResolved>(InstanceTypes);
				}				
			}
		}

		/// <summary>
		/// Removes a type.
		/// </summary>
		/// <param name="value">The type to remove.</param>
		public void Remove(Type value)
		{
			Resolution.EnsureNotFrozen();
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
		public void Remove<T>()
		{
			Remove(typeof (T));
		}

		/// <summary>
		/// Adds a Type to the end of the list.
		/// </summary>
		/// <param name="value">The object to be added.</param>
		public void Add(Type value)
		{
			Resolution.EnsureNotFrozen();
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
		public void Add<T>()
		{
			Add(typeof (T));
		}

		/// <summary>
		/// Clears the list.
		/// </summary>
		public void Clear()
		{
			Resolution.EnsureNotFrozen();
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
		public void Insert(int index, Type value)
		{
			Resolution.EnsureNotFrozen();
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
		public void Insert<T>(int index)
		{
			Insert(index, typeof (T));
		}

		private void EnsureCorrectType(Type t)
		{
			if (!TypeHelper.IsTypeAssignableFrom<TResolved>(t))
				throw new InvalidOperationException("The resolver " + this.GetType() + " can only accept types of " + typeof(TResolved) + ". The Type passed in to this method is " + t);
		}

	}
}