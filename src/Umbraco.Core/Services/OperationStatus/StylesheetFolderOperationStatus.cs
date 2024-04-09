namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum StylesheetFolderOperationStatus
{
    Success,
    AlreadyExists,
    NotFound,
    NotEmpty,
    ParentNotFound,
    InvalidName,
}
