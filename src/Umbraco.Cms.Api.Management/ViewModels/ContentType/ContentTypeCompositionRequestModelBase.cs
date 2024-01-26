namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeCompositionRequestModelBase
{
    /// <summary>
    ///     Gets or sets the content type key.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    ///     Gets or sets the currently selected property aliases.
    /// </summary>
    /// <remarks>
    ///     This is required because when creating/modifying a content type, new properties being added to
    ///     it are not yet persisted so cannot be looked up via the db, they need to be passed in.
    /// </remarks>
    public IEnumerable<string> CurrentPropertyAliases { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the keys of the currently selected content types for composition (also direct inheritance).
    /// </summary>
    /// <remarks>
    ///     Any content types containing those aliases will be filtered out along with any content types
    ///     that have matching property types that are included in the specified ones.
    /// </remarks>
    public IEnumerable<Guid> CurrentCompositeIds { get; set; } = Array.Empty<Guid>();
}
