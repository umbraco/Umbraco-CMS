using Microsoft.AspNetCore.Http;
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
    private SemaphoreSlim _firstBackOfficeRequestLocker = new(1); // this only works because this is a singleton
    private ISet<string> _knownHosts = new HashSet<string>(); // this only works because this is a singleton

    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRuntimeState _runtimeState;
    private readonly WebRoutingSettings _webRoutingSettings;

    [Obsolete("Use the non-obsolete constructor. This will be removed in Umbraco 16.")]
    public BackOfficeAuthorizationInitializationMiddleware(
        UmbracoRequestPaths umbracoRequestPaths,
        IServiceProvider serviceProvider,
        IRuntimeState runtimeState)
    : this(
        umbracoRequestPaths,
        serviceProvider,
        runtimeState,
        StaticServiceProvider.Instance.GetRequiredService<IOptions<WebRoutingSettings>>())
    {
    }

    [Obsolete("Use the non-obsolete constructor. This will be removed in Umbraco 17.")]
    public BackOfficeAuthorizationInitializationMiddleware(
        UmbracoRequestPaths umbracoRequestPaths,
        IServiceProvider serviceProvider,
        IRuntimeState runtimeState,
        IOptions<GlobalSettings> globalSettings,
        IOptions<WebRoutingSettings> webRoutingSettings,
        IHostingEnvironment hostingEnvironment)
        : this(umbracoRequestPaths, serviceProvider, runtimeState, webRoutingSettings)
    {
    }

    public BackOfficeAuthorizationInitializationMiddleware(
        UmbracoRequestPaths umbracoRequestPaths,
        IServiceProvider serviceProvider,
        IRuntimeState runtimeState,
        IOptions<WebRoutingSettings> webRoutingSettings)
    {
        _umbracoRequestPaths = umbracoRequestPaths;
        _serviceProvider = serviceProvider;
        _runtimeState = runtimeState;
        _webRoutingSettings = webRoutingSettings.Value;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await InitializeBackOfficeAuthorizationOnceAsync(context);
        await next(context);
    }

    private async Task InitializeBackOfficeAuthorizationOnceAsync(HttpContext context)
    {
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

        if (_knownHosts.Add($"{context.Request.Scheme}://{context.Request.Host}") is false)
        {
            return;
        }

        await _firstBackOfficeRequestLocker.WaitAsync();

        // ensure we explicitly add UmbracoApplicationUrl if configured (https://github.com/umbraco/Umbraco-CMS/issues/16179)
        if (_webRoutingSettings.UmbracoApplicationUrl.IsNullOrWhiteSpace() is false)
        {
            _knownHosts.Add(_webRoutingSettings.UmbracoApplicationUrl);
        }

        Uri[] backOfficeHosts = _knownHosts
            .Select(host => Uri.TryCreate(host, UriKind.Absolute, out Uri? hostUri)
                ? hostUri
                : null)
            .WhereNotNull()
            .ToArray();

        using IServiceScope scope = _serviceProvider.CreateScope();
        IBackOfficeApplicationManager backOfficeApplicationManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        await backOfficeApplicationManager.EnsureBackOfficeApplicationAsync(backOfficeHosts);

        _firstBackOfficeRequestLocker.Release();
    }
}
