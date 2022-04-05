using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    public class Metric
    {
        [DataMember(Name = "name")]
        public string Name { get; }

        [DataMember(Name = "data")]
        public string Data { get; }

        public Metric(string name, string data)
        {
            Name = name;
            Data = data;
        }
    }
}
