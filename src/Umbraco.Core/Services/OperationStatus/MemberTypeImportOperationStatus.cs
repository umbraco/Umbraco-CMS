namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum MemberTypeImportOperationStatus
{
    SuccessCreated,
    SuccessUpdated,
    TemporaryFileNotFound,
    TemporaryFileConversionFailure,
    MemberTypeExists,
    TypeMismatch,
    IdMismatch
}
