namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a segment operation.
/// </summary>
public enum KeyValueOperationStatus
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The Value could not be set.
    /// </summary>
    NoValueSet,
}
