using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Used to rebuild Umbraco indexes.
    /// </summary>
    public interface IIndexRebuilder
    {
        /// <summary>
        /// Checks if the index has a populator assigned and can be rebuilt.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        bool CanRebuild(IIndex index);

        /// <summary>
        /// Rebuilds a single index.
        /// </summary>
        /// <param name="indexName"></param>
        void RebuildIndex(string indexName);

        /// <summary>
        /// Rebuilds all indexes.
        /// </summary>
        /// <param name="onlyEmptyIndexes"></param>
        void RebuildIndexes(bool onlyEmptyIndexes);
    }
}
