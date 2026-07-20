using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

/// <summary>
/// Represents a response model containing information about a data type item in the Umbraco CMS management API.
/// </summary>
public class DataTypeItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the alias of the editor UI used by the data type.
    /// </summary>
    public string? EditorUiAlias { get; set; }

    /// <summary>
    /// Gets or sets the editor alias for the data type.
    /// </summary>
    public string EditorAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this data type item can be deleted.
    /// </summary>
    public bool IsDeletable { get; set; }
}
