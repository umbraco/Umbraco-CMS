using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Serves as the base class for data type models in the Umbraco CMS Management API.
/// </summary>
public abstract class DataTypeModelBase
{
    /// <summary>
    /// Gets or sets the name of the data type.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the editor alias for the data type.
    /// </summary>
    [Required]
    public string EditorAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias of the editor UI associated with this data type.
    /// </summary>
    public string EditorUiAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of property values associated with the data type.
    /// </summary>
    public IEnumerable<DataTypePropertyPresentationModel> Values { get; set; } = Enumerable.Empty<DataTypePropertyPresentationModel>();
}
