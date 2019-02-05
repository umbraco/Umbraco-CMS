using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Provides a base class for auto-routed Umbraco API controllers.
    /// </summary>
    public abstract class UmbracoApiController : UmbracoApiControllerBase, IDiscoverable
    {
        protected UmbracoApiController()
        {
        }

        protected UmbracoApiController(UmbracoHelper umbracoHelper, IGlobalSettings globalSettings, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState)
            : base(umbracoHelper, globalSettings, sqlContext, services, appCaches, logger, runtimeState)
        {
        }
    }
}
