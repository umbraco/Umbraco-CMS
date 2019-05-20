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
    /// <para>This is a very inefficient way to lock as it allocates one object each time we lock,
    /// so it's OK to use this class for things that happen once, where it is convenient, but not
    /// for performance-critical code!</para>
    /// </remarks>
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
