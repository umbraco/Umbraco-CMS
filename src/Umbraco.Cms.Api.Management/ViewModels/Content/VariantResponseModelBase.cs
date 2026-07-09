using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Serves as the base class for response models representing content variants in the API.
/// </summary>
public abstract class VariantResponseModelBase : VariantModelBase
{
    /// <summary>
    /// Gets or sets the creation date of the content variant.
    /// </summary>
    public DateTimeOffset CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the content variant was last updated.
    /// </summary>
    public DateTimeOffset UpdateDate { get; set; }
}
