using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
	public interface IFilteredControllerFactory : IControllerFactory
	{
		/// <summary>
		/// Determines whether this instance can handle the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns><c>true</c> if this instance can handle the specified request; otherwise, <c>false</c>.</returns>
		/// <remarks></remarks>
		bool CanHandle(RequestContext request);

        /// <summary>
        /// Returns the controller type for the controller name otherwise null if not found
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        Type GetControllerType(RequestContext requestContext, string controllerName);
	}
}