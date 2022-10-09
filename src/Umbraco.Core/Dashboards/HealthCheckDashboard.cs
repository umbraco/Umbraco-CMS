using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(50)]
public class HealthCheckDashboard : IDashboard
{
    public string Alias => "settingsHealthCheck";

    public string[] Sections => new[] { "settings" };

    public string View => "views/dashboard/settings/healthcheck.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
