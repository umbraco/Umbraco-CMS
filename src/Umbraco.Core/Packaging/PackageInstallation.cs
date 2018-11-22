using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;
using umbraco.interfaces;
using File = System.IO.File;

namespace Umbraco.Core.Packaging
{
    internal class PackageInstallation : IPackageInstallation
    {
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly IPackagingService _packagingService;
        private IConflictingPackageData _conflictingPackageData;
        private readonly IPackageExtraction _packageExtraction;
        private string _fullPathToRoot;

        public PackageInstallation(IPackagingService packagingService, IMacroService macroService,
            IFileService fileService, IPackageExtraction packageExtraction)
            : this(packagingService, macroService, fileService, packageExtraction, GlobalSettings.FullpathToRoot)
        {}

        public PackageInstallation(IPackagingService packagingService, IMacroService macroService,
            IFileService fileService, IPackageExtraction packageExtraction, string fullPathToRoot)
        {
            if (packageExtraction != null) _packageExtraction = packageExtraction; 
            else throw new ArgumentNullException("packageExtraction");
            
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
            
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");

            if (packagingService != null) _packagingService = packagingService;
            else throw new ArgumentNullException("packagingService");

            _fullPathToRoot = fullPathToRoot;
        }
        
        public IConflictingPackageData ConflictingPackageData
        {
            private get
            {
                return _conflictingPackageData ??
                       (_conflictingPackageData = new ConflictingPackageData(_macroService, _fileService));
            }
            set
            {
                if (_conflictingPackageData != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }
                _conflictingPackageData = value;
            }
        }

        public string FullPathToRoot
        {
            private get { return _fullPathToRoot; }
            set
            {

                if (_fullPathToRoot != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }

                _fullPathToRoot = value;
            }
        }
        
        public MetaData GetMetaData(string packageFilePath)
        {
            try
            {
                XElement rootElement = GetConfigXmlElement(packageFilePath);
                return GetMetaData(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFilePath, e);
            }
        }

        public PreInstallWarnings GetPreInstallWarnings(string packageFilePath)
        {
            try
            {
                XElement rootElement = GetConfigXmlElement(packageFilePath);
                return GetPreInstallWarnings(packageFilePath, rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFilePath, e);
            }
        }

        public InstallationSummary InstallPackage(string packageFile, int userId)
        {
            XElement dataTypes;
            XElement languages;
            XElement dictionaryItems;
            XElement macroes;
            XElement files;
            XElement templates;
            XElement documentTypes;
            XElement styleSheets;
            XElement documentSet;
            XElement documents;
            XElement actions;
            MetaData metaData;
            InstallationSummary installationSummary;
            
            try
            {
                XElement rootElement = GetConfigXmlElement(packageFile);
                PackageSupportedCheck(rootElement);
                PackageStructureSanetyCheck(packageFile, rootElement);
                dataTypes = rootElement.Element(Constants.Packaging.DataTypesNodeName);
                languages = rootElement.Element(Constants.Packaging.LanguagesNodeName);
                dictionaryItems = rootElement.Element(Constants.Packaging.DictionaryItemsNodeName);
                macroes = rootElement.Element(Constants.Packaging.MacrosNodeName);
                files = rootElement.Element(Constants.Packaging.FilesNodeName);
                templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
                documentTypes = rootElement.Element(Constants.Packaging.DocumentTypesNodeName);
                styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
                documentSet = rootElement.Element(Constants.Packaging.DocumentSetNodeName);
                documents = rootElement.Element(Constants.Packaging.DocumentsNodeName);
                actions = rootElement.Element(Constants.Packaging.ActionsNodeName);

                metaData = GetMetaData(rootElement);
                installationSummary = new InstallationSummary {MetaData = metaData};
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFile, e);
            }

            try
            {
                var dataTypeDefinitions = EmptyEnumerableIfNull<IDataTypeDefinition>(dataTypes) ?? InstallDataTypes(dataTypes, userId);
                installationSummary.DataTypesInstalled = dataTypeDefinitions;

                var languagesInstalled = EmptyEnumerableIfNull<ILanguage>(languages) ?? InstallLanguages(languages, userId);
                installationSummary.LanguagesInstalled = languagesInstalled;

                var dictionaryInstalled = EmptyEnumerableIfNull<IDictionaryItem>(dictionaryItems) ?? InstallDictionaryItems(dictionaryItems);
                installationSummary.DictionaryItemsInstalled = dictionaryInstalled;

                var macros = EmptyEnumerableIfNull<IMacro>(macroes) ?? InstallMacros(macroes, userId);
                installationSummary.MacrosInstalled = macros;

                var keyValuePairs = EmptyEnumerableIfNull<string>(packageFile) ?? InstallFiles(packageFile, files);
                installationSummary.FilesInstalled = keyValuePairs;

                var templatesInstalled = EmptyEnumerableIfNull<ITemplate>(templates) ?? InstallTemplats(templates, userId);
                installationSummary.TemplatesInstalled = templatesInstalled;

                var documentTypesInstalled = EmptyEnumerableIfNull<IContentType>(documentTypes) ?? InstallDocumentTypes(documentTypes, userId);
                installationSummary.ContentTypesInstalled =documentTypesInstalled;

                var stylesheetsInstalled = EmptyEnumerableIfNull<IFile>(styleSheets) ?? InstallStylesheets(styleSheets, userId);
                installationSummary.StylesheetsInstalled = stylesheetsInstalled;

                var documentsInstalled = documents != null ? InstallDocuments(documents, userId) 
                    : EmptyEnumerableIfNull<IContent>(documentSet) 
                    ?? InstallDocuments(documentSet, userId);
                installationSummary.ContentInstalled = documentsInstalled;

                var packageActions = EmptyEnumerableIfNull<PackageAction>(actions) ?? GetPackageActions(actions, metaData.Name);
                installationSummary.Actions = packageActions;

                installationSummary.PackageInstalled = true;

                return installationSummary;
            }
            catch (Exception e)
            {
                throw new Exception("Error installing package " + packageFile, e);
            }
        }

        /// <summary>
        /// Temperary check to test that we support stylesheets
        /// </summary>
        /// <param name="rootElement"></param>
        private void PackageSupportedCheck(XElement rootElement)
        {
            XElement styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            if (styleSheets != null && styleSheets.Elements().Any())
                throw new NotSupportedException("Stylesheets is not suported in this version of umbraco");
        }

        private static T[] EmptyArrayIfNull<T>(object obj)
        {
            return obj == null ? new T[0] : null;
        }

        private static IEnumerable<T> EmptyEnumerableIfNull<T>(object obj)
        {
            return obj == null ? Enumerable.Empty<T>() : null;
        }

        private XDocument GetConfigXmlDoc(string packageFilePath)
        {
            string filePathInPackage;
            string configXmlContent = _packageExtraction.ReadTextFileFromArchive(packageFilePath,
                Constants.Packaging.PackageXmlFileName, out filePathInPackage);

            return XDocument.Parse(configXmlContent);
        }

        public XElement GetConfigXmlElement(string packageFilePath)
        {
            XDocument document = GetConfigXmlDoc(packageFilePath);
            if (document.Root == null ||
                document.Root.Name.LocalName.Equals(Constants.Packaging.UmbPackageNodeName) == false)
            {
                throw new ArgumentException("xml does not have a root node called \"umbPackage\"", packageFilePath);
            }
            return document.Root;
        }

        internal void PackageStructureSanetyCheck(string packageFilePath)
        {
            XElement rootElement = GetConfigXmlElement(packageFilePath);
            PackageStructureSanetyCheck(packageFilePath, rootElement);
        }

        private void PackageStructureSanetyCheck(string packageFilePath, XElement rootElement)
        {
            XElement filesElement = rootElement.Element(Constants.Packaging.FilesNodeName);
            if (filesElement != null)
            {
                var sourceDestination = ExtractSourceDestinationFileInformation(filesElement).ToArray();

                var missingFiles = _packageExtraction.FindMissingFiles(packageFilePath, sourceDestination.Select(i => i.Key)).ToArray();

                if (missingFiles.Any())
                {
                    throw new Exception("The following file(s) are missing in the package: " +
                                        string.Join(", ", missingFiles.Select(
                                            mf =>
                                            {
                                                var sd = sourceDestination.Single(fi => fi.Key == mf);
                                                return string.Format("source: \"{0}\" destination: \"{1}\"",
                                                    sd.Key, sd.Value);
                                            })));
                }

                IEnumerable<string> dubletFileNames = _packageExtraction.FindDubletFileNames(packageFilePath).ToArray();

                if (dubletFileNames.Any())
                {
                    throw new Exception("The following filename(s) are found more than one time in the package, since the filename is used ad primary key, this is not allowed: " +
                                        string.Join(", ", dubletFileNames));
                }
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


                    XAttribute attr = elemet.Attribute(Constants.Packaging.RunatNodeAttribute);

                    ActionRunAt runAt;
                    if (attr != null && Enum.TryParse(attr.Value, true, out runAt)) { packageAction.RunAt = runAt; }

                    attr = elemet.Attribute(Constants.Packaging.UndoNodeAttribute);

                    bool undo;
                    if (attr != null && bool.TryParse(attr.Value, out undo)) { packageAction.Undo = undo; }


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

        private IEnumerable<IFile> InstallStylesheets(XElement styleSheetsElement, int userId = 0)
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

        private IEnumerable<ITemplate> InstallTemplats(XElement templateElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.TemplatesNodeName + "\" as root",
                    "templateElement");
            }
            return _packagingService.ImportTemplates(templateElement, userId);
        }

        private IEnumerable<string> InstallFiles(string packageFilePath, XElement filesElement)
        {
            var sourceDestination = ExtractSourceDestinationFileInformation(filesElement);
            sourceDestination = AppendRootToDestination(FullPathToRoot, sourceDestination);

            _packageExtraction.CopyFilesFromArchive(packageFilePath, sourceDestination);
            
            return sourceDestination.Select(sd => sd.Value).ToArray();
        }

        private KeyValuePair<string, string>[] AppendRootToDestination(string fullpathToRoot, IEnumerable<KeyValuePair<string, string>> sourceDestination)
        {
            return
                sourceDestination.Select(
                    sd => new KeyValuePair<string, string>(sd.Key, Path.Combine(fullpathToRoot, sd.Value))).ToArray();
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

        private IEnumerable<IDataTypeDefinition> InstallDataTypes(XElement dataTypeElements, int userId = 0)
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

        private PreInstallWarnings GetPreInstallWarnings(string packagePath, XElement rootElement)
        {
            XElement files = rootElement.Element(Constants.Packaging.FilesNodeName);
            XElement styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            XElement templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
            XElement alias = rootElement.Element(Constants.Packaging.MacrosNodeName);
            
            var sourceDestination = EmptyArrayIfNull<KeyValuePair<string, string>>(files) ?? ExtractSourceDestinationFileInformation(files);

            var installWarnings = new PreInstallWarnings();

            var macroAliases = EmptyEnumerableIfNull<IMacro>(alias) ?? ConflictingPackageData.FindConflictingMacros(alias);
            installWarnings.ConflictingMacroAliases = macroAliases;

            var templateAliases = EmptyEnumerableIfNull<ITemplate>(templates) ?? ConflictingPackageData.FindConflictingTemplates(templates);
            installWarnings.ConflictingTemplateAliases = templateAliases;

            var stylesheetNames = EmptyEnumerableIfNull<IFile>(styleSheets) ?? ConflictingPackageData.FindConflictingStylesheets(styleSheets);
            installWarnings.ConflictingStylesheetNames = stylesheetNames;
            
            installWarnings.UnsecureFiles = FindUnsecureFiles(sourceDestination);
            installWarnings.FilesReplaced = FindFilesToBeReplaced(sourceDestination);
            installWarnings.AssembliesWithLegacyPropertyEditors = FindLegacyPropertyEditors(packagePath, sourceDestination);
            
            return installWarnings;
        }

        private KeyValuePair<string, string>[] FindFilesToBeReplaced(IEnumerable<KeyValuePair<string, string>> sourceDestination)
        {
            return sourceDestination.Where(sd => File.Exists(Path.Combine(FullPathToRoot, sd.Value))).ToArray();
        }

        private IEnumerable<string> FindLegacyPropertyEditors(string packagePath, IEnumerable<KeyValuePair<string, string>> sourceDestinationPair)
        {
            var dlls = sourceDestinationPair.Where(
                sd => (Path.GetExtension(sd.Value) ?? string.Empty).Equals(".dll", StringComparison.InvariantCultureIgnoreCase)).Select(sd => sd.Key).ToArray();

            if (dlls.Any() == false) { return new List<string>(); }
            
            // Now we want to see if the DLLs contain any legacy data types since we want to warn people about that
            string[] assemblyErrors;
            IEnumerable<byte[]> assemblyesToScan =_packageExtraction.ReadFilesFromArchive(packagePath, dlls);
            return PackageBinaryInspector.ScanAssembliesForTypeReference<IDataType>(assemblyesToScan, out assemblyErrors).ToArray();
        }

        private KeyValuePair<string, string>[] FindUnsecureFiles(IEnumerable<KeyValuePair<string, string>> sourceDestinationPair)
        {
            return sourceDestinationPair.Where(sd => IsFileDestinationUnsecure(sd.Value)).ToArray();
        }

        private bool IsFileDestinationUnsecure(string destination)
        {
            var unsecureDirNames = new[] {"bin", "app_code"};
            if(unsecureDirNames.Any(ud => destination.StartsWith(ud, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            string extension = Path.GetExtension(destination);
            return extension != null && extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase);
        }
        
        private KeyValuePair<string, string>[] ExtractSourceDestinationFileInformation(XElement filesElement)
        {
            if (string.Equals(Constants.Packaging.FilesNodeName, filesElement.Name.LocalName) == false)
            {
                throw new ArgumentException("the root element must be \"Files\"", "filesElement");
            }

            return filesElement.Elements(Constants.Packaging.FileNodeName)
                .Select(e =>
                {
                    XElement guidElement = e.Element(Constants.Packaging.GuidNodeName);
                    if (guidElement == null)
                    {
                        throw new ArgumentException("Missing element \"" + Constants.Packaging.GuidNodeName + "\"",
                            "filesElement");
                    }

                    XElement orgPathElement = e.Element(Constants.Packaging.OrgPathNodeName);
                    if (orgPathElement == null)
                    {
                        throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgPathNodeName + "\"",
                            "filesElement");
                    }

                    XElement orgNameElement = e.Element(Constants.Packaging.OrgNameNodeName);
                    if (orgNameElement == null)
                    {
                        throw new ArgumentException("Missing element \"" + Constants.Packaging.OrgNameNodeName + "\"",
                            "filesElement");
                    }

                    var fileName = PrepareAsFilePathElement(orgNameElement.Value);
                    var relativeDir = UpdatePathPlaceholders(PrepareAsFilePathElement(orgPathElement.Value));

                    var relativePath = Path.Combine(relativeDir, fileName);
                    

                    return new KeyValuePair<string, string>(guidElement.Value, relativePath);
                }).ToArray();
        }

        private static string PrepareAsFilePathElement(string pathElement)
        {
            return pathElement.TrimStart(new[] {'\\', '/', '~'}).Replace("/", "\\");
        }

        private MetaData GetMetaData(XElement xRootElement)
        {
            XElement infoElement = xRootElement.Element(Constants.Packaging.InfoNodeName);

            if (infoElement == null)
            {
                throw new ArgumentException("Did not hold a \"" + Constants.Packaging.InfoNodeName + "\" element",
                    "xRootElement");
            }

            var majorElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsMajorXpath);
            var minorElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsMinorXpath);
            var patchElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsPatchXpath);
            var nameElement = infoElement.XPathSelectElement(Constants.Packaging.PackageNameXpath);
            var versionElement = infoElement.XPathSelectElement(Constants.Packaging.PackageVersionXpath);
            var urlElement = infoElement.XPathSelectElement(Constants.Packaging.PackageUrlXpath);
            var licenseElement = infoElement.XPathSelectElement(Constants.Packaging.PackageLicenseXpath);
            var authorNameElement = infoElement.XPathSelectElement(Constants.Packaging.AuthorNameXpath);
            var authorUrlElement = infoElement.XPathSelectElement(Constants.Packaging.AuthorWebsiteXpath);
            var readmeElement = infoElement.XPathSelectElement(Constants.Packaging.ReadmeXpath);

            XElement controlElement = xRootElement.Element(Constants.Packaging.ControlNodeName);

            return new MetaData
                   {
                       Name = StringValue(nameElement),
                       Version = StringValue(versionElement),
                       Url = StringValue(urlElement),
                       License = StringValue(licenseElement),
                       LicenseUrl = StringAttribute(licenseElement, Constants.Packaging.PackageLicenseXpathUrlAttribute),
                       AuthorName = StringValue(authorNameElement),
                       AuthorUrl = StringValue(authorUrlElement),
                       Readme = StringValue(readmeElement),
                       Control = StringValue(controlElement),
                       ReqMajor = IntValue(majorElement),
                       ReqMinor = IntValue(minorElement),
                       ReqPatch = IntValue(patchElement)
                   };
        }

        private static string StringValue(XElement xElement, string defaultValue = "")
        {
            return xElement == null ? defaultValue : xElement.Value;
        }

        private static string StringAttribute(XElement xElement, string attribute, string defaultValue = "")
        {
            return xElement == null
                        ? defaultValue
                        : xElement.HasAttributes ? xElement.AttributeValue<string>(attribute) : defaultValue;
        }

        private static int IntValue(XElement xElement, int defaultValue = 0)
        {
            int val;
            return xElement == null ? defaultValue : int.TryParse(xElement.Value, out val) ? val : defaultValue;
        }
        
        private static string UpdatePathPlaceholders(string path)
        {
            if (path.Contains("[$"))
            {
                //this is experimental and undocumented...
                path = path.Replace("[$UMBRACO]", SystemDirectories.Umbraco);
                path = path.Replace("[$UMBRACOCLIENT]", SystemDirectories.UmbracoClient);
                path = path.Replace("[$CONFIG]", SystemDirectories.Config);
                path = path.Replace("[$DATA]", SystemDirectories.Data);
            }
            return path;
        }
    }
}