namespace Umbraco.Cms.Core.Models.TemporaryFile;

public enum TemporaryFileStatus
{
    Success = 0,
    FileExtensionNotAllowed = 1,
    KeyAlreadyUsed = 2,
    NotFound = 3,
}
