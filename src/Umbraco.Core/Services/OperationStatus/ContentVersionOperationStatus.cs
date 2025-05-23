namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentVersionOperationStatus
{
    Success,
    NotFound,
    ContentNotFound,
    InvalidSkipTake,
    RollBackFailed,
    RollBackCanceled
}
