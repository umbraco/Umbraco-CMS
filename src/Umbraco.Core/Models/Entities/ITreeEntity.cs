namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Defines an entity that belongs to a tree.
/// </summary>
public interface ITreeEntity : IEntity
{
    /// <summary>
    ///     Gets or sets the name of the entity.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who created this entity.
    /// </summary>
    int CreatorId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the parent entity.
    /// </summary>
    int ParentId { get; set; }

    /// <summary>
    ///     Gets or sets the level of the entity.
    /// </summary>
    int Level { get; set; }

    /// <summary>
    ///     Gets or sets the path to the entity.
    /// </summary>
    string Path { get; set; }

    /// <summary>
    ///     Gets or sets the sort order of the entity.
    /// </summary>
    int SortOrder { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this entity is trashed.
    /// </summary>
    /// <remarks>
    ///     <para>Trashed entities are located in the recycle bin.</para>
    ///     <para>Always false for entities that do not support being trashed.</para>
    /// </remarks>
    bool Trashed { get; }

    /// <summary>
    ///     Sets the parent entity.
    /// </summary>
    /// <remarks>
    ///     Use this method to set the parent entity when the parent entity is known, but has not
    ///     been persistent and does not yet have an identity. The parent identifier will be retrieved
    ///     from the parent entity when needed. If the parent entity still does not have an entity by that
    ///     time, an exception will be thrown by <see cref="ParentId" /> getter.
    /// </remarks>
    void SetParent(ITreeEntity? parent);
}
