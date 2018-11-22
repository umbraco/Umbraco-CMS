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
    /// <para>Intended as an infrastructure class.</para>
    /// <para>This is a very unefficient way to lock as it allocates one object each time we lock,
    /// so it's OK to use this class for things that happen once, where it is convenient, but not
    /// for performance-critical code!</para>
    /// </remarks>
    public class WriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteLock"/> class.
        /// </summary>
        /// <param name="rwLock">The rw lock.</param>
        public WriteLock(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            _rwLock.EnterWriteLock();
        }

        void IDisposable.Dispose()
        {
            _rwLock.ExitWriteLock();
        }
    }
}
