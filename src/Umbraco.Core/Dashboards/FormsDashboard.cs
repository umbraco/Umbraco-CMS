using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(10)]
public class FormsDashboard : IDashboard
{
    public string Alias => "formsInstall";

    public string[] Sections => new[] { Constants.Applications.Forms };

    public string View => "views/dashboard/forms/formsdashboardintro.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
