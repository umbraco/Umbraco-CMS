namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines a Template File (Mvc View)
/// </summary>
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
    bool IsLayout { get; set; }

    /// <summary>
    ///     Returns true if the template is used as a layout for other templates (i.e. it has 'children')
    /// </summary>
    [Obsolete("Use IsLayout instead. This will be removed in Umbraco 19.")]
    bool IsMasterTemplate { get => IsLayout; set => IsLayout = value; }

    /// <summary>
    ///     Returns the layout alias (the parent template this template inherits from)
    /// </summary>
    string? LayoutAlias { get; }

    /// <summary>
    ///     Returns the master template alias
    /// </summary>
    [Obsolete("Use LayoutAlias instead. This will be removed in Umbraco 19.")]
    string? MasterTemplateAlias => LayoutAlias;

    /// <summary>
    ///     Set the layout template
    /// </summary>
    /// <param name="layout">The layout template</param>
    [Obsolete("Layout is now calculated from the content. This will be removed in Umbraco 19.")]
    void SetLayout(ITemplate? layout);

    /// <summary>
    ///     Set the master template
    /// </summary>
    /// <param name="masterTemplate">The master template</param>
    [Obsolete("Use SetLayout instead. This will be removed in Umbraco 19.")]
    void SetMasterTemplate(ITemplate? masterTemplate) => SetLayout(masterTemplate);
}
