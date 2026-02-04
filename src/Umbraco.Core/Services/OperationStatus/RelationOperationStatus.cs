namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a relation operation.
/// </summary>
public enum RelationOperationStatus
{
    /// <summary>
    /// The relation operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified relation type was not found.
    /// </summary>
    RelationTypeNotFound,
}
