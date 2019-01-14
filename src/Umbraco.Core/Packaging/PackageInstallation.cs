using System;
using System.Collections.Generic;
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
        private readonly IPackagingService _packagingService;
        private readonly PackageExtraction _packageExtraction;
        private readonly PackageFileInstallation _packageFileInstallation;
        private readonly CompiledPackageXmlParser _parser;
        private readonly string _packagesFolderPath;
        private readonly DirectoryInfo _packageExtractionFolder;
        private readonly DirectoryInfo _applicationRootFolder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packagingService"></param>
        /// <param name="packageFileInstallation"></param>
        /// <param name="parser"></param>
        /// <param name="packagesFolderPath">
        /// The relative path of the package storage folder (i.e. ~/App_Data/Packages )
        /// </param>
        /// <param name="applicationRootFolder">
        /// The root folder of the application
        /// </param>
        /// <param name="packageExtractionFolder">
        /// The destination root folder to extract the package files (generally the same as applicationRoot) but can be modified for testing
        /// </param>
        public PackageInstallation(IPackagingService packagingService, PackageFileInstallation packageFileInstallation, CompiledPackageXmlParser parser,
            string packagesFolderPath, DirectoryInfo applicationRootFolder, DirectoryInfo packageExtractionFolder)
        {
            _packageExtraction = new PackageExtraction();
            _packageFileInstallation = packageFileInstallation;
            _packagingService = packagingService ?? throw new ArgumentNullException(nameof(packagingService));
            _parser = parser;
            _packagesFolderPath = packagesFolderPath;
            _applicationRootFolder = applicationRootFolder;
            _packageExtractionFolder = packageExtractionFolder;
        }

        public CompiledPackage ReadPackage(string packageFileName)
        {
            if (packageFileName == null) throw new ArgumentNullException(nameof(packageFileName));
            var packageZipFile = GetPackageZipFile(packageFileName);
            var doc = GetConfigXmlDoc(packageZipFile);

            var compiledPackage = _parser.ToCompiledPackage(doc, Path.GetFileName(packageZipFile.FullName), _applicationRootFolder.FullName);

            ValidatePackageFile(packageZipFile, compiledPackage);

            return compiledPackage;
        }

        //fixme: Should we move all of the ImportXXXX methods here instead of on the IPackagingService? we don't want to have cicurlar refs

        public IEnumerable<string> InstallPackageFiles(PackageDefinition packageDefinition, CompiledPackage compiledPackage, int userId)
        {
            if (packageDefinition == null) throw new ArgumentNullException(nameof(packageDefinition));
            if (compiledPackage == null) throw new ArgumentNullException(nameof(compiledPackage));

            //these should be the same, TODO: we should have a better validator for this
            if (packageDefinition.Name != compiledPackage.Name)
                throw new InvalidOperationException("The package definition does not match the compiled package manifest");

            var packageZipFile = GetPackageZipFile(compiledPackage.PackageFileName);

            return _packageFileInstallation.InstallFiles(compiledPackage, packageZipFile, _packageExtractionFolder.FullName);
        }

        public InstallationSummary InstallPackageData(PackageDefinition packageDefinition, CompiledPackage compiledPackage, int userId)
        {
            //fixme: fill this in
            throw new NotImplementedException();
        }

        //public InstallationSummary InstallPackage(FileInfo packageFile, int userId)
        //{
        //    XElement dataTypes;
        //    XElement languages;
        //    XElement dictionaryItems;
        //    XElement macroes;
        //    XElement files;
        //    XElement templates;
        //    XElement documentTypes;
        //    XElement styleSheets;
        //    XElement documentSet;
        //    XElement documents;
        //    XElement actions;
        //    IPackageInfo metaData;
        //    InstallationSummary installationSummary;

        //    try
        //    {
        //        XElement rootElement = GetConfigXmlElement(packageFile);
        //        PackageSupportedCheck(rootElement);
        //        PackageStructureSanityCheck(packageFile, rootElement);
        //        dataTypes = rootElement.Element(Constants.Packaging.DataTypesNodeName);
        //        languages = rootElement.Element(Constants.Packaging.LanguagesNodeName);
        //        dictionaryItems = rootElement.Element(Constants.Packaging.DictionaryItemsNodeName);
        //        macroes = rootElement.Element(Constants.Packaging.MacrosNodeName);
        //        files = rootElement.Element(Constants.Packaging.FilesNodeName);
        //        templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
        //        documentTypes = rootElement.Element(Constants.Packaging.DocumentTypesNodeName);
        //        styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
        //        documentSet = rootElement.Element(Constants.Packaging.DocumentSetNodeName);
        //        documents = rootElement.Element(Constants.Packaging.DocumentsNodeName);
        //        actions = rootElement.Element(Constants.Packaging.ActionsNodeName);

        //        metaData = GetMetaData(rootElement);
        //        installationSummary = new InstallationSummary {MetaData = metaData};
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error reading " + packageFile, e);
        //    }

        //    try
        //    {
        //        var dataTypeDefinitions = EmptyEnumerableIfNull<IDataType>(dataTypes) ?? InstallDataTypes(dataTypes, userId);
        //        installationSummary.DataTypesInstalled = dataTypeDefinitions;

        //        var languagesInstalled = EmptyEnumerableIfNull<ILanguage>(languages) ?? InstallLanguages(languages, userId);
        //        installationSummary.LanguagesInstalled = languagesInstalled;

        //        var dictionaryInstalled = EmptyEnumerableIfNull<IDictionaryItem>(dictionaryItems) ?? InstallDictionaryItems(dictionaryItems);
        //        installationSummary.DictionaryItemsInstalled = dictionaryInstalled;

        //        var macros = EmptyEnumerableIfNull<IMacro>(macroes) ?? InstallMacros(macroes, userId);
        //        installationSummary.MacrosInstalled = macros;

        //        var templatesInstalled = EmptyEnumerableIfNull<ITemplate>(templates) ?? InstallTemplats(templates, userId);
        //        installationSummary.TemplatesInstalled = templatesInstalled;

        //        var documentTypesInstalled = EmptyEnumerableIfNull<IContentType>(documentTypes) ?? InstallDocumentTypes(documentTypes, userId);
        //        installationSummary.ContentTypesInstalled =documentTypesInstalled;

        //        var stylesheetsInstalled = EmptyEnumerableIfNull<IFile>(styleSheets) ?? InstallStylesheets(styleSheets);
        //        installationSummary.StylesheetsInstalled = stylesheetsInstalled;

        //        var documentsInstalled = documents != null ? InstallDocuments(documents, userId)
        //            : EmptyEnumerableIfNull<IContent>(documentSet)
        //            ?? InstallDocuments(documentSet, userId);
        //        installationSummary.ContentInstalled = documentsInstalled;

        //        var packageActions = EmptyEnumerableIfNull<PackageAction>(actions) ?? GetPackageActions(actions, metaData.Name);
        //        installationSummary.Actions = packageActions;

        //        installationSummary.PackageInstalled = true;

        //        return installationSummary;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Error installing package " + packageFile, e);
        //    }
        //}

        private FileInfo GetPackageZipFile(string packageFileName) => new FileInfo(IOHelper.MapPath(_packagesFolderPath).EnsureEndsWith('\\') + packageFileName);

        private static IEnumerable<T> EmptyEnumerableIfNull<T>(object obj)
        {
            return obj == null ? Enumerable.Empty<T>() : null;
        }

        private XDocument GetConfigXmlDoc(FileInfo packageFile)
        {
            var configXmlContent = _packageExtraction.ReadTextFileFromArchive(packageFile, Constants.Packaging.PackageXmlFileName, out _);

            var document = XDocument.Parse(configXmlContent);

            if (document.Root == null ||
                document.Root.Name.LocalName.Equals(Constants.Packaging.UmbPackageNodeName) == false)
                throw new FormatException("xml does not have a root node called \"umbPackage\"");

            return document;
        }

        public XElement GetConfigXmlElement(FileInfo packageFile)
        {
            var document = GetConfigXmlDoc(packageFile);
            return document.Root;
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
                                            var sd = sourceDestination.Single(fi => fi.packageUniqueFile == mf);
                                            return $"source: \"{sd.packageUniqueFile}\" destination: \"{sd.appRelativePath}\"";
                                        })));
            }

            IEnumerable<string> duplicates = _packageExtraction.FindDuplicateFileNames(packageFile).ToArray();

            if (duplicates.Any())
            {
                throw new Exception("The following filename(s) are found more than one time in the package, since the filename is used ad primary key, this is not allowed: " +
                                    string.Join(", ", duplicates));
            }
        }

        private static IEnumerable<PackageAction> GetPackageActions(XElement actionsElement, string packageName)
        {
            if (actionsElement == null) { return new PackageAction[0]; }

            if (string.Equals(Constants.Packaging.ActionsNodeName, actionsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.ActionsNodeName + "\" as root",
                    "actionsElement");
            }

            return actionsElement.Elements(Constants.Packaging.ActionNodeName)
                .Select(elemet =>
                {
                    XAttribute aliasAttr = elemet.Attribute(Constants.Packaging.AliasNodeNameCapital);
                    if (aliasAttr == null)
                        throw new ArgumentException(
                            "missing \"" + Constants.Packaging.AliasNodeNameCapital + "\" atribute in alias element",
                            "actionsElement");

                    var packageAction = new PackageAction
                    {
                        XmlData = elemet,
                        Alias = aliasAttr.Value,
                        PackageName = packageName,
                    };


                    var attr = elemet.Attribute(Constants.Packaging.RunatNodeAttribute);

                    if (attr != null && Enum.TryParse(attr.Value, true, out ActionRunAt runAt)) { packageAction.RunAt = runAt; }

                    attr = elemet.Attribute(Constants.Packaging.UndoNodeAttribute);

                    if (attr != null && bool.TryParse(attr.Value, out var undo)) { packageAction.Undo = undo; }


                    return packageAction;
                }).ToArray();
        }

        private IEnumerable<IContent> InstallDocuments(XElement documentsElement, int userId = 0)
        {
            if ((string.Equals(Constants.Packaging.DocumentSetNodeName, documentsElement.Name.LocalName) == false)
                && (string.Equals(Constants.Packaging.DocumentsNodeName, documentsElement.Name.LocalName) == false))
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.DocumentsNodeName + "\" as root",
                    "documentsElement");
            }

            if (string.Equals(Constants.Packaging.DocumentSetNodeName, documentsElement.Name.LocalName))
                return _packagingService.ImportContent(documentsElement, -1, userId);

            return
                documentsElement.Elements(Constants.Packaging.DocumentSetNodeName)
                    .SelectMany(documentSetElement => _packagingService.ImportContent(documentSetElement, -1, userId))
                    .ToArray();
        }

        private IEnumerable<IFile> InstallStylesheets(XElement styleSheetsElement)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, styleSheetsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.StylesheetsNodeName + "\" as root",
                    "styleSheetsElement");
            }

            // TODO: Call _packagingService when import stylesheets import has been implimentet
            if (styleSheetsElement.HasElements == false) { return new List<IFile>(); }

            throw new NotImplementedException("The packaging service do not yes have a method for importing stylesheets");
        }

        private IEnumerable<IContentType> InstallDocumentTypes(XElement documentTypes, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentTypesNodeName, documentTypes.Name.LocalName) == false)
            {
                if (string.Equals(Constants.Packaging.DocumentTypeNodeName, documentTypes.Name.LocalName) == false)
                    throw new ArgumentException(
                        "Must be \"" + Constants.Packaging.DocumentTypesNodeName + "\" as root", "documentTypes");

                documentTypes = new XElement(Constants.Packaging.DocumentTypesNodeName, documentTypes);
            }

            return _packagingService.ImportContentTypes(documentTypes, userId);
        }

        private IEnumerable<ITemplate> InstallTemplates(XElement templateElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.TemplatesNodeName + "\" as root",
                    "templateElement");
            }
            return _packagingService.ImportTemplates(templateElement, userId);
        }

        private IEnumerable<IMacro> InstallMacros(XElement macroElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.MacrosNodeName, macroElements.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.MacrosNodeName + "\" as root",
                    "macroElements");
            }
            return _packagingService.ImportMacros(macroElements, userId);
        }

        private IEnumerable<IDictionaryItem> InstallDictionaryItems(XElement dictionaryItemsElement)
        {
            if (string.Equals(Constants.Packaging.DictionaryItemsNodeName, dictionaryItemsElement.Name.LocalName) ==
                false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.DictionaryItemsNodeName + "\" as root",
                    "dictionaryItemsElement");
            }
            return _packagingService.ImportDictionaryItems(dictionaryItemsElement);
        }

        private IEnumerable<ILanguage> InstallLanguages(XElement languageElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.LanguagesNodeName, languageElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.LanguagesNodeName + "\" as root", "languageElement");
            }
            return _packagingService.ImportLanguages(languageElement, userId);
        }

        private IEnumerable<IDataType> InstallDataTypes(XElement dataTypeElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DataTypesNodeName, dataTypeElements.Name.LocalName) == false)
            {
                if (string.Equals(Constants.Packaging.DataTypeNodeName, dataTypeElements.Name.LocalName) == false)
                {
                    throw new ArgumentException("Must be \"" + Constants.Packaging.DataTypeNodeName + "\" as root", "dataTypeElements");
                }
            }
            return _packagingService.ImportDataTypeDefinitions(dataTypeElements, userId);
        }
        
    }
}
