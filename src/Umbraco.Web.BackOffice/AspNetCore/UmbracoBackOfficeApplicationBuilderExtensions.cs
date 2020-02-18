using System;
using Microsoft.AspNetCore.Builder;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<UmbracoMiddleware>();
        }
    }
}
