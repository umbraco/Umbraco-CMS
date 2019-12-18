using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Install.Models
{
    // TODO: do we need this?
    [Obsolete("This is only used for the obsolete controller InstallPackageController")]
    [DataContract(Name = "installPackage", Namespace = "")]
    public class InstallPackageModel
    {
        [DataMember(Name = "kitGuid")]
        public Guid KitGuid { get; set; }
        [DataMember(Name = "packageId")]
        public int PackageId { get; set; }
        [DataMember(Name = "packageFile")]
        public string PackageFile { get; set; }

    }
}
