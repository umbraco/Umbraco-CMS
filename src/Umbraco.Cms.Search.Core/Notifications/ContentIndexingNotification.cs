using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Notifications;

/// <summary>
/// Notification published while a single item is being indexed, carrying the fields about to be written.
/// </summary>
/// <remarks>
/// This notification is cancelable, allowing handlers to add, remove, or replace fields before they are written to the index,
/// or to skip indexing the item entirely.
/// </remarks>
public sealed class ContentIndexingNotification : ICancelableNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentIndexingNotification"/> class.
    /// </summary>
    /// <param name="indexAlias">The alias of the index the item is being written to.</param>
    /// <param name="id">The key of the item being indexed.</param>
    /// <param name="objectType">The entity type of the item being indexed.</param>
    /// <param name="variations">The culture/segment variations the item is being indexed for.</param>
    /// <param name="fields">The fields to be written to the index.</param>
    public ContentIndexingNotification(
        string indexAlias,
        Guid id,
        UmbracoObjectTypes objectType,
        IEnumerable<Variation> variations,
        IEnumerable<IndexField> fields)
    {
        IndexAlias = indexAlias;
        Id = id;
        ObjectType = objectType;
        Variations = variations;
        Fields = fields;
    }

    /// <summary>
    /// Gets the alias of the index the item is being written to.
    /// </summary>
    public string IndexAlias { get; }

    /// <summary>
    /// Gets the key of the item being indexed.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the entity type of the item being indexed.
    /// </summary>
    public UmbracoObjectTypes ObjectType { get; }

    /// <summary>
    /// Gets the culture/segment variations the item is being indexed for.
    /// </summary>
    public IEnumerable<Variation> Variations { get; }

    /// <summary>
    /// Gets or sets the fields to be written to the index. Handlers may replace this to alter what gets indexed.
    /// </summary>
    public IEnumerable<IndexField> Fields { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether indexing this item should be canceled.
    /// </summary>
    public bool Cancel { get; set; }
}
