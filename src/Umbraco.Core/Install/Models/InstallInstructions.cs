using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models;

[Obsolete("Will no longer be required with the new backoffice API")]
[DataContract(Name = "installInstructions", Namespace = "")]
public class InstallInstructions
{
    [DataMember(Name = "instructions")]
    public IDictionary<string, object>? Instructions { get; set; }

    [DataMember(Name = "installId")]
    public Guid InstallId { get; set; }
}
