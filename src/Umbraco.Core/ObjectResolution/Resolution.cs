using System;
using System.Threading;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// Represents the status of objects resolution.
	/// </summary>
	/// <remarks>
	/// <para>Before resolution is frozen it is possible to access its configuration, but not to get values.</para>
	/// <para>Once resolution is frozen, it is not possible to access its configuration anymore, but it is possible to get values.</para>
	/// </remarks>
	internal static class Resolution
	{
		private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		/// <summary>
		/// Occurs when resolution is frozen.
		/// </summary>
		/// <remarks>Occurs only once, since resolution can be frozen only once.</remarks>
		public static event EventHandler Frozen;

		/// <summary>
		/// Gets or sets a value indicating whether resolution of objects is frozen.
		/// </summary>
		public static bool IsFrozen { get; private set; }

		public static void EnsureIsFrozen()
		{
			if (!IsFrozen)
                throw new InvalidOperationException("Resolution is not frozen, it is not yet possible to get values from it.");
		}

		/// <summary>
		/// Returns a disposable object that represents safe access to unfrozen resolution configuration.
		/// </summary>
		/// <remarks>Should be used in a <c>using(Resolution.Configuration) { ... }</c>  mode.</remarks>
		public static IDisposable Configuration
		{
			get
			{
				IDisposable l = new WriteLock(_lock);
				if (Resolution.IsFrozen)
				{
					l.Dispose();
					throw new InvalidOperationException("Resolution is frozen, it is not possible to configure it anymore.");
				}
				return l;
			}
		}

        /// <summary>
        /// Returns a disposable object that reprents dirty access to temporarily unfrozen resolution configuration.
        /// </summary>
        /// <remarks>
        /// <para>Should not be used.</para>
        /// <para>Should be used in a <c>using(Resolution.DirtyBackdoorToConfiguration) { ... }</c> mode.</para>
        /// <para>Because we just lift the frozen state, and we don't actually re-freeze, the <c>Frozen</c> event does not trigger.</para>
        /// </remarks>
        internal static IDisposable DirtyBackdoorToConfiguration
        {
            get { return new DirtyBackdoor(); }
        }

        // keep the class here because it needs write-access to Resolution.IsFrozen
        private class DirtyBackdoor : IDisposable
        {
            private static readonly System.Threading.ReaderWriterLockSlim _dirtyLock = new ReaderWriterLockSlim();

            private IDisposable _lock;
            private bool _frozen;

            public DirtyBackdoor()
            {
                _lock = new WriteLock(_dirtyLock);
                _frozen = Resolution.IsFrozen;
                Resolution.IsFrozen = false;
            }

            public void Dispose()
            {
                Resolution.IsFrozen = _frozen;
                _lock.Dispose();
            }
        }

		/// <summary>
		/// Freezes resolution.
		/// </summary>
		/// <exception cref="InvalidOperationException">resolution is already frozen.</exception>
		public static void Freeze()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("Resolution is frozen. It is not possible to freeze it again.");

			IsFrozen = true;
			if (Frozen != null)
				Frozen(null, null);
		}
        
        /// <summary>
        /// Resets resolution, ie unfreezes it and clears Frozen event.
        /// </summary>
        /// <remarks>To be used in unit tests.</remarks>
        internal static void Reset()
        {
            IsFrozen = false;
            Frozen = null;
        }
	}
}
