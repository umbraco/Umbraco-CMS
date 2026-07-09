namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents the response model containing information about a property type reference within a content type.
/// </summary>
public abstract class ContentTypePropertyTypeReferenceResponseModel : ReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the alias of the property type reference.
    /// </summary>
    public string? Alias { get; set; }
}
