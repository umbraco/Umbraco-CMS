using System;
using Microsoft.AspNetCore.Builder;
using Smidge;
using Smidge.Models;

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

            app.UseRuntimeMinifier();

            return app;
        }

        public static IApplicationBuilder UseRuntimeMinifier(this IApplicationBuilder app)
        {
            app.UseSmidge(bundles =>
            {
                bundles.Create("default-css",
                    new CssFile("~/assets/css/umbraco.css"),
                    new CssFile("~/lib/bootstrap-social/bootstrap-social.css"),
                    new CssFile("~/lib/font-awesome/css/font-awesome.min.css"));

                bundles.Create("index-css",
                    new CssFile("assets/css/canvasdesigner.css"));

                bundles.Create("default-js", WebFileType.Js,
                    "//cdnjs.cloudflare.com/ajax/libs/jquery/3.4.1/jquery.min.js",
                    "//cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.19.0/jquery.validate.min.js",
                    "//cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/3.2.11/jquery.validate.unobtrusive.min.js");
            });
            
            return app;
        }
    }
}
