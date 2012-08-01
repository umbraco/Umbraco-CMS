using System;
using System.Threading;

namespace Umbraco.Core.Resolving
{
	/// <summary>
	/// base class for resolvers which declare a singleton accessor
	/// </summary>
	/// <typeparam name="TResolver"></typeparam>
	internal abstract class ResolverBase<TResolver> 
		where TResolver : class
	{
		static TResolver _resolver;
		
		/// <summary>
		/// The lock for the singleton
		/// </summary>
		/// <remarks>
		/// Though resharper says this is in error, it is actually correct. We want a different lock object for each generic type.
		/// See this for details: http://confluence.jetbrains.net/display/ReSharper/Static+field+in+generic+type
		/// </remarks>
		static readonly ReaderWriterLockSlim ResolversLock = new ReaderWriterLockSlim();

		public static TResolver Current
		{
			get
			{
				using (new ReadLock(ResolversLock))
				{
					if (_resolver == null)
						throw new InvalidOperationException("Current has not been initialized. You must initialize Current before trying to read it.");
					return _resolver;
				}
			}

			set
			{
				using (new WriteLock(ResolversLock))
				{
					if (value == null)
						throw new ArgumentNullException("value");
					if (_resolver != null)
						throw new InvalidOperationException("Current has already been initialized. It is not possible to re-initialize Current once it has been initialized.");
					_resolver = value;
				}
			}
		}

		/// <summary>
		/// used in unit tests to reset current to null
		/// </summary>
		internal static void Reset()
		{
			_resolver = null;
		}
	}
}
