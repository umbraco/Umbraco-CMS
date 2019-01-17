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
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoApiController"/> with auto dependencies.
        /// </summary>
        /// <remarks>Dependencies are obtained from the <see cref="Current"/> service locator.</remarks>
        protected UmbracoApiController()
        { }

        /// <summary>
        /// Initialize a new instance of the <see cref="UmbracoApiController"/> with all its dependencies.
        /// </summary>
        protected UmbracoApiController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches applicationCache, IProfilingLogger logger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, applicationCache, logger, runtimeState)
        { }
    }
}
