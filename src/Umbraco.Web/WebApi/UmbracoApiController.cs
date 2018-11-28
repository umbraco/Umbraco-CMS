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
        { }

        protected UmbracoApiController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, ILogger logger, IProfilingLogger profilingLogger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContext, sqlContext, services, applicationCache, logger, profilingLogger, runtimeState)
        { }
    }
}
