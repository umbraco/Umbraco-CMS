using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;

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
