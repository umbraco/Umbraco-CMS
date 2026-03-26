namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a reference to a property of a data type in the Umbraco CMS Management API.
/// </summary>
public class DataTypePropertyReferenceViewModel
{
    /// <summary>
    /// Gets or sets the name of the property reference.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the alias identifying the data type property.
    /// </summary>
    public required string Alias { get; init; }
}
