using System.Collections.Generic;
using System.Threading;

namespace Umbraco.Core.Resolving
{
	//NOTE: This class should also support creating instances of the object and managing their lifespan,
	// for example, some resolvers would want to return new object each time, per request or per application lifetime.

	/// <summary>
	/// A Resolver which manages an ordered list of objects.
	/// </summary>
	/// <typeparam name="TResolver">The type of the resolver.</typeparam>
	/// <typeparam name="TResolved">The type of the resolved objects.</typeparam>
	/// <remarks>
	/// Used to resolve multiple types from a collection. The collection can also be modified at runtime/application startup.
	/// An example of this is MVCs ViewEngines collection.
	/// </remarks>
	internal abstract class ManyObjectResolverBase<TResolver, TResolved> : ResolverBase<TResolver> 
		where TResolver : class 
		where TResolved : class
	{
		readonly List<TResolved> _resolved;
		protected readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectResolverBase{TResolver,TResolved}"/> class with an empty list of objects.
		/// </summary>
		protected ManyObjectResolverBase()
		{
			_resolved = new List<TResolved>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ManyObjectResolverBase{TResolver,TResolved}"/> class with an initial list of objects.
		/// </summary>
		/// <param name="values">The list of objects.</param>
		protected ManyObjectResolverBase(IEnumerable<TResolved> value)
		{
			_resolved = new List<TResolved>(value);
		}

		/// <summary>
		/// Gets the list of objects.
		/// </summary>
		protected IEnumerable<TResolved> Values
		{
			get { return _resolved; }
		}

		/// <summary>
		/// Removes an object.
		/// </summary>
		/// <param name="value">The object to remove.</param>
		public void Remove(TResolved value)
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{							
				_resolved.Remove(value);
			}
		}

		/// <summary>
		/// Adds an object to the end of the list.
		/// </summary>
		/// <param name="value">The object to be added.</param>
		public void Add(TResolved value)
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{
				_resolved.Add(value);			
			}
		}

		/// <summary>
		/// Clears the list.
		/// </summary>
		public void Clear()
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{
				_resolved.Clear();
			}
		}

		/// <summary>
		/// Inserts an object at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the object should be inserted.</param>
		/// <param name="value">The object to insert.</param>
		public void Insert(int index, TResolved value)
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{
				_resolved.Insert(index, value);
			}
		}
		
	}
}