using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// The base controller for all tree requests
    /// </summary>
    public abstract class TreeController : TreeControllerBase
    {
        private TreeAttribute _attribute;
        private string _rootNodeDisplayName;

        protected TreeController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, IProfilingLogger logger, IRuntimeState runtimeState) : base(globalSettings, umbracoContext, sqlContext, services, applicationCache, logger, runtimeState)
        {
        }

        protected TreeController()
        {
            Initialize();
        }

        /// <summary>
        /// The name to display on the root node
        /// </summary>
        public override string RootNodeDisplayName
        {
            get
            {
                throw new NotImplementedException();
                //return _rootNodeDisplayName
                //       ?? (_rootNodeDisplayName = Services.ApplicationTreeService.GetByAlias(_attribute.Alias)
                //           ?.GetRootNodeDisplayName(Services.TextService));
            }
        }

        /// <summary>
        /// Gets the current tree alias from the attribute assigned to it.
        /// </summary>
        public override string TreeAlias => _attribute.Alias;

        private void Initialize()
        {
            throw new NotImplementedException();
            //_attribute = GetType().GetTreeAttribute();
        }
    }
}
