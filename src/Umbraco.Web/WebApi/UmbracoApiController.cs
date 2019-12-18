using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
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

        [Obsolete("This constructor is obsolete since it doesn't inject the UmbracoMapper. The UmbracoMapper will be resolved from the service locator Current.Mapper, which is not good for testability. Inject the UmbracoMapper using full constructor injection instead.")]
        protected UmbracoApiController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        protected UmbracoApiController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper, UmbracoMapper umbracoMapper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper, umbracoMapper)
        {
        }
    }
}
