namespace Umbraco.Cms.Core.Dashboards;

public class AnalyticsDashboard : IDashboard
{
    public string Alias => "settingsAnalytics";

    public string[] Sections => new[] { Constants.Applications.Settings };

    public string View => "views/dashboard/settings/analytics.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
