using System;
using Microsoft.AspNetCore.Builder;

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoWebsite(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<UmbracoWebsiteMiddleware>();
        }
    }
}
