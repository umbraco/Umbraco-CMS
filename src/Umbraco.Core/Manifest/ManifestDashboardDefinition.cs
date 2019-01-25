using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Umbraco.Core.Dashboards;
using Umbraco.Core.IO;

namespace Umbraco.Core.Manifest
{
    public class ManifestDashboardDefinition : IDashboardSection
    {
        private string _view;

        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; set; }

        [JsonProperty("weight", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(100)]
        public int Weight { get; set; }

        [JsonProperty("view", Required = Required.Always)]
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        [JsonProperty("sections")]
        public string[] Sections { get; set; } = Array.Empty<string>();

        [JsonProperty("access")]
        public IAccessRule[] AccessRules { get; set; } = Array.Empty<IAccessRule>();

    }
}
