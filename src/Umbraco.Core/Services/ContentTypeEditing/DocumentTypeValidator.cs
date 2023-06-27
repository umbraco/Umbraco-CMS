using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public class DocumentTypeValidator : ContentTypeValidator<DocumentTypeBase, DocumentPropertyType, DocumentTypePropertyContainer>
{
    public DocumentTypeValidator(IContentTypeService contentTypeService, IDataTypeService dataTypeService)
        : base(contentTypeService, dataTypeService)
    {
    }

    public async Task<ContentTypeOperationStatus> ValidateCreate(DocumentTypeCreateModel createModel)
    {
        ContentTypeOperationStatus commonStatus = await ValidateCommon(createModel);

        if (commonStatus is not ContentTypeOperationStatus.Success)
        {
            return commonStatus;
        }

        throw new NotImplementedException();
    }
}
