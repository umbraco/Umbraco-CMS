using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(50)]
    [DataContract]
    public class HealthCheckDashboard : IDashboardSection
    {
        [DataMember(Name="name")]
        public string Name => "Health Check";

        [DataMember(Name = "alias")]
        public string Alias => "settingsHealthCheck";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "settings" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/settings/healthcheck.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
