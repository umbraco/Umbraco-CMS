using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Accessors;
using Umbraco.Cms.Api.Delivery.Caching;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Handlers;
using Umbraco.Cms.Api.Delivery.Json;
using Umbraco.Cms.Api.Delivery.Rendering;
using Umbraco.Cms.Api.Delivery.Routing;
using Umbraco.Cms.Api.Delivery.Security;
using Umbraco.Cms.Api.Delivery.Services;
using Umbraco.Cms.Api.Delivery.Services.QueryBuilders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddDeliveryApi(this IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IRequestStartItemProvider, RequestStartItemProvider>();
        builder.Services.AddScoped<RequestContextOutputExpansionStrategy>();
        builder.Services.AddScoped<RequestContextOutputExpansionStrategyV2>();
        builder.Services.AddScoped<IOutputExpansionStrategy>(provider =>
        {
            HttpContext? httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            ApiVersion? apiVersion = httpContext?.GetRequestedApiVersion();
            if (apiVersion is null)
            {
                return provider.GetRequiredService<RequestContextOutputExpansionStrategyV2>();
            }

            // V1 of the Delivery API uses a different expansion strategy than V2+
            return apiVersion.MajorVersion == 1
                ? provider.GetRequiredService<RequestContextOutputExpansionStrategy>()
                : provider.GetRequiredService<RequestContextOutputExpansionStrategyV2>();
        });
        builder.Services.AddSingleton<IRequestCultureService, RequestCultureService>();
        builder.Services.AddSingleton<IRequestSegmmentService, RequestSegmentService>();
        builder.Services.AddSingleton<IRequestSegmentService, RequestSegmentService>();
        builder.Services.AddSingleton<IRequestRoutingService, RequestRoutingService>();
        builder.Services.AddSingleton<IRequestRedirectService, RequestRedirectService>();
        builder.Services.AddSingleton<IRequestPreviewService, RequestPreviewService>();
        builder.Services.AddSingleton<IOutputExpansionStrategyAccessor, RequestContextOutputExpansionStrategyAccessor>();
        builder.Services.AddSingleton<IRequestStartItemProviderAccessor, RequestContextRequestStartItemProviderAccessor>();
        builder.Services.AddSingleton<IApiAccessService, ApiAccessService>();
        builder.Services.AddSingleton<IApiContentQueryService, ApiContentQueryService>();
        builder.Services.AddSingleton<IApiContentQueryProvider, ApiContentQueryProvider>();
        builder.Services.AddSingleton<IApiContentQueryFactory, ApiContentQueryFactory>();
        builder.Services.AddSingleton<IApiMediaQueryService, ApiMediaQueryService>();
        builder.Services.AddTransient<IMemberApplicationManager, MemberApplicationManager>();
        builder.Services.AddTransient<IRequestMemberAccessService, RequestMemberAccessService>();
        builder.Services.AddTransient<ICurrentMemberClaimsProvider, CurrentMemberClaimsProvider>();

        builder.Services.ConfigureOptions<ConfigureUmbracoDeliveryApiSwaggerGenOptions>();
        builder.AddUmbracoApiOpenApiUI();

        builder
            .Services
            .AddControllers()
            .AddJsonOptions(Constants.JsonOptionsNames.DeliveryApi, options =>
            {
                // all Delivery API specific JSON options go here
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.TypeInfoResolver = new DeliveryApiJsonTypeResolver();
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddAuthentication();
        builder.AddUmbracoOpenIddict();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, InitializeMemberApplicationNotificationHandler>();
        builder.AddNotificationAsyncHandler<MemberSavedNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
        builder.AddNotificationAsyncHandler<MemberDeletedNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
        builder.AddNotificationAsyncHandler<AssignedMemberRolesNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
        builder.AddNotificationAsyncHandler<RemovedMemberRolesNotification, RevokeMemberAuthenticationTokensNotificationHandler>();

        // FIXME: remove this when Delivery API V1 is removed
        builder.Services.AddSingleton<MatcherPolicy, DeliveryApiItemsEndpointsMatcherPolicy>();

        builder.AddOutputCache();
        return builder;
    }

    private static IUmbracoBuilder AddOutputCache(this IUmbracoBuilder builder)
    {
        DeliveryApiSettings.OutputCacheSettings outputCacheSettings =
            builder.Config.GetSection(Constants.Configuration.ConfigDeliveryApi).Get<DeliveryApiSettings>()?.OutputCache
            ?? new DeliveryApiSettings.OutputCacheSettings();

        if (outputCacheSettings.Enabled is false || outputCacheSettings is { ContentDuration.TotalSeconds: <= 0, MediaDuration.TotalSeconds: <= 0 })
        {
            return builder;
        }

        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(build => build.AddPolicy<NoOutputCachePolicy>());

            if (outputCacheSettings.ContentDuration.TotalSeconds > 0)
            {
                options.AddPolicy(
                    Constants.DeliveryApi.OutputCache.ContentCachePolicy,
                    new DeliveryApiOutputCachePolicy(
                        outputCacheSettings.ContentDuration,
                        new StringValues([Constants.DeliveryApi.HeaderNames.AcceptLanguage, Constants.DeliveryApi.HeaderNames.AcceptSegment, Constants.DeliveryApi.HeaderNames.StartItem])));
            }

            if (outputCacheSettings.MediaDuration.TotalSeconds > 0)
            {
                options.AddPolicy(
                    Constants.DeliveryApi.OutputCache.MediaCachePolicy,
                    new DeliveryApiOutputCachePolicy(
                        outputCacheSettings.MediaDuration,
                        Constants.DeliveryApi.HeaderNames.StartItem));
            }
        });

        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new OutputCachePipelineFilter("UmbracoDeliveryApiOutputCache")));
        return builder;
    }
}
