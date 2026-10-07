namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines a Template File (Mvc View)
/// </summary>
/// <remarks>
///     The content of a template lives in its view file. Templates read through <c>ITemplateService</c> load it on
///     demand; templates reached in other ways, for example through a content type's allowed templates, may not have
///     it loaded, in which case <see cref="IFile.Content" /> is <c>null</c>.
/// </remarks>
public interface ITemplate : IFile
{
    /// <summary>
    ///     Gets the Name of the File including extension
    /// </summary>
    new string? Name { get; set; }

    /// <summary>
    ///     Gets the Alias of the File, which is the name without the extension
    /// </summary>
    new string Alias { get; set; }

    /// <summary>
    ///     Returns true if the template is used as a layout for other templates (i.e. it has 'children')
    /// </summary>
    bool IsLayoutTemplate { get; set; }

    /// <summary>
    ///     Returns the layout template alias (the parent template this template inherits from).
    /// </summary>
    string? LayoutTemplateAlias { get; }

    /// <inheritdoc cref="IsLayoutTemplate" />
    [Obsolete("Use IsLayoutTemplate instead. Scheduled for removal in Umbraco 20.")]
    bool IsMasterTemplate { get => IsLayoutTemplate; set => IsLayoutTemplate = value; }

    /// <inheritdoc cref="LayoutTemplateAlias" />
    [Obsolete("Use LayoutTemplateAlias instead. Scheduled for removal in Umbraco 20.")]
    string? MasterTemplateAlias => LayoutTemplateAlias;
}
