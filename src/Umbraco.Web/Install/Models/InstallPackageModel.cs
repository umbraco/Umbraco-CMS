using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Install.Models
{

    [Obsolete("This is only used for the obsolete controller InstallPackageController")]
    [DataContract(Name = "installPackage", Namespace = "")]
    public class InstallPackageModel
    {
        [DataMember(Name = "kitGuid")]
        public Guid KitGuid { get; set; }
        [DataMember(Name = "manifestId")]
        public int ManifestId { get; set; }
        [DataMember(Name = "packageFile")]
        public string PackageFile { get; set; }

    }
}
