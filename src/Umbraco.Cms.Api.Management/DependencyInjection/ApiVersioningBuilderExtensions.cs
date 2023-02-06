using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ApiVersioningBuilderExtensions
{
    internal static IUmbracoBuilder AddApiVersioning(this IUmbracoBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = ManagementApiConfiguration.DefaultApiVersion;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.UseApiBehavior = false;
        });

        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.DefaultApiVersion = ManagementApiConfiguration.DefaultApiVersion;
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            options.AddApiVersionParametersWhenVersionNeutral = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
        });

        return builder;
    }
}
