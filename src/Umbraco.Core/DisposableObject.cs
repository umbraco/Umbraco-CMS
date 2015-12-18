using System;
using System.Threading;

namespace Umbraco.Core
{
	/// <summary>
	/// Abstract implementation of IDisposable.
	/// </summary>
	/// <remarks>
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

        // finalizer
		~DisposableObject()
		{
			Dispose(false);
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