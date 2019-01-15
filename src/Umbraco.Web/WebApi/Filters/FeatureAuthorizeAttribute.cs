using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core.Composing;
using Umbraco.Web.Features;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the controller is an authorized feature.
    /// </summary>
    /// <remarks>Else returns unauthorized.</remarks>
    public sealed class FeatureAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly UmbracoFeatures _features;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureAuthorizeAttribute"/> class.
        /// </summary>
        public FeatureAuthorizeAttribute()
        {
            // attributes have to use Current.Container
            _features = Current.Factory?.TryGetInstance<UmbracoFeatures>();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // if no features resolver has been set then return true, this will occur in unit
            // tests and we don't want users to have to set a resolver
            //just so their unit tests work.

            if (_features == null) return true;

            var controllerType = actionContext.ControllerContext.ControllerDescriptor.ControllerType;
            return _features.IsControllerEnabled(controllerType);
        }
    }
}
