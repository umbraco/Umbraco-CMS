using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Features;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// Provides a base class for Umbraco API controllers.
    /// </summary>
    /// <remarks>
    /// <para>These controllers are NOT auto-routed.</para>
    /// <para>The base class is <see cref="ControllerBase"/> which are netcore API controllers without any view support</para>
    /// </remarks>
    [FeatureAuthorize] // TODO: This could be part of our conventions
    [TypeFilter(typeof(HttpResponseExceptionFilter))] // TODO: This could be part of our conventions
    [UmbracoApiController]
    public abstract class UmbracoApiControllerBase : ControllerBase, IUmbracoFeature
    {
        public UmbracoApiControllerBase()
        {
        }
    }
}
