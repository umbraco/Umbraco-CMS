using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Packaging
{
    [DataContract(Name = "packageInstance")]
    public class PackageDefinition
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        //TODO: I don't see why this is necessary
        [DataMember(Name = "repositoryGuid")]
        public string RepositoryGuid { get; set; }

        [DataMember(Name = "packageGuid")]
        public string PackageGuid { get; set; }

        [DataMember(Name = "hasUpdate")]
        public bool HasUpdate { get; set; }

        [DataMember(Name = "name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [DataMember(Name = "url")]
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;

        [DataMember(Name = "folder")]
        public string Folder { get; set; } = string.Empty;

        [DataMember(Name = "packagePath")]
        public string PackagePath { get; set; } = string.Empty;

        [DataMember(Name = "version")]
        [Required]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// The minimum umbraco version that this package requires
        /// </summary>
        [DataMember(Name = "umbracoVersion")]
        public Version UmbracoVersion { get; set; }
        
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
        public bool ContentLoadChildNodes { get; set; } = false;

        [DataMember(Name = "contentNodeId")]
        public string ContentNodeId { get; set; } = string.Empty;

        [DataMember(Name = "macros")]
        public List<string> Macros { get; set; } = new List<string>();

        [DataMember(Name = "languages")]
        public List<string> Languages { get; set; } = new List<string>();

        [DataMember(Name = "dictionaryItems")]
        public List<string> DictionaryItems { get; set; } = new List<string>();

        [DataMember(Name = "templates")]
        public List<string> Templates { get; set; } = new List<string>();

        [DataMember(Name = "documentTypes")]
        public List<string> DocumentTypes { get; set; } = new List<string>();

        [DataMember(Name = "stylesheets")]
        public List<string> Stylesheets { get; set; } = new List<string>();

        [DataMember(Name = "files")]
        public List<string> Files { get; set; } = new List<string>();

        //TODO: Change this to angular view
        [DataMember(Name = "loadControl")]
        public string LoadControl { get; set; } = string.Empty;

        [DataMember(Name = "actions")]
        public string Actions { get; set; }

        [DataMember(Name = "dataTypes")]
        public List<string> DataTypes { get; set; } = new List<string>();

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; } = string.Empty;

    }
}
