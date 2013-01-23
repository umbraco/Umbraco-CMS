using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// A base class for lazily resolving types for a resolver
	/// </summary>
	/// <typeparam name="TResolver"></typeparam>
	/// <typeparam name="TResolved"></typeparam>
	/// <remarks>
	/// This is a special case resolver for when types get lazily resolved in order to resolve the actual types. This is useful
	/// for when there is some processing overhead (i.e. Type finding in assemblies) to return the Types used to instantiate the instances. 
	/// In some these cases we don't want to have to type find during application startup, only when we need to resolve the instances.
	/// 
	/// Important notes about this resolver: This does not support Insert or Remove and therefore does not support any ordering unless 
	/// the types are marked with the WeightedPluginAttribute.
	/// </remarks>
	internal abstract class LazyManyObjectsResolverBase<TResolver, TResolved> : ManyObjectsResolverBase<TResolver, TResolved>
		where TResolved : class
		where TResolver : class
	{
		#region Constructors
		
		protected LazyManyObjectsResolverBase(ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
			: base(scope)
		{			
		}
		
		protected LazyManyObjectsResolverBase(HttpContextBase httpContext)
			: base(httpContext)
		{
		}

		/// <summary>
		/// Constructor accepting a list of lazy types
		/// </summary>
		/// <param name="listOfLazyTypes"></param>
		/// <param name="scope"></param>
		protected LazyManyObjectsResolverBase(IEnumerable<Lazy<Type>> listOfLazyTypes, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
			: this(scope)
		{			
			AddTypes(listOfLazyTypes);
		}

		/// <summary>
		/// Constructor accepting a delegate to return a list of types
		/// </summary>
		/// <param name="typeListDelegate"></param>
		/// <param name="scope"></param>
		protected LazyManyObjectsResolverBase(Func<IEnumerable<Type>> typeListDelegate, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
			: this(scope)
		{
			_listOfTypeListDelegates.Add(typeListDelegate);
		}

		/// <summary>
		/// Constructor accepting a list of lazy types
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="listOfLazyTypes"></param>
		protected LazyManyObjectsResolverBase(HttpContextBase httpContext, IEnumerable<Lazy<Type>> listOfLazyTypes)
			: this(httpContext)
		{
			AddTypes(listOfLazyTypes);
		}

		/// <summary>
		/// Constructor accepting a delegate to return a list of types
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="typeListDelegate"></param>
		protected LazyManyObjectsResolverBase(HttpContextBase httpContext, Func<IEnumerable<Type>> typeListDelegate)
			: this(httpContext)
		{
			_listOfTypeListDelegates.Add(typeListDelegate);
		} 

		#endregion	

		private readonly List<Lazy<Type>> _lazyTypeList = new List<Lazy<Type>>();
		private readonly List<Func<IEnumerable<Type>>> _listOfTypeListDelegates = new List<Func<IEnumerable<Type>>>();
		private List<Type> _resolvedTypes = null;
		private readonly ReaderWriterLockSlim _typeResolutionLock = new ReaderWriterLockSlim();
		
		/// <summary>
		/// Used for unit tests
		/// </summary>
		internal bool HasResolvedTypes
		{
			get { return _resolvedTypes != null; }
		}

		/// <summary>
		/// Once this is called this will resolve all types registered in the lazy list
		/// </summary>
		protected override IEnumerable<Type> InstanceTypes
		{
			get
			{
				using (var lck = new UpgradeableReadLock(_typeResolutionLock))
				{
					var lazyTypeList = _lazyTypeList.Select(x => x.Value).ToArray();
					var listofTypeListDelegates = _listOfTypeListDelegates.SelectMany(x => x()).ToArray();

					//we need to validate each resolved type now since we could not do it before when inserting the lazy delegates
					if (!HasResolvedTypes)
					{
						lck.UpgradeToWriteLock();

						_resolvedTypes = new List<Type>();

						//first iterate the lazy type list
						foreach (var l in lazyTypeList)
						{
							UpdateUniqueList(_resolvedTypes, l);
						}

						//next iterate the list of list type delegates
						foreach (var l in listofTypeListDelegates)
						{
							UpdateUniqueList(_resolvedTypes, l);
						}
					}

					return _resolvedTypes;	
				}				
			}
		}

		private void UpdateUniqueList(List<Type> uniqueList, Type toAdd)
		{
			EnsureCorrectType(toAdd);
			if (uniqueList.Contains(toAdd))
			{
				throw new InvalidOperationException("The Type " + toAdd + " already exists in the collection");
			}
			uniqueList.Add(toAdd);
		}

		/// <summary>
		/// Allows adding of multiple lazy types at once
		/// </summary>
		/// <param name="types"></param>
		protected void AddTypes(IEnumerable<Lazy<Type>> types)
		{
			EnsureAddSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				foreach (var t in types)
				{				
					_lazyTypeList.Add(t);
				}
			}
		}

		/// <summary>
		/// Adds a type list delegate to the collection
		/// </summary>
		/// <param name="typeListDelegate"></param>
		public void AddTypeListDelegate(Func<IEnumerable<Type>> typeListDelegate)
		{
			EnsureAddSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				_listOfTypeListDelegates.Add(typeListDelegate);
			}
		}

		/// <summary>
		/// Adds a lazy type to the list
		/// </summary>
		/// <param name="value"></param>
		public void AddType(Lazy<Type> value)
		{
			EnsureAddSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{				
				_lazyTypeList.Add(value);
			}
		}

		/// <summary>
		/// Converts the static type added to a lazy type and adds it to the internal list
		/// </summary>
		/// <param name="value"></param>
		public override void AddType(Type value)
		{
			AddType(new Lazy<Type>(() => value));
		}

		/// <summary>
		/// Clears all lazy types
		/// </summary>
		public override void Clear()
		{
			EnsureClearSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				_lazyTypeList.Clear();
			}
		}

		/// <summary>
		/// Does not support removal
		/// </summary>
		protected override bool SupportsRemove
		{
			get { return false; }
		}

		/// <summary>
		/// Does not support insert
		/// </summary>
		protected override bool SupportsInsert
		{
			get { return false; }
		}
	}
}