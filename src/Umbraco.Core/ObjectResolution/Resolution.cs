using System;
using System.Linq;
using System.Threading;
using Umbraco.Core.Logging;

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
		private static readonly ReaderWriterLockSlim ConfigurationLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private volatile static bool _isFrozen;

	    /// <summary>
		/// Occurs when resolution is frozen.
		/// </summary>
		/// <remarks>Occurs only once, since resolution can be frozen only once.</remarks>
		public static event EventHandler Frozen;

		/// <summary>
		/// Gets or sets a value indicating whether resolution of objects is frozen.
		/// </summary>
		// internal for unit tests, use ReadFrozen if you want to be sure
		internal static bool IsFrozen
		{
		    get
		    {
		        using (new ReadLock(ConfigurationLock))
		        {
                    return _isFrozen;
                }
		    }
		}

	    public static IDisposable Reader(bool canReadUnfrozen = false)
	    {
            IDisposable l = new ReadLock(ConfigurationLock);
            if (canReadUnfrozen || _isFrozen) return l;

            l.Dispose();
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
				IDisposable l = new WriteLock(ConfigurationLock);
			    if (_isFrozen == false) return l;

			    l.Dispose();
			    throw new InvalidOperationException("Resolution is frozen, it is not possible to configure it anymore.");
			}
		}

        // NOTE - the ugly code below exists only because of umbraco.BusinessLogic.Actions.Action.ReRegisterActionsAndHandlers
        // which wants to re-register actions and handlers instead of properly restarting the application. Don't even think
        // about using it for anything else. Also, while the backdoor is open, the resolution system is locked so nothing
        // can work properly => deadlocks. Therefore, open the backdoor, do resolution changes EXCLUSIVELY, and close the door!

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

            private readonly IDisposable _lock;
            private readonly bool _frozen;

            public DirtyBackdoor()
            {
                LogHelper.Debug(typeof(DirtyBackdoor), "Creating back door for resolution");

                _lock = new WriteLock(ConfigurationLock);
                _frozen = _isFrozen;
                _isFrozen = false;
            }

            public void Dispose()
            {
                LogHelper.Debug(typeof(DirtyBackdoor), "Disposing back door for resolution");

                _isFrozen = _frozen;
                _lock.Dispose();
            }
        }

		/// <summary>
		/// Freezes resolution.
		/// </summary>
		/// <exception cref="InvalidOperationException">resolution is already frozen.</exception>
		public static void Freeze()
		{
            LogHelper.Debug(typeof (Resolution), "Freezing resolution");

		    using (new WriteLock(ConfigurationLock))
		    {
                if (_isFrozen)
                    throw new InvalidOperationException("Resolution is frozen. It is not possible to freeze it again.");

                _isFrozen = true;
            }

            LogHelper.Debug(typeof(Resolution), "Resolution is frozen");

		    if (Frozen == null) return;

		    try
		    {
		        Frozen(null, null);
		    }
		    catch (Exception e)
		    {
		        LogHelper.Error(typeof (Resolution), "Exception in Frozen event handler.", e);
		        throw;
		    }
		}
        
        /// <summary>
        /// Resets resolution, ie unfreezes it and clears Frozen event.
        /// </summary>
        /// <remarks>To be used in unit tests.</remarks>
        internal static void Reset()
        {
            LogHelper.Debug(typeof(Resolution), "Resetting resolution");

            /*
            var trace = new System.Diagnostics.StackTrace();
            var testing = trace.GetFrames().Any(frame =>
                frame.GetMethod().DeclaringType.FullName.StartsWith("Umbraco.Tests"));
            if (testing == false)
                throw new InvalidOperationException("Only unit tests can reset configuration.");
            */

            using (new WriteLock(ConfigurationLock))
            {
                _isFrozen = false;
            }
            Frozen = null;
        }
	}
}
