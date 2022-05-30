using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.TestData.Configuration;

namespace Umbraco.TestData.Extensions;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoTestData(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => x.ServiceType == typeof(LoadTestController)))
        {
            // We assume the test data project is composed if any implementations of LoadTestController exist.
            return builder;
        }

        var testDataSection = builder.Config.GetSection("Umbraco:CMS:TestData");
        var config = testDataSection.Get<TestDataSettings>();
        if (config == null || config.Enabled == false)
        {
            return builder;
        }

        builder.Services.Configure<TestDataSettings>(testDataSection);

        if (config.IgnoreLocalDb)
        {
            builder.Services.AddSingleton(factory => new PublishedSnapshotServiceOptions { IgnoreLocalDb = true });
        }

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
            options.AddFilter(new UmbracoPipelineFilter(nameof(LoadTestController))
            {
                Endpoints = app => app.UseEndpoints(endpoints =>
                    endpoints.MapControllerRoute(
                        "LoadTest",
                        "/LoadTest/{action}",
                        new { controller = "LoadTest", Action = "Index" }))
            }));

        builder.Services.AddScoped(typeof(LoadTestController));

        return builder;
    }
}
