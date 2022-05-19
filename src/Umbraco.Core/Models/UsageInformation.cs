using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract]
public class UsageInformation
{
    public UsageInformation(string name, object data)
    {
        Name = name;
        Data = data;
    }

    [DataMember(Name = "name")]
    public string Name { get; }

    [DataMember(Name = "data")]
    public object Data { get; }
}
