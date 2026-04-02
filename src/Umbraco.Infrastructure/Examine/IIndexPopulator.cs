using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Defines the contract for classes responsible for populating search indexes with data.
/// Implementations of this interface handle the logic required to add or update content in an index.
/// </summary>
public interface IIndexPopulator
{
    /// <summary>
    ///     If this index is registered with this populator
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool IsRegistered(IIndex index);

    /// <summary>
    ///     Populate indexers
    /// </summary>
    /// <param name="indexes"></param>
    void Populate(params IIndex[] indexes);
}
