using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Models.Packaging
{
    /// <summary>
    /// The model of the package definition within an umbraco (zip) package file
    /// </summary>
    public class CompiledPackage : IPackageInfo
    {
        public string PackageFileName { get; set; }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string License { get; set; }
        public string LicenseUrl { get; set; }
        public Version UmbracoVersion { get; set; }
        public RequirementsType UmbracoVersionRequirementsType { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public string Readme { get; set; }
        public string Control { get; set; }
        public string IconUrl { get; set; }

        public string Actions { get; set; } //fixme: Should we make this strongly typed to IEnumerable<PackageAction> ?

        public PreInstallWarnings Warnings { get; set; } = new PreInstallWarnings();

        public List<CompiledPackageFile> Files { get; set; } = new List<CompiledPackageFile>();

        public IEnumerable<XElement> Macros { get; set; } //fixme: make strongly typed
        public IEnumerable<XElement> Templates { get; set; } //fixme: make strongly typed
        public IEnumerable<XElement> Stylesheets { get; set; } //fixme: make strongly typed

    }
    
    public class CompiledPackageFile
    {
        public string OriginalPath { get; set; }
        public string UniqueFileName { get; set; }
        public string OriginalName { get; set; }

    }


}
