using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;

namespace Umbraco.Cms.ModelsBuilder.Embedded
{
    [Weight(40)]
    public class ModelsBuilderDashboard : IDashboard
    {
        public string Alias => "settingsModelsBuilder";

        public string[] Sections => new [] { "settings" };

        public string View => "views/dashboard/settings/modelsbuildermanagement.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
