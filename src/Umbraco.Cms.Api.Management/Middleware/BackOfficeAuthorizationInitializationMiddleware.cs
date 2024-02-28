﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Management.Middleware;

public class BackOfficeAuthorizationInitializationMiddleware : IMiddleware
{
    private bool _firstBackOfficeRequest; // this only works because this is a singleton
    private SemaphoreSlim _firstBackOfficeRequestLocker = new(1); // this only works because this is a singleton

    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRuntimeState _runtimeState;

    public BackOfficeAuthorizationInitializationMiddleware(
        UmbracoRequestPaths umbracoRequestPaths,
        IServiceProvider serviceProvider,
        IRuntimeState runtimeState)
    {
        _umbracoRequestPaths = umbracoRequestPaths;
        _serviceProvider = serviceProvider;
        _runtimeState = runtimeState;
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

        // Install is okay without this, because we do not need a token to install,
        // but upgrades do, so we need to execute for everything higher then or equal to upgrade.
        if (_runtimeState.Level < RuntimeLevel.Upgrade)
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
