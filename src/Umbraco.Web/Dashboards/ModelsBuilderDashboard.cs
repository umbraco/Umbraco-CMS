using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(40)]
    [DataContract]
    public class ModelsBuilderDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Models Builder";

        [DataMember(Name = "alias")]
        public string Alias => "settingsModelsBuilder";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "settings" };

        [DataMember(Name = "view")]
        public string View => "/App_Plugins/ModelsBuilder/modelsbuilder.htm";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
