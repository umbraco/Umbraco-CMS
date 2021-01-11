using System;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// The result from evaluating if a route can be hijacked
    /// </summary>
    public class HijackedRouteResult
    {
        /// <summary>
        /// Returns a failed result
        /// </summary>
        public static HijackedRouteResult Failed() => new HijackedRouteResult(false, null, null, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="HijackedRouteResult"/> class.
        /// </summary>
        public HijackedRouteResult(
            bool success,
            string controllerName,
            Type controllerType,
            string actionName)
        {
            Success = success;
            ControllerName = controllerName;
            ControllerType = controllerType;
            ActionName = actionName;
        }

        /// <summary>
        /// Gets a value indicating if the route could be hijacked
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the Controller name
        /// </summary>
        public string ControllerName { get; }

        /// <summary>
        /// Gets the Controller type
        /// </summary>
        public Type ControllerType { get; }

        /// <summary>
        /// Gets the Acton name
        /// </summary>
        public string ActionName { get; }
    }
}
