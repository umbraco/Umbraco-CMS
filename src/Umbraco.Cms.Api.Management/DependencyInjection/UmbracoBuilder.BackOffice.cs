using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
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
    public static IUmbracoBuilder
        AddBackOffice(this IUmbracoBuilder builder, Action<IMvcBuilder>? configureMvc = null) => builder
        .AddConfiguration()
        .AddUmbracoCore()
        .AddWebComponents()
        .AddHelpers()
        .AddBackOfficeCore()
        .AddBackOfficeIdentity()
        .AddBackOfficeAuthentication()
        .AddTokenRevocation()
        .AddMembersIdentity()
        .AddUmbracoProfiler()
        .AddMvcAndRazor(configureMvc)
        .AddRecurringBackgroundJobs()
        .AddUmbracoHybridCache()
        .AddDistributedCache()
        .AddCoreNotifications();

    public static IUmbracoBuilder AddBackOfficeCore(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IBackOfficePathGenerator, UmbracoBackOfficePathGenerator>();
        builder.Services.AddUnique<IPhysicalFileSystem>(factory =>
        {
            var path = "~/";
            IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
            return new PhysicalFileSystem(
                factory.GetRequiredService<IIOHelper>(),
                hostingEnvironment,
                factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                hostingEnvironment.MapPathContentRoot(path),
                hostingEnvironment.ToAbsolute(path));
        });

        return builder;
    }
}
