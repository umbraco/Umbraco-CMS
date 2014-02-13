using System;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Represents some metadata about the controller
	/// </summary>
	internal class PluginControllerMetadata
	{
		internal Type ControllerType { get; set; }
		internal string ControllerName { get; set; }
		internal string ControllerNamespace { get; set; }
		internal string AreaName { get; set; }

        /// <summary>
        /// This is determined by another attribute [IsBackOffice] which slightly modifies the route path 
        /// allowing us to determine if it is indeed a back office request or not
        /// </summary>
        internal bool IsBackOffice { get; set; }
	}
}