namespace Umbraco.Cms.Core.Services;

public interface IConflictingRouteService
{
    public bool HasConflictingRoutes(out string controllerName);
}
