using System;

namespace Umbraco.Core
{
    public abstract class DisposableObjectSlim : IDisposable
    {
        private bool _disposed;
        private readonly object _locko = new object();

        // gets a value indicating whether this instance is disposed.
        // for internal tests only (not thread safe)
        //TODO make this internal + rename "Disposed" when we can break compatibility
        public bool IsDisposed { get { return _disposed; } }

        // implements IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //TODO make this private, non-virtual when we can break compatibility
        protected virtual void Dispose(bool disposing)
        {
            lock (_locko)
            {
                if (_disposed) return;
                _disposed = true;
            }

            DisposeUnmanagedResources();

            if (disposing)
                DisposeResources();
        }

        protected abstract void DisposeResources();

        protected virtual void DisposeUnmanagedResources()
        { }
    }
}
