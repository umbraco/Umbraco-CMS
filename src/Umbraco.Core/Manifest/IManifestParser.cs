using System.Collections.Generic;
using Umbraco.Core.Configuration.Grid;
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
        IPackageManifest Manifest { get; }

        /// <summary>
        /// Parses a manifest.
        /// </summary>
        IPackageManifest ParseManifest(string text);

        IEnumerable<IGridEditorConfig> ParseGridEditors(string text);
    }
}
