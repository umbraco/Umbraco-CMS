using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.IO;

namespace Umbraco.Core.Manifest
{
    public class ManifestDashboardDefinition
    {
        private string _view;

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

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
