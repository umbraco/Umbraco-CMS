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

        ////TODO: Do we need this?
        //[DataMember(Name = "repositoryGuid")]
        //public Guid RepositoryGuid { get; set; }


        [DataMember(Name = "zipFileName")]
        public string ZipFileName { get; set; }

        /// <summary>
        /// During installation this can be used to track any pending appdomain restarts
        /// </summary>
        [DataMember(Name = "isRestarting")]
        public bool IsRestarting { get; set; }
    }
}
