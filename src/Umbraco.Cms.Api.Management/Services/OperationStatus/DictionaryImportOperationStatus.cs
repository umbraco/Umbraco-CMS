namespace Umbraco.Cms.Api.Management.Services.OperationStatus;

/// <summary>
/// Represents the status of a dictionary import operation.
/// </summary>
public enum DictionaryImportOperationStatus
{
    Success,
    ParentNotFound,
    InvalidFileContent,
    InvalidFileType,
    TemporaryFileNotFound,
}
