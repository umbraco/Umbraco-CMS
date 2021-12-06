using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Core.Telemetry
{
    public class TelemetryService
    {
        private readonly IManifestParser _manifestParser;

        public TelemetryService(IManifestParser manifestParser)
        {
            _manifestParser = manifestParser;
        }

        public IEnumerable<string> GetInstalledPackages()
        {
            List<string> packages = new ();

            var manifests = _manifestParser.GetManifests();
            foreach (var manifest in manifests)
            {
                packages.Add(manifest.PackageName);
            }

            return packages;
        }
    }
}
