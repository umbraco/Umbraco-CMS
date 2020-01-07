using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
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
        private readonly IRuntimeState _runtimeState;

        public WebProfilingController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper, IShortStringHelper shortStringHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper, shortStringHelper)
        {
            _runtimeState = runtimeState;
        }

        public object GetStatus()
        {
            return new
            {
                Enabled = _runtimeState.Debug
            };
        }
    }}
