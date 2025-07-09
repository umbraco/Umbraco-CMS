namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a long-running operation.
/// </summary>
public class LongRunningOperation
{
    /// <summary>
    /// Gets the unique identifier for the long-running operation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the type of the long-running operation.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the status of the long-running operation.
    /// </summary>
    public required LongRunningOperationStatus Status { get; set; }
}
