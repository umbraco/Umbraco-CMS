using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.propertytype;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Diagnostics;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.template;
using Umbraco.Core.IO;

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
        private string _name;
        private string _version;
        private string _url;
        private string _license;
        private string _licenseUrl;
        private int _reqMajor;
        private int _reqMinor;
        private int _reqPatch;
        private string _authorName;
        private string _authorUrl;
        private string _readme;
        private string _control;
        private bool _containUnsecureFiles = false;
        private List<string> _unsecureFiles = new List<string>();
        private bool _containsMacroConflict = false;
        private Dictionary<string, string> _conflictingMacroAliases = new Dictionary<string, string>();
        private bool _containsTemplateConflict = false;
        private Dictionary<string, string> _conflictingTemplateAliases = new Dictionary<string, string>();
        private bool _containsStyleSheetConflict = false;
        private Dictionary<string, string> _conflictingStyleSheetNames = new Dictionary<string, string>();

        private ArrayList _macros = new ArrayList();
        private XmlDocument _packageConfig;

        public string Name { get { return _name; } }
        public string Version { get { return _version; } }
        public string Url { get { return _url; } }
        public string License { get { return _license; } }
        public string LicenseUrl { get { return _licenseUrl; } }
        public string Author { get { return _authorName; } }
        public string AuthorUrl { get { return _authorUrl; } }
        public string ReadMe { get { return _readme; } }
        public string Control { get { return _control; } }

        public bool ContainsMacroConflict { get { return _containsMacroConflict; } }
        public IDictionary<string, string> ConflictingMacroAliases { get { return _conflictingMacroAliases; } }

        public bool ContainsUnsecureFiles { get { return _containUnsecureFiles; } }
        public List<string> UnsecureFiles { get { return _unsecureFiles; } }

        public bool ContainsTemplateConflicts { get { return _containsTemplateConflict; } }
        public IDictionary<string, string> ConflictingTemplateAliases { get { return _conflictingTemplateAliases; } }

        public bool ContainsStyleSheeConflicts { get { return _containsStyleSheetConflict; } }
        public IDictionary<string, string> ConflictingStyleSheetNames { get { return _conflictingStyleSheetNames; } }

        public int RequirementsMajor { get { return _reqMajor; } }
        public int RequirementsMinor { get { return _reqMinor; } }
        public int RequirementsPatch { get { return _reqPatch; } }

        /// <summary>
        /// The xmldocument, describing the contents of a package.
        /// </summary>
        public XmlDocument Config
        {
            get { return _packageConfig; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Installer()
        {
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
            _name = Name;
            _version = Version;
            _url = Url;
            _license = License;
            _licenseUrl = LicenseUrl;
            _reqMajor = RequirementsMajor;
            _reqMinor = RequirementsMinor;
            _reqPatch = RequirementsPatch;
            _authorName = Author;
            _authorUrl = AuthorUrl;
            _readme = Readme;
            _control = Control;
        }

        #region Public Methods
        
        /// <summary>
        /// Adds the macro to the package
        /// </summary>
        /// <param name="MacroToAdd">Macro to add</param>
        [Obsolete("This method does nothing but add the macro to an ArrayList that is never used, so don't call this method.")]
        public void AddMacro(Macro MacroToAdd)
        {
            _macros.Add(MacroToAdd);
        }
        
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
                string tempDir = "";
                if (File.Exists(IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + InputFile)))
                {
                    FileInfo fi = new FileInfo(IOHelper.MapPath(SystemDirectories.Data + Path.DirectorySeparatorChar + InputFile));
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
            string _packName = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/name"));
            string _packAuthor = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/name"));
            string _packAuthorUrl = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/website"));
            string _packVersion = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/version"));
            string _packReadme = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            string _packLicense = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/license "));
            string _packUrl = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/url "));

            bool _enableSkins = false;
            string _skinRepoGuid = "";

            if (_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/enableSkins") != null)
            {
                XmlNode _skinNode = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/enableSkins");
                _enableSkins = bool.Parse(xmlHelper.GetNodeValue(_skinNode));
                if (_skinNode.Attributes["repository"] != null && !string.IsNullOrEmpty(_skinNode.Attributes["repository"].Value))
                    _skinRepoGuid = _skinNode.Attributes["repository"].Value;
            }

            //Create a new package instance to record all the installed package adds - this is the same format as the created packages has.
            //save the package meta data
            packager.InstalledPackage insPack = packager.InstalledPackage.MakeNew(_packName);
            insPack.Data.Author = _packAuthor;
            insPack.Data.AuthorUrl = _packAuthorUrl;
            insPack.Data.Version = _packVersion;
            insPack.Data.Readme = _packReadme;
            insPack.Data.License = _packLicense;
            insPack.Data.Url = _packUrl;

            //skinning
            insPack.Data.EnableSkins = _enableSkins;
            insPack.Data.SkinRepoGuid = string.IsNullOrEmpty(_skinRepoGuid) ? Guid.Empty : new Guid(_skinRepoGuid);

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
                packager.InstalledPackage insPack = packager.InstalledPackage.GetById(packageId);

                // Move files
                //string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
                string basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

                foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//file"))
                {
                    //we enclose the whole file-moving to ensure that the entire installer doesn't crash
                    try
                    {
                        String destPath = GetFileName(basePath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                        String sourceFile = GetFileName(tempDir, xmlHelper.GetNodeValue(n.SelectSingleNode("guid")));
                        String destFile = GetFileName(destPath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                        // Create the destination directory if it doesn't exist
                        if (!Directory.Exists(destPath))
                            Directory.CreateDirectory(destPath);
                        //If a file with this name exists, delete it
                        else if (File.Exists(destFile))
                            File.Delete(destFile);

                        // Move the file
                        File.Move(sourceFile, destFile);

                        //PPH log file install
                        insPack.Data.Files.Add(xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")) + "/" + xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));
                    }
                    catch (Exception ex)
                    {
						LogHelper.Error<Installer>("Package install error", ex);
                    }
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
                var rootElement = _packageConfig.DocumentElement.GetXElement();
                var packagingService = ApplicationContext.Current.Services.PackagingService;

                #region DataTypes
                var dataTypeElement = rootElement.Descendants("DataTypes").FirstOrDefault();
                if(dataTypeElement != null)
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
                foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//macro"))
                {
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
                foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
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
                foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Actions/Action"))
                {
                    if (n.Attributes["undo"] == null || n.Attributes["undo"].Value == "true")
                    {
                        insPack.Data.Actions += n.OuterXml;
                    }

                    if (n.Attributes["runat"] != null && n.Attributes["runat"].Value == "install")
                    {
                        try
                        {
                            PackageAction.RunPackageAction(insPack.Data.Name, n.Attributes["alias"].Value, n);
                        }
                        catch
                        {

                        }
                    }
                }
                #endregion

                // Trigger update of Apps / Trees config.
                // (These are ApplicationStartupHandlers so just instantiating them will trigger them)
                new ApplicationRegistrar();
                new ApplicationTreeRegistrar();

                insPack.Save();
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
        /// Invoking this method installs the entire current package
        /// </summary>
        /// <param name="tempDir">Temporary folder where the package's content are extracted to</param>
        /// <param name="guid"></param>
        /// <param name="repoGuid"></param>
        public void Install(string tempDir, string guid, string repoGuid)
        {
            //PPH added logging of installs, this adds all install info in the installedPackages config file.
            string packName = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/name"));
            string packAuthor = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/name"));
            string packAuthorUrl = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/website"));
            string packVersion = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/version"));
            string packReadme = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            string packLicense = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/license "));


            //Create a new package instance to record all the installed package adds - this is the same format as the created packages has.
            //save the package meta data
            var insPack = InstalledPackage.MakeNew(packName);
            insPack.Data.Author = packAuthor;
            insPack.Data.AuthorUrl = packAuthorUrl;
            insPack.Data.Version = packVersion;
            insPack.Data.Readme = packReadme;
            insPack.Data.License = packLicense;
            insPack.Data.PackageGuid = guid; //the package unique key.
            insPack.Data.RepositoryGuid = repoGuid; //the repository unique key, if the package is a file install, the repository will not get logged.

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
            var rootElement = _packageConfig.DocumentElement.GetXElement();
            var packagingService = ApplicationContext.Current.Services.PackagingService;

            #region DataTypes
            var dataTypeElement = rootElement.Descendants("DataTypes").FirstOrDefault();
            if(dataTypeElement != null)
            {
                var dataTypeDefinitions = packagingService.ImportDataTypeDefinitions(dataTypeElement, currentUser.Id);
                foreach (var dataTypeDefinition in dataTypeDefinitions)
                {
                    insPack.Data.DataTypes.Add(dataTypeDefinition.Id.ToString(CultureInfo.InvariantCulture));
                }
            }
            #endregion

            #region Install Languages
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//Language"))
            {
                language.Language newLang = language.Language.Import(n);

                if (newLang != null)
                    insPack.Data.Languages.Add(newLang.id.ToString());
            }
            #endregion

            #region Install Dictionary Items
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("./DictionaryItems/DictionaryItem"))
            {
                Dictionary.DictionaryItem newDi = Dictionary.DictionaryItem.Import(n);

                if (newDi != null)
                    insPack.Data.DictionaryItems.Add(newDi.id.ToString());
            }
            #endregion

            #region Install Macros
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//macro"))
            {
                Macro m = Macro.Import(n);

                if (m != null)
                    insPack.Data.Macros.Add(m.Id.ToString());
            }
            #endregion

            #region Move files
            string basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//file"))
            {
                String destPath = GetFileName(basePath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                String sourceFile = GetFileName(tempDir, xmlHelper.GetNodeValue(n.SelectSingleNode("guid")));
                String destFile = GetFileName(destPath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                // Create the destination directory if it doesn't exist
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);
                // If a file with this name exists, delete it
                else if (File.Exists(destFile))
                    File.Delete(destFile);
                // Move the file
                File.Move(sourceFile, destFile);

                //PPH log file install
                insPack.Data.Files.Add(xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")) + "/" + xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));
            }
            #endregion

            #region Install Templates
            var templateElement = rootElement.Descendants("Templates").FirstOrDefault();
            if (templateElement != null)
            {
                var templates = packagingService.ImportTemplates(templateElement, currentUser.Id);
                foreach (var template in templates)
                {
                    insPack.Data.Templates.Add(template.Id.ToString(CultureInfo.InvariantCulture));
                }
            }
            /*foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template"))
            {
                Template t = Template.MakeNew(xmlHelper.GetNodeValue(n.SelectSingleNode("Name")), currentUser);
                t.Alias = xmlHelper.GetNodeValue(n.SelectSingleNode("Alias"));

                t.ImportDesign(xmlHelper.GetNodeValue(n.SelectSingleNode("Design")));

                insPack.Data.Templates.Add(t.Id.ToString());
            }
            
            // Add master templates
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template"))
            {
                string master = xmlHelper.GetNodeValue(n.SelectSingleNode("Master"));
                Template t = Template.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Alias")));
                if (master.Trim() != "")
                {
                    Template masterTemplate = Template.GetByAlias(master);
                    if (masterTemplate != null)
                    {
                        t.MasterTemplate = Template.GetByAlias(master).Id;
                        if (UmbracoSettings.UseAspNetMasterPages)
                            t.SaveMasterPageFile(t.Design);
                    }
                }
                // Master templates can only be generated when their master is known
                if (UmbracoSettings.UseAspNetMasterPages)
                {
                    t.ImportDesign(xmlHelper.GetNodeValue(n.SelectSingleNode("Design")));
                    t.SaveMasterPageFile(t.Design);
                }
            }*/
            #endregion

            #region Install DocumentTypes
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
                }
            }
            /*foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("DocumentTypes/DocumentType"))
            {
                ImportDocumentType(n, currentUser, false);
            }

            // Add documenttype structure
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("DocumentTypes/DocumentType"))
            {
                DocumentType dt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Alias")));
                if (dt != null)
                {
                    ArrayList allowed = new ArrayList();
                    foreach (XmlNode structure in n.SelectNodes("Structure/DocumentType"))
                    {
                        DocumentType dtt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(structure));
                        allowed.Add(dtt.Id);
                    }
                    int[] adt = new int[allowed.Count];
                    for (int i = 0; i < allowed.Count; i++)
                        adt[i] = (int)allowed[i];
                    dt.AllowedChildContentTypeIDs = adt;

                    //PPH we log the document type install here.
                    insPack.Data.Documenttypes.Add(dt.Id.ToString());
                }
            }*/
            #endregion

            #region Install Stylesheets
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
            {
                StyleSheet s = StyleSheet.MakeNew(
                    currentUser,
                    xmlHelper.GetNodeValue(n.SelectSingleNode("Name")),
                    xmlHelper.GetNodeValue(n.SelectSingleNode("FileName")),
                    xmlHelper.GetNodeValue(n.SelectSingleNode("Content")));

                foreach (XmlNode prop in n.SelectNodes("Properties/Property"))
                {
                    StylesheetProperty sp = StylesheetProperty.MakeNew(
                        xmlHelper.GetNodeValue(prop.SelectSingleNode("Name")),
                        s,
                        currentUser);
                    sp.Alias = xmlHelper.GetNodeValue(prop.SelectSingleNode("Alias"));
                    sp.value = xmlHelper.GetNodeValue(prop.SelectSingleNode("Value"));
                }
                s.saveCssToFile();
                s.Save();

                insPack.Data.Stylesheets.Add(s.Id.ToString());
            }
            #endregion

            #region Install Documents
            var documentElement = rootElement.Descendants("DocumentSet").FirstOrDefault(); 
            if(documentElement != null)
            {
                var content = packagingService.ImportContent(documentElement, -1, currentUser.Id);

                var firstContentItem = content.First();
                insPack.Data.ContentNodeId = firstContentItem.Id.ToString(CultureInfo.InvariantCulture);
            }
            /*foreach (XmlElement n in _packageConfig.DocumentElement.SelectNodes("Documents/DocumentSet [@importMode = 'root']/*"))
            {
                Document.Import(-1, currentUser, n);

                //PPH todo log document install... 
            }*/
            #endregion

            #region Install Actions
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Actions/Action [@runat != 'uninstall']"))
            {
                try
                {
                    PackageAction.RunPackageAction(packName, n.Attributes["alias"].Value, n);
                }
                catch { }
            }

            //saving the uninstall actions untill the package is uninstalled.
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Actions/Action [@undo != false()]"))
            {
                insPack.Data.Actions += n.OuterXml;
            }
            #endregion

            insPack.Save();
        }
        
        /// <summary>
        /// Reads the configuration of the package from the configuration xmldocument
        /// </summary>
        /// <param name="tempDir">The folder to which the contents of the package is extracted</param>
        public void LoadConfig(string tempDir)
        {
            _packageConfig = new XmlDocument();
            _packageConfig.Load(tempDir + Path.DirectorySeparatorChar + "package.xml");

            _name = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/name").FirstChild.Value;
            _version = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/version").FirstChild.Value;
            _url = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/url").FirstChild.Value;
            _license = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/license").FirstChild.Value;
            _licenseUrl = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/license").Attributes.GetNamedItem("url").Value;
            _reqMajor = int.Parse(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/major").FirstChild.Value);
            _reqMinor = int.Parse(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/minor").FirstChild.Value);
            _reqPatch = int.Parse(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/patch").FirstChild.Value);
            _authorName = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/name").FirstChild.Value;
            _authorUrl = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/website").FirstChild.Value;

            string basePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;

            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//file"))
            {
                bool badFile = false;
                string destPath = GetFileName(basePath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                string destFile = GetFileName(destPath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                if (destPath.ToLower().Contains(IOHelper.DirSepChar + "app_code"))
                    badFile = true;

                if (destPath.ToLower().Contains(IOHelper.DirSepChar + "bin"))
                    badFile = true;

                if (destFile.ToLower().EndsWith(".dll"))
                    badFile = true;

                if (badFile)
                {
                    _containUnsecureFiles = true;
                    _unsecureFiles.Add(xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));
                }
            }

            //this will check for existing macros with the same alias
            //since we will not overwrite on import it's a good idea to inform the user what will be overwritten
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//macro"))
            {
                var alias = n.SelectSingleNode("alias").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    try
                    {
                        var m = new Macro(alias);
                        this._containsMacroConflict = true;

                        if (_conflictingMacroAliases.ContainsKey(m.Name) == false)
                        {
                            _conflictingMacroAliases.Add(m.Name, alias);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                    } //thrown when the alias doesn't exist in the DB, ie - macro not there
                }
            }

            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template"))
            {
                var alias = n.SelectSingleNode("Alias").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var t = Template.GetByAlias(alias);
                    if (t != null)
                    {
                        this._containsTemplateConflict = true;
                        if (_conflictingTemplateAliases.ContainsKey(t.Text) == false)
                        {
                            _conflictingTemplateAliases.Add(t.Text, alias);
                        }
                    }
                }
            }

            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
            {
                var alias = n.SelectSingleNode("Name").InnerText;
                if (!string.IsNullOrEmpty(alias))
                {
                    var s = StyleSheet.GetByName(alias);
                    if (s != null)
                    {
                        this._containsStyleSheetConflict = true;
                        if (_conflictingStyleSheetNames.ContainsKey(s.Text) == false)
                        {
                            _conflictingStyleSheetNames.Add(s.Text, alias);   
                        }                        
                    }
                }
            }

            try
            {
                _readme = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            }
            catch { }

            try
            {
                _control = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/control"));
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
            if (!Directory.Exists(IOHelper.MapPath(SystemDirectories.Packages)))
                Directory.CreateDirectory(IOHelper.MapPath(SystemDirectories.Packages));

            var wc = new System.Net.WebClient();

            wc.DownloadFile(
                "http://" + UmbracoSettings.PackageServer + "/fetch?package=" + Package.ToString(),
                IOHelper.MapPath(SystemDirectories.Packages + "/" + Package.ToString() + ".umb"));

            return "packages\\" + Package.ToString() + ".umb";
        }
        
        #endregion

        #region Public Static Methods

        [Obsolete("This method is empty, so calling it will have no effect whatsoever.")]
        public static void updatePackageInfo(Guid Package, int VersionMajor, int VersionMinor, int VersionPatch, User User)
        {
            //Why does this even exist?
        }

        [Obsolete("Use ApplicationContext.Current.Services.PackagingService.ImportContentTypes instead")]
        public static void ImportDocumentType(XmlNode n, User u, bool ImportStructure)
        {
            var element = n.GetXElement();
            var contentTypes = ApplicationContext.Current.Services.PackagingService.ImportContentTypes(element, ImportStructure, u.Id);
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
                if (!fileNameStartsWithSlash)
                    // No double slash, just concatenate
                    return path + fileName;
                else
                    // Double slash, exclude that of the file
                    return path + fileName.Substring(1);
            }
            else
            {
                if (fileNameStartsWithSlash)
                    // Required slash specified, just concatenate
                    return path + fileName;
                else
                    // Required slash missing, add it
                    return path + Path.DirectorySeparatorChar + fileName;
            }
        }

        private static int FindDataTypeDefinitionFromType(ref Guid dtId)
        {
            int dfId = 0;
            foreach (datatype.DataTypeDefinition df in datatype.DataTypeDefinition.GetAll())
                if (df.DataType.Id == dtId)
                {
                    dfId = df.Id;
                    break;
                }
            return dfId;
        }

        private static string UnPack(string zipName)
        {
            // Unzip
            string tempDir = IOHelper.MapPath(SystemDirectories.Data) + Path.DirectorySeparatorChar + Guid.NewGuid().ToString();
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
    }

    public class Package
    {
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public Package()
        {
        }

        /// <summary>
        /// Initialize package install status object by specifying the internal id of the installation. 
        /// The id is specific to the local umbraco installation and cannot be used to identify the package in general. 
        /// Use the Package(Guid) constructor to check whether a package has been installed
        /// </summary>
        /// <param name="Id">The internal id.</param>
        public Package(int Id)
        {
            initialize(Id);
        }

        public Package(Guid Id)
        {
            int installStatusId = SqlHelper.ExecuteScalar<int>(
                "select id from umbracoInstalledPackages where package = @package and upgradeId = 0",
                SqlHelper.CreateParameter("@package", Id));

            if (installStatusId > 0)
                initialize(installStatusId);
            else
                throw new ArgumentException("Package with id '" + Id.ToString() + "' is not installed");
        }

        private void initialize(int id)
        {

            IRecordsReader dr =
                SqlHelper.ExecuteReader(
                "select id, uninstalled, upgradeId, installDate, userId, package, versionMajor, versionMinor, versionPatch from umbracoInstalledPackages where id = @id",
                SqlHelper.CreateParameter("@id", id));

            if (dr.Read())
            {
                Id = id;
                Uninstalled = dr.GetBoolean("uninstalled");
                UpgradeId = dr.GetInt("upgradeId");
                InstallDate = dr.GetDateTime("installDate");
                User = User.GetUser(dr.GetInt("userId"));
                PackageId = dr.GetGuid("package");
                VersionMajor = dr.GetInt("versionMajor");
                VersionMinor = dr.GetInt("versionMinor");
                VersionPatch = dr.GetInt("versionPatch");
            }
            dr.Close();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {

            IParameter[] values = {
                SqlHelper.CreateParameter("@uninstalled", Uninstalled),
                SqlHelper.CreateParameter("@upgradeId", UpgradeId),
                SqlHelper.CreateParameter("@installDate", InstallDate),
                SqlHelper.CreateParameter("@userId", User.Id),
                SqlHelper.CreateParameter("@versionMajor", VersionMajor),
                SqlHelper.CreateParameter("@versionMinor", VersionMinor),
                SqlHelper.CreateParameter("@versionPatch", VersionPatch),
                SqlHelper.CreateParameter("@id", Id)
            };

            // check if package status exists
            if (Id == 0)
            {
                // The method is synchronized
                SqlHelper.ExecuteNonQuery("INSERT INTO umbracoInstalledPackages (uninstalled, upgradeId, installDate, userId, versionMajor, versionMinor, versionPatch) VALUES (@uninstalled, @upgradeId, @installDate, @userId, @versionMajor, @versionMinor, @versionPatch)", values);
                Id = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoInstalledPackages");
            }

            SqlHelper.ExecuteNonQuery(
                "update umbracoInstalledPackages set " +
                "uninstalled = @uninstalled, " +
                "upgradeId = @upgradeId, " +
                "installDate = @installDate, " +
                "userId = @userId, " +
                "versionMajor = @versionMajor, " +
                "versionMinor = @versionMinor, " +
                "versionPatch = @versionPatch " +
                "where id = @id",
                values);
        }

        private bool _uninstalled;

        public bool Uninstalled
        {
            get { return _uninstalled; }
            set { _uninstalled = value; }
        }


        private User _user;

        public User User
        {
            get { return _user; }
            set { _user = value; }
        }


        private DateTime _installDate;

        public DateTime InstallDate
        {
            get { return _installDate; }
            set { _installDate = value; }
        }


        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }


        private int _upgradeId;

        public int UpgradeId
        {
            get { return _upgradeId; }
            set { _upgradeId = value; }
        }


        private Guid _packageId;

        public Guid PackageId
        {
            get { return _packageId; }
            set { _packageId = value; }
        }


        private int _versionPatch;

        public int VersionPatch
        {
            get { return _versionPatch; }
            set { _versionPatch = value; }
        }


        private int _versionMinor;

        public int VersionMinor
        {
            get { return _versionMinor; }
            set { _versionMinor = value; }
        }


        private int _versionMajor;

        public int VersionMajor
        {
            get { return _versionMajor; }
            set { _versionMajor = value; }
        }



    }
}
