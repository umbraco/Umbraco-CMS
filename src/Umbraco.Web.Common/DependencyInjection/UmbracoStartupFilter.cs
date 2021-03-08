using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.DependencyInjection
{
    /// <summary>
    /// A <see cref="IStartupFilter"/> registered early in DI so that it executes before any user IStartupFilters
    /// to ensure that all Umbraco service and requirements are started correctly and in order.
    /// </summary>
    public sealed class UmbracoStartupFilter : IStartupFilter
    {
        private readonly IOptions<UmbracoStartupFilterOptions> _options;
        public UmbracoStartupFilter(IOptions<UmbracoStartupFilterOptions> options) => _options = options;

        /// <inheritdoc/>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                _options.Value.PreUmbracoPipeline(app);

                app.UseUmbraco();
                next(app);
            };
    }
}
