using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Packaging;
using File = System.IO.File;

namespace Umbraco.Cms.Core.Packaging
{
    /// <summary>
    /// Installs package files
    /// </summary>
    public class PackageFileInstallation
    {
        private readonly CompiledPackageXmlParser _parser;
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProfilingLogger _logger;
        private readonly PackageExtraction _packageExtraction;

        public PackageFileInstallation(CompiledPackageXmlParser parser, IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IProfilingLogger logger)
        {
            _parser = parser;
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;
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

        public IEnumerable<string> UninstallFiles(PackageDefinition package)
        {
            var removedFiles = new List<string>();

            foreach (var item in package.Files.ToArray())
            {
                removedFiles.Add(_ioHelper.GetRelativePath(item));

                //here we need to try to find the file in question as most packages does not support the tilde char
                var file = _ioHelper.FindFile(item);
                if (file != null)
                {
                    file = file.EnsureStartsWith("/");
                    var filePath = _hostingEnvironment.MapPathContentRoot(file);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                }
                package.Files.Remove(file);
            }

            return removedFiles;
        }

        private static IEnumerable<(string packageUniqueFile, string appAbsolutePath)> AppendRootToDestination(string applicationRootFolder, IEnumerable<(string packageUniqueFile, string appRelativePath)> sourceDestination)
        {
            return
                sourceDestination.Select(
                    sd => (sd.packageUniqueFile, Path.Combine(applicationRootFolder, sd.appRelativePath))).ToArray();
        }
    }
}
