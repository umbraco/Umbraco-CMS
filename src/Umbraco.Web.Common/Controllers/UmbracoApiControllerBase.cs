using System;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Features;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// Provides a base class for Umbraco API controllers.
    /// </summary>
    /// <remarks>These controllers are NOT auto-routed.</remarks>
    [FeatureAuthorize]
    public abstract class UmbracoApiControllerBase : Controller, IUmbracoFeature
    {

    }
}
