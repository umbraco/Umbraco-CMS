namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

/// <summary>
///     Defines an interface which must be implemented by custom components responsible for detecting specific transient
///     conditions.
/// </summary>
public interface ITransientErrorDetectionStrategy
{
    /// <summary>
    ///     Determines whether the specified exception represents a transient failure that can be compensated by a retry.
    /// </summary>
    /// <param name="ex">The exception object to be verified.</param>
    /// <returns>True if the specified exception is considered as transient, otherwise false.</returns>
    bool IsTransient(Exception ex);
}
