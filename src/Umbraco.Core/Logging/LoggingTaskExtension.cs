namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Provides extension methods for <see cref="Task"/> to handle and log exceptions.
/// </summary>
internal static class LoggingTaskExtension
{
    /// <summary>
    ///     This task shouldn't be waited on (as it's not guaranteed to run), and you shouldn't wait on the parent task either
    ///     (because it might throw an
    ///     exception that doesn't get handled). If you want to be waiting on something, use LogErrorsWaitable instead.
    ///     None of these methods are suitable for tasks that return a value. If you're wanting a result, you should probably
    ///     be handling
    ///     errors yourself.
    /// </summary>
    /// <param name="task">The task to attach error logging to.</param>
    /// <param name="logMethod">The logging method to call with error message and exception.</param>
    /// <returns>A continuation task that logs errors if the original task faults.</returns>
    public static Task LogErrors(this Task task, Action<string, Exception> logMethod) =>
        task.ContinueWith(
            t => LogErrorsInner(t, logMethod),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,

            // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
            TaskScheduler.Default);

    /// <summary>
    ///     This task can be waited on (as it's guaranteed to run), and you should wait on this rather than the parent task.
    ///     Because it's
    ///     guaranteed to run, it may be slower than using LogErrors, and you should consider using that method if you don't
    ///     want to wait.
    ///     None of these methods are suitable for tasks that return a value. If you're wanting a result, you should probably
    ///     be handling
    ///     errors yourself.
    /// </summary>
    /// <param name="task">The task to attach error logging to.</param>
    /// <param name="logMethod">The logging method to call with error message and exception.</param>
    /// <returns>A continuation task that logs errors, guaranteed to run regardless of task completion state.</returns>
    public static Task LogErrorsWaitable(this Task task, Action<string, Exception> logMethod) =>
        task.ContinueWith(
            t => LogErrorsInner(t, logMethod),

            // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
            TaskScheduler.Default);

    /// <summary>
    ///     Logs any exceptions from the task using the provided log action.
    /// </summary>
    /// <param name="task">The task that may contain exceptions.</param>
    /// <param name="logAction">The action to invoke for logging errors.</param>
    private static void LogErrorsInner(Task task, Action<string, Exception> logAction)
    {
        if (task.Exception != null)
        {
            logAction(
                "Aggregate Exception with " + task.Exception.InnerExceptions.Count + " inner exceptions: ",
                task.Exception);
            foreach (Exception innerException in task.Exception.InnerExceptions)
            {
                logAction("Inner exception from aggregate exception: ", innerException);
            }
        }
    }
}
