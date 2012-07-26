using System;

namespace Umbraco.Core.Resolving
{
	/// <summary>
	/// A Resolver to return and set a Single registered object.
	/// </summary>
	/// <typeparam name="TResolver"></typeparam>
	/// <typeparam name="TResolved"></typeparam>
	/// <remarks>
	/// Used for 'singly' registered objects. An example is like the MVC Controller Factory, only one exists application wide and it can
	/// be get/set.
	/// </remarks>
	internal abstract class SingleResolverBase<TResolver, TResolved> : ResolverBase<TResolver> 
		where TResolver : class 
		where TResolved : class
	{
		TResolved _resolved;
		readonly bool _canBeNull;

		protected SingleResolverBase()
			: this(false)
		{ }

		protected SingleResolverBase(TResolved value)
			: this(false)
		{
			_resolved = value;
		}

		protected SingleResolverBase(bool canBeNull)
		{
			_canBeNull = canBeNull;
		}

		protected SingleResolverBase(TResolved value, bool canBeNull)
		{
			_resolved = value;
			_canBeNull = canBeNull;
		}

		/// <summary>
		/// Gets/sets the value of the object
		/// </summary>
		protected TResolved Value
		{
			get
			{
				return _resolved;
			}

			set
			{
				Resolution.EnsureNotFrozen();

				if (!_canBeNull && value == null)
					throw new ArgumentNullException("value");
				_resolved = value;
			}
		}
	}
}
