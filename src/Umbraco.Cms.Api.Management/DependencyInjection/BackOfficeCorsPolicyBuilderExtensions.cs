using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Configuration;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class BackOfficeCorsPolicyBuilderExtensions
{
    internal static IUmbracoBuilder AddCorsPolicy(this IUmbracoBuilder builder)
    {
        // FIXME: Get the correct settings once NewBackOfficeSettings is merged with relevant existing settings from Core
        Uri? backOfficeHost = builder.Config
            .GetSection($"{Constants.Configuration.ConfigPrefix}NewBackOffice")
            .Get<NewBackOfficeSettings>()?.BackOfficeHost;

        if (backOfficeHost is null)
        {
            return builder;
        }

        const string policyName = "AllowCustomBackOfficeOrigin";

        // The specified URL must not contain a trailing slash (/)
        var customOrigin = backOfficeHost.ToString().TrimEnd(Constants.CharArrays.ForwardSlash);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: policyName,
                policy =>
                {
                    policy
                        .WithOrigins(customOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.PipelineFilters.Insert(0, new UmbracoPipelineFilter("UmbracoManagementApiCustomHostCorsPolicy")
            {
                PrePipeline = app => app.UseCors(policyName),
            });
        });

        return builder;
    }
}
