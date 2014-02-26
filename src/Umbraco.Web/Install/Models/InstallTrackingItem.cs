using System.Collections.Generic;

namespace Umbraco.Web.Install.Models
{
    internal class InstallTrackingItem
    {
        public InstallTrackingItem()
        {
            AdditionalData = new Dictionary<string, object>();
        }

        public bool IsComplete { get; set; }

        public IDictionary<string, object> AdditionalData { get; set; }
    }
}