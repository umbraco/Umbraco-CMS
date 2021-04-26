using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Umbraco.Cms.Web.Common.DependencyInjection
{
    /// <summary>
    /// A <see cref="IStartupFilter"/> registered to automatically capture application services
    /// </summary>
    internal class UmbracoApplicationServicesCapture : IStartupFilter
    {
        /// <inheritdoc/>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                StaticServiceProvider.Instance = app.ApplicationServices;
                next(app);
            };
    }
}
