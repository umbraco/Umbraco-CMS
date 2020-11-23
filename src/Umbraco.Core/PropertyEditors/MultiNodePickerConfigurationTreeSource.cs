using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the 'startNode' value for the <see cref="Umbraco.Web.PropertyEditors.MultiNodePickerConfiguration"/>
    /// </summary>
    [DataContract]
    public class MultiNodePickerConfigurationTreeSource
    {
        [DataMember(Name = "type")]
        public string ObjectType { get; set; }

        [DataMember(Name = "query")]
        public string StartNodeQuery { get; set; }

        [DataMember(Name = "id")]
        public Udi StartNodeId { get; set; }
    }
}
