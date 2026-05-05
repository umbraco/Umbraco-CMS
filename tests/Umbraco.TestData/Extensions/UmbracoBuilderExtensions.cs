using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Website.Collections;
using Umbraco.TestData.Configuration;

namespace Umbraco.TestData.Extensions;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddUmbracoLoadTest(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => x.ServiceType == typeof(LoadTestController)))
        {
            return builder;
        }

        var testDataSection = builder.Config.GetSection("Umbraco:CMS:TestData");
        var config = testDataSection.Get<TestDataSettings>();
        if (config == null || config.Enabled == false)
        {
            return builder;
        }

        builder.Services.Configure<TestDataSettings>(testDataSection);

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
            options.AddFilter(new UmbracoPipelineFilter(nameof(LoadTestController))
            {
                PreMapEndpoints = endpoints =>
                    endpoints.MapControllerRoute(
                        "LoadTest",
                        "/LoadTest/{action}",
                        new { controller = "LoadTest", Action = "Index" }),
            }));

        builder.Services.AddScoped(typeof(LoadTestController));

        return builder;
    }

    public static IUmbracoBuilder AddUmbracoTestData(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => x.ServiceType == typeof(UmbracoTestDataController)))
        {
            return builder;
        }

        var testDataSection = builder.Config.GetSection("Umbraco:CMS:TestData");
        var config = testDataSection.Get<TestDataSettings>();
        if (config == null || config.Enabled == false)
        {
            return builder;
        }

        builder.Services.Configure<TestDataSettings>(testDataSection);

        builder.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
            .Add<UmbracoTestDataController>();

        return builder;
    }
}
