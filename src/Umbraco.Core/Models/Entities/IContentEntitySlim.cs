namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents a lightweight content entity, managed by the entity service.
/// </summary>
public interface IContentEntitySlim : IEntitySlim
{
    /// <summary>
    ///     Gets the content type alias.
    /// </summary>
    string ContentTypeAlias { get; }

    /// <summary>
    ///     Gets the content type icon.
    /// </summary>
    string? ContentTypeIcon { get; }

    /// <summary>
    ///     Gets the content type thumbnail.
    /// </summary>
    string? ContentTypeThumbnail { get; }
}
