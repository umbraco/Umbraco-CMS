using System.Web.Mvc;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Represents the data required to route to a specific controller/action during an Umbraco request
	/// </summary>
	internal class RouteDefinition
	{
		public string ControllerName { get; set; }
		public string ActionName { get; set; }

		/// <summary>
		/// The Controller instance found for routing to
		/// </summary>
		public ControllerBase Controller { get; set; }

		/// <summary>
		/// The current RenderModel found for the request
		/// </summary>
		public DocumentRequest DocumentRequest { get; set; }

		/// <summary>
		/// Gets/sets whether the current request has a hijacked route/user controller routed for it
		/// </summary>
		public bool HasHijackedRoute { get; set; }
	}
}