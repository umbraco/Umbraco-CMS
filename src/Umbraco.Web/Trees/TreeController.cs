using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Type, TreeAttribute> TreeAttributeCache = new ConcurrentDictionary<Type, TreeAttribute>();

        private readonly TreeAttribute _treeAttribute;

        protected TreeController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContext, sqlContext, services, appCaches, logger, runtimeState)
        {
            _treeAttribute = GetTreeAttribute();
        }

        protected TreeController()
        {
            _treeAttribute = GetTreeAttribute();
        }

        /// <inheritdoc />
        public override string RootNodeDisplayName => Tree.GetRootNodeDisplayName(this, Services.TextService);

        /// <inheritdoc />
        public override string TreeGroup => _treeAttribute.TreeGroup;

        /// <inheritdoc />
        public override string TreeAlias => _treeAttribute.TreeAlias;

        /// <inheritdoc />
        public override string TreeTitle => _treeAttribute.TreeTitle;

        /// <inheritdoc />
        public override string SectionAlias => _treeAttribute.SectionAlias;

        /// <inheritdoc />
        public override int SortOrder => _treeAttribute.SortOrder;

        /// <inheritdoc />
        public override bool IsSingleNodeTree => _treeAttribute.IsSingleNodeTree;

        private TreeAttribute GetTreeAttribute()
        {
            return TreeAttributeCache.GetOrAdd(GetType(), type =>
            {
                var treeAttribute = type.GetCustomAttribute<TreeAttribute>(false);
                if (treeAttribute == null)
                    throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");
                return treeAttribute;
            });
        }
    }
}
