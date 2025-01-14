namespace Umbraco.Cms.Api.Management.ViewModels.Item;

// IMPORTANT: do NOT unseal this class as long as we have polymorphism support in OpenAPI; it will make a mess of all models.
public sealed class ItemReferenceByIdResponseModel
{
    public Guid Id { get; set; }
}
