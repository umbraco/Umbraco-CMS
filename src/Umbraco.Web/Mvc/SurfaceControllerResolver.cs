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
	}
}