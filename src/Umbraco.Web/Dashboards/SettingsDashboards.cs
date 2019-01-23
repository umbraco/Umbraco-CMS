using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    [DataContract]
    public class SettingsDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Welcome";

        [DataMember(Name = "alias")]
        public string Alias => "settingsWelcome";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "settings" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/settings/settingsdashboardintro.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
