using System;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Identifies an application tree
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TreeAttribute : Attribute, ITree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeAttribute"/> class.
        /// </summary>
        /// <param name="appAlias">The app alias.</param>
        /// <param name="treeAlias"></param>
        public TreeAttribute(string appAlias,
            string treeAlias) : this(appAlias, treeAlias, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeAttribute"/> class.
        /// </summary>
        /// <param name="appAlias">The app alias.</param>
        /// <param name="treeAlias"></param>
        /// <param name="treeTitle"></param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpen">The icon open.</param>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="isSingleNodeTree">Flag to define if this tree is a single node tree (will never contain child nodes, full screen app)</param>
        public TreeAttribute(string appAlias,
            string treeAlias,
            string treeTitle,
            string iconClosed = "icon-folder",
            string iconOpen = "icon-folder-open",
            bool initialize = true,
            int sortOrder = 0,
            bool isSingleNodeTree = false)
        {
            ApplicationAlias = appAlias;
            TreeAlias = treeAlias;
            TreeTitle = treeTitle;
            IconClosed = iconClosed;
            IconOpen = iconOpen;
            Initialize = initialize;
            SortOrder = sortOrder;
            IsSingleNodeTree = isSingleNodeTree;
        }

        public string ApplicationAlias { get; }
        public string TreeAlias { get; }
        public string TreeTitle { get; }
        public string IconClosed { get; }
        public string IconOpen { get; }
        public bool Initialize { get; }
        public int SortOrder { get; }

        /// <summary>
        /// Flag to define if this tree is a single node tree (will never contain child nodes, full screen app)
        /// </summary>
        public bool IsSingleNodeTree { get; }
    }
}
