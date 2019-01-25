using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class MediaDashboard : IDashboardSection
    {
        public string Name => "Content";

        public string Alias => "mediaFolderBrowser";

        public string[] Sections => new [] { "media" };

        public string View => "views/dashboard/media/mediafolderbrowser.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
