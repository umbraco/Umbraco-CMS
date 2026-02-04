namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for detecting conflicting routes in the application.
/// </summary>
public interface IConflictingRouteService
{
    /// <summary>
    ///     Checks if there are any conflicting routes registered in the application.
    /// </summary>
    /// <param name="controllerName">When this method returns, contains the name of the conflicting controller if a conflict exists.</param>
    /// <returns><c>true</c> if there are conflicting routes; otherwise, <c>false</c>.</returns>
    public bool HasConflictingRoutes(out string controllerName);
}
