using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    [DataContract]
    public class FormsDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Install Umbraco Forms";

        [DataMember(Name = "alias")]
        public string Alias => "formsInstall";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "forms" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/forms/formsdashboardintro.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
