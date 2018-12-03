using System;
using System.Linq;
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

        protected TreeController()
        {
            Initialize();
        }

        /// <summary>
        /// The name to display on the root node
        /// </summary>
        public override string RootNodeDisplayName
            => _rootNodeDisplayName
                    ?? (_rootNodeDisplayName = Services.ApplicationTreeService.GetByAlias(_attribute.Alias)
                            ?.GetRootNodeDisplayName(Services.TextService));

        /// <summary>
        /// Gets the current tree alias from the attribute assigned to it.
        /// </summary>
        public override string TreeAlias
        {
            get { return _attribute.Alias; }
        }

        private void Initialize()
        {
            _attribute = GetType().GetTreeAttribute();
        }
    }
}
