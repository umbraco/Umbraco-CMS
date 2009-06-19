
namespace umbraco.Linq.Core
{
    /// <summary>
    /// Base of an umbraco content tree
    /// </summary>
    public interface ITree
    {
        /// <summary>
        /// Gets the <see cref="umbracoDataProvider"/> Provider associated with this instance
        /// </summary>
        /// <value>The provider.</value>
        umbracoDataProvider Provider { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Reloads the cache for the particular tree
        /// </summary>
        void ReloadCache();
    }
}
