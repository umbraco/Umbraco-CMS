using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Manifest
{
    public interface IManifestParser
    {
        string Path { get; set; }

        /// <summary>
        /// Gets all manifests, merged into a single manifest object.
        /// </summary>
        /// <returns></returns>
        PackageManifest Manifest { get; }

        /// <summary>
        /// Parses a manifest.
        /// </summary>
        PackageManifest ParseManifest(string text);

        IEnumerable<GridEditor> ParseGridEditors(string text);
    }
}
