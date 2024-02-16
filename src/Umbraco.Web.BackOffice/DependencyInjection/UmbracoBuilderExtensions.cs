using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.Common.Hosting;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all required components to run the Umbraco back office
    /// </summary>
    public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder, Action<IMvcBuilder>? configureMvc = null) => builder
            .AddConfiguration()
            .AddUmbracoCore()
            .AddWebComponents()
            .AddRuntimeMinifier()
            .AddBackOfficeCore()
            .AddBackOfficeIdentity()
            .AddMembersIdentity()
            .AddUmbracoProfiler()
            .AddMvcAndRazor(configureMvc)
            .AddWebServer()
            .AddRecurringBackgroundJobs()
            .AddNuCache()
            .AddDistributedCache()
            .AddCoreNotifications()
            .AddLogViewer()
            .AddExamine()
            .AddExamineIndexes()
            .AddSupplemenataryLocalizedTextFileSources();

    public static IUmbracoBuilder AddBackOfficeCore(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IStaticFilePathGenerator, UmbracoStaticFilePathGenerator>();
        builder.Services.AddScoped<BackOfficeSessionIdValidator>();
        builder.Services.AddScoped<BackOfficeSecurityStampValidator>();

        builder.Services.AddUnique<IPhysicalFileSystem>(factory =>
        {
            var path = "~/";
            IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
            return new PhysicalFileSystem(
                factory.GetRequiredService<IIOHelper>(),
                hostingEnvironment,
                factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                hostingEnvironment.MapPathContentRoot(path),
                hostingEnvironment.ToAbsolute(path)
            );
        });

        builder.Services.AddUnique<IConflictingRouteService, ConflictingRouteService>();
        return builder;
    }
}
