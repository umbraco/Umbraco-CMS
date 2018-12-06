using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace umbraco.cms.businesslogic.packager
{
    [DataContract(Name = "packageInstance")]
    public class PackageInstance
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "repositoryGuid")]
        public string RepositoryGuid { get; set; }

        [DataMember(Name = "packageGuid")]
        public string PackageGuid { get; set; }

        [DataMember(Name = "hasUpdate")]
        public bool HasUpdate { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "folder")]
        public string Folder { get; set; }

        [DataMember(Name = "packagePath")]
        public string PackagePath { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// The minimum umbraco version that this package requires
        /// </summary>
        [DataMember(Name = "umbracoVersion")]
        public Version UmbracoVersion { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "authorUrl")]
        public string AuthorUrl { get; set; }

        [DataMember(Name = "license")]
        public string License { get; set; }

        [DataMember(Name = "licenseUrl")]
        public string LicenseUrl { get; set; }

        [DataMember(Name = "readme")]
        public string Readme { get; set; }

        [DataMember(Name = "contentLoadChildNodes")]
        public bool ContentLoadChildNodes { get; set; }

        [DataMember(Name = "contentNodeId")]
        public string ContentNodeId { get; set; }

        [DataMember(Name = "macros")]
        public List<string> Macros { get; set; }

        [DataMember(Name = "languages")]
        public List<string> Languages { get; set; }

        [DataMember(Name = "dictionaryItems")]
        public List<string> DictionaryItems { get; set; }

        [DataMember(Name = "templates")]
        public List<string> Templates { get; set; }

        [DataMember(Name = "documenttypes")]
        public List<string> Documenttypes { get; set; }

        [DataMember(Name = "stylesheets")]
        public List<string> Stylesheets { get; set; }

        [DataMember(Name = "files")]
        public List<string> Files { get; set; }

        [DataMember(Name = "loadControl")]
        public string LoadControl { get; set; }

        [DataMember(Name = "actions")]
        public string Actions { get; set; }

        [DataMember(Name = "dataTypes")]
        public List<string> DataTypes { get; set; }

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; }

        public PackageInstance()
        {
            Name = string.Empty;
            Url = string.Empty;
            Folder = string.Empty;
            PackagePath = string.Empty;
            Version = string.Empty;
            UmbracoVersion = null;
            Author = string.Empty;
            AuthorUrl = string.Empty;
            License = string.Empty;
            LicenseUrl = string.Empty;
            Readme = string.Empty;
            ContentNodeId = string.Empty;
            IconUrl = string.Empty;
            Macros = new List<string>();
            Languages = new List<string>();
            DictionaryItems = new List<string>();
            Templates = new List<string>();
            Documenttypes = new List<string>();
            Stylesheets = new List<string>();
            Files = new List<string>();
            LoadControl = string.Empty;
            DataTypes = new List<string>();
            ContentLoadChildNodes = false;
        }
    }
}
