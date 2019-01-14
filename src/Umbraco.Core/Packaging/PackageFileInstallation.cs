using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Installs package files
    /// </summary>
    internal class PackageFileInstallation
    {
        private readonly CompiledPackageXmlParser _parser;
        private readonly IProfilingLogger _logger;
        private readonly PackageExtraction _packageExtraction;

        public PackageFileInstallation(CompiledPackageXmlParser parser, IProfilingLogger logger)
        {
            _parser = parser;
            _logger = logger;
            _packageExtraction = new PackageExtraction();
        }
        
        /// <summary>
        /// Returns a list of all installed file paths
        /// </summary>
        /// <param name="compiledPackage"></param>
        /// <param name="packageFile"></param>
        /// <param name="targetRootFolder">
        /// The absolute path of where to extract the package files (normally the application root)
        /// </param>
        /// <returns></returns>
        public IEnumerable<string> InstallFiles(CompiledPackage compiledPackage, FileInfo packageFile, string targetRootFolder)
        {
            using (_logger.DebugDuration<PackageFileInstallation>(
                "Installing package files for package " + compiledPackage.Name,
                "Package file installation complete for package " + compiledPackage.Name))
            {
                var sourceAndRelativeDest = _parser.ExtractSourceDestinationFileInformation(compiledPackage.Files);
                var sourceAndAbsDest = AppendRootToDestination(targetRootFolder, sourceAndRelativeDest);

                _packageExtraction.CopyFilesFromArchive(packageFile, sourceAndAbsDest);

                return sourceAndRelativeDest.Select(sd => sd.appRelativePath).ToArray();
            }
        }

        private static IEnumerable<(string packageUniqueFile, string appAbsolutePath)> AppendRootToDestination(string applicationRootFolder, IEnumerable<(string packageUniqueFile, string appRelativePath)> sourceDestination)
        {
            return
                sourceDestination.Select(
                    sd => (sd.packageUniqueFile, Path.Combine(applicationRootFolder, sd.appRelativePath))).ToArray();
        }
    }
}
