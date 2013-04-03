using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Web;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace umbraco.cms.businesslogic.skinning
{

    //TODO: Convert the caching to use ApplicationContext.Current.ApplicationCache

    public class Skinning
    {
        static private readonly Hashtable CheckedPages = new Hashtable();
        static private XmlDocument _skinningXmlContent;
        static private string _skinningXmlSource = IOHelper.MapPath(SystemFiles.SkinningXml, false);

        private const string Cachekey = "SkinnableTemplates";

        private static void ClearCheckPages()
        {
            CheckedPages.Clear();
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
            Save();
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

                Save();
            }
        }
        
        private static void Save()
        {
            var f = File.Open(_skinningXmlSource, FileMode.Create);
            SkinXml.Save(f);
            f.Close();
        }

        public static string GetCurrentSkinAlias(int templateID)
        {
            var x = (XmlElement)GetTemplate(templateID);
            if (x != null && x.HasAttribute("alias") && !string.IsNullOrEmpty(x.Attributes["alias"].Value))
                return x.Attributes["alias"].Value;

            return string.Empty;
        }

        private static void SetSkin(int templateId, string skinAlias)
        {
            var x = (XmlElement)GetTemplate(templateId);
            if (x == null)
            {
                x = (XmlElement)_skinningXmlContent.CreateNode(XmlNodeType.Element, "skin", "");
                SkinXml.DocumentElement.AppendChild(x);
            }

            x.SetAttribute("template", templateId.ToString());
            x.SetAttribute("alias", skinAlias);
            Save();

            ClearCheckPages();
        }

        private static void RemoveSkin(int templateId)
        {
            var x = (XmlElement)GetTemplate(templateId);
            if (x != null)
            {
                x.ParentNode.RemoveChild(x);
                Save();
                ClearCheckPages();
            }
        }
        
        private static XmlNode GetTemplate(int templateId)
        {
            var x = SkinXml.SelectSingleNode("/skinning/skin [@template=" + templateId.ToString() + "]");
            return x;
        }

        public static List<Skin> GetAllSkins()
        {
            var skins = new List<Skin>();

            foreach (var dir in Directory.GetDirectories(IOHelper.MapPath(SystemDirectories.Masterpages)))
            {
                if (File.Exists(Path.Combine(dir, "skin.xml")))
                {
                    var s = Skin.CreateFromFile((Path.Combine(dir, "skin.xml")));
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

                    if (!File.Exists(_skinningXmlSource))
                    {
                        var f = File.Open(_skinningXmlSource, FileMode.Create);
                        var sw = new StreamWriter(f);
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
            var dts = (Dictionary<string, Dictionary<string, string>>)HttpRuntime.Cache[Cachekey];
            if (dts == null)
                dts = RegisterSkinnableTemplates();

            return dts;
        }

        //this is an pretty expensive operation, so we will cache the result...
        private static Dictionary<string, Dictionary<string, string>> RegisterSkinnableTemplates()
        {
            HttpRuntime.Cache.Remove(Cachekey);

            var allowedTemplates = new Dictionary<string, Dictionary<string, string>>();

            foreach (string dir in Directory.GetDirectories(IOHelper.MapPath(SystemDirectories.Masterpages)))
            {
                if (File.Exists(Path.Combine(dir, "skin.xml")))
                {
                    var manifest = new XmlDocument();
                    manifest.Load(Path.Combine(dir, "skin.xml"));

                    var name = XmlHelper.GetNodeValue(manifest.SelectSingleNode("/Skin/Name"));
                    var types = XmlHelper.GetNodeValue(manifest.SelectSingleNode("/Skin/AllowedRootTemplate")).Split(',');
                    var alias = new DirectoryInfo(dir).Name;

                    //foreach allowed type, test if it is already there...
                    foreach (var t in types)
                    {
                        if (!allowedTemplates.ContainsKey(t))
                            allowedTemplates.Add(t, new Dictionary<string, string>());

                        if (!allowedTemplates[t].ContainsKey(alias))
                            allowedTemplates[t].Add(alias, name);
                    }
                }
            }
            HttpRuntime.Cache.Insert(Cachekey, allowedTemplates, new CacheDependency(IOHelper.MapPath(SystemDirectories.Masterpages)));

            return allowedTemplates;
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
            var template = new Template(templateID);
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
            foreach (var p in packager.InstalledPackage.GetAllInstalledPackages())
            {
                if (p.Data.EnableSkins)
                    return true;

            }
            return false;
        }

        public static Guid? StarterKitGuid()
        {
            foreach (var p in packager.InstalledPackage.GetAllInstalledPackages())
            {
                if (p.Data.EnableSkins)
                    return new Guid(p.Data.PackageGuid);

            }
            return null;
        }

        public static Guid? StarterKitGuid(int template)
        {
            string packageFile = IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config";
            var installed = new XmlDocument();
            if (File.Exists(packageFile))
            {
                installed.Load(packageFile);

                var starterKit = installed.SelectSingleNode(
                    string.Format("//package [@enableSkins = 'True' and @packageGuid != '' and contains(./templates, '{0}')]", template));

                if (starterKit != null)
                    return new Guid(starterKit.Attributes["packageGuid"].Value);
            }

            return null;
        }

        public static bool HasAvailableSkins(int template)
        {
            var r = false;

            var g = StarterKitGuid(template);

            if (g != null)
            {
                try
                {
                    var skinRepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

                    if (packager.InstalledPackage.GetByGuid(g.ToString()).Data.SkinRepoGuid != null &&
                        packager.InstalledPackage.GetByGuid(g.ToString()).Data.SkinRepoGuid != Guid.Empty)
                    {
                        skinRepoGuid = packager.InstalledPackage.GetByGuid(g.ToString()).Data.SkinRepoGuid.ToString();
                    }


                    packager.repositories.Repository repo = packager.repositories.Repository.getByGuid(skinRepoGuid);

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
            var installed = new XmlDocument();
            installed.Load(IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config");

            var packageNode = installed.SelectSingleNode(
                string.Format("//package [@packageGuid = '{0}']", PackageGuid.ToString()));

            return packageNode != null;
        }

        public static bool IsPackageInstalled(string Name)
        {
            var installed = new XmlDocument();
            installed.Load(IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config");

            var packageNode = installed.SelectSingleNode(
                string.Format("//package [@name = '{0}']", Name));

            return packageNode != null;
        }

        public static string GetModuleAlias(string Name)
        {
            var installed = new XmlDocument();
            installed.Load(IOHelper.MapPath(SystemDirectories.Packages) + "/installed/installedPackages.config");

            var packageNode = installed.SelectSingleNode(
                string.Format("//package [@name = '{0}']", Name));

            var macroNode = packageNode.SelectSingleNode(".//macros");

            var m = new macro.Macro(Convert.ToInt32(macroNode.InnerText.Split(',')[0]));

            return m.Alias;
        }

    }
}
