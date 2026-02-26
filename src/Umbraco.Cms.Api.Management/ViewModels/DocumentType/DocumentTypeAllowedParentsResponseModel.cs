namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeAllowedParentsResponseModel
{
    public required ISet<ReferenceByIdModel> AllowedParentIds { get; set; }
}
