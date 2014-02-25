using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        UpgradeWithoutToken
    }

    [DataContract(Name = "installSetup", Namespace = "")]
    public class InstallSetup
    {
        [DataMember(Name = "status")]
        public InstallStatus Status { get; set; }

    }
}
