using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Changes;

/// <summary>
///     Represents a change to a content type.
/// </summary>
/// <typeparam name="TItem">The type of content type that changed.</typeparam>
public class ContentTypeChange<TItem>
    where TItem : class, IContentTypeComposition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeChange{TItem}"/> class.
    /// </summary>
    /// <param name="item">The content type that changed.</param>
    /// <param name="changeTypes">The types of changes that occurred.</param>
    public ContentTypeChange(TItem item, ContentTypeChangeTypes changeTypes)
    {
        Item = item;
        ChangeTypes = changeTypes;
    }

    /// <summary>
    ///     Gets the content type that changed.
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    ///     Gets or sets the types of changes that occurred.
    /// </summary>
    public ContentTypeChangeTypes ChangeTypes { get; set; }
}
