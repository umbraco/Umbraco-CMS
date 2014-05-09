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
using Umbraco.Core.Packaging;

namespace Umbraco.Core.Services
{
    public class PackageInstallerService : IPackageInstallerService
    {
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly IPackagingService _packagingService;
        private IConflictingPackageContentFinder _conflictingPackageContentFinder;
        private IUnpackHelper _unpackHelper;

        public PackageInstallerService(IPackagingService packagingService, IMacroService macroService,
            IFileService fileService)
        {
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");
            if (packagingService != null) _packagingService = packagingService;
            else throw new ArgumentNullException("packagingService");
        }


        public IConflictingPackageContentFinder ConflictingPackageContentFinder
        {
            private get
            {
                return _conflictingPackageContentFinder ??
                       (_conflictingPackageContentFinder = new ConflictingPackageContentFinder(_macroService, _fileService));
            }
            set
            {
                if (_conflictingPackageContentFinder != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }
                _conflictingPackageContentFinder = value;
            }
        }


        public IUnpackHelper UnpackHelper
        {
            private get { return _unpackHelper ?? (_unpackHelper = new UnpackHelper()); }
            set
            {
                if (_unpackHelper != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }
                _unpackHelper = value;
            }
        }

        private string _fullpathToRoot;
        public string FullpathToRoot
        {
            private get { return _fullpathToRoot ?? (_fullpathToRoot = GlobalSettings.FullpathToRoot); }
            set
            {

                if (_fullpathToRoot != null)
                {
                    throw new PropertyConstraintException("This property already have a value");
                }

                _fullpathToRoot = value;
            }
        }


        public PackageMetaData GetMetaData(string packageFilePath)
        {
            try
            {
                XElement rootElement = GetConfigXmlRootElementFromPackageFile(packageFilePath);
                return GetMetaData(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFilePath, e);
            }
        }

        public PackageImportIssues FindPackageImportIssues(string packageFilePath)
        {
            try
            {
                XElement rootElement = GetConfigXmlRootElementFromPackageFile(packageFilePath);
                return FindImportIssues(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFilePath, e);
            }
        }

        public PackageInstallationSummary InstallPackageFile(string packageFile, int userId)
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
            XElement actions;
            PackageMetaData metaData;
            
            try
            {
                XElement rootElement = GetConfigXmlRootElementFromPackageFile(packageFile);
                PackageStructureSanetyCheck(packageFile);
                dataTypes = rootElement.Element(Constants.Packaging.DataTypesNodeName);
                languages = rootElement.Element(Constants.Packaging.LanguagesNodeName);
                dictionaryItems = rootElement.Element(Constants.Packaging.DictionaryItemsNodeName);
                macroes = rootElement.Element(Constants.Packaging.MacrosNodeName);
                files = rootElement.Element(Constants.Packaging.FilesNodeName);
                templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
                documentTypes = rootElement.Element(Constants.Packaging.DocumentTypesNodeName);
                styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
                documentSet = rootElement.Element(Constants.Packaging.DocumentSetNodeName);
                actions = rootElement.Element(Constants.Packaging.ActionsNodeName);

                metaData = GetMetaData(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading " + packageFile, e);
            }

            try
            {
                
                return new PackageInstallationSummary
                {
                    MetaData = metaData,
                    DataTypesInstalled =
                        dataTypes == null ? new IDataTypeDefinition[0] : InstallDataTypes(dataTypes, userId),
                    LanguagesInstalled = languages == null ? new ILanguage[0] : InstallLanguages(languages, userId),
                    DictionaryItemsInstalled =
                        dictionaryItems == null ? new IDictionaryItem[0] : InstallDictionaryItems(dictionaryItems),
                    MacrosInstalled = macroes == null ? new IMacro[0] : InstallMacros(macroes, userId),
                    FilesInstalled =
                        packageFile == null
                            ? Enumerable.Empty<KeyValuePair<string, bool>>()
                            : InstallFiles(packageFile, files),
                    TemplatesInstalled = templates == null ? new ITemplate[0] : InstallTemplats(templates, userId),
                    DocumentTypesInstalled =
                        documentTypes == null ? new IContentType[0] : InstallDocumentTypes(documentTypes, userId),
                    StylesheetsInstalled =
                        styleSheets == null ? new IStylesheet[0] : InstallStylesheets(styleSheets, userId),
                    DocumentsInstalled = documentSet == null ? new IContent[0] : InstallDocuments(documentSet, userId),
                    PackageInstallActions =
                        actions == null ? Enumerable.Empty<KeyValuePair<string, XElement>>() : GetInstallActions(actions),
                    PackageUninstallActions = actions == null ? string.Empty : GetUninstallActions(actions)
                };
            }
            catch (Exception e)
            {
                throw new Exception("Error installing package " + packageFile, e);
            }
        }




        private XDocument GetConfigXmlDocFromPackageFile(string packageFilePath)
        {
            string filePathInPackage;
            string configXmlContent = UnpackHelper.ReadTextFileFromArchive(packageFilePath,
                Constants.Packaging.PackageXmlFileName, out filePathInPackage);

            return XDocument.Parse(configXmlContent);
        }


        private XElement GetConfigXmlRootElementFromPackageFile(string packageFilePath)
        {
            XDocument document = GetConfigXmlDocFromPackageFile(packageFilePath);
            if (document.Root == null ||
                document.Root.Name.LocalName.Equals(Constants.Packaging.UmbPackageNodeName) == false)
            {
                throw new ArgumentException("xml does not have a root node called \"umbPackage\"", packageFilePath);
            }
            return document.Root;
        }

        private void PackageStructureSanetyCheck(string packageFilePath)
        {
            XElement rootElement = GetConfigXmlRootElementFromPackageFile(packageFilePath);
            XElement filesElement = rootElement.Element(Constants.Packaging.FilesNodeName);
            if (filesElement != null)
            {
                IEnumerable<FileInPackageInfo> extractFileInPackageInfos =
                    ExtractFileInPackageInfos(filesElement).ToArray();

                IEnumerable<string> missingFiles =
                    _unpackHelper.FindMissingFiles(packageFilePath,
                        extractFileInPackageInfos.Select(i => i.FileNameInPackage)).ToArray();

                if (missingFiles.Any())
                {
                    throw new Exception("The following file(s) are missing in the package: " +
                                        string.Join(", ", missingFiles.Select(
                                            mf =>
                                            {
                                                FileInPackageInfo fileInPackageInfo =
                                                    extractFileInPackageInfos.Single(fi => fi.FileNameInPackage == mf);
                                                return string.Format("Guid: \"{0}\" Original File: \"{1}\"",
                                                    fileInPackageInfo.FileNameInPackage, fileInPackageInfo.RelativePath);
                                            })));
                }

                IEnumerable<string> dubletFileNames = _unpackHelper.FindDubletFileNames(packageFilePath).ToArray();
                if (dubletFileNames.Any())
                {
                    throw new Exception("The following filename(s) are found more than one time in the package, since the filename is used ad primary key, this is not allowed: " +
                                        string.Join(", ", dubletFileNames));
                }



            }
        }


        private static string GetUninstallActions(XElement actionsElement)
        {
            //saving the uninstall actions untill the package is uninstalled.
            return actionsElement.Elements(Constants.Packaging.ActionNodeName)
                .Where(
                    e =>
                        e.HasAttributes && e.Attribute(Constants.Packaging.UndoNodeAttribute) != null &&
                        e.Attribute(Constants.Packaging.UndoNodeAttribute)
                            .Value.Equals("false()", StringComparison.InvariantCultureIgnoreCase) == false)
                // SelectNodes("Actions/Action [@undo != false()]");
                .Select(m => m.Value).Aggregate((workingSentence, next) => next + workingSentence);
        }

        private static IEnumerable<KeyValuePair<string, XElement>> GetInstallActions(XElement actionsElement)
        {
            if (actionsElement == null)
            {
                return Enumerable.Empty<KeyValuePair<string, XElement>>();
            }

            if (string.Equals(Constants.Packaging.ActionsNodeName, actionsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.ActionsNodeName + "\" as root",
                    "actionsElement");
            }

            return actionsElement.Elements(Constants.Packaging.ActionNodeName)
                .Where(
                    e =>
                        e.HasAttributes &&
                        (e.Attribute(Constants.Packaging.RunatNodeAttribute) == null ||
                         e.Attribute(Constants.Packaging.RunatNodeAttribute)
                             .Value.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase) ==
                         false)) // .SelectNodes("Actions/Action [@runat != 'uninstall']")
                .Select(elemet =>
                {
                    XAttribute aliasAttr = elemet.Attribute(Constants.Packaging.AliasNodeNameSmall) ?? elemet.Attribute(Constants.Packaging.AliasNodeNameCapital);
                    if (aliasAttr == null)
                        throw new ArgumentException(
                            "missing \"" + Constants.Packaging.AliasNodeNameSmall + "\" atribute in alias element",
                            "actionsElement");
                    return new {elemet, alias = aliasAttr.Value};
                }).ToDictionary(x => x.alias, x => x.elemet);
        }

        private IContent[] InstallDocuments(XElement documentsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentSetNodeName, documentsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.DocumentSetNodeName + "\" as root",
                    "documentsElement");
            }
            return _packagingService.ImportContent(documentsElement, -1, userId).ToArray();
        }

        private IStylesheet[] InstallStylesheets(XElement styleSheetsElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, styleSheetsElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.StylesheetsNodeName + "\" as root",
                    "styleSheetsElement");
            }
            return _packagingService.ImportStylesheets(styleSheetsElement, userId).ToArray();
        }

        private IContentType[] InstallDocumentTypes(XElement documentTypes, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DocumentTypesNodeName, documentTypes.Name.LocalName) == false)
            {
                if (string.Equals(Constants.Packaging.DocumentTypeNodeName, documentTypes.Name.LocalName) == false)
                    throw new ArgumentException(
                        "Must be \"" + Constants.Packaging.DocumentTypesNodeName + "\" as root", "documentTypes");

                documentTypes = new XElement(Constants.Packaging.DocumentTypesNodeName, documentTypes);
            }

            return _packagingService.ImportContentTypes(documentTypes, userId).ToArray();
        }

        private ITemplate[] InstallTemplats(XElement templateElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.TemplatesNodeName + "\" as root",
                    "templateElement");
            }
            return _packagingService.ImportTemplates(templateElement, userId).ToArray();
        }


        private IEnumerable<KeyValuePair<string, bool>> InstallFiles(string packageFilePath, XElement filesElement)
        {
            return ExtractFileInPackageInfos(filesElement).Select(fpi =>
            {
                bool existingOverrided = _unpackHelper.CopyFileFromArchive(packageFilePath, fpi.FileNameInPackage,
                    fpi.FullPath);

                return new KeyValuePair<string, bool>(fpi.FullPath, existingOverrided);
            }).ToArray();
        }

        private IMacro[] InstallMacros(XElement macroElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.MacrosNodeName, macroElements.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.MacrosNodeName + "\" as root",
                    "macroElements");
            }
            return _packagingService.ImportMacros(macroElements, userId).ToArray();
        }

        private IDictionaryItem[] InstallDictionaryItems(XElement dictionaryItemsElement)
        {
            if (string.Equals(Constants.Packaging.DictionaryItemsNodeName, dictionaryItemsElement.Name.LocalName) ==
                false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.DictionaryItemsNodeName + "\" as root",
                    "dictionaryItemsElement");
            }
            return _packagingService.ImportDictionaryItems(dictionaryItemsElement).ToArray();
        }

        private ILanguage[] InstallLanguages(XElement languageElement, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.LanguagesNodeName, languageElement.Name.LocalName) == false)
            {
                throw new ArgumentException("Must be \"" + Constants.Packaging.LanguagesNodeName + "\" as root", "languageElement");
            }
            return _packagingService.ImportLanguages(languageElement, userId).ToArray();
        }

        private IDataTypeDefinition[] InstallDataTypes(XElement dataTypeElements, int userId = 0)
        {
            if (string.Equals(Constants.Packaging.DataTypesNodeName, dataTypeElements.Name.LocalName) == false)
            {
                if (string.Equals(Constants.Packaging.DataTypeNodeName, dataTypeElements.Name.LocalName) == false)
                {
                    throw new ArgumentException("Must be \"" + Constants.Packaging.DataTypeNodeName + "\" as root", "dataTypeElements");
                }
            }
            return _packagingService.ImportDataTypeDefinitions(dataTypeElements, userId).ToArray();
        }

        private PackageImportIssues FindImportIssues(XElement rootElement)
        {
            XElement files = rootElement.Element(Constants.Packaging.FilesNodeName);
            XElement styleSheets = rootElement.Element(Constants.Packaging.StylesheetsNodeName);
            XElement templates = rootElement.Element(Constants.Packaging.TemplatesNodeName);
            XElement alias = rootElement.Element(Constants.Packaging.MacrosNodeName);
            var packageImportIssues = new PackageImportIssues
            {
                UnsecureFiles = files == null ? new IFileInPackageInfo[0] : FindUnsecureFiles(files),
                ConflictingMacroAliases = alias == null ? new IMacro[0] : FindConflictingMacroAliases(alias),
                ConflictingTemplateAliases =
                    templates == null ? new ITemplate[0] : FindConflictingTemplateAliases(templates),
                ConflictingStylesheetNames =
                    styleSheets == null ? new IStylesheet[0] : FindConflictingStylesheetNames(styleSheets)
            };

            return packageImportIssues;
        }

        private IFileInPackageInfo[] FindUnsecureFiles(XElement fileElement)
        {
            return ExtractFileInPackageInfos(fileElement)
                .Where(IsFileNodeUnsecure).Cast<IFileInPackageInfo>().ToArray();
        }

        private IStylesheet[] FindConflictingStylesheetNames(XElement stylesheetNotes)
        {
            if (string.Equals(Constants.Packaging.StylesheetsNodeName, stylesheetNotes.Name.LocalName) == false)
            {
                throw new ArgumentException("the root element must be \"" + Constants.Packaging.StylesheetsNodeName + "\"", "stylesheetNotes");
            }

            return stylesheetNotes.Elements(Constants.Packaging.StylesheetNodeName)
                .Select(n =>
                {
                    XElement xElement = n.Element(Constants.Packaging.NameNodeName);
                    if (xElement == null)
                    {
                        throw new ArgumentException("Missing \"" + Constants.Packaging.NameNodeName + "\" element",
                            "stylesheetNotes");
                    }

                    string name = xElement.Value;

                    IStylesheet existingStyleSheet;
                    if (ConflictingPackageContentFinder.StylesheetExists(name, out existingStyleSheet))
                    {
                        // Don't know what to put in here... existing path was the best i could come up with
                        return existingStyleSheet;
                    }
                    return null;
                })
                .Where(v => v != null).ToArray();
        }

        private ITemplate[] FindConflictingTemplateAliases(XElement templateNotes)
        {
            if (string.Equals(Constants.Packaging.TemplatesNodeName, templateNotes.Name.LocalName) == false)
            {
                throw new ArgumentException("Node must be a \"" + Constants.Packaging.TemplatesNodeName + "\" node",
                    "templateNotes");
            }

            return templateNotes.Elements(Constants.Packaging.TemplateNodeName)
                .Select(n =>
                {
                    XElement alias = n.Element(Constants.Packaging.AliasNodeNameCapital) ?? n.Element(Constants.Packaging.AliasNodeNameSmall);
                    if (alias == null)
                    {
                        throw new ArgumentException("missing a \"" + Constants.Packaging.AliasNodeNameCapital + "\" element",
                            "templateNotes");
                    }
                    string aliasStr = alias.Value;

                    ITemplate existingTemplate;

                    if (ConflictingPackageContentFinder.TemplateExists(aliasStr, out existingTemplate))
                    {
                        return existingTemplate;
                    }

                    return null;
                })
                .Where(v => v != null).ToArray();
        }

        private IMacro[] FindConflictingMacroAliases(XElement macroNodes)
        {
            return macroNodes.Elements(Constants.Packaging.MacroNodeName)
                .Select(n =>
                {
                    XElement xElement = n.Element(Constants.Packaging.AliasNodeNameSmall) ?? n.Element(Constants.Packaging.AliasNodeNameCapital);
                    if (xElement == null)
                    {
                        throw new ArgumentException(string.Format("missing a \"{0}\" element in {0} element", Constants.Packaging.AliasNodeNameSmall),
                            "macroNodes");
                    }
                    string alias = xElement.Value;

                    IMacro existingMacro;
                    if (ConflictingPackageContentFinder.MacroExists(alias, out existingMacro))
                    {
                        return existingMacro;
                    }

                    return null;
                })
                .Where(v => v != null).ToArray();
        }


        private bool IsFileNodeUnsecure(FileInPackageInfo fileInPackageInfo)
        {

            // Should be done with regex :)
            if (fileInPackageInfo.Directory.ToLower().Contains(IOHelper.DirSepChar + "app_code")) return true;
            if (fileInPackageInfo.Directory.ToLower().Contains(IOHelper.DirSepChar + "bin")) return true;

            string extension = Path.GetExtension(fileInPackageInfo.Directory);

            return extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase);
        }


        private IEnumerable<FileInPackageInfo> ExtractFileInPackageInfos(XElement filesElement)
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


                    return new FileInPackageInfo
                    {
                        FileNameInPackage = guidElement.Value,
                        FileName = PrepareAsFilePathElement(orgNameElement.Value),
                        RelativeDir = UpdatePathPlaceholders(
                            PrepareAsFilePathElement(orgPathElement.Value)),
                        DestinationRootDir = FullpathToRoot
                    };
                }).ToArray();
        }

        private static string PrepareAsFilePathElement(string pathElement)
        {
            return pathElement.TrimStart(new[] {'\\', '/', '~'}).Replace("/", "\\");
        }


        private PackageMetaData GetMetaData(XElement xRootElement)
        {
            XElement infoElement = xRootElement.Element(Constants.Packaging.InfoNodeName);

            if (infoElement == null)
            {
                throw new ArgumentException("Did not hold a \"" + Constants.Packaging.InfoNodeName + "\" element",
                    "xRootElement");
            }

            XElement majorElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsMajorXpath);
            XElement minorElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsMinorXpath);
            XElement patchElement = infoElement.XPathSelectElement(Constants.Packaging.PackageRequirementsPatchXpath);
            XElement nameElement = infoElement.XPathSelectElement(Constants.Packaging.PackageNameXpath);
            XElement versionElement = infoElement.XPathSelectElement(Constants.Packaging.PackageVersionXpath);
            XElement urlElement = infoElement.XPathSelectElement(Constants.Packaging.PackageUrlXpath);
            XElement licenseElement = infoElement.XPathSelectElement(Constants.Packaging.PackageLicenseXpath);
            XElement authorNameElement = infoElement.XPathSelectElement(Constants.Packaging.AuthorNameXpath);
            XElement authorUrlElement = infoElement.XPathSelectElement(Constants.Packaging.AuthorWebsiteXpath);
            XElement readmeElement = infoElement.XPathSelectElement(Constants.Packaging.ReadmeXpath);

            XElement controlElement = xRootElement.Element(Constants.Packaging.ControlNodeName);

            int val;

            return new PackageMetaData
            {
                Name = nameElement == null ? string.Empty : nameElement.Value,
                Version = versionElement == null ? string.Empty : versionElement.Value,
                Url = urlElement == null ? string.Empty : urlElement.Value,
                License = licenseElement == null ? string.Empty : licenseElement.Value,
                LicenseUrl =
                    licenseElement == null
                        ? string.Empty
                        : licenseElement.HasAttributes ? licenseElement.AttributeValue<string>("url") : string.Empty,
                AuthorName = authorNameElement == null ? string.Empty : authorNameElement.Value,
                AuthorUrl = authorUrlElement == null ? string.Empty : authorUrlElement.Value,
                Readme = readmeElement == null ? string.Empty : readmeElement.Value,
                ReqMajor = majorElement == null ? 0 : int.TryParse(majorElement.Value, out val) ? val : 0,
                ReqMinor = minorElement == null ? 0 : int.TryParse(minorElement.Value, out val) ? val : 0,
                ReqPatch = patchElement == null ? 0 : int.TryParse(patchElement.Value, out val) ? val : 0,
                Control = controlElement == null ? string.Empty : controlElement.Value
            };
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

    public class FileInPackageInfo : IFileInPackageInfo
    {
        public string RelativePath
        {
            get { return Path.Combine(RelativeDir, FileName); }
        }

        public string FileNameInPackage { get; set; }
        public string RelativeDir { get; set; }
        public string DestinationRootDir { private get; set; }

        public string Directory
        {
            get { return Path.Combine(DestinationRootDir, RelativeDir); }
        }

        public string FullPath
        {
            get { return Path.Combine(DestinationRootDir, RelativePath); }
        }

        public string FileName { get; set; }
    }

    public interface IFileInPackageInfo
    {
        string RelativeDir { get; }
        string RelativePath { get; }
        string FileName { get; set; }
    }
}