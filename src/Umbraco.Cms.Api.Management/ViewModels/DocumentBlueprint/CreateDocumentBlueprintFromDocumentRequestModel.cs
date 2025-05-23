namespace Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;

public class CreateDocumentBlueprintFromDocumentRequestModel
{
    public required ReferenceByIdModel Document { get; set; }

    public Guid? Id { get; set; }

    public required string Name { get; set; } = string.Empty;

    public ReferenceByIdModel? Parent { get; set; }
}
