using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using StackExchange.Profiling;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler;

internal sealed class ConfigureMiniProfilerOptions : IConfigureOptions<MiniProfilerOptions>
{
    private readonly string _backOfficePath;

    public ConfigureMiniProfilerOptions(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment)
        => _backOfficePath = globalSettings.Value.GetBackOfficePath(hostingEnvironment);

    public void Configure(MiniProfilerOptions options)
    {
        options.RouteBasePath = WebPath.Combine(_backOfficePath, "profiler");
        // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
        options.ShouldProfile = request => false;

        options.IgnoredPaths.Clear();
        options.IgnoredPaths.Add(WebPath.Combine(_backOfficePath, "swagger"));
        options.IgnoredPaths.Add(WebPath.Combine(options.RouteBasePath, "results-list"));
        options.IgnoredPaths.Add(WebPath.Combine(options.RouteBasePath, "results-index"));
        options.IgnoredPaths.Add(WebPath.Combine(options.RouteBasePath, "results"));

        options.ResultsAuthorizeAsync = IsBackofficeUserAuthorized;
        options.ResultsListAuthorizeAsync = IsBackofficeUserAuthorized;
    }

    private async Task<bool> IsBackofficeUserAuthorized(HttpRequest request)
    {
        AuthenticateResult authenticateResult = await request.HttpContext.AuthenticateBackOfficeAsync();
        ClaimsIdentity? identity = authenticateResult.Principal?.GetUmbracoIdentity();

        return identity?.GetClaims(Core.Constants.Security.AllowedApplicationsClaimType)
            .InvariantContains(Core.Constants.Applications.Settings) ?? false;

    }
}
