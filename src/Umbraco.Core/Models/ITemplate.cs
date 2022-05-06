using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines a Template File (Mvc View)
/// </summary>
public interface ITemplate : IFile, IRememberBeingDirty, ICanBeDirty
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
    bool IsMasterTemplate { get; set; }

    /// <summary>
    ///     returns the master template alias
    /// </summary>
    string? MasterTemplateAlias { get; }

    /// <summary>
    ///     Set the mastertemplate
    /// </summary>
    /// <param name="masterTemplate"></param>
    void SetMasterTemplate(ITemplate? masterTemplate);
}
