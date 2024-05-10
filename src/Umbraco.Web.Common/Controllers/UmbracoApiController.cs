using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Provides a base class for auto-routed Umbraco API controllers.
/// </summary>
public abstract class UmbracoApiController : UmbracoApiControllerBase, IDiscoverable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApiController" /> class.
    /// </summary>
    protected UmbracoApiController()
    {
    }
}
