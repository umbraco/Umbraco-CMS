namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for managing web profiler status per user.
/// </summary>
public interface IWebProfilerRepository
{
    /// <summary>
    ///     Sets the web profiler status for a user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="status">The status to set (<c>true</c> to enable, <c>false</c> to disable).</param>
    void SetStatus(int userId, bool status);

    /// <summary>
    ///     Gets the web profiler status for a user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns><c>true</c> if the web profiler is enabled for the user; otherwise, <c>false</c>.</returns>
    bool GetStatus(int userId);
}
