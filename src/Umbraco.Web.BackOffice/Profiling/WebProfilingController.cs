using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Web.BackOffice.Authorization;

namespace Umbraco.Web.BackOffice.Profiling
{
    /// <summary>
    /// The API controller used to display the state of the web profiler
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
    public class WebProfilingController : UmbracoAuthorizedJsonController
    {
        private readonly IHostingEnvironment _hosting;

        public WebProfilingController(IHostingEnvironment hosting)
        {
            _hosting = hosting;
        }

        public object GetStatus()
        {
            return new
            {
                Enabled = _hosting.IsDebugMode
            };
        }
    }}
