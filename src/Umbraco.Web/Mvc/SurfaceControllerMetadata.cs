using System;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Represents some metadata about the surface controller
	/// </summary>
	internal class SurfaceControllerMetadata
	{
		internal Type ControllerType { get; set; }
		internal string ControllerName { get; set; }
		internal string ControllerNamespace { get; set; }
		internal Guid? ControllerId { get; set; }
	}
}