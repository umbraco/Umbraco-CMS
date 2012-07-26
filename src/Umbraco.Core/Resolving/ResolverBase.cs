using System;
using System.Threading;

namespace Umbraco.Core.Resolving
{
	internal abstract class ResolverBase<TResolver> where TResolver : class
	{
		static TResolver _resolver;
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
	}
}
