using Umbraco.Core.Plugins;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Provides a base class for auto-routed Umbraco API controllers.
    /// </summary>
    public abstract class UmbracoApiController : UmbracoApiControllerBase, IDiscoverable
    { }
}
