using System;
using System.Collections.Generic;
using System.Reflection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects package versions telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class PackageVersionsTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IManifestParser _manifestParser;
        private readonly ITypeFinder _typeFinder;

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.PackageVersions
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersionsTelemetryDataCollector" /> class.
        /// </summary>
        public PackageVersionsTelemetryDataCollector(IManifestParser manifestParser, ITypeFinder typeFinder)
        {
            _manifestParser = manifestParser;
            _typeFinder = typeFinder;
        }

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => s_data;

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.PackageVersions => GetPackageVersions(),
            _ => throw new NotSupportedException()
        };

        private IEnumerable<PackageTelemetry> GetPackageVersions()
        {
            List<PackageTelemetry> packages = new();

            foreach (PackageManifest manifest in _manifestParser.GetManifests())
            {
                string name = manifest.PackageName;
                if (string.IsNullOrEmpty(name) || manifest.AllowPackageTelemetry is false)
                {
                    continue;
                }

                string version = manifest.Version;
                if (string.IsNullOrEmpty(version))
                {
                    version = GetAssemblyVersion(name);
                }

                packages.Add(new PackageTelemetry
                {
                    Name = name,
                    Version = version
                });
            }

            return packages;
        }

        private string GetAssemblyVersion(string name)
        {
            foreach (Assembly assembly in _typeFinder.AssembliesToScan)
            {
                AssemblyName assemblyName = assembly.GetName();
                if (string.Equals(assemblyName.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    AssemblyInformationalVersionAttribute attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                    if (attribute is not null &&
                        SemVersion.TryParse(attribute.InformationalVersion, out SemVersion semVersion))
                    {
                        return semVersion.ToSemanticStringWithoutBuild();
                    }
                    else
                    {
                        return assemblyName.Version.ToString(3);
                    }
                }
            }

            return null;
        }
    }
}
