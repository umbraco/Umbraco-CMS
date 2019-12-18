using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    public class FormsDashboard : IDashboard
    {
        public string Alias => "formsInstall";

        public string[] Sections => new [] { Constants.Applications.Forms };

        public string View => "views/dashboard/forms/formsdashboardintro.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
