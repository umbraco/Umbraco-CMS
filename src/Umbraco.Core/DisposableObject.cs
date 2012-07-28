using System;
using System.Threading;

namespace Umbraco.Core
{
	/// <summary>
	/// Abstract implementation of logic commonly required to safely handle disposable unmanaged resources.
	/// </summary>
	public abstract class DisposableObject : IDisposable
	{
		private bool _disposed;
		private readonly ReaderWriterLockSlim _disposalLocker = new ReaderWriterLockSlim();

		/// <summary>
		/// Gets a value indicating whether this instance is disposed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is disposed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisposed
		{
			get { return _disposed; }
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);

			// Use SupressFinalize in case a subclass of this type implements a finalizer.
			GC.SuppressFinalize(this);
		}

		~DisposableObject()
		{
			// Run dispose but let the class know it was due to the finalizer running.
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			// Only operate if we haven't already disposed
			if (IsDisposed || !disposing) return;

			using (new WriteLock(_disposalLocker))
			{
				// Check again now we're inside the lock
				if (IsDisposed) return;

				// Call to actually release resources. This method is only
				// kept separate so that the entire disposal logic can be used as a VS snippet
				DisposeResources();

				// Indicate that the instance has been disposed.
				_disposed = true;
			}
		}

		/// <summary>
		/// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
		/// </summary>
		protected abstract void DisposeResources();

		protected virtual void DisposeUnmanagedResources()
		{

		}
	}
}