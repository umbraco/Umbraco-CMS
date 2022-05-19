using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract]
public class UserData
{
    public UserData(string name, string data)
    {
        Name = name;
        Data = data;
    }

    [DataMember(Name = "name")]
    public string Name { get; }

    [DataMember(Name = "data")]
    public string Data { get; }
}
