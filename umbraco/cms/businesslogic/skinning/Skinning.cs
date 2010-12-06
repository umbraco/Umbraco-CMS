using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using umbraco.IO;
using System.IO;
using System.Collections;
using System.Web;
using System.Web.Caching;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace umbraco.cms.businesslogic.skinning
{
    public class Skinning
    {
        static private Hashtable _checkedPages = new Hashtable();
        static private XmlDocument _skinningXmlContent;
        static private string _skinningXmlSource = IOHelper.MapPath(SystemFiles.SkinningXml, false);

        private const string CACHEKEY = "SkinnableTemplates";

        private static void clearCheckPages()
        {
            _checkedPages.Clear();
        }


        public static void RollbackSkin(int template)
        {
            string currentSkin = GetCurrentSkinAlias(template);
            Skin skin = Skin.CreateFromAlias(currentSkin);

            if (skin != null)
            {
                skin.RollbackDependencies();

                if (skin.OverridesTemplates())
                    skin.RollbackTemplateFiles();

                //else
                //    skin.RollbackDependencies();
            }

            RemoveSkin(template);
            save();
        }

        public static void ActivateAsCurrentSkin(Skin skin)
        {
            Template t = Template.GetByAlias(skin.AllowedRootTemplate);
            ActivateAsCurrentSkin(t.Id, skin.Alias);
        }

        public static void ActivateAsCurrentSkin(int template, string skinAlias)
        {
            //lookup template in skinning.config
            string currentSkin = GetCurrentSkinAlias(template);

            //if different from current, and the template is skinned
            if (currentSkin != skinAlias)
            {

                //this will restore the files to the standard runway, as they looked before the skin was applied
                if (currentSkin != string.Empty)
                {
                    RollbackSkin(template);
                }

                Skin newSkin = Skin.CreateFromAlias(skinAlias);

                if (newSkin.OverridesTemplates())
                    newSkin.DeployTemplateFiles();

                SetSkin(template, skinAlias);

                newSkin.ExecuteInstallTasks();

                save();
            }
        }



        private static void save()
        {
            System.IO.FileStream f = System.IO.File.Open(_skinningXmlSource, FileMode.Create);
            SkinXml.Save(f);
            f.Close();
        }

        public static string GetCurrentSkinAlias(int templateID)
        {
            XmlElement x = (XmlElement)getTemplate(templateID);
            if (x != null && x.HasAttribute("alias") && !string.IsNullOrEmpty(x.Attributes["alias"].Value))
                return x.Attributes["alias"].Value;

            return string.Empty;
        }

        private static void SetSkin(int templateID, string skinAlias)
        {
            XmlElement x = (XmlElement)getTemplate(templateID);
            if (x == null)
            {
                x = (XmlElement)_skinningXmlContent.CreateNode(XmlNodeType.Element, "skin", "");
                SkinXml.DocumentElement.AppendChild(x);
            }

            x.SetAttribute("template", templateID.ToString());
            x.SetAttribute("alias", skinAlias);
            save();

            clearCheckPages();
        }

        private static void RemoveSkin(int templateID)
        {
            XmlElement x = (XmlElement)getTemplate(templateID);
            if (x != null)
            {
                x.ParentNode.RemoveChild(x);
                save();
                clearCheckPages();
            }
        }


        private static XmlNode getTemplate(int templateId)
        {
            XmlNode x = SkinXml.SelectSingleNode("/skinning/skin [@template=" + templateId.ToString() + "]");
            return x;
        }

        public static List<Skin> GetAllSkins()
        {
            List<Skin> skins = new List<Skin>();

            foreach (string dir in Directory.GetDirectories(IO.IOHelper.MapPath(SystemDirectories.Masterpages)))
            {
                if (File.Exists(Path.Combine(dir, "skin.xml")))
                {
                    Skin s = Skin.CreateFromFile((Path.Combine(dir, "skin.xml")));
                    s.Alias = new DirectoryInfo(dir).Name;
                    skins.Add(s);
                }
            }
            return skins;
        }


        private static XmlDocument SkinXml
        {
            get
            {
                if (_skinningXmlContent == null)
                {
                    if (_skinningXmlSource == null)
                    {
                        //if we pop it here it'll make for better stack traces ;)
                        _skinningXmlSource = IOHelper.MapPath(SystemFiles.SkinningXml, false);
                    }

                    _skinningXmlContent = new XmlDocument();

                    if (!System.IO.File.Exists(_skinningXmlSource))
                    {
                        System.IO.FileStream f = System.IO.File.Open(_skinningXmlSource, FileMode.Create);
                        System.IO.StreamWriter sw = new StreamWriter(f);
                        sw.WriteLine("<skinning/>");
                        sw.Close();
                        f.Close();
                    }
                    _skinningXmlContent.Load(_skinningXmlSource);
                }
                return _skinningXmlContent;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> SkinnableTemplates()
        {
            Dictionary<string, Dictionary<string, string>> dts = (Dictionary<string, Dictionary<string, string>>)HttpRuntime.Cache[CACHEKEY];
            if (dts == null)
                dts = registerSkinnableTemplates();

            return dts;
        }

        //this is an pretty expensive operation, so we will cache the result...
        private static Dictionary<string, Dictionary<string, string>> registerSkinnableTemplates()
        {
            HttpRuntime.Cache.Remove(CACHEKEY);
            Dictionary<string, Dictionary<string, string>> _allowedTemplates = new Dictionary<string, Dictionary<string, string>>();

            foreach (string dir in Directory.GetDirectories(IO.IOHelper.MapPath(SystemDirectories.Masterpages)))
            {
                if (File.Exists(Path.Combine(dir, "skin.xml")))
                {
                    XmlDocument manifest = new XmlDocument();
                    manifest.Load(Path.Combine(dir, "skin.xml"));

                    string name = umbraco.xmlHelper.GetNodeValue(manifest.SelectSingleNode("/Skin/Name"));
                    string[] types = umbraco.xmlHelper.GetNodeValue(manifest.SelectSingleNode("/Skin/AllowedRootTemplate")).Split(',');
                    string alias = new DirectoryInfo(dir).Name;

                    //foreach allowed type, test if it is already there...
                    foreach (string t in types)
                    {
                        if (!_allowedTemplates.ContainsKey(t))
                            _allowedTemplates.Add(t, new Dictionary<string, string>());

                        if (!_allowedTemplates[t].ContainsKey(alias))
                            _allowedTemplates[t].Add(alias, name);
                    }
                }
            }
            HttpRuntime.Cache.Insert(CACHEKEY, _allowedTemplates, new CacheDependency(IO.IOHelper.MapPath(SystemDirectories.Masterpages)));

            return _allowedTemplates;
        }

        //Helpers for detecting what skins work with what document types
        public static bool IsSkinnable(string templateAlias)
        {
            return SkinnableTemplates().ContainsKey(templateAlias);
        }

        public static bool IsSkinnable(Template template)
        {
            return IsSkinnable(template.Alias);
        }


        public static Dictionary<string, string> AllowedSkins(int templateID)
        {
            Template template = new Template(templateID);
            return AllowedSkins(template.Alias);
        }

        public static Dictionary<string, string> AllowedSkins(Template template)
        {
            return AllowedSkins(template.Alias);
        }

        public static Dictionary<string, string> AllowedSkins(string templateAlias)
        {
            if (IsSkinnable(templateAlias))
            {
                return SkinnableTemplates()[templateAlias];
            }
            else
                return new Dictionary<string, string>();
        }

        public static bool IsStarterKitInstalled()
        {
            foreach (packager.InstalledPackage p in packager.InstalledPackage.GetAllInstalledPackages())
            {
                if (p.Data.EnableSkins)
                    return true;

            }
            return false;
        }

        public static Guid? StarterKitGuid()
        {
            foreach (packager.InstalledPackage p in packager.InstalledPackage.GetAllInstalledPackages())
            {
                if (p.Data.EnableSkins)
                    return new Guid(p.Data.PackageGuid);

            }
            return null;
        }

        public static Guid? StarterKitGuid(int template)
        {
            string packageFile = IO.IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config";
            XmlDocument installed = new XmlDocument();
            if (File.Exists(packageFile))
            {
                installed.Load(packageFile);

                XmlNode starterKit = installed.SelectSingleNode(
                    string.Format("//package [@enableSkins = 'True' and @packageGuid != '' and contains(./templates, '{0}')]", template));

                if (starterKit != null)
                    return new Guid(starterKit.Attributes["packageGuid"].Value);
            }

            return null;
        }

        public static bool HasAvailableSkins(int template)
        {
            bool r = false;

            Guid? g = StarterKitGuid(template);

            if (g != null)
            {
                try
                {
                    string url = umbraco.cms.businesslogic.packager.InstalledPackage.GetByGuid(g.ToString()).Data.SkinWebserviceUrl;
                    umbraco.cms.businesslogic.packager.repositories.Repository repo = cms.businesslogic.packager.repositories.Repository.getByGuid("65194810-1f85-11dd-bd0b-0800200c9a66");

                    if (!string.IsNullOrEmpty(url))
                        repo.WebserviceUrl = url;

                    r = repo.Webservice.Skins(g.ToString()).Length > 0;
                }
                catch { }
            }
            return r;
        }

        public static bool IsSkinInstalled(Guid SkinGuid)
        {

            return IsPackageInstalled(SkinGuid);
        }

        public static bool IsPackageInstalled(Guid PackageGuid)
        {
            XmlDocument installed = new XmlDocument();
            installed.Load(IO.IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config");

            XmlNode packageNode = installed.SelectSingleNode(
                string.Format("//package [@packageGuid = '{0}']", PackageGuid.ToString()));

            return packageNode != null;
        }

        public static bool IsPackageInstalled(string Name)
        {
            XmlDocument installed = new XmlDocument();
            installed.Load(IO.IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config");

            XmlNode packageNode = installed.SelectSingleNode(
                string.Format("//package [@name = '{0}']", Name));

            return packageNode != null;
        }

        public static string GetModuleAlias(string Name)
        {
            XmlDocument installed = new XmlDocument();
            installed.Load(IO.IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config");

            XmlNode packageNode = installed.SelectSingleNode(
                string.Format("//package [@name = '{0}']", Name));

            XmlNode macroNode = packageNode.SelectSingleNode(".//macros");

            cms.businesslogic.macro.Macro m = new cms.businesslogic.macro.Macro(Convert.ToInt32(macroNode.InnerText.Split(',')[0]));

            return m.Alias;
        }
        #region old code



        /*
        
        public static string FindAppliedSkin(int DocumentId, string Path)
        {
            string skinAlias = string.Empty;

            if (!_checkedPages.ContainsKey(DocumentId))
            {
                foreach (string id in Path.Split(','))
                {
                    XmlNode n = getPage(int.Parse(id));
                    if (n != null && n.Attributes["skin"] != null && !string.IsNullOrEmpty(n.Attributes["skin"].Value))
                    {
                        skinAlias = n.Attributes["skin"].Value;
                        break;
                    }
                }

                // Add thread safe updating to the hashtable
                System.Web.HttpContext.Current.Application.Lock();
                if (!_checkedPages.ContainsKey(DocumentId))
                    _checkedPages.Add(DocumentId, skinAlias);

                System.Web.HttpContext.Current.Application.UnLock();
            }
            else
                skinAlias = (string)_checkedPages[DocumentId];

            return skinAlias;
        }
         * 
         * 
        public static Skin AppliedSkin(int DocumentId, string Path)
        {
            string active = FindAppliedSkin(DocumentId, Path);
            if (!string.IsNullOrEmpty(active))
            {
               
                return Skin.CreateFromFile(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + active + "/skin.xml");
            }

            return null;
        }

        
        public static Dictionary<string, Dictionary<string, string>> SkinnableDocumentTypes()
        {            
            Dictionary<string, Dictionary<string, string>> dts = (Dictionary<string, Dictionary<string, string>>)HttpRuntime.Cache[CACHEKEY];
            if (dts == null)
                dts = registerAllowedDocumentTypes();
            
            return dts;
        }
                      
        //this is an pretty expensive operation, so we will cache the result...
        private static Dictionary<string, Dictionary<string, string>> registerAllowedDocumentTypes()
        {
            HttpRuntime.Cache.Remove(CACHEKEY);
            Dictionary<string, Dictionary<string, string>> _allowedDocumentTypes = new Dictionary<string, Dictionary<string, string>>();

            foreach (string dir in Directory.GetDirectories(IO.IOHelper.MapPath(SystemDirectories.Masterpages)))
            {
                if( File.Exists( Path.Combine( dir , "skin.xml" ) )) {
                    XmlDocument manifest = new XmlDocument();
                    manifest.Load(Path.Combine(dir, "skin.xml"));

                    string name = umbraco.xmlHelper.GetNodeValue( manifest.SelectSingleNode("/Skin/Name"));
                    string[] types = umbraco.xmlHelper.GetNodeValue( manifest.SelectSingleNode("/Skin/AllowedDocumentTypes")).Split(',');
                    string alias = new DirectoryInfo(dir).Name;
                    
                    //foreach allowed type, test if it is already there...
                    foreach(string t in types){
                        if (!_allowedDocumentTypes.ContainsKey(t))
                            _allowedDocumentTypes.Add(t, new Dictionary<string,string>());

                        if (!_allowedDocumentTypes[t].ContainsKey(alias))
                            _allowedDocumentTypes[t].Add(alias, name);
                    }
                }
            }
            HttpRuntime.Cache.Insert(CACHEKEY, _allowedDocumentTypes, new CacheDependency( IO.IOHelper.MapPath(SystemDirectories.Masterpages) ));

            return _allowedDocumentTypes;
        }
        
        public static void SetSkin(int DocumentId, string skinAlias)
        {

                XmlElement x = (XmlElement)getPage(DocumentId);
                if (x == null)
                {
                    x = (XmlElement)_skinningXmlContent.CreateNode(XmlNodeType.Element, "page", "");
                    SkinXml.DocumentElement.AppendChild(x);
                }
               
                x.SetAttribute("id", DocumentId.ToString());
                x.SetAttribute("skin", skinAlias);
                save();
                    
                clearCheckPages();
        }
               
        public static void RemoveSkin(int DocumentId)
        {
            XmlElement x = (XmlElement)getPage(DocumentId);
            if (x != null)
            {   
                x.ParentNode.RemoveChild(x);
                save();
                clearCheckPages();
            }
        }
        
        private static XmlNode getPage(int documentId)
        {
            XmlNode x = SkinXml.SelectSingleNode("/skinning/page [@id=" + documentId.ToString() + "]");
            return x;
        }           
        
        //Helpers for detecting what skins work with what document types
        public static bool IsSkinnable(string documentTypeAlias)
        {
            return SkinnableDocumentTypes().ContainsKey(documentTypeAlias);
        }

        public static bool IsSkinnable(DocumentType documentType)
        {
            return IsSkinnable(documentType.Alias);
        }
        

        public static Dictionary<string, string> AllowedSkins(DocumentType documentType)
        {
            return AllowedSkins(documentType.Alias);
        }
        
        public static Dictionary<string, string> AllowedSkins(string documentTypeAlias)
        {
            if (IsSkinnable(documentTypeAlias))
            {
                return SkinnableDocumentTypes()[documentTypeAlias];
            }
            else
                return new Dictionary<string, string>();
        }
         * 
         * 
        */
        #endregion

    }
}
