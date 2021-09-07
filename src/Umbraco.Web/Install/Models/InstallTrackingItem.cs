using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Install.Models
{
    internal class InstallTrackingItem : IEquatable<InstallTrackingItem>
    {
        public InstallTrackingItem(string name, int serverOrder)
        {
            Name = name;
            ServerOrder = serverOrder;
            AdditionalData = new Dictionary<string, object>();
        }

        public string Name { get; set; }
        public int ServerOrder { get; set; }
        public bool IsComplete { get; set; }
        public IDictionary<string, object> AdditionalData { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstallTrackingItem);
        }

        public bool Equals(InstallTrackingItem other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
