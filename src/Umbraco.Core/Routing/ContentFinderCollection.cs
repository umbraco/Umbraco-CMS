using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Represents a collection of <see cref="IContentFinder" /> instances.
/// </summary>
public class ContentFinderCollection : BuilderCollectionBase<IContentFinder>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the content finders.</param>
    public ContentFinderCollection(Func<IEnumerable<IContentFinder>> items)
        : base(items)
    {
    }
}
