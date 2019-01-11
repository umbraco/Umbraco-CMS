using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [DataContract(Name = "packageInstance")]
    public class PackageDefinition : IPackageInfo
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "packageGuid")]
        public Guid PackageId { get; set; }

        [DataMember(Name = "name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [DataMember(Name = "url")]
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// This is a generated GUID which is used to determine a temporary folder name for processing the package
        /// </summary>
        [DataMember(Name = "folder")]
        public Guid FolderId { get; set; }

        [ReadOnly(true)]
        [DataMember(Name = "packagePath")]
        public string PackagePath { get; set; } = string.Empty;

        [DataMember(Name = "version")]
        [Required]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// The minimum umbraco version that this package requires
        /// </summary>
        [DataMember(Name = "umbracoVersion")]
        public Version UmbracoVersion { get; set; } = Configuration.UmbracoVersion.Current;
        
        [DataMember(Name = "author")]
        [Required]
        public string Author { get; set; } = string.Empty;

        [DataMember(Name = "authorUrl")]
        [Required]
        [Url]
        public string AuthorUrl { get; set; } = string.Empty;

        [DataMember(Name = "license")]
        public string License { get; set; } = "MIT License";

        [DataMember(Name = "licenseUrl")]
        public string LicenseUrl { get; set; } = "http://opensource.org/licenses/MIT";

        [DataMember(Name = "readme")]
        public string Readme { get; set; } = string.Empty;

        [DataMember(Name = "contentLoadChildNodes")]
        public bool ContentLoadChildNodes { get; set; }

        [DataMember(Name = "contentNodeId")]
        public string ContentNodeId { get; set; } = string.Empty;

        [DataMember(Name = "macros")]
        public IList<string> Macros { get; set; } = new List<string>();

        [DataMember(Name = "languages")]
        public IList<string> Languages { get; set; } = new List<string>();

        [DataMember(Name = "dictionaryItems")]
        public IList<string> DictionaryItems { get; set; } = new List<string>();

        [DataMember(Name = "templates")]
        public IList<string> Templates { get; set; } = new List<string>();

        [DataMember(Name = "documentTypes")]
        public IList<string> DocumentTypes { get; set; } = new List<string>();

        [DataMember(Name = "stylesheets")]
        public IList<string> Stylesheets { get; set; } = new List<string>();

        [DataMember(Name = "files")]
        public IList<string> Files { get; set; } = new List<string>();

        //fixme: Change this to angular view
        [DataMember(Name = "loadControl")]
        public string Control { get; set; } = string.Empty;

        [DataMember(Name = "actions")]
        public string Actions { get; set; } = "<actions></actions>";

        [DataMember(Name = "dataTypes")]
        public IList<string> DataTypes { get; set; } = new List<string>();

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; } = string.Empty;

    }
}
