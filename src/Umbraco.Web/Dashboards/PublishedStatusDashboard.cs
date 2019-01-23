using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(30)]
    [DataContract]
    public class PublishedStatusDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Published Status";

        [DataMember(Name = "alias")]
        public string Alias => "settingsPublishedStatus";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "settings" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/settings/publishedstatus.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }


}
