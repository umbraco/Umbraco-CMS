using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for property value responses including editor information.
/// </summary>
public abstract class ValueResponseModelBase : ValueModelBase
{
    /// <summary>
    ///     Gets or sets the alias of the property editor used for this value.
    /// </summary>
    [Required]
    public string EditorAlias { get; set; } = string.Empty;
}
