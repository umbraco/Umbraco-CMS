namespace Umbraco.Cms.Core.Models;

public interface INavigationModel
{
    /// <summary>
    ///     Gets or sets the integer identifier of the entity.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    ///     Gets or sets the Guid unique identifier of the entity.
    /// </summary>
    Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the Guid unique identifier of the entity's content type.
    /// </summary>
    public Guid ContentTypeKey { get; set; }

    /// <summary>
    ///     Gets or sets the integer identifier of the parent entity.
    /// </summary>
    int ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the sort order of the entity.
    /// </summary>
    int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this entity is in the recycle bin.
    /// </summary>
    bool Trashed { get; set; }
}
