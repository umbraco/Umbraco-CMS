using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using System.Runtime.CompilerServices;

using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.propertytype;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Diagnostics;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.template;

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

        /// <summary>
        /// Adds the macro to the package
        /// </summary>
        /// <param name="MacroToAdd">Macro to add</param>
        public void AddMacro(businesslogic.macro.Macro MacroToAdd)
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
            string tempDir = "";
            if (File.Exists(HttpContext.Current.Server.MapPath(GlobalSettings.StorageDirectory + Path.DirectorySeparatorChar + InputFile)))
            {
                FileInfo fi = new FileInfo(HttpContext.Current.Server.MapPath(GlobalSettings.StorageDirectory + Path.DirectorySeparatorChar + InputFile));
                // Check if the file is a valid package
                if (fi.Extension.ToLower() == ".umb")
                {
                    try
                    {
                        tempDir = unPack(fi.FullName);
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
                throw new Exception("Error - file not found. Could find file named '" + HttpContext.Current.Server.MapPath(GlobalSettings.StorageDirectory + Path.DirectorySeparatorChar + InputFile) + "'");
            return tempDir;
        }


        public int CreateManifest(string tempDir, string guid, string repoGuid) {
            //This is the new improved install rutine, which chops up the process into 3 steps, creating the manifest, moving files, and finally handling umb objects
            string _packName = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/name"));
            string _packAuthor = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/name"));
            string _packAuthorUrl = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/website"));
            string _packVersion = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/version"));
            string _packReadme = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            string _packLicense = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/license "));


            //Create a new package instance to record all the installed package adds - this is the same format as the created packages has.
            //save the package meta data
            packager.InstalledPackage insPack = packager.InstalledPackage.MakeNew(_packName);
            insPack.Data.Author = _packAuthor;
            insPack.Data.AuthorUrl = _packAuthorUrl;
            insPack.Data.Version = _packVersion;
            insPack.Data.Readme = _packReadme;
            insPack.Data.License = _packLicense;
            insPack.Data.PackageGuid = guid; //the package unique key.
            insPack.Data.RepositoryGuid = repoGuid; //the repository unique key, if the package is a file install, the repository will not get logged.
            insPack.Save();
            
            return insPack.Data.Id;
        }

        public void InstallFiles(int packageId, string tempDir) {

            //retrieve the manifest to continue installation
            packager.InstalledPackage insPack = packager.InstalledPackage.GetById(packageId);

            // Move files
            string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
            string basePath = HttpContext.Current.Server.MapPath(virtualBasePath);
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//file")) {
                //we enclose the whole file-moving to ensure that the entire installer doesn't crash
                try {
                    String destPath = getFileName(basePath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                    String sourceFile = getFileName(tempDir, xmlHelper.GetNodeValue(n.SelectSingleNode("guid")));
                    String destFile = getFileName(destPath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

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
                } catch (Exception ex) {
                    Log.Add(LogTypes.Error, -1, "Package install error: " + ex.ToString());
                }
            }

            insPack.Save();
        }


        public void InstallBusinessLogic(int packageId, string tempDir) {
            
            //retrieve the manifest to continue installation
            packager.InstalledPackage insPack = packager.InstalledPackage.GetById(packageId);
            bool saveNeeded = false;

            //Install DataTypes
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//DataType")) {
                cms.businesslogic.datatype.DataTypeDefinition newDtd = cms.businesslogic.datatype.DataTypeDefinition.Import(n);

                if (newDtd != null) {
                    insPack.Data.DataTypes.Add(newDtd.Id.ToString());
                    saveNeeded = true;
                }
            }

            if (saveNeeded) {insPack.Save(); saveNeeded = false;}

            //Install languages
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//Language")) {
                language.Language newLang = language.Language.Import(n);

                if (newLang != null) {
                    insPack.Data.Languages.Add(newLang.id.ToString());
                    saveNeeded = true;
                }
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }

            //Install dictionary items
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("./DictionaryItems/DictionaryItem")) {
                Dictionary.DictionaryItem newDi = Dictionary.DictionaryItem.Import(n);

                if (newDi != null) {
                    insPack.Data.DictionaryItems.Add(newDi.id.ToString());
                    saveNeeded = true;
                }
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }

            // Install macros
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//macro")) {
                cms.businesslogic.macro.Macro m = cms.businesslogic.macro.Macro.Import(n);

                if (m != null) {
                    insPack.Data.Macros.Add(m.Id.ToString());
                    saveNeeded = true;
                }
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }

            // Get current user, with a fallback
            User u = new User(0);
            if (!string.IsNullOrEmpty(BasePages.UmbracoEnsuredPage.umbracoUserContextID)) {
                if (BasePages.UmbracoEnsuredPage.ValidateUserContextID(BasePages.UmbracoEnsuredPage.umbracoUserContextID)) {
                    BasePages.UmbracoEnsuredPage uep = new umbraco.BasePages.UmbracoEnsuredPage();
                    u = uep.getUser();
                }
            }


            // Add Templates
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template")) {
                var t = Template.Import(n, u);
                
                insPack.Data.Templates.Add(t.Id.ToString());

                saveNeeded = true;
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }


            // Add master templates
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template")) {
                string master = xmlHelper.GetNodeValue(n.SelectSingleNode("Master"));
                template.Template t = template.Template.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Alias")));
                if (master.Trim() != "") {
                    template.Template masterTemplate = template.Template.GetByAlias(master);
                    if (masterTemplate != null) {
                        t.MasterTemplate = template.Template.GetByAlias(master).Id;
                        if (UmbracoSettings.UseAspNetMasterPages)
                            t.SaveMasterPageFile(t.Design);
                    }
                }
                // Master templates can only be generated when their master is known
                if (UmbracoSettings.UseAspNetMasterPages) {
                    t.ImportDesign(xmlHelper.GetNodeValue(n.SelectSingleNode("Design")));
                    t.SaveMasterPageFile(t.Design);
                }
            }

            // Add documenttypes
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("DocumentTypes/DocumentType")) {
                ImportDocumentType(n, u, false);
                saveNeeded = true;
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }


            // Add documenttype structure
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("DocumentTypes/DocumentType")) {
                DocumentType dt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Alias")));
                if (dt != null) {
                    ArrayList allowed = new ArrayList();
                    foreach (XmlNode structure in n.SelectNodes("Structure/DocumentType")) {
                        DocumentType dtt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(structure));
                        allowed.Add(dtt.Id);
                    }
                    int[] adt = new int[allowed.Count];
                    for (int i = 0; i < allowed.Count; i++)
                        adt[i] = (int)allowed[i];
                    dt.AllowedChildContentTypeIDs = adt;

                    //PPH we log the document type install here.
                    insPack.Data.Documenttypes.Add(dt.Id.ToString());
                    saveNeeded = true;
                }
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }

            // Stylesheets
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Stylesheets/Stylesheet")) {
                StyleSheet s = StyleSheet.Import(n, u);

                insPack.Data.Stylesheets.Add(s.Id.ToString());
                saveNeeded = true;
            }

            if (saveNeeded) { insPack.Save(); saveNeeded = false; }

            // Documents
            foreach (XmlElement n in _packageConfig.DocumentElement.SelectNodes("Documents/DocumentSet [@importMode = 'root']/node")) {
                insPack.Data.ContentNodeId = cms.businesslogic.web.Document.Import(-1, u, n).ToString();
            }

            //Package Actions
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Actions/Action")) {

                if (n.Attributes["undo"] == null || n.Attributes["undo"].Value == "false") {
                    insPack.Data.Actions += n.OuterXml;
                    Log.Add(LogTypes.Debug, -1, HttpUtility.HtmlEncode(n.OuterXml));
                }

                if (n.Attributes["runat"] != null && n.Attributes["runat"].Value == "install") {
                    try {
                        packager.PackageAction.RunPackageAction(insPack.Data.Name, n.Attributes["alias"].Value, n);
                    } catch {

                    }
                }
            }

            insPack.Save();
        }

        public void InstallCleanUp(int packageId, string tempDir) {

            //this will contain some logic to clean up all those old folders

        }

        /// <summary>
        /// Invoking this method installs the entire current package
        /// </summary>
        /// <param name="tempDir">Temporary folder where the package's content are extracted to</param>
        public void Install(string tempDir, string guid, string repoGuid)
        {
            //PPH added logging of installs, this adds all install info in the installedPackages config file.
            string _packName = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/name"));
            string _packAuthor= xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/name"));
            string _packAuthorUrl = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/website"));
            string _packVersion = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/version"));
            string _packReadme = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/readme"));
            string _packLicense = xmlHelper.GetNodeValue(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/license "));
        

            //Create a new package instance to record all the installed package adds - this is the same format as the created packages has.
            //save the package meta data
            packager.InstalledPackage insPack = packager.InstalledPackage.MakeNew(_packName);
            insPack.Data.Author = _packAuthor;
            insPack.Data.AuthorUrl = _packAuthorUrl;
            insPack.Data.Version = _packVersion;
            insPack.Data.Readme = _packReadme;
            insPack.Data.License = _packLicense;
            insPack.Data.PackageGuid = guid; //the package unique key.
            insPack.Data.RepositoryGuid = repoGuid; //the repository unique key, if the package is a file install, the repository will not get logged.
            
           
            //Install languages
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//Language")) {
                language.Language newLang = language.Language.Import(n);

                if (newLang != null)
                    insPack.Data.Languages.Add(newLang.id.ToString());
            }

            //Install dictionary items
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("./DictionaryItems/DictionaryItem")) {
               Dictionary.DictionaryItem newDi = Dictionary.DictionaryItem.Import(n);

               if (newDi != null)
                   insPack.Data.DictionaryItems.Add(newDi.id.ToString());
            }

            // Install macros
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//macro"))
            {
                cms.businesslogic.macro.Macro m = cms.businesslogic.macro.Macro.Import(n);

                if(m != null)
                    insPack.Data.Macros.Add(m.Id.ToString());
            }

            // Move files
            string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
            string basePath = HttpContext.Current.Server.MapPath(virtualBasePath);
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//file"))
            {
                String destPath = getFileName(basePath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                String sourceFile = getFileName(tempDir, xmlHelper.GetNodeValue(n.SelectSingleNode("guid")));
                String destFile = getFileName(destPath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

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


            // Get current user
            BasePages.UmbracoEnsuredPage uep = new umbraco.BasePages.UmbracoEnsuredPage();
            BusinessLogic.User u = uep.getUser();

            // Add Templates
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template"))
            {
                template.Template t = template.Template.MakeNew(xmlHelper.GetNodeValue(n.SelectSingleNode("Name")), u);
                t.Alias = xmlHelper.GetNodeValue(n.SelectSingleNode("Alias"));

                t.ImportDesign( xmlHelper.GetNodeValue(n.SelectSingleNode("Design")) );
             
                insPack.Data.Templates.Add(t.Id.ToString());
            }

            // Add master templates
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Templates/Template"))
            {
                string master = xmlHelper.GetNodeValue(n.SelectSingleNode("Master"));
                template.Template t = template.Template.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Alias")));
                if (master.Trim() != "")
                {
                    template.Template masterTemplate = template.Template.GetByAlias(master);
                    if (masterTemplate != null) {
                        t.MasterTemplate = template.Template.GetByAlias(master).Id;
                        if (UmbracoSettings.UseAspNetMasterPages)
                            t.SaveMasterPageFile(t.Design);
                    }
                }
                // Master templates can only be generated when their master is known
                if (UmbracoSettings.UseAspNetMasterPages) {
                    t.ImportDesign(xmlHelper.GetNodeValue(n.SelectSingleNode("Design")));
                    t.SaveMasterPageFile(t.Design);
                }
            }

            // Add documenttypes
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("DocumentTypes/DocumentType"))
            {
                ImportDocumentType(n, u, false);
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
            }

            // Stylesheets
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Stylesheets/Stylesheet"))
            {
                StyleSheet s = StyleSheet.MakeNew(
                    u,
                    xmlHelper.GetNodeValue(n.SelectSingleNode("Name")),
                    xmlHelper.GetNodeValue(n.SelectSingleNode("FileName")),
                    xmlHelper.GetNodeValue(n.SelectSingleNode("Content")));

                foreach (XmlNode prop in n.SelectNodes("Properties/Property"))
                {
                    StylesheetProperty sp = StylesheetProperty.MakeNew(
                        xmlHelper.GetNodeValue(prop.SelectSingleNode("Name")),
                        s,
                        u);
                    sp.Alias = xmlHelper.GetNodeValue(prop.SelectSingleNode("Alias"));
                    sp.value = xmlHelper.GetNodeValue(prop.SelectSingleNode("Value"));
                }
                s.saveCssToFile();

                insPack.Data.Stylesheets.Add(s.Id.ToString());
            }

            // Documents
            foreach (XmlElement n in _packageConfig.DocumentElement.SelectNodes("Documents/DocumentSet [@importMode = 'root']/node")) {
                cms.businesslogic.web.Document.Import(-1, u, n);

                //PPH todo log document install... 
            }
                        
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Actions/Action [@runat != 'uninstall']")) {
                try {
                    packager.PackageAction.RunPackageAction(_packName, n.Attributes["alias"].Value, n);
                } catch { }
            }

            //saving the uninstall actions untill the package is uninstalled.
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("Actions/Action [@undo != false()]")) {
                insPack.Data.Actions += n.OuterXml;
            }
            
            
            insPack.Save();
            
            
        }

        public static void ImportDocumentType(XmlNode n, BusinessLogic.User u, bool ImportStructure)
        {
            DocumentType dt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Alias")));
            if (dt == null)
            {
                dt = DocumentType.MakeNew(u, xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Name")));
                dt.Alias = xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Alias"));

            
                //Master content type
                DocumentType mdt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Master")));
                if (mdt != null)
                    dt.MasterContentType = mdt.Id;
            }
            else
            {
                dt.Text = xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Name"));
            }

            
            // Info
            dt.IconUrl = xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Icon"));
            dt.Thumbnail = xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Thumbnail"));
            dt.Description = xmlHelper.GetNodeValue(n.SelectSingleNode("Info/Description"));

            // Templates	
            ArrayList templates = new ArrayList();
            foreach (XmlNode tem in n.SelectNodes("Info/AllowedTemplates/Template"))
            {
                template.Template t = template.Template.GetByAlias(xmlHelper.GetNodeValue(tem));
                if (t != null)
                    templates.Add(t);
            }

            try
            {
                template.Template[] at = new template.Template[templates.Count];
                for (int i = 0; i < templates.Count; i++)
                    at[i] = (template.Template)templates[i];
                dt.allowedTemplates = at;
            }
            catch (Exception ee)
            {
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, u, dt.Id, "Packager: Error handling allowed templates: " + ee.ToString());
            }

            // Default template
            try
            {
                if (xmlHelper.GetNodeValue(n.SelectSingleNode("Info/DefaultTemplate")) != "")
                    dt.DefaultTemplate = template.Template.GetByAlias(xmlHelper.GetNodeValue(n.SelectSingleNode("Info/DefaultTemplate"))).Id;
            }
            catch (Exception ee)
            {
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, u, dt.Id, "Packager: Error assigning default template: " + ee.ToString());
            }

            // Tabs
            cms.businesslogic.ContentType.TabI[] tabs = dt.getVirtualTabs;
            string tabNames = ";";
            for (int t = 0; t < tabs.Length; t++)
                tabNames += tabs[t].Caption + ";";

            Hashtable ht = new Hashtable();
            foreach (XmlNode t in n.SelectNodes("Tabs/Tab"))
            {
                if (tabNames.IndexOf(";" + xmlHelper.GetNodeValue(t.SelectSingleNode("Caption")) + ";") == -1)
                {
                    ht.Add(int.Parse(xmlHelper.GetNodeValue(t.SelectSingleNode("Id"))),
                        dt.AddVirtualTab(xmlHelper.GetNodeValue(t.SelectSingleNode("Caption"))));
                }
            }


            // Get all tabs in hashtable
            Hashtable tabList = new Hashtable();
            foreach (cms.businesslogic.ContentType.TabI t in dt.getVirtualTabs)
            {
                if (!tabList.ContainsKey(t.Caption))
                    tabList.Add(t.Caption, t.Id);
            }

            // Generic Properties
            datatype.controls.Factory f = new datatype.controls.Factory();
            foreach (XmlNode gp in n.SelectNodes("GenericProperties/GenericProperty"))
            {   
                int dfId = 0;
                Guid dtId = new Guid(xmlHelper.GetNodeValue(gp.SelectSingleNode("Type")));

                if (gp.SelectSingleNode("Definition") != null && !string.IsNullOrEmpty(xmlHelper.GetNodeValue(gp.SelectSingleNode("Definition")))) {
                    Guid dtdId = new Guid(xmlHelper.GetNodeValue(gp.SelectSingleNode("Definition")));
                    if (CMSNode.IsNode(dtdId))
                        dfId = new CMSNode(dtdId).Id;
                } 
                if (dfId == 0) {
                    try {
                        dfId = findDataTypeDefinitionFromType(ref dtId);
                    } catch {
                        throw new Exception(String.Format("Could not find datatype with id {0}.", dtId));
                    }
                }

                // Fix for rich text editor backwards compatibility 
                if (dfId == 0 && dtId == new Guid("a3776494-0574-4d93-b7de-efdfdec6f2d1"))
                {
                    dtId = new Guid("83722133-f80c-4273-bdb6-1befaa04a612");
                    dfId = findDataTypeDefinitionFromType(ref dtId);
                }

                if (dfId != 0)
                {
                    PropertyType pt = dt.getPropertyType(xmlHelper.GetNodeValue(gp.SelectSingleNode("Alias")));
                    if (pt == null)
                    {
                        dt.AddPropertyType(
                            datatype.DataTypeDefinition.GetDataTypeDefinition(dfId),
                            xmlHelper.GetNodeValue(gp.SelectSingleNode("Alias")),
                            xmlHelper.GetNodeValue(gp.SelectSingleNode("Name"))
                            );
                        pt = dt.getPropertyType(xmlHelper.GetNodeValue(gp.SelectSingleNode("Alias")));
                    }
                    else
                    {
                        pt.DataTypeDefinition = datatype.DataTypeDefinition.GetDataTypeDefinition(dfId);
                        pt.Name = xmlHelper.GetNodeValue(gp.SelectSingleNode("Name"));
                    }

                    pt.Mandatory = bool.Parse(xmlHelper.GetNodeValue(gp.SelectSingleNode("Mandatory")));
                    pt.ValidationRegExp = xmlHelper.GetNodeValue(gp.SelectSingleNode("Validation"));
                    pt.Description = xmlHelper.GetNodeValue(gp.SelectSingleNode("Description"));

                    // tab
                    try
                    {
                        if (tabList.ContainsKey(xmlHelper.GetNodeValue(gp.SelectSingleNode("Tab"))))
                            pt.TabId = (int)tabList[xmlHelper.GetNodeValue(gp.SelectSingleNode("Tab"))];
                    }
                    catch (Exception ee)
                    {
                        BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, u, dt.Id, "Packager: Error assigning property to tab: " + ee.ToString());
                    }
                }
            }

            if (ImportStructure)
            {
                if (dt != null)
                {
                    ArrayList allowed = new ArrayList();
                    foreach (XmlNode structure in n.SelectNodes("Structure/DocumentType"))
                    {
                        DocumentType dtt = DocumentType.GetByAlias(xmlHelper.GetNodeValue(structure));
                        if (dtt != null)
                            allowed.Add(dtt.Id);
                    }
                    int[] adt = new int[allowed.Count];
                    for (int i = 0; i < allowed.Count; i++)
                        adt[i] = (int)allowed[i];
                    dt.AllowedChildContentTypeIDs = adt;
                }
            }

            // clear caching
            foreach(DocumentType.TabI t in dt.getVirtualTabs)
                DocumentType.FlushTabCache(t.Id, dt.Id);

        }

        /// <summary>
        /// Gets the name of the file in the specified path.
        /// Corrects possible problems with slashes that would result from a simple concatenation.
        /// Can also be used to concatenate paths.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The name of the file in the specified path.</returns>
        private static String getFileName(String path, string fileName)
        {
            Debug.Assert(path != null && path.Length >= 1);
            Debug.Assert(fileName != null && fileName.Length >= 1);

            path = path.Replace('/', '\\');
            fileName = fileName.Replace('/','\\');

            // Does filename start with a slash? Does path end with one?
            bool fileNameStartsWithSlash = (fileName[0] == Path.DirectorySeparatorChar);
            bool pathEndsWithSlash = (path[path.Length-1] == Path.DirectorySeparatorChar);

            // Path ends with a slash
            if (pathEndsWithSlash)
            {
                if(!fileNameStartsWithSlash)
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

        private static int findDataTypeDefinitionFromType(ref Guid dtId)
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
            _reqMinor = int.Parse(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/major").FirstChild.Value);
            _reqPatch = int.Parse(_packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/package/requirements/patch").FirstChild.Value);
            _authorName = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/name").FirstChild.Value;
            _authorUrl = _packageConfig.DocumentElement.SelectSingleNode("/umbPackage/info/author/website").FirstChild.Value;
            
            string virtualBasePath = System.Web.HttpContext.Current.Request.ApplicationPath;
            string basePath = HttpContext.Current.Server.MapPath(virtualBasePath);
            
            foreach (XmlNode n in _packageConfig.DocumentElement.SelectNodes("//file")) {
                bool badFile = false;
                string destPath = getFileName(basePath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgPath")));
                string destFile = getFileName(destPath, xmlHelper.GetNodeValue(n.SelectSingleNode("orgName")));

                if (destPath.ToLower().Contains("\\app_code"))
                    badFile = true;
                
                 if (destPath.ToLower().Contains("\\bin"))
                    badFile = true;

                 if (destFile.ToLower().EndsWith(".dll"))
                     badFile = true;

                 if (badFile) {
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
                        this._conflictingMacroAliases.Add(m.Name, alias);
                    }
                    catch (IndexOutOfRangeException) { } //thrown when the alias doesn't exist in the DB, ie - macro not there
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
                        this._conflictingTemplateAliases.Add(t.Text, alias);
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
                        this._conflictingStyleSheetNames.Add(s.Text, alias);
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

        private string unPack(string ZipName)
        {
            // Unzip
            string tempDir = HttpContext.Current.Server.MapPath(GlobalSettings.StorageDirectory) + Path.DirectorySeparatorChar + Guid.NewGuid().ToString();
            Directory.CreateDirectory(tempDir);

            ZipInputStream s = new ZipInputStream(File.OpenRead(ZipName));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
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
            File.Delete(ZipName);

            return tempDir;

        }

        //this uses the old method of fetching and only supports the packages.umbraco.org repository.
        public string Fetch(Guid Package)
        {

            // Check for package directory
            if (!System.IO.Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.StorageDirectory + "\\packages")))
                System.IO.Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.StorageDirectory + "\\packages"));

            System.Net.WebClient wc = new System.Net.WebClient();

            wc.DownloadFile(
                "http://" + UmbracoSettings.PackageServer + "/fetch?package=" + Package.ToString(),
                System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.StorageDirectory + "\\packages\\" + Package.ToString() + ".umb"));

            return "packages\\" + Package.ToString() + ".umb";
        }


        public static void updatePackageInfo(Guid Package, int VersionMajor, int VersionMinor, int VersionPatch, User User) {

        }
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
                SqlHelper.ExecuteNonQuery("INSERT INTO umbracoInstalledPackages (uninstalled, upgradeId, installDate, userId, versionMajor, versionMinor, versionPatch) VALUES (@uninstalled, @upgradeId, @installDate, @userId, @versionMajor, @versionMinor, @versionPatch)",values);
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
