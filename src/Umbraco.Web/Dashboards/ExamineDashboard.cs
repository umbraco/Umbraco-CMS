using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(20)]
    [DataContract]
    public class ExamineDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Examine Management";

        [DataMember(Name = "alias")]
        public string Alias => "settingsExamine";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "settings" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/settings/examinemanagement.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
