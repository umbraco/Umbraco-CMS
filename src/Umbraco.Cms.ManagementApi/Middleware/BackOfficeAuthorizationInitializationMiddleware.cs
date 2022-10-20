using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.New.Cms.Infrastructure.Security;

namespace Umbraco.Cms.ManagementApi.Middleware;

public class BackOfficeAuthorizationInitializationMiddleware : IMiddleware
{
    private static bool _firstBackOfficeRequest;
    private static SemaphoreSlim _firstBackOfficeRequestLocker = new(1);

    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IServiceProvider _serviceProvider;

    public BackOfficeAuthorizationInitializationMiddleware(UmbracoRequestPaths umbracoRequestPaths, IServiceProvider serviceProvider)
    {
        _umbracoRequestPaths = umbracoRequestPaths;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await InitializeBackOfficeAuthorizationOnceAsync(context);
        await next(context);
    }

    private async Task InitializeBackOfficeAuthorizationOnceAsync(HttpContext context)
    {
        if (_firstBackOfficeRequest)
        {
            return;
        }

        if (_umbracoRequestPaths.IsBackOfficeRequest(context.Request.Path) == false)
        {
            return;
        }

        await _firstBackOfficeRequestLocker.WaitAsync();
        if (_firstBackOfficeRequest == false)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IBackOfficeApplicationManager backOfficeApplicationManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
            await backOfficeApplicationManager.EnsureBackOfficeApplicationAsync(new Uri(context.Request.GetDisplayUrl()));
            _firstBackOfficeRequest = true;
        }

        _firstBackOfficeRequestLocker.Release();
    }
}

// TODO: remove this (used for testing BackOfficeAuthorizationInitializationMiddleware until it can be added to the existing UseBackOffice extension)
// public static class UmbracoApplicationBuilderExtensions
// {
//     public static IUmbracoApplicationBuilderContext UseNewBackOffice(this IUmbracoApplicationBuilderContext builder)
//     {
//         builder.AppBuilder.UseMiddleware<BackOfficeAuthorizationInitializationMiddleware>();
//         return builder;
//     }
// }
