using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    public class UsageInformation
    {
        [DataMember(Name = "name")]
        public string Name { get; }

        [DataMember(Name = "data")]
        public object Data { get; }

        public UsageInformation(string name, object data)
        {
            Name = name;
            Data = data;
        }
    }
}
