using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class TestConflictingRouteService : IConflictingRouteService
{
    public bool HasConflictingRoutes(out string controllername)
    {
        controllername = string.Empty;
        return false;
    }
}
