using System;
using System.Threading;

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
        where TResolver : ResolverBase
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly bool _canBeNull;
		private TResolved _value;

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
			_value = value;
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
			_value = value;
			_canBeNull = canBeNull;
		}

		#endregion

        /// <summary>
        /// Gets or sets a value indicating whether the resolver can resolve objects before resolution is frozen.
        /// </summary>
        /// <remarks>This is false by default and is used for some special internal resolvers.</remarks>
        internal bool CanResolveBeforeFrozen { get; set; }

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
			get { return _value != null; }
		}

		/// <summary>
		/// Gets or sets the resolved object instance.
		/// </summary>
		/// <remarks></remarks>
		/// <exception cref="ArgumentNullException">value is set to null, but cannot be null (<c>CanBeNull</c> is <c>false</c>).</exception>
		/// <exception cref="InvalidOperationException">value is read and is null, but cannot be null (<c>CanBeNull</c> is <c>false</c>),
		/// or value is set (read) and resolution is (not) frozen.</exception>
		protected TResolved Value
		{
			get
			{
                using (Resolution.Reader(CanResolveBeforeFrozen))
				using (new ReadLock(_lock))
				{
					if (!_canBeNull && _value == null)
						throw new InvalidOperationException(string.Format(
							"Resolver {0} requires a value, and none was supplied.", this.GetType().FullName));

					return _value;
				}
			}

			set
			{
				using (Resolution.Configuration)
				using (var l = new UpgradeableReadLock(_lock))
				{
					if (!_canBeNull && value == null)
						throw new ArgumentNullException("value");

					l.UpgradeToWriteLock();
					_value = value;
				}
			}
		}
	}
}
