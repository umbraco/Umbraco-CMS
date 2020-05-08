using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Core;

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoWebsite(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();

            // can't continue if boot failed
            if (runtime.State.Level <= RuntimeLevel.BootFailed) return app;

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            app.UseImageSharp();
            app.UseStaticFiles();

            return app;
        }
    }
}
