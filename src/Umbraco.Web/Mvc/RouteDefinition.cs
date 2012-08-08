using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Represents the data required to route to a specific controller/action during an Umbraco request
	/// </summary>
	public class RouteDefinition
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
		public object DocumentRequest { get; set; }
	}
}