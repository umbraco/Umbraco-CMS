using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler;

internal sealed class ConfigureMiniProfilerOptions(IHostingEnvironment hostingEnvironment, IUserService userService)
    : IConfigureOptions<MiniProfilerOptions>
{
    private readonly string _backOfficePath = hostingEnvironment.GetBackOfficePath();

    public void Configure(MiniProfilerOptions options)
    {
        options.RouteBasePath = WebPath.Combine(_backOfficePath, "profiler");
        // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
        options.ShouldProfile = request => false;

        options.IgnoredPaths.Clear();
        options.IgnoredPaths.Add(WebPath.Combine(_backOfficePath, "openapi"));
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

        Guid? userKey = identity?.GetUserKey();
        if (userKey is null)
        {
            return false;
        }

        IUser? user = await userService.GetAsync(userKey.Value);
        if (user is null)
        {
            return false;
        }

        return user.AllowedSections.Contains(Core.Constants.Applications.Settings);
    }
}
