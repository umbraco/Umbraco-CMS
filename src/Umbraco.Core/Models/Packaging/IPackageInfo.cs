using System;

namespace Umbraco.Core.Models.Packaging
{
    public interface IPackageInfo
    {
        string Name { get; }
        string Version { get; }
        string Url { get; }
        string License { get; }
        string LicenseUrl { get; }
        Version UmbracoVersion { get; }
        string Author { get; }
        string AuthorUrl { get; }
        string Readme { get; }
        string Control { get; } //fixme - this needs to be an angular view
        string IconUrl { get; }
    }
}
