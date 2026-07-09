using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a property defined in a stylesheet for use in the backoffice RTE.
/// </summary>
public interface IStylesheetProperty : IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the alias of the stylesheet property.
    /// </summary>
    string Alias { get; set; }

    /// <summary>
    ///     Gets the name of the stylesheet property.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets or sets the CSS value of the stylesheet property.
    /// </summary>
    string Value { get; set; }
}
