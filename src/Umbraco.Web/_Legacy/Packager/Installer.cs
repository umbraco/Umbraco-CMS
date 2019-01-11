using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Xml;
using Umbraco.Web._Legacy.Packager.PackageInstance;
using File = System.IO.File;
using PackageAction = Umbraco.Web._Legacy.Packager.PackageInstance.PackageAction;

namespace Umbraco.Web._Legacy.Packager
{
    /// <summary>
    /// The packager is a component which enables sharing of both data and functionality components between different umbraco installations.
    ///
    /// The output is a .umb (a zip compressed file) which contains the exported documents/medias/macroes/documenttypes (etc.)
    /// in a Xml document, along with the physical files used (images/usercontrols/xsl documents etc.)
    ///
    /// Partly implemented, import of packages is done, the export is *under construction*.
    /// </summary>
    /// <remarks>
    /// Ruben Verborgh 31/12/2007: I had to change some code, I marked my changes with "DATALAYER".
    /// Reason: @@IDENTITY can't be used with the new datalayer.
    /// I wasn't able to test the code, since I'm not aware how the code functions.
    /// </remarks>
    public class Installer
    {
        private const string PackageServer = "packages.umbraco.org";

        private readonly List<string> _unsecureFiles = new List<string>();
        private readonly Dictionary<string, string> _conflictingMacroAliases = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _conflictingTemplateAliases = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _conflictingStyleSheetNames = new Dictionary<string, string>();

        private readonly List<string> _binaryFileErrors = new List<string>();
        private int _currentUserId = -1;
        private static WebClient _webClient;


        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Url { get; private set; }
        public string License { get; private set; }
        public string LicenseUrl { get; private set; }
        public string Author { get; private set; }
        public string AuthorUrl { get; private set; }
        public string ReadMe { get; private set; }
        public string Control { get; private set; }

        public bool ContainsMacroConflict { get; private set; }
        public IDictionary<string, string> ConflictingMacroAliases { get { return _conflictingMacroAliases; } }

        public bool ContainsUnsecureFiles { get; private set; }
        public List<string> UnsecureFiles { get { return _unsecureFiles; } }

        public bool ContainsTemplateConflicts { get; private set; }
        public IDictionary<string, string> ConflictingTemplateAliases { get { return _conflictingTemplateAliases; } }

        /// <summary>
        /// Indicates that the package contains assembly reference errors
        /// </summary>
        public bool ContainsBinaryFileErrors { get; private set; }

        /// <summary>
        /// List each assembly reference error
        /// </summary>
        public List<string> BinaryFileErrors { get { return _binaryFileErrors; } }

        public bool ContainsStyleSheeConflicts { get; private set; }
        public IDictionary<string, string> ConflictingStyleSheetNames { get { return _conflictingStyleSheetNames; } }

        public int RequirementsMajor { get; private set; }
        public int RequirementsMinor { get; private set; }
        public int RequirementsPatch { get; private set; }

        public RequirementsType RequirementsType { get; private set; }

        public string IconUrl { get; private set; }

        /// <summary>
        /// The xmldocument, describing the contents of a package.
        /// </summary>
        public XmlDocument Config { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Installer()
        {
            Initialize();
        }

        public Installer(int currentUserId)
        {
            Initialize();
            _currentUserId = currentUserId;
        }

        private void Initialize()
        {
            ContainsBinaryFileErrors = false;
            ContainsTemplateConflicts = false;
            ContainsUnsecureFiles = false;
            ContainsMacroConflict = false;
            ContainsStyleSheeConflicts = false;
        }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the package</param>
        /// <param name="version">The version of the package</param>
        /// <param name="url">The url to a descriptionpage</param>
        /// <param name="license">The license under which the package is released (preferably GPL ;))</param>
        /// <param name="licenseUrl">The url to a licensedescription</param>
        /// <param name="author">The original author of the package</param>
        /// <param name="authorUrl">The url to the Authors website</param>
        /// <param name="requirementsMajor">Umbraco version major</param>
        /// <param name="requirementsMinor">Umbraco version minor</param>
        /// <param name="requirementsPatch">Umbraco version patch</param>
        /// <param name="readme">The readme text</param>
        /// <param name="control">The name of the usercontrol used to configure the package after install</param>
        /// <param name="requirementsType"></param>
        /// <param name="iconUrl"></param>
        public Installer(string name, string version, string url, string license, string licenseUrl, string author, string authorUrl, int requirementsMajor, int requirementsMinor, int requirementsPatch, string readme, string control, RequirementsType requirementsType, string iconUrl)
        {
            ContainsBinaryFileErrors = false;
            ContainsTemplateConflicts = false;
            ContainsUnsecureFiles = false;
            ContainsMacroConflict = false;
            ContainsStyleSheeConflicts = false;
            this.Name = name;
            this.Version = version;
            this.Url = url;
            this.License = license;
            this.LicenseUrl = licenseUrl;
            this.RequirementsMajor = requirementsMajor;
            this.RequirementsMinor = requirementsMinor;
            this.RequirementsPatch = requirementsPatch;
            this.RequirementsType = requirementsType;
            this.Author = author;
            this.AuthorUrl = authorUrl;
            this.IconUrl = iconUrl;
            ReadMe = readme;
            this.Control = control;
        }

        #region Public Methods

        /// <summary>
        /// Imports the specified package
        /// </summary>
        /// <param name="inputFile">Filename of the umbracopackage</param>
        /// <param name="deleteFile">true if the input file should be deleted after import</param>
        /// <returns></returns>
        public string Import(string inputFile, bool deleteFile)
        {
            using (Current.ProfilingLogger.DebugDuration<Installer>(
                $"Importing package file {inputFile}.",
                $"Package file {inputFile} imported."))
            {
                var tempDir = "";
                if (File.Exists(IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + inputFile)))
                {
                    var fi = new FileInfo(IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + inputFile));
                    // Check if the file is a valid package
                    if (fi.Extension.ToLower() == ".umb")
                    {
                        try
                        {
                            tempDir = UnPack(fi.FullName, deleteFile);
                            LoadConfig(tempDir);
                        }
                        catch (Exception ex)
                        {
                            Current.Logger.Error<Installer>(ex, "Error importing file {FileName}", fi.FullName);
                            throw;
                        }
                    }
                    else
                        throw new Exception("Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
                }
                else
                    throw new Exception("Error - file not found. Could find file named '" + IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + inputFile) + "'");
                return tempDir;
            }

        }

        /// <summary>
        /// Imports the specified package
        /// </summary>
        /// <param name="inputFile">Filename of the umbracopackage</param>
        /// <returns></returns>
        public string Import(string inputFile)
        {
            return Import(inputFile, true);
        }

        public int CreateManifest(string tempDir, string guid, string repoGuid)
        {
            //This is the new improved install rutine, which chops up the process into 3 steps, creating the manifest, moving files, and finally handling umb objects
            var packName = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/name"));
            var packAuthor = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/author/name"));
            var packAuthorUrl = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/author/website"));
            var packVersion = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/version"));
            var packReadme = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            var packLicense = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/license "));
            var packUrl = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/url "));
            var iconUrl = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/iconUrl"));

            var enableSkins = false;
            var skinRepoGuid = "";

            if (Config.DocumentElement.SelectSingleNode("/umbPackage/enableSkins") != null)
            {
                var skinNode = Config.DocumentElement.SelectSingleNode("/umbPackage/enableSkins");
                enableSkins = bool.Parse(XmlHelper.GetNodeValue(skinNode));
                if (skinNode.Attributes["repository"] != null && string.IsNullOrEmpty(skinNode.Attributes["repository"].Value) == false)
                    skinRepoGuid = skinNode.Attributes["repository"].Value;
            }

            //Create a new package instance to record all the installed package adds - this is the same format as the created packages has.
            //save the package meta data
            var insPack = InstalledPackage.MakeNew(packName);
            insPack.Data.Author = packAuthor;
            insPack.Data.AuthorUrl = packAuthorUrl;
            insPack.Data.Version = packVersion;
            insPack.Data.Readme = packReadme;
            insPack.Data.License = packLicense;
            insPack.Data.Url = packUrl;
            insPack.Data.IconUrl = iconUrl;

            insPack.Data.PackageGuid = guid; //the package unique key.
            insPack.Data.RepositoryGuid = repoGuid; //the repository unique key, if the package is a file install, the repository will not get logged.
            insPack.Save();

            return insPack.Data.Id;
        }

        public void InstallFiles(int packageId, string tempDir)
        {
            using (Current.ProfilingLogger.DebugDuration<Installer>(
                "Installing package files for package id " + packageId + " into temp folder " + tempDir,
                "Package file installation complete for package id " + packageId))
            {
                //retrieve the manifest to continue installation
                var insPack = InstalledPackage.GetById(packageId);

                //TODO: Depending on some files, some files should be installed differently.
                //i.e. if stylsheets should probably be installed via business logic, media items should probably use the media IFileSystem!

                // Move files
                //string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
                string basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

                try
                {
                    foreach (XmlNode n in Config.DocumentElement.SelectNodes("//file"))
                    {
                        var destPath = GetFileName(basePath, XmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                        var sourceFile = GetFileName(tempDir, XmlHelper.GetNodeValue(n.SelectSingleNode("guid")));
                        var destFile = GetFileName(destPath, XmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                        // Create the destination directory if it doesn't exist
                        if (Directory.Exists(destPath) == false)
                            Directory.CreateDirectory(destPath);
                        //If a file with this name exists, delete it
                        else if (File.Exists(destFile))
                            File.Delete(destFile);

                        // Copy the file
                        // SJ: Note - this used to do a move but some packages included the same file to be
                        // copied to multiple locations like so:
                        //
                        // <file>
                        //   <guid>my-icon.png</guid>
                        //   <orgPath>/umbraco/Images/</orgPath>
                        //   <orgName>my-icon.png</orgName>
                        // </file>
                        // <file>
                        //   <guid>my-icon.png</guid>
                        //   <orgPath>/App_Plugins/MyPlugin/Images</orgPath>
                        //   <orgName>my-icon.png</orgName>
                        // </file>
                        //
                        // Since this file unzips as a flat list of files, moving the file the first time means
                        // that when you try to do that a second time, it would result in a FileNotFoundException
                        File.Copy(sourceFile, destFile);

                        //PPH log file install
                        insPack.Data.Files.Add(XmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")) + "/" + XmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                    }

                    // Once we're done copying, remove all the files
                    foreach (XmlNode n in Config.DocumentElement.SelectNodes("//file"))
                    {
                        var sourceFile = GetFileName(tempDir, XmlHelper.GetNodeValue(n.SelectSingleNode("guid")));
                        if (File.Exists(sourceFile))
                            File.Delete(sourceFile);
                    }
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<Installer>(ex, "Package install error");
                    throw;
                }

                // log that a user has install files
                if (_currentUserId > -1)
                {
                    Current.Services.AuditService.Add(AuditType.PackagerInstall,
                        _currentUserId,
                        -1, "Package", string.Format("Package '{0}' installed. Package guid: {1}", insPack.Data.Name, insPack.Data.PackageGuid));
                }

                insPack.Save();
            }
        }

        public void InstallBusinessLogic(int packageId, string tempDir)
        {
            using (Current.ProfilingLogger.DebugDuration<Installer>(
                "Installing business logic for package id " + packageId + " into temp folder " + tempDir,
                "Package business logic installation complete for package id " + packageId))
            {
                InstalledPackage insPack;
                try
                {
                    //retrieve the manifest to continue installation
                    insPack = InstalledPackage.GetById(packageId);
                    //bool saveNeeded = false;

                    // Get current user, with a fallback
                    var currentUser = Current.Services.UserService.GetUserById(Constants.Security.SuperUserId);

                    //TODO: Get rid of this entire class! Until then all packages will be installed by the admin user


                    //Xml as XElement which is used with the new PackagingService
                    var rootElement = Config.DocumentElement.GetXElement();
                    var packagingService = Current.Services.PackagingService;

                    //Perhaps it would have been a good idea to put the following into methods eh?!?

                    #region DataTypes
                    var dataTypeElement = rootElement.Descendants("DataTypes").FirstOrDefault();
                    if (dataTypeElement != null)
                    {
                        var dataTypeDefinitions = packagingService.ImportDataTypeDefinitions(dataTypeElement, currentUser.Id);
                        foreach (var dataTypeDefinition in dataTypeDefinitions)
                        {
                            insPack.Data.DataTypes.Add(dataTypeDefinition.Id.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    #endregion

                    #region Languages
                    var languageItemsElement = rootElement.Descendants("Languages").FirstOrDefault();
                    if (languageItemsElement != null)
                    {
                        var insertedLanguages = packagingService.ImportLanguages(languageItemsElement);
                        insPack.Data.Languages.AddRange(insertedLanguages.Select(l => l.Id.ToString(CultureInfo.InvariantCulture)));
                    }

                    #endregion

                    #region Dictionary items
                    var dictionaryItemsElement = rootElement.Descendants("DictionaryItems").FirstOrDefault();
                    if (dictionaryItemsElement != null)
                    {
                        var insertedDictionaryItems = packagingService.ImportDictionaryItems(dictionaryItemsElement);
                        insPack.Data.DictionaryItems.AddRange(insertedDictionaryItems.Select(d => d.Id.ToString(CultureInfo.InvariantCulture)));
                    }
                    #endregion

                    #region Macros
                    var macroItemsElement = rootElement.Descendants("Macros").FirstOrDefault();
                    if (macroItemsElement != null)
                    {
                        var insertedMacros = packagingService.ImportMacros(macroItemsElement);
                        insPack.Data.Macros.AddRange(insertedMacros.Select(m => m.Id.ToString(CultureInfo.InvariantCulture)));
                    }
                    #endregion

                    #region Templates
                    var templateElement = rootElement.Descendants("Templates").FirstOrDefault();
                    if (templateElement != null)
                    {
                        var templates = packagingService.ImportTemplates(templateElement, currentUser.Id);
                        foreach (var template in templates)
                        {
                            insPack.Data.Templates.Add(template.Id.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    #endregion

                    #region DocumentTypes
                    //Check whether the root element is a doc type rather then a complete package
                    var docTypeElement = rootElement.Name.LocalName.Equals("DocumentType") ||
                                         rootElement.Name.LocalName.Equals("DocumentTypes")
                        ? rootElement
                        : rootElement.Descendants("DocumentTypes").FirstOrDefault();

                    if (docTypeElement != null)
                    {
                        var contentTypes = packagingService.ImportContentTypes(docTypeElement, currentUser.Id);
                        foreach (var contentType in contentTypes)
                        {
                            insPack.Data.Documenttypes.Add(contentType.Id.ToString(CultureInfo.InvariantCulture));
                            //saveNeeded = true;
                        }
                    }
                    #endregion

                    #region Stylesheets
                    foreach (XmlNode n in Config.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
                    {
                        //StyleSheet s = StyleSheet.Import(n, currentUser);


                        string stylesheetName = XmlHelper.GetNodeValue(n.SelectSingleNode("Name"));
                        //StyleSheet s = GetByName(stylesheetName);
                        var s = Current.Services.FileService.GetStylesheetByName(stylesheetName);
                        if (s == null)
                        {
                            s = new Stylesheet(XmlHelper.GetNodeValue(n.SelectSingleNode("FileName"))) { Content = XmlHelper.GetNodeValue(n.SelectSingleNode("Content")) };
                            Current.Services.FileService.SaveStylesheet(s);
                        }

                        foreach (XmlNode prop in n.SelectNodes("Properties/Property"))
                        {
                            string alias = XmlHelper.GetNodeValue(prop.SelectSingleNode("Alias"));
                            var sp = s.Properties.SingleOrDefault(p => p != null && p.Alias == alias);
                            string name = XmlHelper.GetNodeValue(prop.SelectSingleNode("Name"));
                            if (sp == null)
                            {
                                //sp = StylesheetProperty.MakeNew(
                                //    name,
                                //    s,
                                //    u);

                                sp = new StylesheetProperty(name, "#" + name.ToSafeAlias(), "");
                                s.AddProperty(sp);
                            }
                            else
                            {
                                //sp.Text = name;
                                //Changing the name requires removing the current property and then adding another new one
                                if (sp.Name != name)
                                {
                                    s.RemoveProperty(sp.Name);
                                    var newProp = new StylesheetProperty(name, sp.Alias, sp.Value);
                                    s.AddProperty(newProp);
                                    sp = newProp;
                                }
                            }
                            sp.Alias = alias;
                            sp.Value = XmlHelper.GetNodeValue(prop.SelectSingleNode("Value"));
                        }
                        //s.saveCssToFile();
                        Current.Services.FileService.SaveStylesheet(s);




                        insPack.Data.Stylesheets.Add(s.Id.ToString(CultureInfo.InvariantCulture));
                        //saveNeeded = true;
                    }

                    //if (saveNeeded) { insPack.Save(); saveNeeded = false; }
                    #endregion

                    #region Documents
                    var documentElement = rootElement.Descendants("DocumentSet").FirstOrDefault();
                    if (documentElement != null)
                    {
                        var content = packagingService.ImportContent(documentElement, -1, currentUser.Id);
                        var firstContentItem = content.First();
                        insPack.Data.ContentNodeId = firstContentItem.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    #endregion

                    #region Package Actions
                    foreach (XmlNode n in Config.DocumentElement.SelectNodes("Actions/Action"))
                    {
                        if (n.Attributes["undo"] == null || n.Attributes["undo"].Value == "true")
                        {
                            insPack.Data.Actions += n.OuterXml;
                        }

                        //Run the actions tagged only for 'install'

                        if (n.Attributes["runat"] != null && n.Attributes["runat"].Value == "install")
                        {
                            var alias = n.Attributes["alias"] != null ? n.Attributes["alias"].Value : "";

                            if (alias.IsNullOrWhiteSpace() == false)
                            {
                                PackageAction.RunPackageAction(insPack.Data.Name, alias, n);
                            }
                        }
                    }
                    #endregion

                    insPack.Save();
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<Installer>(ex, "Error installing businesslogic");
                    throw;
                }

                OnPackageBusinessLogicInstalled(insPack);
                OnPackageInstalled(insPack);
            }
        }

        /// <summary>
        /// Remove the temp installation folder
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="tempDir"></param>
        public void InstallCleanUp(int packageId, string tempDir)
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Reads the configuration of the package from the configuration xmldocument
        /// </summary>
        /// <param name="tempDir">The folder to which the contents of the package is extracted</param>
        public void LoadConfig(string tempDir)
        {
            Config = new XmlDocument();
            Config.Load(tempDir + Path.DirectorySeparatorChar + "package.xml");

            Name = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/name").FirstChild.Value;
            Version = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/version").FirstChild.Value;
            Url = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/url").FirstChild.Value;
            License = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/license").FirstChild.Value;
            LicenseUrl = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/license").Attributes.GetNamedItem("url").Value;

            RequirementsMajor = int.Parse(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/major").FirstChild.Value);
            RequirementsMinor = int.Parse(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/minor").FirstChild.Value);
            RequirementsPatch = int.Parse(Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/patch").FirstChild.Value);

            var reqNode = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements");
            RequirementsType = reqNode != null && reqNode.Attributes != null && reqNode.Attributes["type"] != null
                ? Enum<RequirementsType>.Parse(reqNode.Attributes["type"].Value, true)
                : RequirementsType.Legacy;
            var iconNode = Config.DocumentElement.SelectSingleNode("/umbPackage/info/package/iconUrl");
            if (iconNode != null && iconNode.FirstChild != null)
            {
                IconUrl = iconNode.FirstChild.Value;
            }

            Author = Config.DocumentElement.SelectSingleNode("/umbPackage/info/author/name").FirstChild.Value;
            AuthorUrl = Config.DocumentElement.SelectSingleNode("/umbPackage/info/author/website").FirstChild.Value;

            var basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            var dllBinFiles = new List<string>();

            foreach (XmlNode n in Config.DocumentElement.SelectNodes("//file"))
            {
                var badFile = false;
                var destPath = GetFileName(basePath, XmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                var orgName = XmlHelper.GetNodeValue(n.SelectSingleNode("orgName"));
                var destFile = GetFileName(destPath, orgName);

                if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code"))
                {
                    badFile = true;
                }

                if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin"))
                {
                    badFile = true;
                }

                if (destFile.ToLower().EndsWith(".dll"))
                {
                    badFile = true;
                    dllBinFiles.Add(Path.Combine(tempDir, orgName));
                }

                if (badFile)
                {
                    ContainsUnsecureFiles = true;
                    _unsecureFiles.Add(XmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));
                }
            }



            //this will check for existing macros with the same alias
            //since we will not overwrite on import it's a good idea to inform the user what will be overwritten
            foreach (XmlNode n in Config.DocumentElement.SelectNodes("//macro"))
            {
                var alias = n.SelectSingleNode("alias").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var m = Current.Services.MacroService.GetByAlias(alias);
                    if (m != null)
                    {
                        ContainsMacroConflict = true;
                        if (_conflictingMacroAliases.ContainsKey(m.Name) == false)
                        {
                            _conflictingMacroAliases.Add(m.Name, alias);
                        }
                    }
                }
            }

            foreach (XmlNode n in Config.DocumentElement.SelectNodes("Templates/Template"))
            {
                var alias = n.SelectSingleNode("Alias").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var t = Current.Services.FileService.GetTemplate(alias);
                    if (t != null)
                    {
                        ContainsTemplateConflicts = true;
                        if (_conflictingTemplateAliases.ContainsKey(t.Alias) == false)
                        {
                            _conflictingTemplateAliases.Add(t.Alias, alias);
                        }
                    }
                }
            }

            foreach (XmlNode n in Config.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
            {
                var alias = n.SelectSingleNode("Name").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var s = Current.Services.FileService.GetStylesheetByName(alias);
                    if (s != null)
                    {
                        ContainsStyleSheeConflicts = true;
                        if (_conflictingStyleSheetNames.ContainsKey(s.Alias) == false)
                        {
                            _conflictingStyleSheetNames.Add(s.Alias, alias);
                        }
                    }
                }
            }

            var readmeNode = Config.DocumentElement.SelectSingleNode("/umbPackage/info/readme");
            if (readmeNode != null)
            {
                ReadMe = XmlHelper.GetNodeValue(readmeNode);
            }

            var controlNode = Config.DocumentElement.SelectSingleNode("/umbPackage/control");
            if (controlNode != null)
            {
                Control = XmlHelper.GetNodeValue(controlNode);
            }
        }

        /// <summary>
        /// This uses the old method of fetching and only supports the packages.umbraco.org repository.
        /// </summary>
        /// <param name="Package"></param>
        /// <returns></returns>
        public string Fetch(Guid Package)
        {
            // Check for package directory
            if (Directory.Exists(IOHelper.MapPath(SystemDirectories.Packages)) == false)
                Directory.CreateDirectory(IOHelper.MapPath(SystemDirectories.Packages));

            if (_webClient == null)
                _webClient = new WebClient();

            _webClient.DownloadFile(
                "http://" + PackageServer + "/fetch?package=" + Package.ToString(),
                IOHelper.MapPath(SystemDirectories.Packages + "/" + Package + ".umb"));

            return "packages\\" + Package + ".umb";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the name of the file in the specified path.
        /// Corrects possible problems with slashes that would result from a simple concatenation.
        /// Can also be used to concatenate paths.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The name of the file in the specified path.</returns>
        private static string GetFileName(string path, string fileName)
        {
            // virtual dir support
            fileName = IOHelper.FindFile(fileName);

            if (path.Contains("[$"))
            {
                //this is experimental and undocumented...
                path = path.Replace("[$UMBRACO]", SystemDirectories.Umbraco);
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
                if (!fileNameStartsWithSlash)
                    // No double slash, just concatenate
                    return path + fileName;
                return path + fileName.Substring(1);
            }
            if (fileNameStartsWithSlash)
                // Required slash specified, just concatenate
                return path + fileName;
            return path + Path.DirectorySeparatorChar + fileName;
        }

        private static string UnPack(string zipName, bool deleteFile)
        {
            // Unzip

            //the temp directory will be the package GUID - this keeps it consistent!
            //the zipName is always the package Guid.umb

            var packageFileName = Path.GetFileNameWithoutExtension(zipName);
            var packageId = Guid.NewGuid();
            Guid.TryParse(packageFileName, out packageId);

            string tempDir = IOHelper.MapPath(SystemDirectories.Data) + Path.DirectorySeparatorChar + packageId.ToString();
            //clear the directory if it exists
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            var s = new ZipInputStream(File.OpenRead(zipName));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string fileName = Path.GetFileName(theEntry.Name);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(tempDir + Path.DirectorySeparatorChar + fileName);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();

                }
            }

            // Clean up
            s.Close();

            if (deleteFile)
            {
                File.Delete(zipName);
            }


            return tempDir;

        }

        #endregion

        internal static event EventHandler<InstalledPackage> PackageBusinessLogicInstalled;

        private static void OnPackageBusinessLogicInstalled(InstalledPackage e)
        {
            EventHandler<InstalledPackage> handler = PackageBusinessLogicInstalled;
            if (handler != null) handler(null, e);
        }

        private void OnPackageInstalled(InstalledPackage insPack)
        {
            // getting an InstallationSummary for sending to the PackagingService.ImportedPackage event
            var fileService = Current.Services.FileService;
            var macroService = Current.Services.MacroService;
            var contentTypeService = Current.Services.ContentTypeService;
            var dataTypeService = Current.Services.DataTypeService;
            var localizationService = Current.Services.LocalizationService;

            var installationSummary = insPack.GetInstallationSummary(contentTypeService, dataTypeService, fileService, localizationService, macroService);
            installationSummary.PackageInstalled = true;

            var args = new ImportPackageEventArgs<InstallationSummary>(installationSummary, false);
            PackagingService.OnImportedPackage(args);
        }
    }
}
