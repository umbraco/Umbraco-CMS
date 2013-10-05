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
using System.Xml;

using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.macro;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager {
    /// <summary>
    /// A utillity class for working with packager data.
    /// It provides basic methods for adding new items to a package manifest, moving files and other misc.
    /// </summary>
    public class utill {
        
        /// <summary>
        /// Creates a package manifest containing name, license, version and other meta data.
        /// </summary>
        /// <param name="pack">The packinstance.</param>
        /// <param name="doc">The xml document.</param>
        /// <returns></returns>
        public static XmlNode PackageInfo(PackageInstance pack, XmlDocument doc) {

            XmlNode info = doc.CreateElement("info");

            //Package info
            XmlNode package = doc.CreateElement("package");
            package.AppendChild(_node("name", pack.Name, doc));
            package.AppendChild(_node("version", pack.Version, doc));
            
            XmlNode license = _node("license", pack.License, doc);
            license.Attributes.Append(_attribute("url", pack.LicenseUrl, doc));
            package.AppendChild(license);
            
            package.AppendChild(_node("url", pack.Url, doc));
            
            XmlNode Requirements = doc.CreateElement("requirements");
            Requirements.AppendChild(_node("major", "3", doc));
            Requirements.AppendChild(_node("minor", "0", doc));
            Requirements.AppendChild(_node("patch", "0", doc));
            package.AppendChild(Requirements);
            info.AppendChild(package);

            //Author
            XmlNode author = _node("author", "", doc);
            author.AppendChild(_node("name", pack.Author, doc));
            author.AppendChild(_node("website", pack.AuthorUrl, doc));
            info.AppendChild(author);

            info.AppendChild(_node("readme", "<![CDATA[" + pack.Readme + "]]>", doc));

            return info;
        }


        /// <summary>
        /// Converts an umbraco template to a package xml node
        /// </summary>
        /// <param name="templateId">The template id.</param>
        /// <param name="doc">The xml doc.</param>
        /// <returns></returns>
        public static XmlNode Template(int templateId, XmlDocument doc) {

            Template tmpl = new Template(templateId);

            XmlNode template = doc.CreateElement("Template");
            template.AppendChild(_node("Name", tmpl.Text, doc));
            template.AppendChild(_node("Alias", tmpl.Alias, doc));

            if (tmpl.MasterTemplate != 0) {
                template.AppendChild(_node("Master", new Template(tmpl.MasterTemplate).Alias, doc));
            }
            template.AppendChild(_node("Design", "<![CDATA[" + tmpl.Design + "]]>", doc));

            return template;
        }


        /// <summary>
        /// Converts a umbraco stylesheet to a package xml node
        /// </summary>
        /// <param name="ssId">The ss id.</param>
        /// <param name="incluceProperties">if set to <c>true</c> [incluce properties].</param>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        public static XmlNode Stylesheet(int ssId, bool incluceProperties, XmlDocument doc) {

            StyleSheet sts = new StyleSheet(ssId);
            XmlNode stylesheet = doc.CreateElement("Stylesheet");
            stylesheet.AppendChild(_node("Name", sts.Text, doc));
            stylesheet.AppendChild(_node("FileName", sts.Filename, doc));
            stylesheet.AppendChild(_node("Content", "<![CDATA[" + sts.Content + "]]>", doc));
            if (incluceProperties) {
                XmlNode properties = doc.CreateElement("Properties");
                foreach (StylesheetProperty ssP in sts.Properties) {
                    XmlNode property = doc.CreateElement("Property");
                    property.AppendChild(_node("Name", ssP.Text, doc));
                    property.AppendChild(_node("Alias", ssP.Alias, doc));
                    property.AppendChild(_node("Value", ssP.value, doc));

                    //xnode += "<Property><Name>" + ssP.Text + "</Name><Alias>" + ssP.Alias + "</Alias><Value>" + ssP.value + "</Value></Property>\n";
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
        public static XmlNode Macro(int macroId, bool appendFile, string packageDirectory, XmlDocument doc) {

            Macro mcr = new Macro(macroId);
         
            if (appendFile) {
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
        public static void AppendFileToManifest(string path, string packageDirectory, XmlDocument doc) {

            if (!path.StartsWith("~/") && !path.StartsWith("/"))
                path = "~/" + path;
            
            string serverPath = IOHelper.MapPath(path);

            if (System.IO.File.Exists(serverPath)) {

                AppendFileXml(path, packageDirectory, doc);
                
            } else if(System.IO.Directory.Exists(serverPath)){

                ProcessDirectory(path, packageDirectory, doc);
           }
        }

      

        //Process files in directory and add them to package
        private static void ProcessDirectory(string path, string packageDirectory, XmlDocument doc) {
            string serverPath = IOHelper.MapPath(path);
            if (System.IO.Directory.Exists(serverPath)) {
                
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(serverPath);
                
                foreach (System.IO.FileInfo file in di.GetFiles()) {
                    AppendFileXml(path + "/" + file.Name, packageDirectory, doc);
                }

                foreach (System.IO.DirectoryInfo dir in di.GetDirectories()) {
                    ProcessDirectory(path + "/" + dir.Name, packageDirectory, doc);
                }
            }
        }

        private static void AppendFileXml(string path, string packageDirectory, XmlDocument doc) {

            string serverPath = IOHelper.MapPath(path);

            string orgPath = path.Substring(0, (path.LastIndexOf('/')));
            string orgName = path.Substring((path.LastIndexOf('/') + 1));
            string newFileName = orgName;

            if (System.IO.File.Exists(packageDirectory + "/" + orgName)) {
                string fileGuid = System.Guid.NewGuid().ToString();
                newFileName = fileGuid + "_" + newFileName;
            }

            //Copy file to directory for zipping...
            System.IO.File.Copy(serverPath, packageDirectory + "/" + newFileName, true);

            //Append file info to files xml node
            XmlNode files = doc.SelectSingleNode("/umbPackage/files");

            XmlNode file = doc.CreateElement("file");
            file.AppendChild(_node("guid", newFileName, doc));
            file.AppendChild(_node("orgPath", orgPath == "" ? "/" : orgPath, doc));
            file.AppendChild(_node("orgName", orgName, doc));

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
        public static bool IsFileInManifest(string guid, XmlDocument doc) {
            return false;
        }
               
        private static XmlNode _node(string name, string value, XmlDocument doc) {
            XmlNode node = doc.CreateElement(name);
            node.InnerXml = value;
            return node;
        }

        private static XmlAttribute _attribute(string name, string value, XmlDocument doc) {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        /// <summary>
        /// Zips the package.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="savePath">The save path.</param>
        public static void ZipPackage(string Path, string savePath) {
            string OutPath = savePath;

            ArrayList ar = GenerateFileList(Path);
            // generate file list
            // find number of chars to remove from orginal file path
            int TrimLength = (Directory.GetParent(Path)).ToString().Length;

            TrimLength += 1;

            //remove '\'
            FileStream ostream;

            byte[] obuffer;

            ZipOutputStream oZipStream = new ZipOutputStream(System.IO.File.Create(OutPath));
            // create zip stream


            oZipStream.SetLevel(9);
            // 9 = maximum compression level
            ZipEntry oZipEntry;

            foreach (string Fil in ar) // for each file, generate a zipentry
            {
                oZipEntry = new ZipEntry(Fil.Remove(0, TrimLength));
                oZipStream.PutNextEntry(oZipEntry);


                if (!Fil.EndsWith(@"/")) // if a file ends with '/' its a directory
                {
                    ostream = File.OpenRead(Fil);

                    obuffer = new byte[ostream.Length];

                    // byte buffer
                    ostream.Read(obuffer, 0, obuffer.Length);

                    oZipStream.Write(obuffer, 0, obuffer.Length);
                    ostream.Close();
                }
            }
            oZipStream.Finish();
            oZipStream.Close();
            oZipStream.Dispose();
            oZipStream = null;

            oZipEntry = null;


            ostream = null;
            ar.Clear();
            ar = null;
        }

        private static ArrayList GenerateFileList(string Dir) {
            ArrayList mid = new ArrayList();

            bool Empty = true;

            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                mid.Add(file);
                Empty = false;
            }

            if (Empty) {
                if (Directory.GetDirectories(Dir).Length == 0) // if directory is completely empty, add it
                {
                    mid.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // do this recursively
            {
                foreach (object obj in GenerateFileList(dirs)) {
                    mid.Add(obj);
                }
            }
            return mid;  // return file list
        }
    }
}
