namespace Umbraco.Web.Trees
{
    // TODO: we don't really use this, it is nice to have the treecontroller, attribute and ApplicationTree streamlined to implement this but it's not used
    //leave as internal for now, maybe we'll use in the future, means we could pass around ITree 
    internal interface ITree
    {
        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        int SortOrder { get; }

        /// <summary>
        /// Gets the section alias.
        /// </summary>
        string ApplicationAlias { get; }

        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        string TreeAlias { get; }

        /// <summary>
        /// Gets or sets the tree title (fallback if the tree alias isn't localized)
        /// </summary>
        string TreeTitle { get; }

        /// <summary>
        /// Flag to define if this tree is a single node tree (will never contain child nodes, full screen app)
        /// </summary>
        bool IsSingleNodeTree { get; }
    }
}
