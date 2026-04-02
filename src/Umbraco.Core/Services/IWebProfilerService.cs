using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for managing the web profiler (MiniProfiler) status.
/// </summary>
public interface IWebProfilerService
{
    /// <summary>
    /// Gets the current status of the web profiler.
    /// </summary>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> containing the profiler enabled status and operation status.</returns>
    Task<Attempt<bool, WebProfilerOperationStatus>> GetStatus();

    /// <summary>
    /// Sets the status of the web profiler.
    /// </summary>
    /// <param name="status">A value indicating whether the profiler should be enabled.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> containing the new profiler status and operation status.</returns>
    Task<Attempt<bool, WebProfilerOperationStatus>> SetStatus(bool status);
}
