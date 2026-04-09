namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A default implementation of <see cref="IConflictingRouteService"/> that never reports conflicts.
/// </summary>
/// <remarks>
/// This is used when the backoffice is not registered, as route conflict detection
/// is primarily relevant for backoffice controller routes.
/// </remarks>
public sealed class NoopConflictingRouteService : IConflictingRouteService
{
    /// <inheritdoc />
    public bool HasConflictingRoutes(out string controllerName)
    {
        controllerName = string.Empty;
        return false;
    }
}
