namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum PartialViewFolderOperationStatus
{
    Success,
    AlreadyExists,
    NotFound,
    NotEmpty,
    ParentNotFound,
    InvalidName,
}
