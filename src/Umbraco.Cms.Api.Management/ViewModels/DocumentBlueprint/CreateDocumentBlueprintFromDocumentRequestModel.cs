namespace Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;

/// <summary>
/// Represents a request to create a document blueprint from an existing document.
/// </summary>
public class CreateDocumentBlueprintFromDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the reference to the document, identified by its ID, from which the blueprint will be created.
    /// </summary>
    public required ReferenceByIdModel Document { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the document blueprint.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the document blueprint.
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference to the parent entity by ID for the new document blueprint.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
