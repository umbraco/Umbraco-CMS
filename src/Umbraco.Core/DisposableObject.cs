using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Abstract implementation of IDisposable.
    /// </summary>
    /// <remarks>
    /// This is for objects that DO have unmanaged resources. Use <see cref="DisposableObjectSlim"/>
    /// for objects that do NOT have unmanaged resources, and avoid creating a finalizer.
    ///
    /// Can also be used as a pattern for when inheriting is not possible.
    ///
    /// See also: https://msdn.microsoft.com/en-us/library/b1yfkh5e%28v=vs.110%29.aspx
    /// See also: https://lostechies.com/chrispatterson/2012/11/29/idisposable-done-right/
    ///
    /// Note: if an object's ctor throws, it will never be disposed, and so if that ctor
    /// has allocated disposable objects, it should take care of disposing them.
    /// </remarks>
    public abstract class DisposableObject : IDisposable
    {
        private readonly object _locko = new object();

        // gets a value indicating whether this instance is disposed.
        // for internal tests only (not thread safe)
        public bool Disposed { get; private set; }

        // implements IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // finalizer
        ~DisposableObject()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            // can happen if the object construction failed
            if (_locko == null)
                return;

            lock (_locko)
            {
                if (Disposed) return;
                Disposed = true;
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
