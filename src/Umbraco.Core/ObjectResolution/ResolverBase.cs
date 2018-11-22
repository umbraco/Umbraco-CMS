using System;
using System.Threading;

namespace Umbraco.Core.ObjectResolution
{
    /// <summary>
    /// Base non-generic class for resolvers
    /// </summary>
    public abstract class ResolverBase
    {
        protected ResolverBase(Action resetAction)
        {
            //add itself to the internal collection
            ResolverCollection.Add(this, resetAction);
        }

    }

	/// <summary>
	/// The base class for all resolvers.
	/// </summary>
	/// <typeparam name="TResolver">The type of the concrete resolver class.</typeparam>
	/// <remarks>Provides singleton management to all resolvers.</remarks>
    public abstract class ResolverBase<TResolver> : ResolverBase
        where TResolver : ResolverBase
	{

        /// <summary>
        /// The underlying singleton object instance
        /// </summary>
        static TResolver _resolver;

        /// <summary>
        /// The lock for the singleton.
        /// </summary>
        /// <remarks>
        /// Though resharper says this is in error, it is actually correct. We want a different lock object for each generic type.
        /// See this for details: http://confluence.jetbrains.net/display/ReSharper/Static+field+in+generic+type
        /// </remarks>
        static readonly ReaderWriterLockSlim ResolversLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Constructor set the reset action for the underlying object
        /// </summary>
	    protected ResolverBase()
	        : base(() => Reset())
	    {

	    }

	    /// <summary>
		/// Gets or sets the resolver singleton instance.
		/// </summary>
		/// <remarks>The value can be set only once, and cannot be read before it has been set.</remarks>
		/// <exception cref="InvalidOperationException">value is read before it has been set, or value is set again once it has already been set.</exception>
		/// <exception cref="ArgumentNullException">value is <c>null</c>.</exception>
		public static TResolver Current
		{
			get
			{
				using (new ReadLock(ResolversLock))
				{
					if (_resolver == null)
						throw new InvalidOperationException(string.Format(
							"Current has not been initialized on {0}. You must initialize Current before trying to read it.",
							typeof(TResolver).FullName));
					return _resolver;
				}
			}

			set
			{
                using (Resolution.Configuration)
				using (new WriteLock(ResolversLock))
				{
					if (value == null)
						throw new ArgumentNullException("value");
					if (_resolver != null)
						throw new InvalidOperationException(string.Format(
							"Current has already been initialized on {0}. It is not possible to re-initialize Current once it has been initialized.",
							typeof(TResolver).FullName));
					_resolver = value;
				}
			}
		}

        /// <summary>
        /// Gets a value indicating whether a the singleton instance has been set.
        /// </summary>
        public static bool HasCurrent
        {
            get
            {
                using (new ReadLock(ResolversLock))
                {
                    return _resolver != null;
                }
            }
        }

		/// <summary>
		/// Resets the resolver singleton instance to null.
		/// </summary>
		/// <remarks>
		/// To be used in unit tests. DO NOT USE THIS DURING PRODUCTION.
		/// </remarks>
		/// <param name="resetResolution">
		/// By default this is true because we always need to reset resolution before we reset a resolver, however in some insanely rare cases like unit testing you might not want to do this.
		/// </param>
		protected internal static void Reset(bool resetResolution = true)
		{

            //In order to reset a resolver, we always must reset the resolution
            if (resetResolution)
            {                
                Resolution.Reset();
            }

            //ensure its removed from the collection
            ResolverCollection.Remove(_resolver);

            using (Resolution.Configuration)
			using (new WriteLock(ResolversLock))
			{
				_resolver = null;
			}
            
		}
	}
}
