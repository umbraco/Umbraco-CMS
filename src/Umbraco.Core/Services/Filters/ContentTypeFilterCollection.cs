using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Services.Filters;

/// <summary>
/// Defines an ordered collection of <see cref="IContentTypeFilter"/>.
/// </summary>
public class ContentTypeFilterCollection : BuilderCollectionBase<IContentTypeFilter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeFilterCollection"/> class.
    /// </summary>
    /// <param name="items">The collection items.</param>
    public ContentTypeFilterCollection(Func<IEnumerable<IContentTypeFilter>> items)
        : base(items)
    {
    }
}
