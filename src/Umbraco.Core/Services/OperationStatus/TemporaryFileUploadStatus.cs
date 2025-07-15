namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum TemporaryFileOperationStatus
{
    Success = 0,
    FileExtensionNotAllowed = 1,
    KeyAlreadyUsed = 2,
    NotFound = 3,
    UploadBlocked = 4,
    InvalidFileName = 5,
}
