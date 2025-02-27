namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentTypeImportOperationStatus
{
    SuccessCreated,
    SuccessUpdated,
    TemporaryFileNotFound,
    TemporaryFileConversionFailure,
    DocumentTypeExists,
    IdMismatch,
    TypeMismatch,
}
