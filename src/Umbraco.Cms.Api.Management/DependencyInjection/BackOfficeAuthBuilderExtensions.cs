using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class BackOfficeAuthBuilderExtensions
{
    public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
    {
        builder
            .AddAuthentication()
            .AddUmbracoOpenIddict()
            .AddBackOfficeLogin();

        return builder;
    }

    private static IUmbracoBuilder AddAuthentication(this IUmbracoBuilder builder)
    {
        builder.Services.AddAuthentication();
        builder.AddAuthorizationPolicies();

        builder.Services.AddTransient<IBackOfficeApplicationManager, BackOfficeApplicationManager>();
        builder.Services.AddSingleton<BackOfficeAuthorizationInitializationMiddleware>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new BackofficePipelineFilter("Backoffice")));

        return builder;
    }

    private static IUmbracoBuilder AddBackOfficeLogin(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddCookie(Constants.Security.NewBackOfficeAuthenticationType, options =>
            {
                options.LoginPath = "/umbraco/login";
                options.Cookie.Name = Constants.Security.NewBackOfficeAuthenticationType;
            });

        return builder;
    }
}

internal class BackofficePipelineFilter : UmbracoPipelineFilter
{
    public BackofficePipelineFilter(string name)
        : base(name)
        => PrePipeline = builder => builder.UseMiddleware<BackOfficeAuthorizationInitializationMiddleware>();
}
