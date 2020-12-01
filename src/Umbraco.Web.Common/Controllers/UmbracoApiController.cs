using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// Provides a base class for auto-routed Umbraco API controllers.
    /// </summary>
    public abstract class UmbracoApiController : UmbracoApiControllerBase, IDiscoverable
    {
        // TODO: Should this only exist in the back office project? These really are only ever used for the back office AFAIK

        protected UmbracoApiController()
        {
        }
    }
}
