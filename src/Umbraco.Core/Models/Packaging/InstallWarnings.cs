namespace Umbraco.Cms.Core.Models.Packaging;

/// <summary>
///     Represents warnings that may occur during package installation due to conflicting entities.
/// </summary>
public class InstallWarnings
{
    // TODO: Shouldn't we detect other conflicting entities too ?
    /// <summary>
    ///     Gets or sets the collection of templates that conflict with existing templates.
    /// </summary>
    public IEnumerable<ITemplate>? ConflictingTemplates { get; set; } = Enumerable.Empty<ITemplate>();

    /// <summary>
    ///     Gets or sets the collection of stylesheets that conflict with existing stylesheets.
    /// </summary>
    public IEnumerable<IFile?>? ConflictingStylesheets { get; set; } = Enumerable.Empty<IFile>();
}
