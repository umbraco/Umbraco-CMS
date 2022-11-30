using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Provides a base class for Umbraco API controllers.
/// </summary>
/// <remarks>
///     <para>These controllers are NOT auto-routed.</para>
///     <para>The base class is <see cref="ControllerBase" /> which are netcore API controllers without any view support</para>
/// </remarks>
[Authorize(Policy = AuthorizationPolicies.UmbracoFeatureEnabled)] // TODO: This could be part of our conventions
[UmbracoApiController]
public abstract class UmbracoApiControllerBase : ControllerBase, IUmbracoFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApiControllerBase" /> class.
    /// </summary>
    protected UmbracoApiControllerBase()
    {
    }
}
