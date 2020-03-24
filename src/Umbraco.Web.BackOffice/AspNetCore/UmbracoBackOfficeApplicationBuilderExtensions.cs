using System;
using Microsoft.AspNetCore.Builder;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app;
        }
    }
}
