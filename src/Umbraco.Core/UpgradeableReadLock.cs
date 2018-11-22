using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides a convenience methodology for implementing locked access to resources. 
    /// </summary>
    /// <remarks>
    /// Intended as an infrastructure class.
    /// </remarks>
    public class UpgradeableReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;
		private bool _upgraded = false;

        /// <summary>
		/// Initializes a new instance of the <see cref="ReadLock"/> class.
        /// </summary>
        /// <param name="rwLock">The rw lock.</param>
		public UpgradeableReadLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            _rwLock.EnterUpgradeableReadLock();
        }

		public void UpgradeToWriteLock()
		{
			_rwLock.EnterWriteLock();
			_upgraded = true;
		}

        void IDisposable.Dispose()
        {
			if (_upgraded)
			{
				_rwLock.ExitWriteLock();
			}
			_rwLock.ExitUpgradeableReadLock();
        }
    }
}
