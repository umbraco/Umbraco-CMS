using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal class PackageInstallation : IPackageInstallation
    {
        private readonly PackageExtraction _packageExtraction;
        private readonly PackageDataInstallation _packageDataInstallation;
        private readonly PackageFileInstallation _packageFileInstallation;
        private readonly CompiledPackageXmlParser _parser;
        private readonly IPackageActionRunner _packageActionRunner;
        private readonly DirectoryInfo _packageExtractionFolder;
        private readonly DirectoryInfo _applicationRootFolder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageDataInstallation"></param>
        /// <param name="packageFileInstallation"></param>
        /// <param name="parser"></param>
        /// <param name="packageActionRunner"></param>
        /// <param name="applicationRootFolder">
        /// The root folder of the application
        /// </param>
        /// <param name="packageExtractionFolder">
        /// The destination root folder to extract the package files (generally the same as applicationRoot) but can be modified for testing
        /// </param>
        public PackageInstallation(PackageDataInstallation packageDataInstallation, PackageFileInstallation packageFileInstallation, CompiledPackageXmlParser parser, IPackageActionRunner packageActionRunner,
            DirectoryInfo applicationRootFolder, DirectoryInfo packageExtractionFolder)
        {
            _packageExtraction = new PackageExtraction();
            _packageFileInstallation = packageFileInstallation ?? throw new ArgumentNullException(nameof(packageFileInstallation));
            _packageDataInstallation = packageDataInstallation ?? throw new ArgumentNullException(nameof(packageDataInstallation));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _packageActionRunner = packageActionRunner ?? throw new ArgumentNullException(nameof(packageActionRunner));
            _applicationRootFolder = applicationRootFolder ?? throw new ArgumentNullException(nameof(applicationRootFolder));
            _packageExtractionFolder = packageExtractionFolder ?? throw new ArgumentNullException(nameof(packageExtractionFolder));
        }

        public CompiledPackage ReadPackage(FileInfo packageFile)
        {
            if (packageFile == null) throw new ArgumentNullException(nameof(packageFile));
            var doc = GetConfigXmlDoc(packageFile);

            var compiledPackage = _parser.ToCompiledPackage(doc, packageFile, _applicationRootFolder.FullName);

            ValidatePackageFile(packageFile, compiledPackage);

            return compiledPackage;
        }

        public IEnumerable<string> InstallPackageFiles(PackageDefinition packageDefinition, CompiledPackage compiledPackage, int userId)
        {
            if (packageDefinition == null) throw new ArgumentNullException(nameof(packageDefinition));
            if (compiledPackage == null) throw new ArgumentNullException(nameof(compiledPackage));

            //these should be the same, TODO: we should have a better validator for this
            if (packageDefinition.Name != compiledPackage.Name)
                throw new InvalidOperationException("The package definition does not match the compiled package manifest");

            var packageZipFile = compiledPackage.PackageFile;

            var files = _packageFileInstallation.InstallFiles(compiledPackage, packageZipFile, _packageExtractionFolder.FullName).ToList();

            packageDefinition.Files = files;

            return files;
        }

        /// <inheritdoc />
        public UninstallationSummary UninstallPackage(PackageDefinition package, int userId)
        {
            //running this will update the PackageDefinition with the items being removed
            var summary = _packageDataInstallation.UninstallPackageData(package, userId);

            summary.Actions = CompiledPackageXmlParser.GetPackageActions(XElement.Parse(package.Actions), package.Name);

            //run actions before files are removed
            summary.ActionErrors = UndoPackageActions(package, summary.Actions).ToList();

            var filesRemoved = _packageFileInstallation.UninstallFiles(package);
            summary.FilesUninstalled = filesRemoved;

            return summary;
        }

        public InstallationSummary InstallPackageData(PackageDefinition packageDefinition, CompiledPackage compiledPackage, int userId)
        {
            var installationSummary = new InstallationSummary
            {
                DataTypesInstalled = _packageDataInstallation.ImportDataTypes(compiledPackage.DataTypes.ToList(), userId),
                LanguagesInstalled = _packageDataInstallation.ImportLanguages(compiledPackage.Languages, userId),
                DictionaryItemsInstalled = _packageDataInstallation.ImportDictionaryItems(compiledPackage.DictionaryItems, userId),
                MacrosInstalled = _packageDataInstallation.ImportMacros(compiledPackage.Macros, userId),
                TemplatesInstalled = _packageDataInstallation.ImportTemplates(compiledPackage.Templates.ToList(), userId),
                DocumentTypesInstalled = _packageDataInstallation.ImportDocumentTypes(compiledPackage.DocumentTypes, userId)
            };

            //we need a reference to the imported doc types to continue
            var importedDocTypes = installationSummary.DocumentTypesInstalled.ToDictionary(x => x.Alias, x => x);

            installationSummary.StylesheetsInstalled = _packageDataInstallation.ImportStylesheets(compiledPackage.Stylesheets, userId);
            installationSummary.ContentInstalled = _packageDataInstallation.ImportContent(compiledPackage.Documents, importedDocTypes, userId);
            installationSummary.Actions = CompiledPackageXmlParser.GetPackageActions(XElement.Parse(compiledPackage.Actions), compiledPackage.Name);
            installationSummary.MetaData = compiledPackage;
            installationSummary.FilesInstalled = packageDefinition.Files;
            
            //make sure the definition is up to date with everything
            foreach (var x in installationSummary.DataTypesInstalled) packageDefinition.DataTypes.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.LanguagesInstalled) packageDefinition.Languages.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.DictionaryItemsInstalled) packageDefinition.DictionaryItems.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.MacrosInstalled) packageDefinition.Macros.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.TemplatesInstalled) packageDefinition.Templates.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.DocumentTypesInstalled) packageDefinition.DocumentTypes.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.StylesheetsInstalled) packageDefinition.Stylesheets.Add(x.Id.ToInvariantString());
            var contentInstalled = installationSummary.ContentInstalled.ToList();
            packageDefinition.ContentNodeId = contentInstalled.Count > 0 ? contentInstalled[0].Id.ToInvariantString() : null;

            //run package actions
            installationSummary.ActionErrors = RunPackageActions(packageDefinition, installationSummary.Actions).ToList();

            return installationSummary;
        }

        private IEnumerable<string> RunPackageActions(PackageDefinition packageDefinition, IEnumerable<PackageAction> actions)
        {
            foreach (var n in actions)
            {
                //if there is an undo section then save it to the definition so we can run it at uninstallation 
                var undo = n.Undo;
                if (undo)
                    packageDefinition.Actions += n.XmlData.ToString();

                //Run the actions tagged only for 'install'
                if (n.RunAt != ActionRunAt.Install) continue;

                if (n.Alias.IsNullOrWhiteSpace()) continue;

                //run the actions and report errors
                if (!_packageActionRunner.RunPackageAction(packageDefinition.Name, n.Alias, n.XmlData, out var err))
                    foreach (var e in err) yield return e;
            }
        }

        private IEnumerable<string> UndoPackageActions(IPackageInfo packageDefinition, IEnumerable<PackageAction> actions)
        {
            foreach (var n in actions)
            {
                //Run the actions tagged only for 'uninstall'
                if (n.RunAt != ActionRunAt.Uninstall) continue;

                if (n.Alias.IsNullOrWhiteSpace()) continue;

                //run the actions and report errors
                if (!_packageActionRunner.UndoPackageAction(packageDefinition.Name, n.Alias, n.XmlData, out var err))
                    foreach (var e in err) yield return e;
            }
        }

        private XDocument GetConfigXmlDoc(FileInfo packageFile)
        {
            var configXmlContent = _packageExtraction.ReadTextFileFromArchive(packageFile, "package.xml", out _);

            var document = XDocument.Parse(configXmlContent);

            if (document.Root == null ||
                document.Root.Name.LocalName.Equals("umbPackage") == false)
                throw new FormatException("xml does not have a root node called \"umbPackage\"");

            return document;
        }

        private void ValidatePackageFile(FileInfo packageFile, CompiledPackage package)
        {
            if (!(package.Files?.Count > 0)) return;

            var sourceDestination = _parser.ExtractSourceDestinationFileInformation(package.Files).ToArray();

            var missingFiles = _packageExtraction.FindMissingFiles(packageFile, sourceDestination.Select(i => i.packageUniqueFile)).ToArray();

            if (missingFiles.Any())
            {
                throw new Exception("The following file(s) are missing in the package: " +
                                    string.Join(", ", missingFiles.Select(
                                        mf =>
                                        {
                                            var (packageUniqueFile, appRelativePath) = sourceDestination.Single(fi => fi.packageUniqueFile == mf);
                                            return $"source: \"{packageUniqueFile}\" destination: \"{appRelativePath}\"";
                                        })));
            }

            IEnumerable<string> duplicates = _packageExtraction.FindDuplicateFileNames(packageFile).ToArray();

            if (duplicates.Any())
            {
                throw new Exception("The following filename(s) are found more than one time in the package, since the filename is used ad primary key, this is not allowed: " +
                                    string.Join(", ", duplicates));
            }
        }

        
        
    }
}
