using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.interfaces;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// A resolver to return all IApplicationEvents objects
	/// </summary>
    internal sealed class ApplicationEventsResolver : ManyObjectsResolverBase<ApplicationEventsResolver, IApplicationEventHandler>
	{

	    private readonly LegacyStartupHandlerResolver _legacyResolver;

	    /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="applicationEventHandlers"></param>		
		internal ApplicationEventsResolver(IEnumerable<Type> applicationEventHandlers)
			: base(applicationEventHandlers)
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

	    private class LegacyStartupHandlerResolver : ManyObjectsResolverBase<ApplicationEventsResolver, IApplicationStartupHandler>
	    {
	        internal LegacyStartupHandlerResolver(IEnumerable<Type> legacyStartupHandlers)
                : base(legacyStartupHandlers)
	        {

	        }

            public IEnumerable<IApplicationStartupHandler> LegacyStartupHandlers
            {
                get { return Values; }
            }
	    }	    

	}
}