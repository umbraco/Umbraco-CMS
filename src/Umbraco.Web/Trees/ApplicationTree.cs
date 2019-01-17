using System;
using System.Diagnostics;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [DebuggerDisplay("Tree - {TreeAlias} ({ApplicationAlias})")]
    public class ApplicationTree : ITree
    {
        public ApplicationTree(int sortOrder, string applicationAlias, string alias, string title, Type treeControllerType, bool isSingleNodeTree)
        {
            SortOrder = sortOrder;
            ApplicationAlias = applicationAlias;
            TreeAlias = alias;
            TreeTitle = title;
            TreeControllerType = treeControllerType;
            IsSingleNodeTree = isSingleNodeTree;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets the application alias.
        /// </summary>
        public string ApplicationAlias { get; set; }

        /// <inheritdoc />
        public string TreeAlias { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the tree title (fallback if the tree alias isn't localized)
        /// </summary>
        /// <value>The title.</value>
        public string TreeTitle { get; set; }

        public bool IsSingleNodeTree { get; }

        public Type TreeControllerType { get; }

        public static string GetRootNodeDisplayName(ITree tree, ILocalizedTextService textService)
        {
            var label = $"[{tree.TreeAlias}]";

            // try to look up a the localized tree header matching the tree alias
            var localizedLabel = textService.Localize("treeHeaders/" + tree.TreeAlias);

            // if the localizedLabel returns [alias] then return the title if it's defined
            if (localizedLabel != null && localizedLabel.Equals(label, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(tree.TreeTitle) == false)
                    label = tree.TreeTitle;
            }
            else
            {
                // the localizedLabel translated into something that's not just [alias], so use the translation
                label = localizedLabel;
            }

            return label;
        }

    }
}
