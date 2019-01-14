//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Xml;
//using System.Xml.Linq;
//using System.Xml.XPath;
//using ICSharpCode.SharpZipLib.Zip;
//using Umbraco.Core;
//using Umbraco.Core.Composing;
//using Umbraco.Core.Events;
//using Umbraco.Core.IO;
//using Umbraco.Core.Logging;
//using Umbraco.Core.Models;
//using Umbraco.Core.Models.Packaging;
//using Umbraco.Core.Packaging;
//using Umbraco.Core.Services.Implement;
//using File = System.IO.File;

//namespace Umbraco.Web._Legacy.Packager
//{
//    /// <summary>
//    /// The packager is a component which enables sharing of both data and functionality components between different umbraco installations.
//    ///
//    /// The output is a .umb (a zip compressed file) which contains the exported documents/medias/macroes/documentTypes (etc.)
//    /// in a Xml document, along with the physical files used (images/usercontrols/xsl documents etc.)
//    ///
//    /// Partly implemented, import of packages is done, the export is *under construction*.
//    /// </summary>
//    /// <remarks>
//    /// Ruben Verborgh 31/12/2007: I had to change some code, I marked my changes with "DATALAYER".
//    /// Reason: @@IDENTITY can't be used with the new datalayer.
//    /// I wasn't able to test the code, since I'm not aware how the code functions.
//    /// </remarks>
//    public class Installer
//    {
//        private const string PackageServer = "packages.umbraco.org";

//        private readonly Dictionary<string, string> _conflictingMacroAliases = new Dictionary<string, string>();
//        private readonly Dictionary<string, string> _conflictingTemplateAliases = new Dictionary<string, string>();
//        private readonly Dictionary<string, string> _conflictingStyleSheetNames = new Dictionary<string, string>();

//        private readonly int _currentUserId = -1;
//        private static WebClient _webClient;


//        public string Name { get; private set; }
//        public string Version { get; private set; }
//        public string Url { get; private set; }
//        public string License { get; private set; }
//        public string LicenseUrl { get; private set; }
//        public string Author { get; private set; }
//        public string AuthorUrl { get; private set; }
//        public string ReadMe { get; private set; }
//        public string Control { get; private set; }

//        public bool ContainsMacroConflict { get; private set; }
//        public IDictionary<string, string> ConflictingMacroAliases => _conflictingMacroAliases;

//        public bool ContainsUnsecureFiles { get; private set; }
//        public List<string> UnsecureFiles { get; } = new List<string>();

//        public bool ContainsTemplateConflicts { get; private set; }
//        public IDictionary<string, string> ConflictingTemplateAliases => _conflictingTemplateAliases;

//        public bool ContainsStyleSheeConflicts { get; private set; }
//        public IDictionary<string, string> ConflictingStyleSheetNames => _conflictingStyleSheetNames;

//        public int RequirementsMajor { get; private set; }
//        public int RequirementsMinor { get; private set; }
//        public int RequirementsPatch { get; private set; }

//        public RequirementsType RequirementsType { get; private set; }

//        public string IconUrl { get; private set; }

//        /// <summary>
//        /// The xml of the compiled package
//        /// </summary>
//        public XDocument Config { get; private set; }

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        public Installer()
//        {
//            Initialize();
//        }

//        public Installer(int currentUserId)
//        {
//            Initialize();
//            _currentUserId = currentUserId;
//        }

//        private void Initialize()
//        {
//            ContainsTemplateConflicts = false;
//            ContainsUnsecureFiles = false;
//            ContainsMacroConflict = false;
//            ContainsStyleSheeConflicts = false;
//        }



//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="name">The name of the package</param>
//        /// <param name="version">The version of the package</param>
//        /// <param name="url">The url to a descriptionpage</param>
//        /// <param name="license">The license under which the package is released (preferably GPL ;))</param>
//        /// <param name="licenseUrl">The url to a licensedescription</param>
//        /// <param name="author">The original author of the package</param>
//        /// <param name="authorUrl">The url to the Authors website</param>
//        /// <param name="requirementsMajor">Umbraco version major</param>
//        /// <param name="requirementsMinor">Umbraco version minor</param>
//        /// <param name="requirementsPatch">Umbraco version patch</param>
//        /// <param name="readme">The readme text</param>
//        /// <param name="control">The name of the usercontrol used to configure the package after install</param>
//        /// <param name="requirementsType"></param>
//        /// <param name="iconUrl"></param>
//        public Installer(string name, string version, string url, string license, string licenseUrl, string author, string authorUrl, int requirementsMajor, int requirementsMinor, int requirementsPatch, string readme, string control, RequirementsType requirementsType, string iconUrl)
//        {
//            ContainsTemplateConflicts = false;
//            ContainsUnsecureFiles = false;
//            ContainsMacroConflict = false;
//            ContainsStyleSheeConflicts = false;
//            this.Name = name;
//            this.Version = version;
//            this.Url = url;
//            this.License = license;
//            this.LicenseUrl = licenseUrl;
//            this.RequirementsMajor = requirementsMajor;
//            this.RequirementsMinor = requirementsMinor;
//            this.RequirementsPatch = requirementsPatch;
//            this.RequirementsType = requirementsType;
//            this.Author = author;
//            this.AuthorUrl = authorUrl;
//            this.IconUrl = iconUrl;
//            ReadMe = readme;
//            this.Control = control;
//        }

//        #region Public Methods

//        /// <summary>
//        /// Imports the specified package
//        /// </summary>
//        /// <param name="inputFile">Filename of the umbracopackage</param>
//        /// <param name="deleteFile">true if the input file should be deleted after import</param>
//        /// <returns></returns>
//        public string Import(string inputFile, bool deleteFile)
//        {
//            using (Current.ProfilingLogger.DebugDuration<Installer>(
//                $"Importing package file {inputFile}.",
//                $"Package file {inputFile} imported."))
//            {
//                var tempDir = "";
//                if (File.Exists(IOHelper.MapPath(SystemDirectories.Data + "/" + inputFile)))
//                {
//                    var fi = new FileInfo(IOHelper.MapPath(SystemDirectories.Data + "/" + inputFile));
//                    // Check if the file is a valid package
//                    if (fi.Extension.ToLower() == ".umb")
//                    {
//                        try
//                        {
//                            tempDir = UnPack(fi.FullName, deleteFile);
//                            LoadConfig(tempDir);
//                        }
//                        catch (Exception ex)
//                        {
//                            Current.Logger.Error<Installer>(ex, "Error importing file {FileName}", fi.FullName);
//                            throw;
//                        }
//                    }
//                    else
//                        throw new Exception("Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
//                }
//                else
//                    throw new Exception("Error - file not found. Could find file named '" + IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + inputFile) + "'");
//                return tempDir;
//            }

//        }

//        /// <summary>
//        /// Imports the specified package
//        /// </summary>
//        /// <param name="inputFile">Filename of the umbracopackage</param>
//        /// <returns></returns>
//        public string Import(string inputFile)
//        {
//            return Import(inputFile, true);
//        }

//        public int CreateManifest(Guid guid)
//        {
//            //This is the new improved install rutine, which chops up the process into 3 steps, creating the manifest, moving files, and finally handling umb objects

//            var parser = new CompiledPackageXmlParser();
//            var def = parser.ToCompiledPackage(Config);

//            //create a new entry in the installedPackages.config
//            var installedPackage = new PackageDefinition
//            {
//                Author = def.Author,
//                AuthorUrl = def.AuthorUrl,
//                Control = def.Control,
//                IconUrl = def.IconUrl,
//                License = def.License,
//                LicenseUrl = def.LicenseUrl,
//                Name = def.Name,
//                Readme = def.Readme,
//                UmbracoVersion = def.UmbracoVersion,
//                Url = def.Url,
//                Version = def.Version,
//                PackageId = guid
//            };

//            if (!Current.Services.PackagingService.SaveInstalledPackage(installedPackage))
//                throw new InvalidOperationException("Could not save package definition");

//            return installedPackage.Id;
//        }

//        public void InstallFiles(int packageId, string tempDir)
//        {
//            var parser = new CompiledPackageXmlParser();

//            using (Current.ProfilingLogger.DebugDuration<Installer>(
//                "Installing package files for package id " + packageId + " into temp folder " + tempDir,
//                "Package file installation complete for package id " + packageId))
//            {
//                //retrieve the manifest to continue installation
//                var insPack = Current.Services.PackagingService.GetInstalledPackageById(packageId);

//                //TODO: Depending on some files, some files should be installed differently.
//                //i.e. if stylsheets should probably be installed via business logic, media items should probably use the media IFileSystem!

//                // Move files
//                //string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
//                string basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

//                var def = parser.ToCompiledPackage(Config);

//                try
//                {
//                    foreach (var f in def.Files)
//                    {
//                        var destPath = GetFileName(basePath, f.OriginalPath);
//                        var sourceFile = GetFileName(tempDir, f.UniqueFileName);
//                        var destFile = GetFileName(destPath, f.OriginalName);

//                        // Create the destination directory if it doesn't exist
//                        if (Directory.Exists(destPath) == false)
//                            Directory.CreateDirectory(destPath);
//                        //If a file with this name exists, delete it
//                        else if (File.Exists(destFile))
//                            File.Delete(destFile);

//                        // Copy the file
//                        // SJ: Note - this used to do a move but some packages included the same file to be
//                        // copied to multiple locations like so:
//                        //
//                        // <file>
//                        //   <guid>my-icon.png</guid>
//                        //   <orgPath>/umbraco/Images/</orgPath>
//                        //   <orgName>my-icon.png</orgName>
//                        // </file>
//                        // <file>
//                        //   <guid>my-icon.png</guid>
//                        //   <orgPath>/App_Plugins/MyPlugin/Images</orgPath>
//                        //   <orgName>my-icon.png</orgName>
//                        // </file>
//                        //
//                        // Since this file unzips as a flat list of files, moving the file the first time means
//                        // that when you try to do that a second time, it would result in a FileNotFoundException
//                        File.Copy(sourceFile, destFile);

//                        //PPH log file install
//                        insPack.Files.Add(f.OriginalPath.EnsureEndsWith('/') + f.OriginalName);

//                    }

//                    // Once we're done copying, remove all the files
//                    foreach (var f in def.Files)
//                    {
//                        var sourceFile = GetFileName(tempDir, f.UniqueFileName);
//                        if (File.Exists(sourceFile))
//                            File.Delete(sourceFile);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Current.Logger.Error<Installer>(ex, "Package install error");
//                    throw;
//                }

//                // log that a user has install files
//                if (_currentUserId > -1)
//                {
//                    Current.Services.AuditService.Add(AuditType.PackagerInstall,
//                        _currentUserId,
//                        -1, "Package", string.Format("Package '{0}' installed. Package guid: {1}", insPack.Name, insPack.PackageId));
//                }

//                Current.Services.PackagingService.SaveInstalledPackage(insPack);
//            }
//        }

//        public void InstallBusinessLogic(int packageId, string tempDir)
//        {
//            using (Current.ProfilingLogger.DebugDuration<Installer>(
//                "Installing business logic for package id " + packageId + " into temp folder " + tempDir,
//                "Package business logic installation complete for package id " + packageId))
//            {
//                PackageDefinition insPack;
//                try
//                {
//                    //retrieve the manifest to continue installation
//                    insPack = Current.Services.PackagingService.GetInstalledPackageById(packageId);
//                    //bool saveNeeded = false;

//                    // Get current user, with a fallback
//                    var currentUser = Current.Services.UserService.GetUserById(Constants.Security.SuperUserId);

//                    //TODO: Get rid of this entire class! Until then all packages will be installed by the admin user

//                    var rootElement = Config.Root;
//                    var packagingService = Current.Services.PackagingService;

//                    //Perhaps it would have been a good idea to put the following into methods eh?!?

//                    #region DataTypes
//                    var dataTypeElement = rootElement.Descendants("DataTypes").FirstOrDefault();
//                    if (dataTypeElement != null)
//                    {
//                        var dataTypeDefinitions = packagingService.ImportDataTypeDefinitions(dataTypeElement, currentUser.Id);
//                        foreach (var dataTypeDefinition in dataTypeDefinitions)
//                        {
//                            insPack.DataTypes.Add(dataTypeDefinition.Id.ToString(CultureInfo.InvariantCulture));
//                        }
//                    }
//                    #endregion

//                    #region Languages
//                    var languageItemsElement = rootElement.Descendants("Languages").FirstOrDefault();
//                    if (languageItemsElement != null)
//                    {
//                        var insertedLanguages = packagingService.ImportLanguages(languageItemsElement);
//                        foreach(var x in insertedLanguages.Select(l => l.Id.ToString(CultureInfo.InvariantCulture)))
//                            insPack.Languages.Add(x);
//                    }

//                    #endregion

//                    #region Dictionary items
//                    var dictionaryItemsElement = rootElement.Descendants("DictionaryItems").FirstOrDefault();
//                    if (dictionaryItemsElement != null)
//                    {
//                        var insertedDictionaryItems = packagingService.ImportDictionaryItems(dictionaryItemsElement);
//                        foreach (var x in insertedDictionaryItems.Select(d => d.Id.ToString(CultureInfo.InvariantCulture)))
//                            insPack.DictionaryItems.Add(x);
//                    }
//                    #endregion

//                    #region Macros
//                    var macroItemsElement = rootElement.Descendants("Macros").FirstOrDefault();
//                    if (macroItemsElement != null)
//                    {
//                        var insertedMacros = packagingService.ImportMacros(macroItemsElement);
//                        foreach (var x in insertedMacros.Select(m => m.Id.ToString(CultureInfo.InvariantCulture)))
//                            insPack.Macros.Add(x);

//                    }
//                    #endregion

//                    #region Templates
//                    var templateElement = rootElement.Descendants("Templates").FirstOrDefault();
//                    if (templateElement != null)
//                    {
//                        var templates = packagingService.ImportTemplates(templateElement, currentUser.Id);
//                        foreach (var template in templates)
//                        {
//                            insPack.Templates.Add(template.Id.ToString(CultureInfo.InvariantCulture));
//                        }
//                    }
//                    #endregion

//                    #region DocumentTypes
//                    //Check whether the root element is a doc type rather then a complete package
//                    var docTypeElement = rootElement.Name.LocalName.Equals("DocumentType") ||
//                                         rootElement.Name.LocalName.Equals("DocumentTypes")
//                        ? rootElement
//                        : rootElement.Descendants("DocumentTypes").FirstOrDefault();

//                    if (docTypeElement != null)
//                    {
//                        var contentTypes = packagingService.ImportContentTypes(docTypeElement, currentUser.Id);
//                        foreach (var contentType in contentTypes)
//                        {
//                            insPack.DocumentTypes.Add(contentType.Id.ToString(CultureInfo.InvariantCulture));
//                            //saveNeeded = true;
//                        }
//                    }
//                    #endregion

//                    #region Stylesheets
//                    foreach (var n in Config.Root.XPathSelectElements("Stylesheets/Stylesheet"))
//                    {
//                        string stylesheetName = n.Element("Name")?.Value;
//                        if (stylesheetName.IsNullOrWhiteSpace()) continue;

//                        var s = Current.Services.FileService.GetStylesheetByName(stylesheetName);
//                        if (s == null)
//                        {
//                            var fileName = n.Element("FileName")?.Value;
//                            if (fileName == null) continue;
//                            var content = n.Element("Content")?.Value;
//                            if (content == null) continue;

//                            s = new Stylesheet(fileName) { Content = content };
//                            Current.Services.FileService.SaveStylesheet(s);
//                        }

//                        foreach (var prop in n.XPathSelectElements("Properties/Property"))
//                        {
//                            string alias = prop.Element("Alias")?.Value;
//                            var sp = s.Properties.SingleOrDefault(p => p != null && p.Alias == alias);
//                            string name = prop.Element("Name")?.Value;
//                            if (sp == null)
//                            {
//                                //sp = StylesheetProperty.MakeNew(
//                                //    name,
//                                //    s,
//                                //    u);

//                                sp = new StylesheetProperty(name, "#" + name.ToSafeAlias(), "");
//                                s.AddProperty(sp);
//                            }
//                            else
//                            {
//                                //sp.Text = name;
//                                //Changing the name requires removing the current property and then adding another new one
//                                if (sp.Name != name)
//                                {
//                                    s.RemoveProperty(sp.Name);
//                                    var newProp = new StylesheetProperty(name, sp.Alias, sp.Value);
//                                    s.AddProperty(newProp);
//                                    sp = newProp;
//                                }
//                            }
//                            sp.Alias = alias;
//                            sp.Value = prop.Element("Value")?.Value;
//                        }
//                        //s.saveCssToFile();
//                        Current.Services.FileService.SaveStylesheet(s);




//                        insPack.Stylesheets.Add(s.Id.ToString(CultureInfo.InvariantCulture));
//                        //saveNeeded = true;
//                    }

//                    //if (saveNeeded) { insPack.Save(); saveNeeded = false; }
//                    #endregion

//                    #region Documents
//                    var documentElement = rootElement.Descendants("DocumentSet").FirstOrDefault();
//                    if (documentElement != null)
//                    {
//                        var content = packagingService.ImportContent(documentElement, -1, currentUser.Id);
//                        var firstContentItem = content.First();
//                        insPack.ContentNodeId = firstContentItem.Id.ToString(CultureInfo.InvariantCulture);
//                    }
//                    #endregion

//                    #region Package Actions
//                    foreach (var n in Config.Root.XPathSelectElements("Actions/Action"))
//                    {
//                        var undo = n.AttributeValue<string>("undo");
//                        if (undo == null || undo == "true")
//                        {
//                            insPack.Actions += n.ToString();
//                        }

//                        //Run the actions tagged only for 'install'
//                        var runat = n.AttributeValue<string>("runat");

//                        if (runat != null && runat == "install")
//                        {
//                            var alias = n.AttributeValue<string>("alias");
//                            if (alias.IsNullOrWhiteSpace() == false)
//                            {
//                                Current.PackageActionRunner.RunPackageAction(insPack.Name, alias, n);
//                            }
//                        }
//                    }
//                    #endregion

//                    Current.Services.PackagingService.SaveInstalledPackage(insPack);
//                }
//                catch (Exception ex)
//                {
//                    Current.Logger.Error<Installer>(ex, "Error installing businesslogic");
//                    throw;
//                }

//                OnPackageInstalled(insPack);
//            }
//        }

//        /// <summary>
//        /// Remove the temp installation folder
//        /// </summary>
//        /// <param name="packageId"></param>
//        /// <param name="tempDir"></param>
//        public void InstallCleanUp(int packageId, string tempDir)
//        {
//            if (Directory.Exists(tempDir))
//            {
//                Directory.Delete(tempDir, true);
//            }
//        }

//        /// <summary>
//        /// Reads the configuration of the package from the configuration xmldocument
//        /// </summary>
//        /// <param name="tempDir">The folder to which the contents of the package is extracted</param>
//        public void LoadConfig(string tempDir)
//        {
//            Config = XDocument.Load(tempDir + Path.DirectorySeparatorChar + "package.xml");

//            var parser = new CompiledPackageXmlParser();
//            var def = parser.ToCompiledPackage(Config);

//            Name = def.Name;
//            Version = def.Version;
//            Url = def.Url;
//            License = def.License;
//            LicenseUrl = def.LicenseUrl;

//            RequirementsMajor = def.UmbracoVersion.Major;
//            RequirementsMinor = def.UmbracoVersion.Minor;
//            RequirementsPatch = def.UmbracoVersion.Build;
//            RequirementsType = def.UmbracoVersionRequirementsType;
//            IconUrl = def.IconUrl;
//            Author = def.Author;
//            AuthorUrl = def.AuthorUrl;
//            ReadMe = def.Readme;
//            Control = def.Control;

//            var basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
//            var dllBinFiles = new List<string>();

//            foreach (var f in def.Files)
//            {
//                var badFile = false;
//                var destPath = GetFileName(basePath, f.OriginalPath);
//                var orgName = f.OriginalName;
//                var destFile = GetFileName(destPath, orgName);

//                if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code"))
//                {
//                    badFile = true;
//                }

//                if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin"))
//                {
//                    badFile = true;
//                }

//                if (destFile.ToLower().EndsWith(".dll"))
//                {
//                    badFile = true;
//                    dllBinFiles.Add(Path.Combine(tempDir, orgName));
//                }

//                if (badFile)
//                {
//                    ContainsUnsecureFiles = true;
//                    UnsecureFiles.Add(f.OriginalName);
//                }
//            }



//            //this will check for existing macros with the same alias
//            //since we will not overwrite on import it's a good idea to inform the user what will be overwritten
//            foreach (var n in Config.Root.XPathSelectElements("//macro"))
//            {
//                var alias = n.Element("alias")?.Value;
//                if (!string.IsNullOrEmpty(alias))
//                {
//                    var m = Current.Services.MacroService.GetByAlias(alias);
//                    if (m != null)
//                    {
//                        ContainsMacroConflict = true;
//                        if (_conflictingMacroAliases.ContainsKey(m.Name) == false)
//                        {
//                            _conflictingMacroAliases.Add(m.Name, alias);
//                        }
//                    }
//                }
//            }

//            foreach (var n in Config.Root.XPathSelectElements("Templates/Template"))
//            {
//                var alias = n.Element("Alias")?.Value;
//                if (!string.IsNullOrEmpty(alias))
//                {
//                    var t = Current.Services.FileService.GetTemplate(alias);
//                    if (t != null)
//                    {
//                        ContainsTemplateConflicts = true;
//                        if (_conflictingTemplateAliases.ContainsKey(t.Alias) == false)
//                        {
//                            _conflictingTemplateAliases.Add(t.Alias, alias);
//                        }
//                    }
//                }
//            }

//            foreach (var n in Config.Root.XPathSelectElements("Stylesheets/Stylesheet"))
//            {
//                var alias = n.Element("Name")?.Value; 
//                if (!string.IsNullOrEmpty(alias))
//                {
//                    var s = Current.Services.FileService.GetStylesheetByName(alias);
//                    if (s != null)
//                    {
//                        ContainsStyleSheeConflicts = true;
//                        if (_conflictingStyleSheetNames.ContainsKey(s.Alias) == false)
//                        {
//                            _conflictingStyleSheetNames.Add(s.Alias, alias);
//                        }
//                    }
//                }
//            }

            
//        }

//        /// <summary>
//        /// This uses the old method of fetching and only supports the packages.umbraco.org repository.
//        /// </summary>
//        /// <param name="Package"></param>
//        /// <returns></returns>
//        public string Fetch(Guid Package)
//        {
//            // Check for package directory
//            if (Directory.Exists(IOHelper.MapPath(SystemDirectories.Packages)) == false)
//                Directory.CreateDirectory(IOHelper.MapPath(SystemDirectories.Packages));

//            if (_webClient == null)
//                _webClient = new WebClient();

//            _webClient.DownloadFile(
//                "http://" + PackageServer + "/fetch?package=" + Package.ToString(),
//                IOHelper.MapPath(SystemDirectories.Packages + "/" + Package + ".umb"));

//            return "packages\\" + Package + ".umb";
//        }

//        #endregion

//        private void OnPackageInstalled(PackageDefinition insPack)
//        {
//            // getting an InstallationSummary for sending to the PackagingService.ImportedPackage event
//            var fileService = Current.Services.FileService;
//            var macroService = Current.Services.MacroService;
//            var contentTypeService = Current.Services.ContentTypeService;
//            var dataTypeService = Current.Services.DataTypeService;
//            var localizationService = Current.Services.LocalizationService;

//            var installationSummary = InstallationSummary.FromPackageDefinition(insPack, contentTypeService, dataTypeService, fileService, localizationService, macroService);
//            installationSummary.PackageInstalled = true;

//            var args = new ImportPackageEventArgs<InstallationSummary>(installationSummary, insPack, false);
//            PackagingService.OnImportedPackage(args);
//        }
//    }
//}
