namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a long-running operation.
/// </summary>
/// <typeparam name="TResult">The type of the result of the long-running operation.</typeparam>
public class LongRunningOperation<TResult> : LongRunningOperation
{
    /// <summary>
    /// Gets or sets the result of the long-running operation.
    /// </summary>
    public TResult? Result { get; set; }
}
