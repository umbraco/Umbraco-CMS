using System;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Umbraco.Web.Dashboards
{
    [Weight(10)]
    [DataContract]
    public class MediaDashboard : IDashboardSection
    {
        [DataMember(Name = "name")]
        public string Name => "Content";

        [DataMember(Name = "alias")]
        public string Alias => "mediaFolderBrowser";

        [IgnoreDataMember]
        public string[] Sections => new string[] { "media" };

        [DataMember(Name = "view")]
        public string View => "views/dashboard/media/mediafolderbrowser.html";

        [IgnoreDataMember]
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
