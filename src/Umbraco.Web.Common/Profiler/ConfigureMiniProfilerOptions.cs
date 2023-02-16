using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler;

internal sealed class ConfigureMiniProfilerOptions : IConfigureOptions<MiniProfilerOptions>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly string _backOfficePath;

    public ConfigureMiniProfilerOptions(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _backOfficePath = globalSettings.Value.GetBackOfficePath(hostingEnvironment);
    }

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

        options.ResultsAuthorize = IsBackofficeUserAuthorized;
        options.ResultsListAuthorize = IsBackofficeUserAuthorized;
    }

    private bool IsBackofficeUserAuthorized(HttpRequest request) => true;// FIXME when we can get current backoffice user, _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser is not null;
}
