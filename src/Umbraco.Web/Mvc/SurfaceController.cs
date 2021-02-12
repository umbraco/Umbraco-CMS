using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.Mvc
{
    public abstract class SurfaceController : PluginController
    {
        protected SurfaceController()
        { }

        protected SurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches,profilingLogger)
        { }

    }
}
