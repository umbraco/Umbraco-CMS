namespace Umbraco.Cms.Api.Management.ViewModels.Relation;

/// <summary>
/// Represents a reference to a relation in the Umbraco CMS Management API.
/// </summary>
public sealed class RelationReferenceModel
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.ViewModels.Relation.RelationReferenceModel"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the relation reference.</param>
    public RelationReferenceModel(Guid id)
        => Id = id;

    /// <summary>
    /// Gets or sets the unique identifier of the relation reference.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the relation reference.
    /// </summary>
    public string? Name { get; set; }
}
