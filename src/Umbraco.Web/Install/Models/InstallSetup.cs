using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Install.Models
{
    /// <summary>
    /// Model containing all the install steps for setting up the UI
    /// </summary>
    [DataContract(Name = "installSetup", Namespace = "")]
    public class InstallSetup
    {
        public InstallSetup()
        {
            Steps = new List<InstallSetupStep>();
            InstallId = Guid.NewGuid();
        }

        [DataMember(Name = "installId")]
        public Guid InstallId { get; private set; }
        
        [DataMember(Name = "steps")]
        public IEnumerable<InstallSetupStep> Steps { get; set; } 

    }
}