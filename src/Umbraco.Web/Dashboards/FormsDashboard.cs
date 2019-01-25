using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class FormsDashboard : IDashboardSection
    {
        public string Name => "Install Umbraco Forms";

        public string Alias => "formsInstall";

        public string[] Sections => new [] { "forms" };

        public string View => "views/dashboard/forms/formsdashboardintro.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
