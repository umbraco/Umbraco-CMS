using System;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// The result from querying a controller/action in the existing routes
    /// </summary>
    public class ControllerActionSearchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerActionSearchResult"/> class.
        /// </summary>
        private ControllerActionSearchResult(
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
        /// Initializes a new instance of the <see cref="ControllerActionSearchResult"/> class.
        /// </summary>
        public ControllerActionSearchResult(
            string controllerName,
            Type controllerType,
            string actionName)
            : this(true, controllerName, controllerType, actionName)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the route could be hijacked
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

        /// <summary>
        /// Returns a failed result
        /// </summary>
        public static ControllerActionSearchResult Failed() => new ControllerActionSearchResult(false, null, null, null);
    }
}
