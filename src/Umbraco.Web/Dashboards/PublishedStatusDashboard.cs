using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(30)]
    public class PublishedStatusDashboard : IDashboardSection
    {
        public string Name => "Published Status";

        public string Alias => "settingsPublishedStatus";

        public string[] Sections => new [] { "settings" };

        public string View => "views/dashboard/settings/publishedstatus.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
