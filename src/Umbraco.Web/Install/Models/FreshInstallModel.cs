using System;
using System.Runtime.Serialization;

namespace Umbraco.Web.Install.Models
{
    [DataContract(Name = "freshInstall", Namespace = "")]
    public class FreshInstallModel
    {
        [DataMember(Name = "user")]
        public UserModel User { get; set; }

        [DataMember(Name = "database")]
        public DatabaseModel Database { get; set; }

        [DataMember(Name = "configureMachineKey")]
        public bool ConfigureMachineKey { get; set; }

        [DataMember(Name = "starterKit")]
        public Guid StarterKit { get; set; }
    }
}
