using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
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
            XmlElement documentElement = GetConfigXmlDocFromPackageFile(packageFilePath);

            return GetMetaData(documentElement);
        }

        public PackageImportIssues FindPackageImportIssues(string packageFilePath)
        {
            XmlElement documentElement = GetConfigXmlDocFromPackageFile(packageFilePath);
            return FindImportIssues(documentElement);
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


        private XmlElement GetConfigXmlDocFromPackageFile(string packageFilePath)
        {
            FileInfo packageFileInfo = GetPackageFileInfo(packageFilePath);

            string configXmlContent = _unpackHelper.ReadSingleTextFile(packageFileInfo.FullName, PACKAGE_XML_FILE_NAME);

            var packageConfig = new XmlDocument();

            packageConfig.LoadXml(configXmlContent);
            XmlElement documentElement = packageConfig.DocumentElement;
            return documentElement;
        }


        private PackageInstallationSummary InstallFromDirectory(string packageDir, int userId)
        {
            XmlElement configXml = GetConfigXmlDocFromPackageDirectory(packageDir);

            return new PackageInstallationSummary
            {
                MetaData = GetMetaData(configXml),
                DataTypesInstalled = InstallDataTypes(configXml, userId),
                LanguagesInstalled = InstallLanguages(configXml, userId),
                DictionaryItemsInstalled = InstallDictionaryItems(configXml, userId),
                MacrosInstalled = InstallMacros(configXml, userId),
                FilesInstalled = InstallFiles(packageDir, configXml),
                TemplatesInstalled = InstallTemplats(configXml, userId),
                DocumentTypesInstalled = InstallDocumentTypes(configXml, userId),
                StylesheetsInstalled = InstallStylesheets(configXml, userId),
                DocumentsInstalled = InstallDocuments(configXml, userId),
                PackageInstallActions = GetInstallActions(configXml),
                PackageUninstallActions = GetUninstallActions(configXml)
            };
        }

        private static string GetUninstallActions(XmlElement configXml)
        {
            var actions = new StringBuilder();
            //saving the uninstall actions untill the package is uninstalled.
            XmlNodeList xmlNodeList = configXml.SelectNodes("Actions/Action [@undo != false()]");
            if (xmlNodeList != null)
            {
                foreach (XmlNode n in xmlNodeList)
                {
                    actions.Append(n.OuterXml);
                }
            }
            return actions.ToString();
        }

        private static Dictionary<string, XmlNode> GetInstallActions(XmlElement configXml)
        {
            XmlNodeList xmlNodeList = configXml.SelectNodes("Actions/Action [@runat != 'uninstall']");
            Dictionary<string, XmlNode> retVal2;
            if (xmlNodeList != null)
            {
                retVal2 = xmlNodeList.OfType<XmlNode>()
                    .Select(an => new
                    {
                        alias =
                            an.Attributes == null
                                ? null
                                : an.Attributes["alias"] == null ? null : an.Attributes["alias"].Value,
                        node = an
                    }).Where(x => string.IsNullOrEmpty(x.alias) == false)
                    .ToDictionary(x => x.alias, x => x.node);
            }
            else
            {
                retVal2 = new Dictionary<string, XmlNode>();
            }
            return retVal2;
        }

        private IEnumerable<int> InstallDocuments(XmlElement configXml, int userId = 0)
        {

            var rootElement = configXml.GetXElement();
            var documentElement = rootElement.Descendants("DocumentSet").FirstOrDefault();
            if (documentElement != null)
            {
                IEnumerable<IContent> content = _packagingService.ImportContent(documentElement, -1, userId);
                return content.Select(c => c.Id);

            }
            return Enumerable.Empty<int>();
        }

        private IEnumerable<int> InstallStylesheets(XmlElement configXml, int userId = 0)
        {
            XmlNodeList xmlNodeList = configXml.SelectNodes("Stylesheets/Stylesheet");
            if (xmlNodeList == null)
            {
                return Enumerable.Empty<int>();
            }

            var retVal = new List<int>();

            foreach (var element in xmlNodeList.OfType<XmlNode>().Select(n => n.GetXElement()))
            {
                retVal.AddRange(_packagingService.ImportStylesheets(element, userId).Select(f => f.Id));

            }
            return retVal;
        }

        private IEnumerable<int> InstallDocumentTypes(XmlElement configXml, int userId = 0)
        {
            XElement rootElement = configXml.GetXElement();
            //Check whether the root element is a doc type rather then a complete package
            XElement docTypeElement = rootElement.Name.LocalName.Equals("DocumentType") ||
                                      rootElement.Name.LocalName.Equals("DocumentTypes")
                ? rootElement
                : rootElement.Descendants("DocumentTypes").FirstOrDefault();
            if (docTypeElement != null)
            {
                IEnumerable<IContentType> contentTypes = _packagingService.ImportContentTypes(docTypeElement, userId);
                return contentTypes.Select(ct => ct.Id);
            }

            return Enumerable.Empty<int>();
        }

        private IEnumerable<int> InstallTemplats(XmlElement configXml, int userId = 0)
        {
            XElement templateElement = configXml.GetXElement().Descendants("Templates").FirstOrDefault();
            IEnumerable<ITemplate> templates = _packagingService.ImportTemplates(templateElement, userId);
            return templates.Select(t => t.Id);
        }


        private static IEnumerable<KeyValuePair<string, bool>> InstallFiles(string packageDir, XmlElement configXml)
        {
            string basePath = HostingEnvironment.ApplicationPhysicalPath;

            XmlNodeList xmlNodeList = configXml.SelectNodes("//file");

            var installedFiles = new List<KeyValuePair<string, bool>>();
            if (xmlNodeList != null)
            {
                foreach (XmlNode n in xmlNodeList)
                {
                    string orgPath = XmlHelper.GetNodeValue(n.SelectSingleNode("orgPath"));
                    string guid = XmlHelper.GetNodeValue(n.SelectSingleNode("guid"));
                    string orgName = XmlHelper.GetNodeValue(n.SelectSingleNode("orgName"));

                    String destPath = GetFileName(basePath, orgPath);
                    String sourceFile = GetFileName(packageDir, guid);
                    String destFile = GetFileName(destPath, orgName);

                    if (Directory.Exists(destPath) == false) Directory.CreateDirectory(destPath);

                    bool overrideExisting = File.Exists(destFile);

                    File.Copy(sourceFile, destFile, true);

                    installedFiles.Add(new KeyValuePair<string, bool>(orgPath + "/" + orgName, overrideExisting));
                }
            }
            return installedFiles;
        }

        private IEnumerable<int> InstallMacros(XmlElement configXml, int userId = 0)
        {
            var xmlNodeList = configXml.SelectNodes("//macro");
            if (xmlNodeList == null)
            {
                return Enumerable.Empty<int>();
            }

            var retVal = new List<int>();
            foreach (var n in xmlNodeList.OfType<XmlNode>().Select(n => n.GetXElement()))
            {
                retVal.AddRange(_packagingService.ImportMacros(n, userId).Select(m => m.Id));
            }

            return retVal;

        }

        private IEnumerable<int> InstallDictionaryItems(XmlElement configXml, int userId = 0)
        {
            var xmlNodeList = configXml.SelectNodes("./DictionaryItems/DictionaryItem");
            if (xmlNodeList == null) { return Enumerable.Empty<int>(); }

            var retVal = new List<int>();
            foreach (var n in xmlNodeList.OfType<XmlNode>().Select(n => n.GetXElement()))
            {
                retVal.AddRange(_packagingService.ImportDictionaryItems(n, userId).Select(di => di.Id));
            }

            return retVal;
        }

        private IEnumerable<int> InstallLanguages(XmlElement configXml, int userId = 0)
        {
            XmlNodeList xmlNodeList = configXml.SelectNodes("//Language");
            if (xmlNodeList == null) { return Enumerable.Empty<int>(); }
            var retVal = new List<int>();
            foreach (var n in xmlNodeList.OfType<XmlNode>().Select(n => n.GetXElement()))
            {
                retVal.AddRange(_packagingService.ImportLanguage(n, userId).Select(l => l.Id));
            }
            return retVal;
        }

        private IEnumerable<int> InstallDataTypes(XmlElement configXml, int userId = 0)
        {
            XElement rootElement = configXml.GetXElement();
            XElement dataTypeElement = rootElement.Descendants("DataTypes").FirstOrDefault();

            if (dataTypeElement != null)
            {
                IEnumerable<IDataTypeDefinition> dataTypeDefinitions =
                    _packagingService.ImportDataTypeDefinitions(dataTypeElement, userId);
                return dataTypeDefinitions.Select(dtd => dtd.Id);
            }
            return Enumerable.Empty<int>();
        }

        private static XmlElement GetConfigXmlDocFromPackageDirectory(string packageDir)
        {
            string packageXmlPath = Path.Combine(packageDir, PACKAGE_XML_FILE_NAME);

            if (File.Exists(packageXmlPath) == false)
            {
                throw new FileNotFoundException("Could not find " + PACKAGE_XML_FILE_NAME + " in package");
            }

            var packageConfig = new XmlDocument();
            packageConfig.Load(packageXmlPath);

            if (packageConfig.DocumentElement == null)
            {
                throw new Exception("Invalid package.xml could not load XML");
            }


            XmlElement xmlRoot = packageConfig.DocumentElement;
            return xmlRoot;
        }


        private PackageImportIssues FindImportIssues(XmlElement documentElement)
        {
            XmlNodeList fileNotes = documentElement.SelectNodes("//file");
            XmlNodeList macroNotes = documentElement.SelectNodes("//macro");
            XmlNodeList templateNotes = documentElement.SelectNodes("Templates/Template");
            XmlNodeList stylesheetNotes = documentElement.SelectNodes("Stylesheets/Stylesheet");

            var packageImportIssues = new PackageImportIssues
            {
                UnsecureFiles = FindUnsecureFiles(fileNotes),
                ConflictingMacroAliases = FindConflictingMacroAliases(macroNotes),
                ConflictingTemplateAliases = FindConflictingTemplateAliases(templateNotes),
                ConflictingStylesheetNames = FindConflictingStylesheetNames(stylesheetNotes)
            };

            return packageImportIssues;
        }

        private IEnumerable<string> FindUnsecureFiles(XmlNodeList fileNotes)
        {
            return fileNotes == null
                ? Enumerable.Empty<string>()
                : fileNotes
                    .OfType<XmlNode>()
                    .Where(FileNodeIsUnsecure)
                    .Select(n => XmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingStylesheetNames(XmlNodeList stylesheetNotes)
        {
            return stylesheetNotes == null
                ? Enumerable.Empty<KeyValuePair<string, string>>()
                : stylesheetNotes.OfType<XmlNode>()
                    .Select(n =>
                    {
                        string name = XmlHelper.GetNodeValue(n.SelectSingleNode("Name"));
                        Stylesheet existingStilesheet = _fileService.GetStylesheetByName(name);


                        // Dont know what to put in here... existing path whas the bedst i culd come up with
                        string existingFilePath = existingStilesheet == null ? null : existingStilesheet.Path;


                        return new KeyValuePair<string, string>(name, existingFilePath);
                    })
                    .Where(kv => kv.Value != null);
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingTemplateAliases(XmlNodeList templateNotes)
        {
            return templateNotes == null
                ? Enumerable.Empty<KeyValuePair<string, string>>()
                : templateNotes.OfType<XmlNode>()
                    .Select(n =>
                    {
                        string alias = XmlHelper.GetNodeValue(n.SelectSingleNode("Alias"));
                        var existingTemplate = _fileService.GetTemplate(alias) as Template;

                        string existingName = existingTemplate == null ? null : existingTemplate.Name;

                        return new KeyValuePair<string, string>(alias, existingName);
                    })
                    .Where(kv => kv.Value != null);
        }

        private IEnumerable<KeyValuePair<string, string>> FindConflictingMacroAliases(XmlNodeList macroNotes)
        {
            return macroNotes == null
                ? Enumerable.Empty<KeyValuePair<string, string>>()
                : macroNotes
                    .OfType<XmlNode>()
                    .Select(n =>
                    {
                        string alias = XmlHelper.GetNodeValue(n.SelectSingleNode("alias"));
                        IMacro macro = _macroService.GetByAlias(alias);
                        string eksistingName = macro == null ? null : macro.Name;

                        return new KeyValuePair<string, string>(alias, eksistingName);
                    })
                    .Where(kv => kv.Value != null);
        }


        private bool FileNodeIsUnsecure(XmlNode fileNode)
        {
            string basePath = HostingEnvironment.ApplicationPhysicalPath;
            string destPath = GetFileName(basePath, XmlHelper.GetNodeValue(fileNode.SelectSingleNode("orgPath")));

            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code")) return true;
            if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin")) return true;

            string destFile = GetFileName(destPath, XmlHelper.GetNodeValue(fileNode.SelectSingleNode("orgName")));

            return destFile.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase);
        }


        private PackageMetaData GetMetaData(XmlElement element)
        {
            XmlNode selectSingleNode = element.SelectSingleNode("/umbPackage/info/package/license");

            string licenseUrl = string.Empty;
            if (selectSingleNode != null && selectSingleNode.Attributes != null)
            {
                XmlNode attribute = selectSingleNode.Attributes.GetNamedItem("url");
                licenseUrl = attribute == null ? string.Empty : attribute.Value ?? string.Empty;
            }

            string reqMajorStr =
                XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/requirements/major"));
            string reqMinorStr =
                XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/requirements/minor"));
            string reqPatchStr =
                XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/requirements/patch"));

            int val;


            return new PackageMetaData
            {
                Name = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/name")),
                Version = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/version")),
                Url = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/url")),
                License = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/package/license")),
                LicenseUrl = licenseUrl,
                AuthorName = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/author/name")),
                AuthorUrl = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/author/website")),
                Readme = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/info/readme")),
                ReqMajor = int.TryParse(reqMajorStr, out val) ? val : 0,
                ReqMinor = int.TryParse(reqMinorStr, out val) ? val : 0,
                ReqPatch = int.TryParse(reqPatchStr, out val) ? val : 0,
                Control = XmlHelper.GetNodeValue(element.SelectSingleNode("/umbPackage/control"))
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