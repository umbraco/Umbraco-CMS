using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Middleware;

/// <summary>
/// Middleware that initializes authorization mechanisms for back office users on incoming HTTP requests.
/// Ensures that authorization requirements are set up before further request processing.
/// </summary>
public class BackOfficeAuthorizationInitializationMiddleware : IMiddleware
{
    private SemaphoreSlim _firstBackOfficeRequestLocker = new(1); // this only works because this is a singleton
    private ISet<string> _knownHosts = new HashSet<string>(); // this only works because this is a singleton

    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRuntimeState _runtimeState;
    private readonly WebRoutingSettings _webRoutingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeAuthorizationInitializationMiddleware"/> class.
    /// </summary>
    /// <param name="umbracoRequestPaths">Provides information about Umbraco-specific request paths used for routing and authorization.</param>
    /// <param name="serviceProvider">The application's dependency injection service provider for resolving services.</param>
    /// <param name="runtimeState">Represents the current runtime state of the Umbraco application.</param>
    /// <param name="webRoutingSettings">The configuration options for web routing settings.</param>
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

    /// <summary>
    /// Invokes the middleware to initialize back office authorization for the current HTTP context,
    /// then calls the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
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

        var host = $"{context.Request.Scheme}://{context.Request.Host}";
        if (_knownHosts.Contains(host))
        {
            return;
        }

        await _firstBackOfficeRequestLocker.WaitAsync();

        // NOTE: _knownHosts is not thread safe; check again after entering the semaphore
        if (_knownHosts.Add(host) is false)
        {
            _firstBackOfficeRequestLocker.Release();
            return;
        }

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
