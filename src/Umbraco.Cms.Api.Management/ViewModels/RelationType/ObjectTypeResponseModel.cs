namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

/// <summary>
/// Represents a response model containing information about an object type used in relation types within the API.
/// </summary>
public class ObjectTypeResponseModel
{
    /// <summary>
    /// Gets or sets the name of the object type.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the object type.
    /// </summary>
    public Guid Id { get; set; }
}
