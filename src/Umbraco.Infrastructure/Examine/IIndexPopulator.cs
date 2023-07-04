using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

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
