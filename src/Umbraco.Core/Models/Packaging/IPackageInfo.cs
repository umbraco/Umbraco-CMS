using System;
using System.Collections.Generic;

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
        IList<string> Contributors { get; }
        string Readme { get; }

        /// <summary>
        /// This is the angular view path that will be loaded when the package installs
        /// </summary>
        string PackageView { get; }

        string IconUrl { get; }
    }
}
