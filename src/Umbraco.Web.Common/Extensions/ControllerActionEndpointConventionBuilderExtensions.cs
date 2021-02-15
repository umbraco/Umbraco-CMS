using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.Common.Extensions
{
    public static class ControllerActionEndpointConventionBuilderExtensions
    {
        /// <summary>
        /// Allows for defining a callback to set the returned <see cref="IPublishedContent"/> for the current request for this route
        /// </summary>
        public static void ForUmbracoPage(
            this ControllerActionEndpointConventionBuilder builder,
            Func<HttpContext, IPublishedContent> findContent)
            => builder.Add(convention =>
            {
                // filter out matched endpoints that are suppressed
                if (convention.Metadata.OfType<ISuppressMatchingMetadata>().FirstOrDefault()?.SuppressMatching != true)
                {
                    // Get the controller action descriptor
                    ControllerActionDescriptor actionDescriptor = convention.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();
                    if (actionDescriptor != null)
                    {
                        // This is more or less like the IApplicationModelProvider, it allows us to add filters, etc... to the ControllerActionDescriptor
                        // dynamically. Here we will add our custom virtual page filter along with a callback in the endpoint's metadata
                        // to execute in order to find the IPublishedContent for the request.

                        var filter = new UmbracoVirtualPageFilterAttribute();
                        actionDescriptor.FilterDescriptors.Add(new FilterDescriptor(filter, 0));
                        convention.Metadata.Add(filter);
                        convention.Metadata.Add(new CustomRouteContentFinderDelegate(findContent));
                    }
                }
            });
    }
}
