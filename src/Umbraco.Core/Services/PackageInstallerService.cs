using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging;
using File = System.IO.File;

namespace Umbraco.Core.Services
{
    public class PackageInstallerService : IPackageInstallerService
    {
        private const string PACKAGE_XML_FILE_NAME = "package.xml";
        private readonly IFileService _fileService;
        private readonly IMacroService _macroService;
        private readonly IPackagingService _packagingService;
        private readonly IUnpackHelper _unpackHelper;

        public PackageInstallerService(IPackagingService packagingService, IMacroService macroService,
            IFileService fileService, IUnpackHelper unpackHelper)
        {
            _packagingService = packagingService;
            if (unpackHelper != null) _unpackHelper = unpackHelper;
            else throw new ArgumentNullException("unpackHelper");
            if (fileService != null) _fileService = fileService;
            else throw new ArgumentNullException("fileService");
            if (macroService != null) _macroService = macroService;
            else throw new ArgumentNullException("macroService");
        }


        public PackageInstallationSummary InstallPackageFile(string packageFilePath, int userId)
        {
            FileInfo fi = GetPackageFileInfo(packageFilePath);
            string tempDir = null;
            try
            {
                tempDir = _unpackHelper.UnPackToTempDirectory(fi.FullName);
                return InstallFromDirectory(tempDir, userId);
            }
            finally
            {
                if (string.IsNullOrEmpty(tempDir) == false && Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        public PackageMetaData GetMetaData(string packageFilePath)
        {
            var documentElement = GetConfigXmlDocFromPackageFile(packageFilePath);

            var rootElement = documentElement.Element("umbPackage");
            if (rootElement == null) { throw new ArgumentException("xml does not have a root node called \"umbPackage\"", packageFilePath); }

            return GetMetaData(rootElement);
        }

        public PackageImportIssues FindPackageImportIssues(string packageFilePath)
        {
            var documentElement = GetConfigXmlDocFromPackageFile(packageFilePath);

            var rootElement = documentElement.Element("umbPackage");

            if (rootElement == null) { throw new ArgumentException("File does not have a root node called \"umbPackage\"", packageFilePath); }

            return FindImportIssues(rootElement);
        }


        private FileInfo GetPackageFileInfo(string packageFilePath)
        {
            if (string.IsNullOrEmpty(packageFilePath))
            {
                throw new ArgumentNullException("packageFilePath");
            }

            var fi = new FileInfo(packageFilePath);

            if (fi.Exists == false)
            {
                throw new Exception("Error - file not found. Could find file named '" + packageFilePath + "'");
            }


            // Check if the file is a valid package
            if (fi.Extension.Equals(".umb", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new Exception(
                    "Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
            }

            return fi;
        }


        private XDocument GetConfigXmlDocFromPackageFile(string packageFilePath)
        {
            FileInfo packageFileInfo = GetPackageFileInfo(packageFilePath);

            string configXmlContent = _unpackHelper.ReadSingleTextFile(packageFileInfo.FullName, PACKAGE_XML_FILE_NAME);

            var packageConfig = XDocument.Parse(configXmlContent);
            return packageConfig;
        }


        private PackageInstallationSummary InstallFromDirectory(string packageDir, int userId)
        {
            var configXml = GetConfigXmlDocFromPackageDirectory(packageDir);
            var rootElement = configXml.XPathSelectElement("/umbPackage");
            if (rootElement == null) { throw new ArgumentException("File does not have a root node called \"umbPackage\"", packageDir); }

            var dataTypes = rootElement.Element("DataTypes");
            var languages = rootElement.Element("Languages");
            var dictionaryItems = rootElement.Element("DictionaryItems");
            var macroes = rootElement.Element("Macros");
            var files = rootElement.Element("Files");
            var templates = rootElement.Element("Templates");
            var documentTypes = rootElement.Element("DocumentTypes");
            var styleSheets = rootElement.Element("Stylesheets");
            var documentSet = rootElement.Element("DocumentSet");
            var actions = rootElement.Element("Actions");

            return new PackageInstallationSummary
            {
                MetaData = GetMetaData(rootElement),
                DataTypesInstalled = dataTypes == null ? Enumerable.Empty<int>() : InstallDataTypes(dataTypes, userId),
                LanguagesInstalled = languages == null ? Enumerable.Empty<int>() : InstallLanguages(languages, userId),
                DictionaryItemsInstalled = dictionaryItems == null ? Enumerable.Empty<int>() : InstallDictionaryItems(dictionaryItems, userId),
                MacrosInstalled = macroes == null ? Enumerable.Empty<int>() : InstallMacros(macroes, userId),
                FilesInstalled = packageDir == null ? Enumerable.Empty<KeyValuePair<string, bool>>() : InstallFiles(packageDir, files),
                TemplatesInstalled = templates == null ? Enumerable.Empty<int>() : InstallTemplats(templates, userId),
                DocumentTypesInstalled = documentTypes == null ? Enumerable.Empty<int>() : InstallDocumentTypes(documentTypes, userId),
                StylesheetsInstalled = styleSheets == null ? Enumerable.Empty<int>() : InstallStylesheets(styleSheets, userId),
                DocumentsInstalled = documentSet == null ? Enumerable.Empty<int>() : InstallDocuments(documentSet, userId),
                PackageInstallActions = actions == null ? Enumerable.Empty<KeyValuePair<string, XElement>>() : GetInstallActions(actions),
                PackageUninstallActions = actions == null ? string.Empty : GetUninstallActions(actions)
            };
        }

        private static string GetUninstallActions(XElement actionsElement)
        {
            //saving the uninstall actions untill the package is uninstalled.
            return actionsElement.Elements("Action").Where(e => e.HasAttributes && e.Attribute("undo") != null && e.Attribute("undo").Value.Equals("false()", StringComparison.InvariantCultureIgnoreCase) == false) // SelectNodes("Actions/Action [@undo != false()]");
                .Select(m => m.Value).Aggregate((workingSentence, next) => next + workingSentence);
        }

        private static IEnumerable<KeyValuePair<string, XElement>> GetInstallActions(XElement actionsElement)
        {
            if (actionsElement == null) { return Enumerable.Empty<KeyValuePair<string, XElement>>(); }

            if ("Actions".Equals(actionsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Actions\" as root", "actionsElement"); }

            return actionsElement.Elements("Action")
                .Where(
                    e =>
                        e.HasAttributes &&
                        (e.Attribute("runat") == null ||
                         e.Attribute("runat").Value.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase) ==
                         false)) // .SelectNodes("Actions/Action [@runat != 'uninstall']")
                .Select(elemet =>
                {
                    var aliasAttr = elemet.Attribute("alias");
                    if (aliasAttr == null)
                        throw new ArgumentException("missing alias atribute in alias element", "actionsElement");
                    return new {elemet, alias = aliasAttr.Value};
                }).ToDictionary(x => x.alias, x => x.elemet);
        }

        private IEnumerable<int> InstallDocuments(XElement documentsElement, int userId = 0)
        {
            if ("DocumentSet".Equals(documentsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"DocumentSet\" as root", "documentsElement"); }
            return _packagingService.ImportContent(documentsElement, -1, userId).Select(c => c.Id);
        }

        private IEnumerable<int> InstallStylesheets(XElement styleSheetsElement, int userId = 0)
        {
            if ("Stylesheets".Equals(styleSheetsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Stylesheets\" as root", "styleSheetsElement"); }
            return _packagingService.ImportStylesheets(styleSheetsElement, userId).Select(f => f.Id);
        }

        private IEnumerable<int> InstallDocumentTypes(XElement documentTypes, int userId = 0)
        {
            if ("DocumentTypes".Equals(documentTypes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                if ("DocumentType".Equals(documentTypes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false)
                    throw new ArgumentException("Must be \"DocumentTypes\" as root", "documentTypes");

                documentTypes = new XElement("DocumentTypes", documentTypes);
            }

            return _packagingService.ImportContentTypes(documentTypes, userId).Select(ct => ct.Id);
        }

        private IEnumerable<int> InstallTemplats(XElement templateElement, int userId = 0)
        {
            if ("Templates".Equals(templateElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "templateElement"); }
            return _packagingService.ImportTemplates(templateElement, userId).Select(t => t.Id);
        }


        private static IEnumerable<KeyValuePair<string, bool>> InstallFiles(string packageDir, XElement filesElement)
        {
            if ("Files".Equals(filesElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("root element must be \"Files\"", "filesElement"); }

            string basePath = HostingEnvironment.ApplicationPhysicalPath;
            
            var xmlNodeList = filesElement.Elements("file");

            return xmlNodeList.Select(e =>
            {
                var orgPathElement = e.Element("orgPath");
                if (orgPathElement == null) { throw new ArgumentException("Missing element \"orgPath\"", "filesElement"); }

                var guidElement = e.Element("guid");
                if (guidElement == null) { throw new ArgumentException("Missing element \"guid\"", "filesElement"); }

                var orgNameElement = e.Element("orgName");
                if (orgNameElement == null) { throw new ArgumentException("Missing element \"orgName\"", "filesElement"); }


                var destPath = GetFileName(basePath, orgPathElement.Value);
                var sourceFile = GetFileName(packageDir, guidElement.Value);
                var destFile = GetFileName(destPath, orgNameElement.Value);

                if (Directory.Exists(destPath) == false) Directory.CreateDirectory(destPath);

                var existingOverrided = File.Exists(destFile);

                File.Copy(sourceFile, destFile, true);
                
                return new KeyValuePair<string, bool>(orgPathElement.Value + "/" + orgNameElement.Value, existingOverrided);
            });
        }

        private IEnumerable<int> InstallMacros(XElement macroElements, int userId = 0)
        {
            if ("Macros".Equals(macroElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "macroElements"); }
            return _packagingService.ImportMacros(macroElements, userId).Select(m => m.Id);
        }

        private IEnumerable<int> InstallDictionaryItems(XElement dictionaryItemsElement, int userId = 0)
        {
            if ("DictionaryItems".Equals(dictionaryItemsElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "dictionaryItemsElement"); }
            return _packagingService.ImportDictionaryItems(dictionaryItemsElement, userId).Select(di => di.Id);
        }

        private IEnumerable<int> InstallLanguages(XElement languageElement, int userId = 0)
        {
            if ("Languages".Equals(languageElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Must be \"Templates\" as root", "languageElement"); }
            return _packagingService.ImportLanguage(languageElement, userId).Select(l => l.Id);
        }

        private IEnumerable<int> InstallDataTypes(XElement dataTypeElements, int userId = 0)
        {
            if ("DataTypes".Equals(dataTypeElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) ==
                false)
            {

                if ("DataType".Equals(dataTypeElements.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) ==
                    false)
                {
                    throw new ArgumentException("Must be \"Templates\" as root", "dataTypeElements");
                }
            }
            return _packagingService.ImportDataTypeDefinitions(dataTypeElements, userId).Select(e => e.Id);
        }

        private static XDocument GetConfigXmlDocFromPackageDirectory(string packageDir)
        {
            string packageXmlPath = Path.Combine(packageDir, PACKAGE_XML_FILE_NAME);
            if (File.Exists(packageXmlPath) == false) { throw new FileNotFoundException("Could not find " + PACKAGE_XML_FILE_NAME + " in package"); }
            return XDocument.Load(packageXmlPath);
        }


        private PackageImportIssues FindImportIssues(XElement rootElement)
        {
            var files = rootElement.Element("Files");
            var styleSheets = rootElement.Element("Stylesheets");
            var templates = rootElement.Element("Templates");
            var alias = rootElement.Element("Macros");
            var packageImportIssues = new PackageImportIssues
            {
                UnsecureFiles = files == null ? Enumerable.Empty<string>() : FindUnsecureFiles(files),
                ConflictingMacroAliases =  alias == null ? Enumerable.Empty<KeyValuePair<string, string>>() : FindConflictingMacroAliases(alias),
                ConflictingTemplateAliases = templates == null ?  Enumerable.Empty<KeyValuePair<string, string>>() : FindConflictingTemplateAliases(templates),
                ConflictingStylesheetNames = styleSheets == null ? Enumerable.Empty<KeyValuePair<string, string>>() : FindConflictingStylesheetNames(styleSheets)
            };

            return packageImportIssues;
        }

        private IEnumerable<string> FindUnsecureFiles(XElement fileElement)
        {
            if ("Files".Equals(fileElement.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("the root element must be \"Files\"", "fileElement"); }

            return fileElement.Elements("file")
                .Where(FileNodeIsUnsecure)
                    .Select(n =>
                    {
                        var xElement = n.Element("orgName");
                        if (xElement == null) { throw new ArgumentException("missing a element: orgName", "n"); }
                        return xElement.Value;
                    });
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingStylesheetNames(XElement stylesheetNotes)
        {
            if ("Stylesheets".Equals(stylesheetNotes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("the root element must be \"Stylesheets\"", "stylesheetNotes"); }

            return stylesheetNotes.Elements("styleSheet")
                    .Select(n =>
                    {
                        var xElement = n.Element("Name");
                        if (xElement == null) { throw new ArgumentException("Missing \"Name\" element", "stylesheetNotes"); }

                        string name = xElement.Name.LocalName;
                        Stylesheet existingStilesheet = _fileService.GetStylesheetByName(name);

                        // Don't know what to put in here... existing path whas the best i could come up with
                        string existingFilePath = existingStilesheet == null ? null : existingStilesheet.Path;

                        return new KeyValuePair<string, string>(name, existingFilePath);
                    })
                    .Where(kv => kv.Value != null);
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingTemplateAliases(XElement templateNotes)
        {
            if ("Templates".Equals(templateNotes.Name.LocalName, StringComparison.InvariantCultureIgnoreCase) == false) { throw new ArgumentException("Node must be a Templates node", "templateNotes"); }

            return templateNotes.Elements("Template")
                    .Select(n =>
                    {
                        var alias = n.Element("Alias");
                        if (alias == null) { throw new ArgumentException("missing a alias element", "templateNotes"); }
                        string aliasStr = alias.Value;
                        var existingTemplate = _fileService.GetTemplate(aliasStr) as Template;
                        string existingName = existingTemplate == null ? null : existingTemplate.Name;

                        return new KeyValuePair<string, string>(aliasStr, existingName);
                    })
                    .Where(kv => kv.Value != null);
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingMacroAliases(XElement macroNodes)
        {
            return  macroNodes.Elements("macro")
                    .Select(n =>
                    {
                        var xElement = n.Element("alias");
                        if (xElement == null) { throw new ArgumentException("missing a alias element", "macroNodes"); }
                        string alias = xElement.Value;
                        IMacro macro = _macroService.GetByAlias(xElement.Value);
                        string eksistingName = macro == null ? null : macro.Name;

                        return new KeyValuePair<string, string>(alias, eksistingName);
                    })
                    .Where(kv => kv.Key != null && kv.Value != null);
        }


        private bool FileNodeIsUnsecure(XElement fileNode)
        {
            string basePath = HostingEnvironment.ApplicationPhysicalPath;
            var orgName = fileNode.Element("orgName");
            if (orgName == null) { throw new ArgumentException("Missing element \"orgName\"", "fileNode"); }

            string destPath = GetFileName(basePath, orgName.Value);

            // Should be done with regex :)
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code")) return true;
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin")) return true;

            return destPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase);
        }


        private PackageMetaData GetMetaData(XElement xRootElement)
        {
            XElement infoElement = xRootElement.Element("info");
            
            if (infoElement == null) { throw new ArgumentException("Did not hold a \"info\" element", "xRootElement"); }

            var majorElement = infoElement.XPathSelectElement("/package/requirements/major");
            var minorElement = infoElement.XPathSelectElement("/package/requirements/minor");
            var patchElement = infoElement.XPathSelectElement("/package/requirements/patch");
            var nameElement = infoElement.XPathSelectElement("/package/name");
            var versionElement = infoElement.XPathSelectElement("/package/version");
            var urlElement = infoElement.XPathSelectElement("/package/url");
            var licenseElement = infoElement.XPathSelectElement("/package/license");
            var authorNameElement = infoElement.XPathSelectElement("/author/name");
            var authorUrlElement = infoElement.XPathSelectElement("/author/website");
            var readmeElement = infoElement.XPathSelectElement("/readme");

            var controlElement = xRootElement.Element("control");

            int val;

            return new PackageMetaData
            {
                Name = nameElement == null ? string.Empty : nameElement.Value,
                Version = versionElement == null ? string.Empty : versionElement.Value,
                Url = urlElement == null ? string.Empty : urlElement.Value,
                License = licenseElement == null ? string.Empty : licenseElement.Value,
                LicenseUrl = licenseElement == null ? string.Empty : licenseElement.HasAttributes ? licenseElement.AttributeValue<string>("url") : string.Empty,
                AuthorName = authorNameElement == null ? string.Empty : authorNameElement.Value,
                AuthorUrl = authorUrlElement == null ? string.Empty : authorUrlElement.Value,
                Readme = readmeElement == null ? string.Empty : readmeElement.Value,
                ReqMajor = majorElement == null ? 0 : int.TryParse(majorElement.Value, out val) ? val : 0,
                ReqMinor = minorElement == null ? 0 : int.TryParse(minorElement.Value, out val) ? val : 0,
                ReqPatch = patchElement == null ? 0 : int.TryParse(patchElement.Value, out val) ? val : 0,
                Control = controlElement == null ? string.Empty : controlElement.Value
            };
        }


        /// <summary>
        ///     Gets the name of the file in the specified path.
        ///     Corrects possible problems with slashes that would result from a simple concatenation.
        ///     Can also be used to concatenate paths.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The name of the file in the specified path.</returns>
        private static String GetFileName(String path, string fileName)
        {
            // virtual dir support
            fileName = IOHelper.FindFile(fileName);

            if (path.Contains("[$"))
            {
                //this is experimental and undocumented...
                path = path.Replace("[$UMBRACO]", SystemDirectories.Umbraco);
                path = path.Replace("[$UMBRACOCLIENT]", SystemDirectories.UmbracoClient);
                path = path.Replace("[$CONFIG]", SystemDirectories.Config);
                path = path.Replace("[$DATA]", SystemDirectories.Data);
            }

            //to support virtual dirs we try to lookup the file... 
            path = IOHelper.FindFile(path);


            Debug.Assert(path != null && path.Length >= 1);
            Debug.Assert(fileName != null && fileName.Length >= 1);

            path = path.Replace('/', '\\');
            fileName = fileName.Replace('/', '\\');

            // Does filename start with a slash? Does path end with one?
            bool fileNameStartsWithSlash = (fileName[0] == Path.DirectorySeparatorChar);
            bool pathEndsWithSlash = (path[path.Length - 1] == Path.DirectorySeparatorChar);

            // Path ends with a slash
            if (pathEndsWithSlash)
            {
                if (fileNameStartsWithSlash == false)
                    // No double slash, just concatenate
                    return path + fileName;
                return path + fileName.Substring(1);
            }
            if (fileNameStartsWithSlash)
                // Required slash specified, just concatenate
                return path + fileName;
            return path + Path.DirectorySeparatorChar + fileName;
        }
    }
}