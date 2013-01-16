using System;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// The base class for all single-object resolvers.
	/// </summary>
	/// <typeparam name="TResolver">The type of the concrete resolver class.</typeparam>
	/// <typeparam name="TResolved">The type of the resolved object.</typeparam>
	/// <remarks>
	/// Resolves "single" objects ie objects for which there is only one application-wide instance, such as the MVC Controller factory.
	/// </remarks>
	public abstract class SingleObjectResolverBase<TResolver, TResolved> : ResolverBase<TResolver>
		where TResolved : class
		where TResolver : class
	{
		TResolved _resolved;
		readonly bool _canBeNull;

		// NOTE - we're not freezing resolution here so it is potentially possible to change the instance at any time?

		#region Constructors

		/// <summary>
		/// Initialize a new instance of the <see cref="SingleObjectResolverBase{TResolver, TResolved}"/> class.
		/// </summary>
		/// <remarks>By default <c>CanBeNull</c> is false, so <c>Value</c> has to be initialized before being read,
		/// otherwise an exception will be thrown when reading it.</remarks>
		protected SingleObjectResolverBase()
			: this(false)
		{ }

		/// <summary>
		/// Initialize a new instance of the <see cref="SingleObjectResolverBase{TResolver, TResolved}"/> class with an instance of the resolved object.
		/// </summary>
		/// <param name="value">An instance of the resolved object.</param>
		/// <remarks>By default <c>CanBeNull</c> is false, so <c>value</c> has to be non-null, or <c>Value</c> has to be
		/// initialized before being accessed, otherwise an exception will be thrown when reading it.</remarks>
		protected SingleObjectResolverBase(TResolved value)
			: this(false)
		{
			_resolved = value;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="SingleObjectResolverBase{TResolver, TResolved}"/> class with a value indicating whether the resolved object instance can be null.
		/// </summary>
		/// <param name="canBeNull">A value indicating whether the resolved object instance can be null.</param>
		/// <remarks>If <c>CanBeNull</c> is false, <c>Value</c> has to be initialized before being read,
		/// otherwise an exception will be thrown when reading it.</remarks>
		protected SingleObjectResolverBase(bool canBeNull)
		{
			_canBeNull = canBeNull;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="SingleObjectResolverBase{TResolver, TResolved}"/> class with an instance of the resolved object,
		/// and a value indicating whether that instance can be null.
		/// </summary>
		/// <param name="value">An instance of the resolved object.</param>
		/// <param name="canBeNull">A value indicating whether the resolved object instance can be null.</param>
		/// <remarks>If <c>CanBeNull</c> is false, <c>value</c> has to be non-null, or <c>Value</c> has to be initialized before being read,
		/// otherwise an exception will be thrown when reading it.</remarks>
		protected SingleObjectResolverBase(TResolved value, bool canBeNull)
		{
			_resolved = value;
			_canBeNull = canBeNull;
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether the resolved object instance can be null.
		/// </summary>
		public bool CanBeNull
		{
			get { return _canBeNull; }
		}

		/// <summary>
		/// Gets a value indicating whether the resolved object instance is null.
		/// </summary>
		public bool HasValue
		{
			get { return _resolved != null; }
		}

		/// <summary>
		/// Gets or sets the resolved object instance.
		/// </summary>
		/// <remarks></remarks>
		/// <exception cref="ArgumentNullException">value is set to null, but cannot be null (<c>CanBeNull</c> is <c>false</c>).</exception>
		/// <exception cref="InvalidOperationException">value is read and is null, but cannot be null (<c>CanBeNull</c> is <c>false</c>).</exception>
		protected TResolved Value
		{
			get
			{
				if (!_canBeNull && _resolved == null)
					throw new InvalidOperationException("");
				return _resolved;
			}

			set
			{
				if (!_canBeNull && value == null)
					throw new ArgumentNullException("value");
				_resolved = value;
			}
		}
	}
}
