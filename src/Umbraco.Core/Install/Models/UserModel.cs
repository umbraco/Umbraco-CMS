using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models;

[DataContract(Name = "user", Namespace = "")]
public class UserModel
{
    [DataMember(Name = "name")] public string Name { get; set; } = null!;

    [DataMember(Name = "email")] public string Email { get; set; } = null!;

    [DataMember(Name = "password")] public string Password { get; set; } = null!;

    [DataMember(Name = "subscribeToNewsLetter")]
    public bool SubscribeToNewsLetter { get; set; }
}
