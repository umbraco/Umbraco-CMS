namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Defines the profiling service.
/// </summary>
public interface IProfiler
{
    /// <summary>
    ///     Gets an <see cref="IDisposable" /> that will time the code between its creation and disposal.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <returns>A step.</returns>
    /// <remarks>The returned <see cref="IDisposable" /> is meant to be used within a <c>using (...) {{ ... }}</c> block.</remarks>
    IDisposable? Step(string name);

    /// <summary>
    ///     Starts the profiler.
    /// </summary>
    void Start();

    /// <summary>
    ///     Stops the profiler.
    /// </summary>
    /// <param name="discardResults">A value indicating whether to discard results.</param>
    /// <remarks>
    ///     Set discardResult to true to abandon all profiling - useful when eg someone is not
    ///     authenticated or you want to clear the results, based upon some other mechanism.
    /// </remarks>
    void Stop(bool discardResults = false);
}
