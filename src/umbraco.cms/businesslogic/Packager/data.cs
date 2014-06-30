using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace umbraco.cms.businesslogic.packager
{
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

                StreamWriter sw = File.CreateText(dataSource);
                sw.Write(umbraco.cms.businesslogic.Packager.FileResources.PackageFiles.Packages);
                sw.Flush();
                sw.Close();
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
            if(reload)
                Reload(dataSource);

            return Source.SelectSingleNode("/packages/package [@id = '" + Id.ToString().ToUpper() + "']");
        }

        public static XmlNode GetFromGuid(string guid, string dataSource, bool reload) {
            if (reload)
                Reload(dataSource);

            return Source.SelectSingleNode("/packages/package [@packageGuid = '" + guid + "']");
        }

        public static PackageInstance MakeNew(string Name, string dataSource)
        {
            PackageInstance retVal = new PackageInstance();

            try
            {
                Reload(dataSource);

                int _maxId = 1;
                // Find max id
                foreach (XmlNode n in Source.SelectNodes("packages/package"))
                {
                    if (int.Parse(n.Attributes.GetNamedItem("id").Value) >= _maxId)
                        _maxId = int.Parse(n.Attributes.GetNamedItem("id").Value) + 1;
                }

                XmlElement instance = Source.CreateElement("package");
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "id", _maxId.ToString()));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "version", ""));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "url", ""));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "name", Name));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "folder", System.Guid.NewGuid().ToString()));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "packagepath", ""));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "repositoryGuid", ""));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "packageGuid", System.Guid.NewGuid().ToString()));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "hasUpdate", "false"));

                instance.Attributes.Append(xmlHelper.addAttribute(Source, "enableSkins", "false"));
                instance.Attributes.Append(xmlHelper.addAttribute(Source, "skinRepoGuid", ""));

                XmlElement license = Source.CreateElement("license");
                license.InnerText = "MIT License";
                license.Attributes.Append(xmlHelper.addAttribute(Source, "url", "http://opensource.org/licenses/MIT"));
                instance.AppendChild(license);

                XmlElement author = Source.CreateElement("author");
                author.InnerText = "";
                author.Attributes.Append(xmlHelper.addAttribute(Source, "url", ""));
                instance.AppendChild(author);
                
                instance.AppendChild(xmlHelper.addTextNode(Source, "readme", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "actions", ""));

                instance.AppendChild(xmlHelper.addTextNode(Source, "datatypes", ""));

                XmlElement content = Source.CreateElement("content");
                content.InnerText = "";
                content.Attributes.Append(xmlHelper.addAttribute(Source, "nodeId", ""));
                content.Attributes.Append(xmlHelper.addAttribute(Source, "loadChildNodes", "false"));
                instance.AppendChild(content);

                instance.AppendChild(xmlHelper.addTextNode(Source, "templates", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "stylesheets", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "documenttypes", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "macros", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "files", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "languages", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "dictionaryitems", ""));
                instance.AppendChild(xmlHelper.addTextNode(Source, "loadcontrol", ""));

                Source.SelectSingleNode("packages").AppendChild(instance);
                Source.Save(dataSource);
                retVal = data.Package(_maxId, dataSource);
            }
            catch (Exception ex)
            {
				LogHelper.Error<data>("An error occurred", ex);
            }

            return retVal;
        }

        public static PackageInstance Package(int id, string datasource) {
            return ConvertXmlToPackage( GetFromId(id, datasource, true) );
        }

        public static PackageInstance Package(string guid, string datasource) {
			try
			{
				XmlNode node = GetFromGuid(guid, datasource, true);
				if (node != null)
					return ConvertXmlToPackage(node);
				else
					return new PackageInstance();
			}
			catch (Exception ex)
			{
				LogHelper.Error<data>("An error occurred", ex);
				return new PackageInstance();
			}
        }

        public static List<PackageInstance> GetAllPackages(string dataSource) {
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

        private static PackageInstance ConvertXmlToPackage(XmlNode n) {
            PackageInstance retVal = new PackageInstance();
                
                if (n != null) {
                    retVal.Id = int.Parse(safeAttribute("id",n));
                    retVal.Name = safeAttribute("name",n);
                    retVal.Folder = safeAttribute("folder", n);
                    retVal.PackagePath = safeAttribute("packagepath", n);
                    retVal.Version = safeAttribute("version", n);
                    retVal.Url = safeAttribute("url", n);
                    retVal.RepositoryGuid = safeAttribute("repositoryGuid", n);
                    retVal.PackageGuid = safeAttribute("packageGuid", n);
                    retVal.HasUpdate = bool.Parse(safeAttribute("hasUpdate",n));

                    bool _enableSkins = false;
                    bool.TryParse(safeAttribute("enableSkins", n), out _enableSkins);
                    retVal.EnableSkins = _enableSkins;

                    retVal.SkinRepoGuid = string.IsNullOrEmpty(safeAttribute("skinRepoGuid", n)) ? Guid.Empty : new Guid(safeAttribute("skinRepoGuid", n));

                    retVal.License = safeNodeValue(n.SelectSingleNode("license"));
                    retVal.LicenseUrl = n.SelectSingleNode("license").Attributes.GetNamedItem("url").Value;

                    retVal.Author = safeNodeValue(n.SelectSingleNode("author"));
                    retVal.AuthorUrl = safeAttribute("url", n.SelectSingleNode("author"));

                    retVal.Readme = safeNodeValue(n.SelectSingleNode("readme"));
                    retVal.Actions = safeNodeInnerXml(n.SelectSingleNode("actions"));

                    retVal.ContentNodeId = safeAttribute("nodeId", n.SelectSingleNode("content"));
                    retVal.ContentLoadChildNodes = bool.Parse(safeAttribute("loadChildNodes",n.SelectSingleNode("content")));
                    

                    retVal.Macros = new List<string>(safeNodeValue(n.SelectSingleNode("macros")).Trim(',').Split(','));
                    retVal.Macros = new List<string>(safeNodeValue(n.SelectSingleNode("macros")).Trim(',').Split(','));
                    retVal.Templates = new List<string>(safeNodeValue(n.SelectSingleNode("templates")).Trim(',').Split(','));
                    retVal.Stylesheets = new List<string>(safeNodeValue(n.SelectSingleNode("stylesheets")).Trim(',').Split(','));
                    retVal.Documenttypes = new List<string>(safeNodeValue(n.SelectSingleNode("documenttypes")).Trim(',').Split(','));
                    retVal.Languages = new List<string>(safeNodeValue(n.SelectSingleNode("languages")).Trim(',').Split(','));
                    retVal.DictionaryItems = new List<string>(safeNodeValue(n.SelectSingleNode("dictionaryitems")).Trim(',').Split(','));
                    retVal.DataTypes = new List<string>(safeNodeValue(n.SelectSingleNode("datatypes")).Trim(',').Split(','));

                    XmlNodeList xmlFiles = n.SelectNodes("files/file");
                    retVal.Files = new List<string>();

                    for (int i = 0; i < xmlFiles.Count; i++)
                        retVal.Files.Add(xmlFiles[i].InnerText);

                    retVal.LoadControl = safeNodeValue(n.SelectSingleNode("loadcontrol"));
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

		public static void UpdateValue(XmlNode n, string Value) 
		{
			if (n.FirstChild != null)
				n.FirstChild.Value = Value;
			else 
			{
                n.AppendChild(Source.CreateTextNode(Value));
			}
			//Save();
		}

		public static void Save(PackageInstance package, string dataSource) 
		{
			try
			{
                Reload(dataSource);
                XmlNode _xmlDef = GetFromId(package.Id, dataSource, false);
                _xmlDef.Attributes.GetNamedItem("name").Value = package.Name;
                _xmlDef.Attributes.GetNamedItem("version").Value = package.Version;
                _xmlDef.Attributes.GetNamedItem("url").Value = package.Url;
                _xmlDef.Attributes.GetNamedItem("packagepath").Value = package.PackagePath;
                _xmlDef.Attributes.GetNamedItem("repositoryGuid").Value = package.RepositoryGuid;
                _xmlDef.Attributes.GetNamedItem("packageGuid").Value = package.PackageGuid;

                _xmlDef.Attributes.GetNamedItem("hasUpdate").Value = package.HasUpdate.ToString();
                _xmlDef.Attributes.GetNamedItem("enableSkins").Value = package.EnableSkins.ToString();
                _xmlDef.Attributes.GetNamedItem("skinRepoGuid").Value = package.SkinRepoGuid.ToString();

                    

                _xmlDef.SelectSingleNode("license").FirstChild.Value = package.License;
                _xmlDef.SelectSingleNode("license").Attributes.GetNamedItem("url").Value = package.LicenseUrl;

                _xmlDef.SelectSingleNode("author").InnerText = package.Author;
                _xmlDef.SelectSingleNode("author").Attributes.GetNamedItem("url").Value = package.AuthorUrl;

                _xmlDef.SelectSingleNode("readme").InnerXml = "<![CDATA[" + package.Readme + "]]>";

                if(_xmlDef.SelectSingleNode("actions") == null)
                    _xmlDef.AppendChild(xmlHelper.addTextNode(Source, "actions", ""));

                _xmlDef.SelectSingleNode("actions").InnerXml = package.Actions;

                _xmlDef.SelectSingleNode("content").Attributes.GetNamedItem("nodeId").Value =  package.ContentNodeId.ToString();
                _xmlDef.SelectSingleNode("content").Attributes.GetNamedItem("loadChildNodes").Value = package.ContentLoadChildNodes.ToString();
                
                _xmlDef.SelectSingleNode("macros").InnerText = joinList(package.Macros, ',');
                _xmlDef.SelectSingleNode("templates").InnerText = joinList(package.Templates, ',');
                _xmlDef.SelectSingleNode("stylesheets").InnerText = joinList(package.Stylesheets, ',');
                _xmlDef.SelectSingleNode("documenttypes").InnerText = joinList(package.Documenttypes, ',');

                _xmlDef.SelectSingleNode("languages").InnerText = joinList(package.Languages, ',');
                _xmlDef.SelectSingleNode("dictionaryitems").InnerText = joinList(package.DictionaryItems, ',');
                _xmlDef.SelectSingleNode("datatypes").InnerText = joinList(package.DataTypes, ',');
                
                _xmlDef.SelectSingleNode("files").InnerXml = "";

                foreach (string fileStr in package.Files) {
                    if(!string.IsNullOrEmpty(fileStr.Trim()))
                        _xmlDef.SelectSingleNode("files").AppendChild(xmlHelper.addTextNode(data.Source, "file", fileStr));
                }

                _xmlDef.SelectSingleNode("loadcontrol").InnerText = package.LoadControl;

                Source.Save(dataSource);

               
            }
			catch(Exception F)
			{
				LogHelper.Error<data>("An error occurred", F);
			}   
			
		}

        private static string safeAttribute(string name, XmlNode n) {
            try {
                return n.Attributes.GetNamedItem(name).Value;
            } catch {
                return "";
            }
        }

        private static string safeNodeValue(XmlNode n) {
            try {
                return xmlHelper.GetNodeValue(n);
            } catch {
                return "";
            }
        }

        private static string safeNodeInnerXml(XmlNode n) {
            try {
                return n.InnerXml;
            } catch {
                return "";
            }
        }


        private static string joinList(List<string> list, char seperator) {
            string retVal = "";
            foreach (string str in list) {
                retVal += str + seperator.ToString();
            }

            return retVal.Trim(seperator);
        }

		public data()
		{

		}
    }
}
