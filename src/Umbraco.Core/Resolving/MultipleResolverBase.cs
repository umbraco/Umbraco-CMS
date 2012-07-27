using System.Collections.Generic;
using System.Threading;

namespace Umbraco.Core.Resolving
{
	/// <summary>
	/// A Resolver to return and set a Multiply registered object.
	/// </summary>
	/// <typeparam name="TResolver"></typeparam>
	/// <typeparam name="TResolved"></typeparam>
	/// <remarks>
	/// Used to resolve multiple types from a collection. The collection can also be modified at runtime/application startup.
	/// An example of this is MVCs ViewEngines collection
	/// </remarks>
	internal abstract class MultipleResolverBase<TResolver, TResolved> : ResolverBase<TResolver> 
		where TResolver : class 
		where TResolved : class
	{
		readonly List<TResolved> _resolved;
		protected readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		protected MultipleResolverBase()
		{
			_resolved = new List<TResolved>();
		}

		protected MultipleResolverBase(IEnumerable<TResolved> value)
		{
			_resolved = new List<TResolved>(value);
		}		

		protected IEnumerable<TResolved>  Values
		{
			get { return _resolved; }
		}

		public void Remove(TResolved item)
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{							
				_resolved.Remove(item);
			}
		}

		public void Add(TResolved item)
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{
				_resolved.Add(item);			
			}
		}

		public void Clear()
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{
				_resolved.Clear();
			}
		}

		public void Insert(int index, TResolved item)
		{
			Resolution.EnsureNotFrozen();
			using (new WriteLock(Lock))
			{
				_resolved.Insert(index, item);
			}
		}
		
	}
}