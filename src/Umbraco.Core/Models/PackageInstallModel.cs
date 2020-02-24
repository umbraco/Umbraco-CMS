using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "packageInstallModel")]
    public class PackageInstallModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "packageGuid")]
        public Guid PackageGuid { get; set; }

        [DataMember(Name = "zipFileName")]
        public string ZipFileName { get; set; }

        /// <summary>
        /// During installation this can be used to track any pending AppDomain restarts
        /// </summary>
        [DataMember(Name = "isRestarting")]
        public bool IsRestarting { get; set; }

    }
}
