using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "publicAccess", Namespace = "")]
public class PublicAccess
{
    [DataMember(Name = "groups")]
    public MemberGroupDisplay[]? Groups { get; set; }

    [DataMember(Name = "loginPage")]
    public EntityBasic? LoginPage { get; set; }

    [DataMember(Name = "errorPage")]
    public EntityBasic? ErrorPage { get; set; }

    [DataMember(Name = "members")]
    public MemberDisplay[]? Members { get; set; }
}
