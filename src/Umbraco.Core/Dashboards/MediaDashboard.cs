using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(10)]
public class MediaDashboard : IDashboard
{
    public string Alias => "mediaFolderBrowser";

    public string[] Sections => new[] { Constants.Applications.Media };

    public string View => "views/dashboard/media/mediafolderbrowser.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
