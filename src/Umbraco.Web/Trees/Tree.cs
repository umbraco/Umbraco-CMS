﻿using System;
using System.Diagnostics;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    [DebuggerDisplay("Tree - {SectionAlias}/{TreeAlias}")]
    public class Tree : ITree
    {
        public Tree(int sortOrder, string applicationAlias, string group, string alias, string title, TreeUse use, Type treeControllerType, bool isSingleNodeTree)
        {
            SortOrder = sortOrder;
            SectionAlias = applicationAlias ?? throw new ArgumentNullException(nameof(applicationAlias));
            TreeGroup = group;
            TreeAlias = alias ?? throw new ArgumentNullException(nameof(alias));
            TreeTitle = title;
            TreeUse = use;
            TreeControllerType = treeControllerType ?? throw new ArgumentNullException(nameof(treeControllerType));
            IsSingleNodeTree = isSingleNodeTree;
        }

        /// <inheritdoc />
        public int SortOrder { get; set; }

        /// <inheritdoc />
        public string SectionAlias { get; set; }

        /// <inheritdoc />
        public string TreeGroup { get; }

        /// <inheritdoc />
        public string TreeAlias { get; }

        /// <inheritdoc />
        public string TreeTitle { get; set; }

        /// <inheritdoc />
        public TreeUse TreeUse { get; set; }

        /// <inheritdoc />
        public bool IsSingleNodeTree { get; }

        /// <summary>
        /// Gets the tree controller type.
        /// </summary>
        public Type TreeControllerType { get; }

        internal static string GetRootNodeDisplayName(ITree tree, ILocalizedTextService textService)
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
