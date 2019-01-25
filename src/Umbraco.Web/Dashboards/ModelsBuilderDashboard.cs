using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(40)]
    public class ModelsBuilderDashboard : IDashboardSection
    {
        public string Alias => "settingsModelsBuilder";

        public string[] Sections => new [] { "settings" };

        public string View => "/App_Plugins/ModelsBuilder/modelsbuilder.htm";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
