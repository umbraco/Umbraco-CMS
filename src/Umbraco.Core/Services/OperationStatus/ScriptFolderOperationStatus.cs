namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ScriptFolderOperationStatus
{
    Success,
    AlreadyExists,
    NotFound,
    NotEmpty,
    ParentNotFound,
    InvalidName,
}
