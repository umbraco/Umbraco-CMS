namespace Umbraco.Cms.Core.Security.OperationStatus;

/// <summary>
/// Specifies the possible outcomes of operations involving back office user client credentials.
/// Typically used to indicate success or various failure states when managing these credentials.
/// </summary>
public enum BackOfficeUserClientCredentialsOperationStatus
{
    Success,
    DuplicateClientId,
    InvalidUser,
    InvalidClientId
}
