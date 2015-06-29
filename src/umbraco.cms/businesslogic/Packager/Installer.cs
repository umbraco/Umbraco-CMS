using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core;
using Umbraco.Core.Auditing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Packaging;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.propertytype;
using umbraco.BusinessLogic;
using System.Diagnostics;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.template;
using umbraco.interfaces;

namespace umbraco.cms.businesslogic.packager
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

        /// <summary>
        /// Indicates that the package contains legacy property editors
        /// </summary>
        public bool ContainsLegacyPropertyEditors { get; private set; }

        public bool ContainsStyleSheeConflicts { get; private set; }
        public IDictionary<string, string> ConflictingStyleSheetNames { get { return _conflictingStyleSheetNames; } }

        public int RequirementsMajor { get; private set; }
        public int RequirementsMinor { get; private set; }
        public int RequirementsPatch { get; private set; }

        /// <summary>
        /// The xmldocument, describing the contents of a package.
        /// </summary>
        public XmlDocument Config { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Installer()
        {
            initialize();
        }

        public Installer(int currentUserId)
        {
            initialize();
            _currentUserId = currentUserId;
        }

        private void initialize()
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
        /// <param name="Name">The name of the package</param>
        /// <param name="Version">The version of the package</param>
        /// <param name="Url">The url to a descriptionpage</param>
        /// <param name="License">The license under which the package is released (preferably GPL ;))</param>
        /// <param name="LicenseUrl">The url to a licensedescription</param>
        /// <param name="Author">The original author of the package</param>
        /// <param name="AuthorUrl">The url to the Authors website</param>
        /// <param name="RequirementsMajor">Umbraco version major</param>
        /// <param name="RequirementsMinor">Umbraco version minor</param>
        /// <param name="RequirementsPatch">Umbraco version patch</param>
        /// <param name="Readme">The readme text</param>
        /// <param name="Control">The name of the usercontrol used to configure the package after install</param>
        public Installer(string Name, string Version, string Url, string License, string LicenseUrl, string Author, string AuthorUrl, int RequirementsMajor, int RequirementsMinor, int RequirementsPatch, string Readme, string Control)
        {
            ContainsBinaryFileErrors = false;
            ContainsTemplateConflicts = false;
            ContainsUnsecureFiles = false;
            ContainsMacroConflict = false;
            ContainsStyleSheeConflicts = false;
            this.Name = Name;
            this.Version = Version;
            this.Url = Url;
            this.License = License;
            this.LicenseUrl = LicenseUrl;
            this.RequirementsMajor = RequirementsMajor;
            this.RequirementsMinor = RequirementsMinor;
            this.RequirementsPatch = RequirementsPatch;
            this.Author = Author;
            this.AuthorUrl = AuthorUrl;
            ReadMe = Readme;
            this.Control = Control;
        }

        #region Public Methods

        /// <summary>
        /// Imports the specified package
        /// </summary>
        /// <param name="InputFile">Filename of the umbracopackage</param>
        /// <returns></returns>
        public string Import(string InputFile)
        {
            using (DisposableTimer.DebugDuration<Installer>(
                () => "Importing package file " + InputFile,
                () => "Package file " + InputFile + "imported"))
            {
                var tempDir = "";
                if (File.Exists(IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + InputFile)))
                {
                    var fi = new FileInfo(IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + InputFile));
                    // Check if the file is a valid package
                    if (fi.Extension.ToLower() == ".umb")
                    {
                        try
                        {
                            tempDir = UnPack(fi.FullName);
                            LoadConfig(tempDir);
                        }
                        catch (Exception unpackE)
                        {
                            throw new Exception("Error unpacking extension...", unpackE);
                        }
                    }
                    else
                        throw new Exception("Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
                }
                else
                    throw new Exception("Error - file not found. Could find file named '" + IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + InputFile) + "'");
                return tempDir;
            }
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

            //skinning
            insPack.Data.EnableSkins = enableSkins;
            insPack.Data.SkinRepoGuid = string.IsNullOrEmpty(skinRepoGuid) ? Guid.Empty : new Guid(skinRepoGuid);

            insPack.Data.PackageGuid = guid; //the package unique key.
            insPack.Data.RepositoryGuid = repoGuid; //the repository unique key, if the package is a file install, the repository will not get logged.
            insPack.Save();

            return insPack.Data.Id;
        }

        public void InstallFiles(int packageId, string tempDir)
        {
            using (DisposableTimer.DebugDuration<Installer>(
                () => "Installing package files for package id " + packageId + " into temp folder " + tempDir,
                () => "Package file installation complete for package id " + packageId))
            {
                //retrieve the manifest to continue installation
                var insPack = InstalledPackage.GetById(packageId);

                //TODO: Depending on some files, some files should be installed differently.
                //i.e. if stylsheets should probably be installed via business logic, media items should probably use the media IFileSystem!

                // Move files
                //string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
                string basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

                foreach (XmlNode n in Config.DocumentElement.SelectNodes("//file"))
                {
                    //we enclose the whole file-moving to ensure that the entire installer doesn't crash
                    try
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

                        // Move the file
                        File.Move(sourceFile, destFile);

                        //PPH log file install
                        insPack.Data.Files.Add(XmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")) + "/" + XmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<Installer>("Package install error", ex);
                    }
                }

                // log that a user has install files
                if (_currentUserId > -1)
                {
                    Audit.Add(AuditTypes.PackagerInstall,
                                            string.Format("Package '{0}' installed. Package guid: {1}", insPack.Data.Name, insPack.Data.PackageGuid),
                                            _currentUserId, -1);
                }

                insPack.Save();

                
            }
        }

        public void InstallBusinessLogic(int packageId, string tempDir)
        {
            using (DisposableTimer.DebugDuration<Installer>(
                () => "Installing business logic for package id " + packageId + " into temp folder " + tempDir,
                () => "Package business logic installation complete for package id " + packageId))
            {
                //retrieve the manifest to continue installation
                var insPack = InstalledPackage.GetById(packageId);
                //bool saveNeeded = false;

                // Get current user, with a fallback
                var currentUser = new User(0);
                if (string.IsNullOrEmpty(BasePages.UmbracoEnsuredPage.umbracoUserContextID) == false)
                {
                    if (BasePages.UmbracoEnsuredPage.ValidateUserContextID(BasePages.UmbracoEnsuredPage.umbracoUserContextID))
                    {
                        currentUser = User.GetCurrent();
                    }
                }

                //Xml as XElement which is used with the new PackagingService
                var rootElement = Config.DocumentElement.GetXElement();
                var packagingService = ApplicationContext.Current.Services.PackagingService;

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
                    insPack.Data.Languages.AddRange(insertedLanguages.Select(l => l.Id.ToString()));
                }

                #endregion

                #region Dictionary items
                var dictionaryItemsElement = rootElement.Descendants("DictionaryItems").FirstOrDefault();
                if (dictionaryItemsElement != null)
                {
                    var insertedDictionaryItems = packagingService.ImportDictionaryItems(dictionaryItemsElement);
                    insPack.Data.DictionaryItems.AddRange(insertedDictionaryItems.Select(d => d.Id.ToString()));
                }
                #endregion

                #region Macros
                foreach (XmlNode n in Config.DocumentElement.SelectNodes("//macro"))
                {
                    //TODO: Fix this, this should not use the legacy API
                    Macro m = Macro.Import(n);

                    if (m != null)
                    {
                        insPack.Data.Macros.Add(m.Id.ToString(CultureInfo.InvariantCulture));
                        //saveNeeded = true;
                    }
                }

                //if (saveNeeded) { insPack.Save(); saveNeeded = false; }
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
                    StyleSheet s = StyleSheet.Import(n, currentUser);

                    insPack.Data.Stylesheets.Add(s.Id.ToString());
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

                // Trigger update of Apps / Trees config.
                // (These are ApplicationStartupHandlers so just instantiating them will trigger them)
                new ApplicationRegistrar();
                new ApplicationTreeRegistrar();

                insPack.Save();

                OnPackageBusinessLogicInstalled(insPack);
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

            if (ContainsUnsecureFiles)
            {
                //Now we want to see if the DLLs contain any legacy data types since we want to warn people about that
                string[] assemblyErrors;
                var assembliesWithReferences = PackageBinaryInspector.ScanAssembliesForTypeReference<IDataType>(tempDir, out assemblyErrors).ToArray();
                if (assemblyErrors.Any())
                {
                    ContainsBinaryFileErrors = true;
                    BinaryFileErrors.AddRange(assemblyErrors);
                }
                if (assembliesWithReferences.Any())
                {
                    ContainsLegacyPropertyEditors = true;
                }
            }

            //this will check for existing macros with the same alias
            //since we will not overwrite on import it's a good idea to inform the user what will be overwritten
            foreach (XmlNode n in Config.DocumentElement.SelectNodes("//macro"))
            {
                var alias = n.SelectSingleNode("alias").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var m = ApplicationContext.Current.Services.MacroService.GetByAlias(alias);
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
                    var t = Template.GetByAlias(alias);
                    if (t != null)
                    {
                        ContainsTemplateConflicts = true;
                        if (_conflictingTemplateAliases.ContainsKey(t.Text) == false)
                        {
                            _conflictingTemplateAliases.Add(t.Text, alias);
                        }
                    }
                }
            }

            foreach (XmlNode n in Config.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
            {
                var alias = n.SelectSingleNode("Name").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var s = StyleSheet.GetByName(alias);
                    if (s != null)
                    {
                        ContainsStyleSheeConflicts = true;
                        if (_conflictingStyleSheetNames.ContainsKey(s.Text) == false)
                        {
                            _conflictingStyleSheetNames.Add(s.Text, alias);
                        }
                    }
                }
            }

            try
            {
                ReadMe = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            }
            catch { }

            try
            {
                Control = XmlHelper.GetNodeValue(Config.DocumentElement.SelectSingleNode("/umbPackage/control"));
            }
            catch { }
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

            var wc = new System.Net.WebClient();

            wc.DownloadFile(
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
        private static string UnPack(string zipName)
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
            File.Delete(zipName);

            return tempDir;

        }

        #endregion

        internal static event EventHandler<InstalledPackage> PackageBusinessLogicInstalled;

        private static void OnPackageBusinessLogicInstalled(InstalledPackage e)
        {
            EventHandler<InstalledPackage> handler = PackageBusinessLogicInstalled;
            if (handler != null) handler(null, e);
        }
    }
}
