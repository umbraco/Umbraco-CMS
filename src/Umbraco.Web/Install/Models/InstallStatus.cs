using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.ComIntegration;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Install.Models
{
    public enum InstallStatus
    {
        NewInstall,
        Upgrade,

        /// <summary>
        /// Used when we detect an upgrade but there is no user token associated
        /// </summary>
        UpgradeWithoutToken,

        /// <summary>
        /// Used if the user presses f5 and is in the middle of an install
        /// </summary>
        InProgress
    }

    [DataContract(Name = "installSetup", Namespace = "")]
    public class InstallSetup
    {
        public InstallSetup()
        {
            Steps = new List<InstallStep>();
        }

        [DataMember(Name = "status")]
        public InstallStatus Status { get; set; }

        [DataMember(Name = "steps")]
        public IEnumerable<InstallStep> Steps { get; set; } 

    }

    [DataContract(Name = "step", Namespace = "")]
    public class InstallStep
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "completed")]
        public bool IsComplete { get; set; }

        [DataMember(Name = "view")]
        public string View { get; set; }
    }

    [DataContract(Name = "instructions", Namespace = "")]
    public class InstallInstructions
    {
        [DataMember(Name = "dbType")]
        public DatabaseType DatabaseType { get; set; }

        [DataMember(Name = "starterKit")]
        public Guid StarterKit { get; set; }

        [DataMember(Name = "user")]
        public UserModel User { get; set; }
    }

    public class UserModel
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        [DataMember(Name = "email")]
        public string Email { get; set; }
        
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }

    public enum DatabaseType
    {
        SqlCe,
        SqlServer,
        MySql,
        SqlAzure,
        Custom
    }
}
