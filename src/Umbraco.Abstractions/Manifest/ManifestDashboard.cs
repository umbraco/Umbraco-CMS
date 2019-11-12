using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Umbraco.Core.Dashboards;

namespace Umbraco.Core.Manifest
{
    [DataContract]
    public class ManifestDashboard : IDashboard
    {
        [DataMember(Name = "alias", IsRequired = true)]
        public string Alias { get; set; }

        //TODO this needs to be tested manually!
        //[JsonProperty("weight", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(100)] // must be equal to DashboardCollectionBuilder.DefaultWeight
        public int Weight { get; set; }

        [DataMember(Name = "view", IsRequired = true)]
        public string View { get; set; }

        [DataMember(Name = "sections")]
        public string[] Sections { get; set; } = Array.Empty<string>();

        [DataMember(Name = "access")]
        public IAccessRule[] AccessRules { get; set; } = Array.Empty<IAccessRule>();
    }
}
