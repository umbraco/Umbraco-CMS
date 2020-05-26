using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Editors;


namespace Umbraco.Web.BackOffice.Profiling
{
    /// <summary>
    /// The API controller used to display the state of the web profiler
    /// </summary>
    [UmbracoApplicationAuthorizeAttribute(Core.Constants.Applications.Settings)]
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
