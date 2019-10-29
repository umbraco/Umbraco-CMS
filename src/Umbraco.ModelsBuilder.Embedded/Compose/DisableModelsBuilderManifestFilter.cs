using System;
using System.Collections.Generic;
using Umbraco.Core.Manifest;

namespace Umbraco.ModelsBuilder.Embedded.Compose
{
    /// <summary>
    /// Removes the built in embedded models builder manifest from being loaded
    /// </summary>
    internal class DisableModelsBuilderManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.RemoveAll(x => x.Source.EndsWith("App_Plugins\\UmbModelsBuilder\\package.manifest", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
