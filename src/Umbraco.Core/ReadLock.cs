using System;
using System.Threading;

namespace Umbraco.Core
{
    [Obsolete("Use ReaderWriterLockSlim directly. This will be removed in future versions.")]
    public class ReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadLock"/> class.
        /// </summary>
        public ReadLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            _rwLock.EnterReadLock();
        }

        void IDisposable.Dispose()
        {
            _rwLock.ExitReadLock();
        }
    }
}
