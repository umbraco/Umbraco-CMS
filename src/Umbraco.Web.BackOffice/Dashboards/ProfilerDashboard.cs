using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(60)]
    public class ProfilerDashboardDashboard : IDashboard
    {
        public string Alias => "settingsProfiler";

        public string[] Sections => new [] { "settings" };

        public string View => "views/dashboard/settings/profiler.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
