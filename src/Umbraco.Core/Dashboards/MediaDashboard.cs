using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Dashboards;

[Weight(10)]
public class MediaDashboard : IDashboard
{
    public string Alias => "mediaFolderBrowser";

    public string[] Sections => new[] { "media" };

    public string View => "views/dashboard/media/mediafolderbrowser.html";

    public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
}
