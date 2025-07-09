namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a long-running operation.
/// </summary>
/// <typeparam name="T">The type of the result of the long-running operation.</typeparam>
public class LongRunningOperation<T> : LongRunningOperation
{
    /// <summary>
    /// Gets or sets the result of the long-running operation.
    /// </summary>
    public T? Result { get; set; }
}
