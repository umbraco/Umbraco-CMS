using System;
using Umbraco.Core;
using System.Collections.Specialized;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// Migrated already to .Net Core without MergeModelStateToChildAction and MergeParentContextViewData action filters
    /// TODO: Migrate MergeModelStateToChildAction and MergeParentContextViewData action filters
    [MergeModelStateToChildAction]
    [MergeParentContextViewData]
    public abstract class SurfaceController : PluginController
    {
        protected SurfaceController()
        { }

        protected SurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches,profilingLogger)
        { }

    }
}
