using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// A Marker interface for defining an Umbraco indexer
    /// </summary>
    public interface IUmbracoIndexer : IIndex
    {
        /// <summary>
        /// When set to true Umbraco will keep the index in sync with Umbraco data automatically
        /// </summary>
        bool EnableDefaultEventHandler { get; }

        /// <summary>
        /// When set to true data will not be deleted from the index if the data is being soft deleted (unpublished or trashed)
        /// </summary>
        bool SupportSoftDelete { get; }
    }
}
