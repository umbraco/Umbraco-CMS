using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Provides a base class for authorized Umbraco controllers.
    /// </summary>
    /// <remarks>
    /// This controller essentially just uses a global UmbracoAuthorizeAttribute, inheritors that require more granular control over the
    /// authorization of each method can use this attribute instead of inheriting from this controller.
    /// </remarks>
    [UmbracoAuthorize]
    [DisableBrowserCache]
    public abstract class UmbracoAuthorizedController : UmbracoController
    {
        protected UmbracoAuthorizedController()
        {
        }

        protected UmbracoAuthorizedController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger)
            : base(globalSettings, umbracoContext, services, appCaches, profilingLogger)
        {
        }
    }
}
