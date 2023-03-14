namespace Umbraco.Cms.Core.Models;

public interface IReadOnlyContentBase
{
    /// <summary>
    ///     Gets the integer identifier of the entity.
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the Guid unique identifier of the entity.
    /// </summary>
    Guid Key { get; }

    /// <summary>
    ///     Gets the creation date.
    /// </summary>
    DateTime CreateDate { get; }

    /// <summary>
    ///     Gets the last update date.
    /// </summary>
    DateTime UpdateDate { get; }

    /// <summary>
    ///     Gets the name of the entity.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the identifier of the user who created this entity.
    /// </summary>
    int CreatorId { get; }

    /// <summary>
    ///     Gets the identifier of the parent entity.
    /// </summary>
    int ParentId { get; }

    /// <summary>
    ///     Gets the level of the entity.
    /// </summary>
    int Level { get; }

    /// <summary>
    ///     Gets the path to the entity.
    /// </summary>
    string? Path { get; }

    /// <summary>
    ///     Gets the sort order of the entity.
    /// </summary>
    int SortOrder { get; }

    /// <summary>
    ///     Gets the content type id
    /// </summary>
    int ContentTypeId { get; }

    /// <summary>
    ///     Gets the identifier of the writer.
    /// </summary>
    int WriterId { get; }

    /// <summary>
    ///     Gets the version identifier.
    /// </summary>
    int VersionId { get; }
}
