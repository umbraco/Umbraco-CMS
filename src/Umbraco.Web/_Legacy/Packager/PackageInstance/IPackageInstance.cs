using System;
namespace umbraco.cms.businesslogic.packager{
    public interface IPackageInstance {
        string Actions { get; set; }
        string Author { get; set; }
        string AuthorUrl { get; set; }
        bool ContentLoadChildNodes { get; set; }
        string ContentNodeId { get; set; }
        System.Collections.Generic.List<string> Documenttypes { get; set; }
        System.Collections.Generic.List<string> Files { get; set; }
        string Folder { get; set; }
        bool HasUpdate { get; set; }
        int Id { get; set; }
        string License { get; set; }
        string LicenseUrl { get; set; }
        string LoadControl { get; set; }
        System.Collections.Generic.List<string> Macros { get; set; }
        string Name { get; set; }
        string PackageGuid { get; set; }
        string PackagePath { get; set; }
        string Readme { get; set; }
        string RepositoryGuid { get; set; }
        System.Collections.Generic.List<string> Stylesheets { get; set; }
        System.Collections.Generic.List<string> Templates { get; set; }
        string Url { get; set; }
        string Version { get; set; }
    }
}
