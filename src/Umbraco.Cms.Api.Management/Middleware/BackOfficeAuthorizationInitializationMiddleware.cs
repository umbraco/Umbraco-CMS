using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Middleware;

public class BackOfficeAuthorizationInitializationMiddleware : IMiddleware
{
    private bool _firstBackOfficeRequest; // this only works because this is a singleton
    private SemaphoreSlim _firstBackOfficeRequestLocker = new(1); // this only works because this is a singleton

    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRuntimeState _runtimeState;
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;
    private readonly IHostingEnvironment _hostingEnvironment;

    [Obsolete("Use the non-obsolete constructor. This will be removed in Umbraco 16.")]
    public BackOfficeAuthorizationInitializationMiddleware(
        UmbracoRequestPaths umbracoRequestPaths,
        IServiceProvider serviceProvider,
        IRuntimeState runtimeState)
    : this(
        umbracoRequestPaths,
        serviceProvider,
        runtimeState,
        StaticServiceProvider.Instance.GetRequiredService<IOptions<GlobalSettings>>(),
        StaticServiceProvider.Instance.GetRequiredService<IOptions<WebRoutingSettings>>(),
        StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>()
        )
    {
    }

    public BackOfficeAuthorizationInitializationMiddleware(
        UmbracoRequestPaths umbracoRequestPaths,
        IServiceProvider serviceProvider,
        IRuntimeState runtimeState,
        IOptions<GlobalSettings> globalSettings,
        IOptions<WebRoutingSettings> webRoutingSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _umbracoRequestPaths = umbracoRequestPaths;
        _serviceProvider = serviceProvider;
        _runtimeState = runtimeState;
        _globalSettings = globalSettings;
        _webRoutingSettings = webRoutingSettings;
        _hostingEnvironment = hostingEnvironment;
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
            Uri? backOfficeUrl = string.IsNullOrWhiteSpace(_webRoutingSettings.Value.UmbracoApplicationUrl) is false
                ? new Uri($"{_webRoutingSettings.Value.UmbracoApplicationUrl.TrimEnd('/')}{_globalSettings.Value.GetBackOfficePath(_hostingEnvironment).EnsureStartsWith('/')}")
                : null;

            using IServiceScope scope = _serviceProvider.CreateScope();
            IBackOfficeApplicationManager backOfficeApplicationManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
            await backOfficeApplicationManager.EnsureBackOfficeApplicationAsync(backOfficeUrl ?? new Uri(context.Request.GetDisplayUrl()));
            _firstBackOfficeRequest = true;
        }

        _firstBackOfficeRequestLocker.Release();
    }
}
