using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class SettingsDashboard : IDashboardSection
    {
        public string Name => "Welcome";

        public string Alias => "settingsWelcome";

        public string[] Sections => new [] { "settings" };

        public string View => "views/dashboard/settings/settingsdashboardintro.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
