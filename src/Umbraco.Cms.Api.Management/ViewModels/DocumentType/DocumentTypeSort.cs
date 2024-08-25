namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeSort
{
    public required ReferenceByIdModel DocumentType { get; init; }

    public int SortOrder { get; init; }
}
