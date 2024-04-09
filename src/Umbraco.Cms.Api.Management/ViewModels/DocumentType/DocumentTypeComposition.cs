using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeComposition
{
    public required ReferenceByIdModel DocumentType { get; init; }

    public required CompositionType CompositionType { get; init; }
}
