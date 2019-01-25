using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(20)]
    public class ExamineDashboard : IDashboardSection
    {
        public string Alias => "settingsExamine";

        public string[] Sections => new [] { "settings" };

        public string View => "views/dashboard/settings/examinemanagement.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
