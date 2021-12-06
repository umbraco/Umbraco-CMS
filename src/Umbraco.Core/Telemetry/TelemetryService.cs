using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry
{
    public class TelemetryService
    {
        private readonly IManifestParser _manifestParser;

        public TelemetryService(IManifestParser manifestParser)
        {
            _manifestParser = manifestParser;
        }

        public IEnumerable<PackageTelemetry> GetInstalledPackages()
        {
            List<PackageTelemetry> packages = new ();

            IEnumerable<PackageManifest> manifests = _manifestParser.GetManifests();
            foreach (PackageManifest manifest in manifests)
            {
                packages.Add(new PackageTelemetry { Name = manifest.PackageName, Version = manifest.Version });
            }

            return packages;
        }
    }
}
