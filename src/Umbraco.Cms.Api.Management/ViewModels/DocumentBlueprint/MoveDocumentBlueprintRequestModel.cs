namespace Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;

/// <summary>
/// Request model for moving a document blueprint.
/// </summary>
public class MoveDocumentBlueprintRequestModel
{
    /// <summary>
    /// Gets or sets the target location, specified by ID, to which the document blueprint will be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
