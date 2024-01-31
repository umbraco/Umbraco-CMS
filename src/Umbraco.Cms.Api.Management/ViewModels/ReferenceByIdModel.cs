namespace Umbraco.Cms.Api.Management.ViewModels;

// IMPORTANT: do NOT unseal this class as long as we have polymorphism support in OpenAPI; it will make a mess of all models.
public sealed class ReferenceByIdModel
{
    public ReferenceByIdModel()
        : this(Guid.Empty)
    {
    }

    public ReferenceByIdModel(Guid id)
        => Id = id;

    public Guid Id { get; set; }
}
