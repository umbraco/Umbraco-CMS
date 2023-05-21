using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Profiling;

/// <summary>
///     The API controller used to display the state of the web profiler
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class WebProfilingController : UmbracoAuthorizedJsonController
{
    private readonly IHostingEnvironment _hosting;

    public WebProfilingController(IHostingEnvironment hosting) => _hosting = hosting;

    public object GetStatus() =>
        new { Enabled = _hosting.IsDebugMode };
}
