using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "packageInstallModel")]
    public class PackageInstallModel
    {
        [DataMember(Name="id")]
        public int Id { get; set; }

        [DataMember(Name = "packageGuid")]
        public Guid PackageGuid { get; set; }

        [DataMember(Name = "repositoryGuid")]
        public Guid RepositoryGuid { get; set; }

        [DataMember(Name = "temporaryDirectoryPath")]
        public string TemporaryDirectoryPath { get; set; }

        [DataMember(Name = "zipFilePath")]
        public string ZipFilePath { get; set; }
    }
}
