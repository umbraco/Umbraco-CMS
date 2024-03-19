namespace Umbraco.Cms.Api.Management.ViewModels.Relation;

public sealed class RelationReferenceModel
{
    public RelationReferenceModel()
        : this(Guid.Empty)
    {
    }

    public RelationReferenceModel(Guid id)
        => Id = id;

    public Guid Id { get; set; }

    public string? Name { get; set; }
}
