using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// Used for custom routed controllers to execute within the context of Umbraco
    /// </summary>
    public interface IVirtualPageController
    {
        /// <summary>
        /// Returns the <see cref="IPublishedContent"/> to use as the current page for the request
        /// </summary>
        IPublishedContent FindContent(ActionExecutingContext actionExecutingContext);
    }
}
