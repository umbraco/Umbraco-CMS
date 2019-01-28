using System;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Identifies a section tree.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TreeAttribute : Attribute, ITree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeAttribute"/> class.
        /// </summary>
        public TreeAttribute(string sectionAlias, string treeAlias)
        {
            SectionAlias = sectionAlias;
            TreeAlias = treeAlias;
        }

        /// <summary>
        /// Gets the section alias.
        /// </summary>
        public string SectionAlias { get; }

        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        public string TreeAlias { get; }

        /// <summary>
        /// Gets or sets the tree title.
        /// </summary>
        public string TreeTitle { get; set; }

        /// <summary>
        /// Gets or sets the group of the tree.
        /// </summary>
        public string TreeGroup { get; set; }

        /// <summary>
        /// Gets the usage of the tree.
        /// </summary>
        public TreeUse TreeUse { get; set; } = TreeUse.Main;

        /// <summary>
        /// Gets or sets the tree icon when closed.
        /// </summary>
        public string IconClosed { get; set; } = "icon-folder";

        /// <summary>
        /// Gets or sets the tree icon when open.
        /// </summary>
        public string IconOpen { get; set; } = "icon-folder-open";

        /// <summary>
        /// Gets or sets the tree sort order.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tree is a single-node tree (no child nodes, full screen app).
        /// </summary>
        public bool IsSingleNodeTree { get; set; }
    }
}
