using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Routing;

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

        protected UmbracoApiController(GlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoMapper umbracoMapper, IPublishedUrlProvider publishedUrlProvider)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
        {
        }
    }
}
