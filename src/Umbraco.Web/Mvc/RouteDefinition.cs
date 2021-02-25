using System;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Web.Mvc
{
    // TODO: Migrated already to .Net Core
    public class RouteDefinition
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        /// <summary>
        /// The Controller type found for routing to
        /// </summary>
        public Type ControllerType { get; set; }


        /// <summary>
        /// Everything related to the current content request including the requested content
        /// </summary>
        public IPublishedRequest PublishedRequest { get; set; }

        /// <summary>
        /// Gets/sets whether the current request has a hijacked route/user controller routed for it
        /// </summary>
        public bool HasHijackedRoute { get; set; }
    }
}
