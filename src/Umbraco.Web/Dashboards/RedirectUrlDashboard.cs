using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(20)]
    [DataContract]
    public class RedirectUrlDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Redirect URL Management";

        [DataMember(Name = "alias")]
        public string Alias => "contentRedirectManager";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "content" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/content/redirecturls.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
