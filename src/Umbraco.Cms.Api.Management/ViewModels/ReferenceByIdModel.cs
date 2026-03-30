namespace Umbraco.Cms.Api.Management.ViewModels;

// IMPORTANT: do NOT unseal this class as long as we have polymorphism support in OpenAPI; it will make a mess of all models.
/// <summary>
/// Represents a model containing a reference identified by an ID.
/// </summary>
public sealed class ReferenceByIdModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceByIdModel"/> class with default values.
    /// </summary>
    public ReferenceByIdModel()
        : this(Guid.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.ViewModels.ReferenceByIdModel"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the reference.</param>
    public ReferenceByIdModel(Guid id)
        => Id = id;

    /// <summary>
    /// Creates a <see cref="Umbraco.Cms.Api.Management.ViewModels.ReferenceByIdModel"/> instance for the specified ID, or returns <c>null</c> if the ID is <c>null</c>.
    /// </summary>
    /// <param name="id">The nullable <see cref="System.Guid"/> to create the reference from.</param>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.ViewModels.ReferenceByIdModel"/> instance if <paramref name="id"/> has a value; otherwise, <c>null</c>.</returns>
    public static ReferenceByIdModel? ReferenceOrNull(Guid? id)
        => id.HasValue ? new ReferenceByIdModel(id.Value) : null;

    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
}
