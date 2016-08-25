using System;
using System.Threading;
using Umbraco.Core.Logging;

namespace Umbraco.Core.ObjectResolution
{
    // fixme
    // this is the last bit that needs to go
    // however, if it goes, we're missing Resolution.Frozen even which is used here and there
    // => how can we do it?

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
