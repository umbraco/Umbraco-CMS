namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum MediaTypeImportOperationStatus
{
    SuccessCreated,
    SuccessUpdated,
    TemporaryFileNotFound,
    TemporaryFileConversionFailure,
    MediaTypeExists,
    TypeMisMatch,
    IdMismatch
}
