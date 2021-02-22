using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.DependencyInjection
{
    /// <summary>
    /// A <see cref="IStartupFilter"/> registered early in DI so that it executes before any user IStartupFilters
    /// to ensure that all Umbraco service and requirements are started correctly and in order.
    /// </summary>
    public sealed class UmbracoStartupFilter : IStartupFilter
    {
        /// <inheritdoc/>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.UseUmbraco();
                next(app);
            };
    }
}
