using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract]
    public class InstalledPackageModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "files")]
        public IEnumerable<string> Files { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "license")]
        public string License { get; set; }

        [DataMember(Name = "licenseUrl")]
        public string LicenseUrl { get; set; }
    }
}