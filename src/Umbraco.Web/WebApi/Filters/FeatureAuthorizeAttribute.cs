using System.Web.Http;
using System.Web.Http.Controllers;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Web.Features;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the controller is an authorized feature.
    /// </summary>
    /// <remarks>Else returns unauthorized.</remarks>
    public sealed class FeatureAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // if no features resolver has been set then return true, this will occur in unit
            // tests and we don't want users to have to set a resolver
            //just so their unit tests work.

            // fixme inject?
            var features = Current.Container?.TryGetInstance<UmbracoFeatures>();
            if (features == null) return true;

            var controllerType = actionContext.ControllerContext.ControllerDescriptor.ControllerType;
            return features.IsControllerEnabled(controllerType);
        }
    }
}
