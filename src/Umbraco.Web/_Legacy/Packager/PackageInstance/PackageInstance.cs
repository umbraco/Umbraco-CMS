using System;
using System.Collections.Generic;

namespace Umbraco.Web._Legacy.Packager.PackageInstance
{
    public class PackageInstance
    {
        public int Id { get; set; }

        public string RepositoryGuid { get; set; }

        public string PackageGuid { get; set; }

        public bool HasUpdate { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Folder { get; set; }

        public string PackagePath { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// The minimum umbraco version that this package requires
        /// </summary>
        public Version UmbracoVersion { get; set; }

        public string Author { get; set; }

        public string AuthorUrl { get; set; }

        public string License { get; set; }

        public string LicenseUrl { get; set; }

        public string Readme { get; set; }

        public bool ContentLoadChildNodes { get; set; }

        public string ContentNodeId { get; set; }

        public List<string> Macros { get; set; }

        public List<string> Languages { get; set; }

        public List<string> DictionaryItems { get; set; }

        public List<string> Templates { get; set; }

        public List<string> Documenttypes { get; set; }

        public List<string> Stylesheets { get; set; }

        public List<string> Files { get; set; }

        public string LoadControl { get; set; }

        public string Actions { get; set; }

        public List<string> DataTypes { get; set; }

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
