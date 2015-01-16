using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Logging;
using umbraco.interfaces;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// A resolver to return all IApplicationEvents objects
	/// </summary>
	/// <remarks>
	/// This is disposable because after the app has started it should be disposed to release any memory being occupied by instances.
	/// </remarks>
    internal  sealed class ApplicationEventsResolver : ManyObjectsResolverBase<ApplicationEventsResolver, IApplicationEventHandler>, IDisposable
	{

	    private readonly LegacyStartupHandlerResolver _legacyResolver;

	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="logger"></param>
	    /// <param name="applicationEventHandlers"></param>
	    /// <param name="serviceProvider"></param>		
	    internal ApplicationEventsResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> applicationEventHandlers)
            : base(serviceProvider, logger, applicationEventHandlers)
		{
            //create the legacy resolver and only include the legacy types
	        _legacyResolver = new LegacyStartupHandlerResolver(
	            applicationEventHandlers.Where(x => !TypeHelper.IsTypeAssignableFrom<IApplicationEventHandler>(x)));
		}

        /// <summary>
        /// Override in order to only return types of IApplicationEventHandler and above, 
        /// do not include the legacy types of IApplicationStartupHandler
        /// </summary>
        protected override IEnumerable<Type> InstanceTypes
        {
            get { return base.InstanceTypes.Where(TypeHelper.IsTypeAssignableFrom<IApplicationEventHandler>); }
        }

	    /// <summary>
		/// Gets the <see cref="IApplicationEventHandler"/> implementations.
		/// </summary>
		public IEnumerable<IApplicationEventHandler> ApplicationEventHandlers
		{
            get { return Values; }
		}

        /// <summary>
        /// Create instances of all of the legacy startup handlers
        /// </summary>
	    public void InstantiateLegacyStartupHandlers()
	    {
            //this will instantiate them all
	        var handlers = _legacyResolver.LegacyStartupHandlers;
	    }

		protected override bool SupportsClear
		{
            get { return false; }
		}		

		protected override bool SupportsInsert
		{
			get { return false; }			
		}

	    private class LegacyStartupHandlerResolver : ManyObjectsResolverBase<ApplicationEventsResolver, IApplicationStartupHandler>, IDisposable
	    {
	        internal LegacyStartupHandlerResolver(IEnumerable<Type> legacyStartupHandlers)
                : base(legacyStartupHandlers)
	        {

	        }

            public IEnumerable<IApplicationStartupHandler> LegacyStartupHandlers
            {
                get { return Values; }
            }

	        public void Dispose()
	        {
                ResetCollections();
	        }
	    }

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

        ~ApplicationEventsResolver()
		{
			// Run dispose but let the class know it was due to the finalizer running.
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			// Only operate if we haven't already disposed
			if (IsDisposed || disposing == false) return;

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
	    /// Clear out all of the instances, we don't want them hanging around and cluttering up memory
	    /// </summary>
	    private void DisposeResources()
	    {
            _legacyResolver.Dispose();
            ResetCollections();
	    }
        
	}
}