using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbraco.Core.ObjectResolution
{
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

		protected LazyManyObjectsResolverBase(IEnumerable<Lazy<Type>> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
			: this(scope)
		{			
			AddTypes(value);
		}

		protected LazyManyObjectsResolverBase(HttpContextBase httpContext, IEnumerable<Lazy<Type>> value)
			: this(httpContext)
		{
			
		} 
		#endregion	

		private readonly List<Lazy<Type>> _lazyTypes = new List<Lazy<Type>>();
		private bool _hasResolvedTypes = false;
		
		/// <summary>
		/// Used for unit tests
		/// </summary>
		internal bool HasResolvedTypes
		{
			get { return _hasResolvedTypes; }
		}

		/// <summary>
		/// Once this is called this will resolve all types registered in the lazy list
		/// </summary>
		protected override IEnumerable<Type> InstanceTypes
		{
			get
			{
				var list = _lazyTypes.Select(x => x.Value).ToArray();

				//we need to validate each resolved type now since we could not do it before when inserting the lazy delegates
				if (!_hasResolvedTypes)
				{
					var uniqueList = new List<Type>();					
					foreach (var l in list)
					{
						EnsureCorrectType(l);
						if (uniqueList.Contains(l))
						{
							throw new InvalidOperationException("The Type " + l + " already exists in the collection");
						}
						uniqueList.Add(l);
					}
					_hasResolvedTypes = true;
				}

				return list;
			}
		}

		protected void AddTypes(IEnumerable<Lazy<Type>> types)
		{
			EnsureAddSupport();

			EnsureResolutionNotFrozen();

			using (GetWriteLock())
			{
				foreach (var t in types)
				{				
					_lazyTypes.Add(t);
				}
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
				_lazyTypes.Add(value);
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
				_lazyTypes.Clear();
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