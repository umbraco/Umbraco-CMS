using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Populates indexes with data
    /// </summary>
    public interface IIndexPopulator
    {
        /// <summary>
        /// Populates indexes with data
        /// </summary>
        /// <param name="indexes"></param>
        void Populate(params IIndexer[] indexes);
    }
}
