namespace Umbraco.Cms.Core;

/// <summary>
///     Provides a mechanism to run tasks in a fire-and-forget manner without blocking the calling thread.
/// </summary>
public interface IFireAndForgetRunner
{
    /// <summary>
    ///     Runs the specified task in the background without waiting for it to complete.
    /// </summary>
    /// <param name="task">A function that returns the task to execute.</param>
    /// <remarks>
    ///     The task will be executed on a background thread. Exceptions will be logged but not propagated to the caller.
    /// </remarks>
    void RunFireAndForget(Func<Task> task);
}
