namespace Umbraco.Cms.Core;

/// <summary>
///     Defines an object that represents a completable operation, typically used for unit of work patterns.
/// </summary>
/// <remarks>
///     The operation should be marked as complete by calling <see cref="Complete" /> before disposing.
///     If the object is disposed without calling <see cref="Complete" />, the operation should be rolled back.
/// </remarks>
public interface ICompletable : IDisposable
{
    /// <summary>
    ///     Marks the operation as complete, indicating that any changes should be committed.
    /// </summary>
    void Complete();
}
