using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

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
