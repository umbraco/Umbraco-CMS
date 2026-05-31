namespace Umbraco.Cms.Api.Management.ViewModels.Item;

// IMPORTANT: do NOT unseal this class as long as we have polymorphism support in OpenAPI; it will make a mess of all models.
/// <summary>
/// Response model containing a reference to an item identified by its ID.
/// </summary>
public sealed class ItemReferenceByIdResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the referenced item.
    /// </summary>
    public Guid Id { get; set; }
}
