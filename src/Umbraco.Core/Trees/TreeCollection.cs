using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Trees;

/// <summary>
///     Represents the collection of section trees.
/// </summary>
public class TreeCollection : BuilderCollectionBase<Tree>
{
    public TreeCollection(Func<IEnumerable<Tree>> items)
        : base(items)
    {
    }
}
