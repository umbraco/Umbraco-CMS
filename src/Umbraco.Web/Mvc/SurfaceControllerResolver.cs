using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Mvc
{
    internal sealed class SurfaceControllerResolver : ManyObjectsResolverBase<SurfaceControllerResolver, SurfaceController>
	{
		public SurfaceControllerResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> surfaceControllers)
            : base(serviceProvider, logger, surfaceControllers)
		{
			
		}

		/// <summary>
		/// Gets the surface controllers
		/// </summary>
        [Obsolete("This property should not be used in code, controllers are to be instantiated via MVC. To get a list of SurfaceController types use the RegisteredSurfaceControllers property.")]
		public IEnumerable<SurfaceController> SurfaceControllers
		{
			get { return Values; }
		}

		/// <summary>
		/// Gets all of the surface controller types
		/// </summary>
		public IEnumerable<Type> RegisteredSurfaceControllers
		{
			get { return InstanceTypes; }
		}
        
	}
}