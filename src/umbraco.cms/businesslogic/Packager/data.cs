using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace umbraco.cms.businesslogic.packager
{
    /// <summary>
    /// This is the xml data for installed packages. This is not the same xml as a pckage format!
    /// </summary>
    public class data
    {
        private static XmlDocument _source;

        public static XmlDocument Source
        {
            get
            {
                return _source;
            }
        }

        public static void Reload(string dataSource)
        {
            //do some error checking and create the folders/files if they don't exist
            if (!File.Exists(dataSource))
            {
                if (!Directory.Exists(IOHelper.MapPath(Settings.PackagerRoot)))
                {
                    Directory.CreateDirectory(IOHelper.MapPath(Settings.PackagerRoot));
                }
                if (!Directory.Exists(IOHelper.MapPath(Settings.PackagesStorage)))
                {
                    Directory.CreateDirectory(IOHelper.MapPath(Settings.PackagesStorage));
                }
                if (!Directory.Exists(IOHelper.MapPath(Settings.InstalledPackagesStorage)))
                {
                    Directory.CreateDirectory(IOHelper.MapPath(Settings.InstalledPackagesStorage));
                }

                using (StreamWriter sw = File.CreateText(dataSource))
                {
                    sw.Write(umbraco.cms.businesslogic.Packager.FileResources.PackageFiles.Packages);
                    sw.Flush();
                }

            }

            if (_source == null)
            {
                _source = new XmlDocument();
            }

            //error checking here
            if (File.Exists(dataSource))
            {
                var isEmpty = false;
                using (var sr = new StreamReader(dataSource))
                {
                    if (sr.ReadToEnd().IsNullOrWhiteSpace())
                    {
                        isEmpty = true;
                    }
                }
                if (isEmpty)
                {
                    File.WriteAllText(dataSource, @"<?xml version=""1.0"" encoding=""utf-8""?><packages></packages>");
                }
            }

            _source.Load(dataSource);
        }

        public static XmlNode GetFromId(int Id, string dataSource, bool reload)
        {
            if (reload)
                Reload(dataSource);

            return Source.SelectSingleNode("/packages/package [@id = '" + Id.ToString().ToUpper() + "']");
        }

        public static XmlNode GetFromGuid(string guid, string dataSource, bool reload)
        {
            if (reload)
                Reload(dataSource);

            return Source.SelectSingleNode("/packages/package [@packageGuid = '" + guid + "']");
        }

        public static PackageInstance MakeNew(string Name, string dataSource)
        {
            Reload(dataSource);

            int maxId = 1;
            // Find max id
            foreach (XmlNode n in Source.SelectNodes("packages/package"))
            {
                if (int.Parse(n.Attributes.GetNamedItem("id").Value) >= maxId)
                    maxId = int.Parse(n.Attributes.GetNamedItem("id").Value) + 1;
            }

            XmlElement instance = Source.CreateElement("package");
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "id", maxId.ToString()));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "version", ""));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "url", ""));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "name", Name));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "folder", Guid.NewGuid().ToString()));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "packagepath", ""));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "repositoryGuid", ""));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "iconUrl", ""));
            //set to current version
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "umbVersion", UmbracoVersion.Current.ToString(3)));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "packageGuid", Guid.NewGuid().ToString()));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "hasUpdate", "false"));

            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "enableSkins", "false"));
            instance.Attributes.Append(XmlHelper.AddAttribute(Source, "skinRepoGuid", ""));

            XmlElement license = Source.CreateElement("license");
            license.InnerText = "MIT License";
            license.Attributes.Append(XmlHelper.AddAttribute(Source, "url", "http://opensource.org/licenses/MIT"));
            instance.AppendChild(license);

            XmlElement author = Source.CreateElement("author");
            author.InnerText = "";
            author.Attributes.Append(XmlHelper.AddAttribute(Source, "url", ""));
            instance.AppendChild(author);

            instance.AppendChild(XmlHelper.AddTextNode(Source, "readme", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "actions", ""));

            instance.AppendChild(XmlHelper.AddTextNode(Source, "datatypes", ""));

            XmlElement content = Source.CreateElement("content");
            content.InnerText = "";
            content.Attributes.Append(XmlHelper.AddAttribute(Source, "nodeId", ""));
            content.Attributes.Append(XmlHelper.AddAttribute(Source, "loadChildNodes", "false"));
            instance.AppendChild(content);

            instance.AppendChild(XmlHelper.AddTextNode(Source, "templates", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "stylesheets", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "documenttypes", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "macros", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "files", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "languages", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "dictionaryitems", ""));
            instance.AppendChild(XmlHelper.AddTextNode(Source, "loadcontrol", ""));

            Source.SelectSingleNode("packages").AppendChild(instance);
            Source.Save(dataSource);
            var retVal = data.Package(maxId, dataSource);

            return retVal;
        }

        public static PackageInstance Package(int id, string datasource)
        {
            return ConvertXmlToPackage(GetFromId(id, datasource, true));
        }

        public static PackageInstance Package(string guid, string datasource)
        {
            XmlNode node = GetFromGuid(guid, datasource, true);
            if (node != null)
                return ConvertXmlToPackage(node);
            else
                return new PackageInstance();
        }

        public static List<PackageInstance> GetAllPackages(string dataSource)
        {
            Reload(dataSource);
            XmlNodeList nList = data.Source.SelectNodes("packages/package");

            List<PackageInstance> retVal = new List<PackageInstance>();

            for (int i = 0; i < nList.Count; i++)
            {
                try
                {
                    retVal.Add(ConvertXmlToPackage(nList[i]));
                }
                catch (Exception ex)
                {
                    LogHelper.Error<data>("An error occurred in GetAllPackages", ex);
                }
            }

            return retVal;
        }

        private static PackageInstance ConvertXmlToPackage(XmlNode n)
        {
            PackageInstance retVal = new PackageInstance();

            if (n != null)
            {
                retVal.Id = int.Parse(SafeAttribute("id", n));
                retVal.Name = SafeAttribute("name", n);
                retVal.Folder = SafeAttribute("folder", n);
                retVal.PackagePath = SafeAttribute("packagepath", n);
                retVal.Version = SafeAttribute("version", n);
                retVal.Url = SafeAttribute("url", n);
                retVal.RepositoryGuid = SafeAttribute("repositoryGuid", n);
                retVal.PackageGuid = SafeAttribute("packageGuid", n);
                retVal.HasUpdate = bool.Parse(SafeAttribute("hasUpdate", n));

                retVal.IconUrl = SafeAttribute("iconUrl", n);
                var umbVersion = SafeAttribute("umbVersion", n);
                Version parsedVersion;
                if (umbVersion.IsNullOrWhiteSpace() == false && Version.TryParse(umbVersion, out parsedVersion))
                {
                    retVal.UmbracoVersion = parsedVersion;
                }

                bool enableSkins = false;
                bool.TryParse(SafeAttribute("enableSkins", n), out enableSkins);
                retVal.EnableSkins = enableSkins;

                retVal.SkinRepoGuid = string.IsNullOrEmpty(SafeAttribute("skinRepoGuid", n)) ? Guid.Empty : new Guid(SafeAttribute("skinRepoGuid", n));

                retVal.License = SafeNodeValue(n.SelectSingleNode("license"));
                retVal.LicenseUrl = n.SelectSingleNode("license").Attributes.GetNamedItem("url").Value;

                retVal.Author = SafeNodeValue(n.SelectSingleNode("author"));
                retVal.AuthorUrl = SafeAttribute("url", n.SelectSingleNode("author"));

                retVal.Readme = SafeNodeValue(n.SelectSingleNode("readme"));
                retVal.Actions = SafeNodeInnerXml(n.SelectSingleNode("actions"));

                retVal.ContentNodeId = SafeAttribute("nodeId", n.SelectSingleNode("content"));
                retVal.ContentLoadChildNodes = bool.Parse(SafeAttribute("loadChildNodes", n.SelectSingleNode("content")));

                retVal.Macros = new List<string>(SafeNodeValue(n.SelectSingleNode("macros")).Trim(',').Split(','));
                retVal.Macros = new List<string>(SafeNodeValue(n.SelectSingleNode("macros")).Trim(',').Split(','));
                retVal.Templates = new List<string>(SafeNodeValue(n.SelectSingleNode("templates")).Trim(',').Split(','));
                retVal.Stylesheets = new List<string>(SafeNodeValue(n.SelectSingleNode("stylesheets")).Trim(',').Split(','));
                retVal.Documenttypes = new List<string>(SafeNodeValue(n.SelectSingleNode("documenttypes")).Trim(',').Split(','));
                retVal.Languages = new List<string>(SafeNodeValue(n.SelectSingleNode("languages")).Trim(',').Split(','));
                retVal.DictionaryItems = new List<string>(SafeNodeValue(n.SelectSingleNode("dictionaryitems")).Trim(',').Split(','));
                retVal.DataTypes = new List<string>(SafeNodeValue(n.SelectSingleNode("datatypes")).Trim(',').Split(','));

                XmlNodeList xmlFiles = n.SelectNodes("files/file");
                retVal.Files = new List<string>();

                for (int i = 0; i < xmlFiles.Count; i++)
                    retVal.Files.Add(xmlFiles[i].InnerText);

                retVal.LoadControl = SafeNodeValue(n.SelectSingleNode("loadcontrol"));
            }

            return retVal;
        }

        public static void Delete(int Id, string dataSource)
        {
            Reload(dataSource);
            // Remove physical xml file if any
            //PackageInstance p = new PackageInstance(Id);

            //TODO DELETE PACKAGE FOLDER...
            //p.Folder

            XmlNode n = data.GetFromId(Id, dataSource, true);
            if (n != null)
            {
                data.Source.SelectSingleNode("/packages").RemoveChild(n);
                data.Source.Save(dataSource);
            }

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is no longer in use and will be removed in the future")]
        public static void UpdateValue(XmlNode n, string Value)
        {
            if (n.FirstChild != null)
                n.FirstChild.Value = Value;
            else
            {
                n.AppendChild(Source.CreateTextNode(Value));
            }
        }

        public static void Save(PackageInstance package, string dataSource)
        {
            Reload(dataSource);
            var xmlDef = GetFromId(package.Id, dataSource, false);
            XmlHelper.SetAttribute(Source, xmlDef, "name", package.Name);
            XmlHelper.SetAttribute(Source, xmlDef, "version", package.Version);
            XmlHelper.SetAttribute(Source, xmlDef, "url", package.Url);
            XmlHelper.SetAttribute(Source, xmlDef, "packagepath", package.PackagePath);
            XmlHelper.SetAttribute(Source, xmlDef, "repositoryGuid", package.RepositoryGuid);
            XmlHelper.SetAttribute(Source, xmlDef, "packageGuid", package.PackageGuid);
            XmlHelper.SetAttribute(Source, xmlDef, "hasUpdate", package.HasUpdate.ToString());
            XmlHelper.SetAttribute(Source, xmlDef, "enableSkins", package.EnableSkins.ToString());
            XmlHelper.SetAttribute(Source, xmlDef, "skinRepoGuid", package.SkinRepoGuid.ToString());
            XmlHelper.SetAttribute(Source, xmlDef, "iconUrl", package.IconUrl);
            if (package.UmbracoVersion != null)
            {
                XmlHelper.SetAttribute(Source, xmlDef, "umbVersion", package.UmbracoVersion.ToString(3));
            }
            

            var licenseNode = xmlDef.SelectSingleNode("license");
            if (licenseNode == null)
            {
                licenseNode = Source.CreateElement("license");
                xmlDef.AppendChild(licenseNode);
            }
            licenseNode.InnerText = package.License;
            XmlHelper.SetAttribute(Source, licenseNode, "url", package.LicenseUrl);

            var authorNode = xmlDef.SelectSingleNode("author");
            if (authorNode == null)
            {
                authorNode = Source.CreateElement("author");
                xmlDef.AppendChild(authorNode);
            }
            authorNode.InnerText = package.Author;
            XmlHelper.SetAttribute(Source, authorNode, "url", package.AuthorUrl);

            XmlHelper.SetCDataNode(Source, xmlDef, "readme", package.Readme);
            XmlHelper.SetInnerXmlNode(Source, xmlDef, "actions", package.Actions);

            var contentNode = xmlDef.SelectSingleNode("content");
            if (contentNode == null)
            {
                contentNode = Source.CreateElement("content");
                xmlDef.AppendChild(contentNode);
            }
            XmlHelper.SetAttribute(Source, contentNode, "nodeId", package.ContentNodeId);
            XmlHelper.SetAttribute(Source, contentNode, "loadChildNodes", package.ContentLoadChildNodes.ToString());

            XmlHelper.SetTextNode(Source, xmlDef, "macros", JoinList(package.Macros, ','));
            XmlHelper.SetTextNode(Source, xmlDef, "templates", JoinList(package.Templates, ','));
            XmlHelper.SetTextNode(Source, xmlDef, "stylesheets", JoinList(package.Stylesheets, ','));
            XmlHelper.SetTextNode(Source, xmlDef, "documenttypes", JoinList(package.Documenttypes, ','));
            XmlHelper.SetTextNode(Source, xmlDef, "languages", JoinList(package.Languages, ','));
            XmlHelper.SetTextNode(Source, xmlDef, "dictionaryitems", JoinList(package.DictionaryItems, ','));
            XmlHelper.SetTextNode(Source, xmlDef, "datatypes", JoinList(package.DataTypes, ','));

            var filesNode = xmlDef.SelectSingleNode("files");
            if (filesNode == null)
            {
                filesNode = Source.CreateElement("files");
                xmlDef.AppendChild(filesNode);
            }
            filesNode.InnerXml = "";

            foreach (var fileStr in package.Files)
            {
                if (string.IsNullOrWhiteSpace(fileStr) == false)
                    filesNode.AppendChild(XmlHelper.AddTextNode(Source, "file", fileStr));
            }

            XmlHelper.SetTextNode(Source, xmlDef, "loadcontrol", package.LoadControl);

            Source.Save(dataSource);
        }



        private static string SafeAttribute(string name, XmlNode n)
        {
            return n.Attributes == null || n.Attributes[name] == null ? string.Empty : n.Attributes[name].Value;
        }

        private static string SafeNodeValue(XmlNode n)
        {
            try
            {
                return XmlHelper.GetNodeValue(n);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string SafeNodeInnerXml(XmlNode n)
        {
            try
            {
                return n.InnerXml;
            }
            catch
            {
                return string.Empty;
            }
        }


        private static string JoinList(List<string> list, char seperator)
        {
            string retVal = "";
            foreach (string str in list)
            {
                retVal += str + seperator;
            }

            return retVal.Trim(seperator);
        }

        public data()
        {

        }
    }
}
