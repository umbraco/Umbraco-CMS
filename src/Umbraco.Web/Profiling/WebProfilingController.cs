using Umbraco.Web.Editors;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Profiling
{
    /// <summary>
    /// The API controller used to display the state of the web profiler
    /// </summary>
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Settings)]
    public class WebProfilingController : UmbracoAuthorizedJsonController
    {
        public object GetStatus()
        {
            return new
            {
                Enabled = Core.Configuration.GlobalSettings.DebugMode
            };
        }
    }}
