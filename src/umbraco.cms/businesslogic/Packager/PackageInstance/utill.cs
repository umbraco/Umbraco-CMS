using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.IO;
using System.IO.Compression;
using System.Xml;

using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.macro;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager
{
    /// <summary>
    /// A utillity class for working with packager data.
    /// It provides basic methods for adding new items to a package manifest, moving files and other misc.
    /// </summary>
    public class utill
    {

        /// <summary>
        /// Creates a package manifest containing name, license, version and other meta data.
        /// </summary>
        /// <param name="pack">The packinstance.</param>
        /// <param name="doc">The xml document.</param>
        /// <returns></returns>
        public static XmlNode PackageInfo(PackageInstance pack, XmlDocument doc)
        {

            XmlNode info = doc.CreateElement("info");

            //Package info
            XmlNode package = doc.CreateElement("package");
            package.AppendChild(CreateNode("name", pack.Name, doc));
            package.AppendChild(CreateNode("version", pack.Version, doc));
            package.AppendChild(CreateNode("iconUrl", pack.IconUrl, doc));

            XmlNode license = CreateNode("license", pack.License, doc);
            license.Attributes.Append(CreateAttribute("url", pack.LicenseUrl, doc));
            package.AppendChild(license);

            package.AppendChild(CreateNode("url", pack.Url, doc));

            XmlNode requirements = doc.CreateElement("requirements");
            //NOTE: The defaults are 3.0.0 - I'm just leaving that here since that's the way it's been //SD
            requirements.AppendChild(CreateNode("major", pack.UmbracoVersion == null ? "3" : pack.UmbracoVersion.Major.ToInvariantString(), doc));
            requirements.AppendChild(CreateNode("minor", pack.UmbracoVersion == null ? "0" : pack.UmbracoVersion.Minor.ToInvariantString(), doc));
            requirements.AppendChild(CreateNode("patch", pack.UmbracoVersion == null ? "0" : pack.UmbracoVersion.Build.ToInvariantString(), doc));
            if (pack.UmbracoVersion != null)
            {
                requirements.Attributes.Append(CreateAttribute("type", "strict", doc));
            }
            package.AppendChild(requirements);
            info.AppendChild(package);

            //Author
            XmlNode author = CreateNode("author", "", doc);
            author.AppendChild(CreateNode("name", pack.Author, doc));
            author.AppendChild(CreateNode("website", pack.AuthorUrl, doc));
            info.AppendChild(author);

            info.AppendChild(CreateNode("readme", "<![CDATA[" + pack.Readme + "]]>", doc));

            return info;
        }


        /// <summary>
        /// Converts an umbraco template to a package xml node
        /// </summary>
        /// <param name="templateId">The template id.</param>
        /// <param name="doc">The xml doc.</param>
        /// <returns></returns>
        public static XmlNode Template(int templateId, XmlDocument doc)
        {

            Template tmpl = new Template(templateId);

            XmlNode template = doc.CreateElement("Template");
            template.AppendChild(CreateNode("Name", tmpl.Text, doc));
            template.AppendChild(CreateNode("Alias", tmpl.Alias, doc));

            if (tmpl.MasterTemplate != 0)
            {
                template.AppendChild(CreateNode("Master", new Template(tmpl.MasterTemplate).Alias, doc));
            }
            template.AppendChild(CreateNode("Design", "<![CDATA[" + tmpl.Design + "]]>", doc));

            return template;
        }

        /// <summary>
        /// Converts a umbraco stylesheet to a package xml node
        /// </summary>
        /// <param name="name">The name of the stylesheet.</param>
        /// <param name="includeProperties">if set to <c>true</c> [incluce properties].</param>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        public static XmlNode Stylesheet(string name, bool includeProperties, XmlDocument doc)
        {
            if (doc == null) throw new ArgumentNullException("doc");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");

            if (ApplicationContext.Current == null)
                throw new NullReferenceException("ApplicationContext is null");
            var fileService = ApplicationContext.Current.Services.FileService;

            var sts = fileService.GetStylesheetByName(name);
            var stylesheet = doc.CreateElement("Stylesheet");
            stylesheet.AppendChild(CreateNode("Name", sts.Alias, doc));
            stylesheet.AppendChild(CreateNode("FileName", sts.Name, doc));
            stylesheet.AppendChild(CreateNode("Content", "<![CDATA[" + sts.Content + "]]>", doc));
            if (includeProperties)
            {
                var properties = doc.CreateElement("Properties");
                foreach (var ssP in sts.Properties)
                {
                    var property = doc.CreateElement("Property");
                    property.AppendChild(CreateNode("Name", ssP.Name, doc));
                    property.AppendChild(CreateNode("Alias", ssP.Alias, doc));
                    property.AppendChild(CreateNode("Value", ssP.Value, doc));
                }
                stylesheet.AppendChild(properties);
            }
            return stylesheet;
        }

        /// <summary>
        /// Converts a macro to a package xml node
        /// </summary>
        /// <param name="macroId">The macro id.</param>
        /// <param name="appendFile">if set to <c>true</c> [append file].</param>
        /// <param name="packageDirectory">The package directory.</param>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        public static XmlNode Macro(int macroId, bool appendFile, string packageDirectory, XmlDocument doc)
        {

            Macro mcr = new Macro(macroId);

            if (appendFile)
            {
                if (!string.IsNullOrEmpty(mcr.Xslt))
                    AppendFileToManifest(IOHelper.ResolveUrl(SystemDirectories.Xslt) + "/" + mcr.Xslt, packageDirectory, doc);
                if (!string.IsNullOrEmpty(mcr.ScriptingFile))
                    AppendFileToManifest(IOHelper.ResolveUrl(SystemDirectories.MacroScripts) + "/" + mcr.ScriptingFile, packageDirectory, doc);

                if (!string.IsNullOrEmpty(mcr.Type))
                    AppendFileToManifest(mcr.Type, packageDirectory, doc);
            }

            return mcr.ToXml(doc);
        }


        /// <summary>
        /// Appends a file to package manifest and copies the file to the correct folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="packageDirectory">The package directory.</param>
        /// <param name="doc">The doc.</param>
        public static void AppendFileToManifest(string path, string packageDirectory, XmlDocument doc)
        {

            if (!path.StartsWith("~/") && !path.StartsWith("/"))
                path = "~/" + path;

            string serverPath = IOHelper.MapPath(path);

            if (System.IO.File.Exists(serverPath))
            {

                AppendFileXml(path, packageDirectory, doc);

            }
            else if (System.IO.Directory.Exists(serverPath))
            {

                ProcessDirectory(path, packageDirectory, doc);
            }
        }



        //Process files in directory and add them to package
        private static void ProcessDirectory(string path, string packageDirectory, XmlDocument doc)
        {
            string serverPath = IOHelper.MapPath(path);
            if (System.IO.Directory.Exists(serverPath))
            {

                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(serverPath);

                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    AppendFileXml(path + "/" + file.Name, packageDirectory, doc);
                }

                foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
                {
                    ProcessDirectory(path + "/" + dir.Name, packageDirectory, doc);
                }
            }
        }

        private static void AppendFileXml(string path, string packageDirectory, XmlDocument doc)
        {

            string serverPath = IOHelper.MapPath(path);

            string orgPath = path.Substring(0, (path.LastIndexOf('/')));
            string orgName = path.Substring((path.LastIndexOf('/') + 1));
            string newFileName = orgName;

            if (System.IO.File.Exists(packageDirectory + "/" + orgName))
            {
                string fileGuid = System.Guid.NewGuid().ToString();
                newFileName = fileGuid + "_" + newFileName;
            }

            //Copy file to directory for zipping...
            System.IO.File.Copy(serverPath, packageDirectory + "/" + newFileName, true);

            //Append file info to files xml node
            XmlNode files = doc.SelectSingleNode("/umbPackage/files");

            XmlNode file = doc.CreateElement("file");
            file.AppendChild(CreateNode("guid", newFileName, doc));
            file.AppendChild(CreateNode("orgPath", orgPath == "" ? "/" : orgPath, doc));
            file.AppendChild(CreateNode("orgName", orgName, doc));

            files.AppendChild(file);
        }

        /// <summary>
        /// Determines whether the file is in the package manifest
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="doc">The doc.</param>
        /// <returns>
        /// 	<c>true</c> if [is file in manifest]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFileInManifest(string guid, XmlDocument doc)
        {
            return false;
        }

        private static XmlNode CreateNode(string name, string value, XmlDocument doc)
        {
            var node = doc.CreateElement(name);
            node.InnerXml = value;
            return node;
        }

        private static XmlAttribute CreateAttribute(string name, string value, XmlDocument doc)
        {
            var attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        /// <summary>
        /// Zips the package.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="savePath">The save path.</param>
        public static void ZipPackage(string Path, string savePath)
        {
            ArrayList ar = GenerateFileList(Path);
            // generate file list
            // find number of chars to remove from orginal file path

            using (var memoryStream = new MemoryStream())
            {   
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string file in ar)
                    {
                        if (file.EndsWith(@"/") != true) // it's not a directory
                        {
                            archive.CreateEntryFromFile(file, System.IO.Path.GetFileName(file));
                        }
                    }
                }

                using (var fileStream = new FileStream(savePath, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }

        }

        private static ArrayList GenerateFileList(string Dir)
        {
            ArrayList mid = new ArrayList();

            bool Empty = true;

            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                mid.Add(file);
                Empty = false;
            }

            if (Empty)
            {
                if (Directory.GetDirectories(Dir).Length == 0) // if directory is completely empty, add it
                {
                    mid.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // do this recursively
            {
                foreach (object obj in GenerateFileList(dirs))
                {
                    mid.Add(obj);
                }
            }
            return mid;  // return file list
        }
    }
}
