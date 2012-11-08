using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Mvc
{
	internal class SurfaceControllerResolver : ManyObjectsResolverBase<SurfaceControllerResolver, SurfaceController>
	{
		public SurfaceControllerResolver(IEnumerable<Type> surfaceControllers)
			: base(surfaceControllers)
		{
			
		}

		/// <summary>
		/// Gets the surface controllers
		/// </summary>
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