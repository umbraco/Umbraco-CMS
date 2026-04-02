namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a presentation model for a data type property in the Umbraco CMS Management API.
/// </summary>
public class DataTypePropertyPresentationModel
{
    /// <summary>
    /// Gets the unique alias of the data type property.
    /// </summary>
    public string Alias { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current value assigned to the data type property. This value may be <c>null</c> if not set.
    /// </summary>
    public object? Value { get; init; }
}
