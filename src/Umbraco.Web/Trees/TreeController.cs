using System;
using System.Collections.Concurrent;
using System.Linq;
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

        protected TreeController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache, IProfilingLogger logger, IRuntimeState runtimeState) : base(globalSettings, umbracoContext, sqlContext, services, applicationCache, logger, runtimeState)
        {
            Initialize();
        }

        protected TreeController()
        {
            Initialize();
        }

        /// <inheritdoc />
        public override string RootNodeDisplayName => Tree.GetRootNodeDisplayName(this, Services.TextService);

        /// <inheritdoc />
        public override string TreeAlias => _attribute.TreeAlias;
        /// <inheritdoc />
        public override string TreeTitle => _attribute.TreeTitle;
        /// <inheritdoc />
        public override string ApplicationAlias => _attribute.ApplicationAlias;
        /// <inheritdoc />
        public override int SortOrder => _attribute.SortOrder;
        /// <inheritdoc />
        public override bool IsSingleNodeTree => _attribute.IsSingleNodeTree;

        private void Initialize()
        {
            _attribute = GetTreeAttribute();
        }

        private static readonly ConcurrentDictionary<Type, TreeAttribute> TreeAttributeCache = new ConcurrentDictionary<Type, TreeAttribute>();

        private TreeAttribute GetTreeAttribute()
        {
            return TreeAttributeCache.GetOrAdd(GetType(), type =>
            {
                //Locate the tree attribute
                var treeAttributes = type
                    .GetCustomAttributes<TreeAttribute>(false)
                    .ToArray();

                if (treeAttributes.Length == 0)
                    throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");

                //assign the properties of this object to those of the metadata attribute
                return treeAttributes[0];
            });
        }
    }
}
