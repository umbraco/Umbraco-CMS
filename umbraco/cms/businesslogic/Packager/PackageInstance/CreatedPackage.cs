using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Web;

using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.macro;


namespace umbraco.cms.businesslogic.packager {
    public class CreatedPackage {

        public static CreatedPackage GetById(int id) {
            CreatedPackage pack = new CreatedPackage(); 
            pack.Data = data.Package(id, HttpContext.Current.Server.MapPath(Settings.CreatedPackagesSettings));
            return pack;    
        }

        public static CreatedPackage MakeNew(string name) {
            CreatedPackage pack = new CreatedPackage(); 
            pack.Data = data.MakeNew(name, HttpContext.Current.Server.MapPath(Settings.CreatedPackagesSettings));

            NewEventArgs e = new NewEventArgs();
            pack.OnNew(e);
            
            return pack;
        }

        public void Save() {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) {
                data.Save(this.Data, HttpContext.Current.Server.MapPath(Settings.CreatedPackagesSettings));
                FireAfterSave(e);
            }
        }

        public void Delete() {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel) {
                data.Delete(this.Data.Id, HttpContext.Current.Server.MapPath(Settings.CreatedPackagesSettings));
                FireAfterDelete(e);
            }
        }

        private PackageInstance m_data;
        public PackageInstance Data {
            get { return m_data; }
            set { m_data = value; }
        }

        public static List<CreatedPackage> GetAllCreatedPackages() {
            List<CreatedPackage> val = new List<CreatedPackage>();

            foreach (PackageInstance pack in data.GetAllPackages(HttpContext.Current.Server.MapPath(Settings.CreatedPackagesSettings))) {
                CreatedPackage crPack = new CreatedPackage();
                crPack.Data = pack;
                val.Add(crPack);
            }

            return val;
        }

        private static XmlDocument _packageManifest;
        private static void createPackageManifest() {
            _packageManifest = new XmlDocument();
            XmlDeclaration xmldecl = _packageManifest.CreateXmlDeclaration("1.0", "UTF-8", "no");

            _packageManifest.AppendChild(xmldecl);

            //root node
            XmlNode umbPackage = _packageManifest.CreateElement("umbPackage");
            _packageManifest.AppendChild(umbPackage);
            //Files node
            umbPackage.AppendChild(_packageManifest.CreateElement("files"));
        }

        private static void appendElement(XmlNode node) {
            XmlNode root = _packageManifest.SelectSingleNode("/umbPackage");
            root.AppendChild(node);
        }


        public void Publish() {

            CreatedPackage package = this;
            PackageInstance pack = package.Data;
                        
            try {

                PublishEventArgs e = new PublishEventArgs();
                package.FireBeforePublish(e);

                if (!e.Cancel) {
                    int outInt = 0;

                    //Path checking...
                    string localPath = HttpContext.Current.Server.MapPath(Settings.PackagesStorage + "/" + pack.Folder);

                    if (!System.IO.Directory.Exists(localPath))
                        System.IO.Directory.CreateDirectory(localPath);

                    //Init package file...
                    createPackageManifest();
                    //Info section..
                    appendElement(utill.PackageInfo(pack, _packageManifest));

                    //Documents...
                    int _contentNodeID = 0;
                    if (!String.IsNullOrEmpty(pack.ContentNodeId) && int.TryParse(pack.ContentNodeId, out _contentNodeID)) {
                        XmlNode documents = _packageManifest.CreateElement("Documents");

                        XmlNode documentSet = _packageManifest.CreateElement("DocumentSet");
                        XmlAttribute importMode = _packageManifest.CreateAttribute("importMode", "");
                        importMode.Value = "root";
                        documentSet.Attributes.Append(importMode);
                        documents.AppendChild(documentSet);

                        //load content from umbraco.
                        cms.businesslogic.web.Document umbDocument = new Document(_contentNodeID);
                        documentSet.AppendChild(umbDocument.ToXml(_packageManifest, pack.ContentLoadChildNodes));

                        appendElement(documents);
                    }

                    //Document types..
                    XmlNode docTypes = _packageManifest.CreateElement("DocumentTypes");
                    foreach (string dtId in pack.Documenttypes) {
                        if (int.TryParse(dtId, out outInt)) {
                            DocumentType docT = new DocumentType(outInt);
                            docTypes.AppendChild(docT.ToXml(_packageManifest));
                        }
                    }
                    appendElement(docTypes);

                    //Templates
                    XmlNode templates = _packageManifest.CreateElement("Templates");
                    foreach (string templateId in pack.Templates) {
                        if (int.TryParse(templateId, out outInt)) {
                            Template t = new Template(outInt);
                            templates.AppendChild(t.ToXml(_packageManifest));
                        }
                    }
                    appendElement(templates);

                    //Stylesheets
                    XmlNode stylesheets = _packageManifest.CreateElement("Stylesheets");
                    foreach (string ssId in pack.Stylesheets) {
                        if (int.TryParse(ssId, out outInt)) {
                            StyleSheet s = new StyleSheet(outInt);
                            stylesheets.AppendChild(s.ToXml(_packageManifest));
                        }
                    }
                    appendElement(stylesheets);

                    //Macros
                    XmlNode macros = _packageManifest.CreateElement("Macros");
                    foreach (string macroId in pack.Macros) {
                        if (int.TryParse(macroId, out outInt)) {
                            macros.AppendChild(utill.Macro(int.Parse(macroId), true, localPath, _packageManifest));
                        }
                    }
                    appendElement(macros);

                    //Dictionary Items
                    XmlNode dictionaryItems = _packageManifest.CreateElement("DictionaryItems");
                    foreach (string dictionaryId in pack.DictionaryItems) {
                        if (int.TryParse(dictionaryId, out outInt)) {
                            Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(outInt);
                            dictionaryItems.AppendChild(di.ToXml(_packageManifest));
                        }
                    }
                    appendElement(dictionaryItems);

                    //Languages
                    XmlNode languages = _packageManifest.CreateElement("Languages");
                    foreach (string langId in pack.Languages) {
                        if (int.TryParse(langId, out outInt)) {
                            language.Language lang = new umbraco.cms.businesslogic.language.Language(outInt);

                            languages.AppendChild(lang.ToXml(_packageManifest));
                        }
                    }
                    appendElement(languages);

                    //Datatypes
                    XmlNode dataTypes = _packageManifest.CreateElement("DataTypes");
                    foreach (string dtId in pack.DataTypes) {
                        if (int.TryParse(dtId, out outInt)) {
                            cms.businesslogic.datatype.DataTypeDefinition dtd = new umbraco.cms.businesslogic.datatype.DataTypeDefinition(outInt);
                            dataTypes.AppendChild(dtd.ToXml(_packageManifest));
                        }
                    }
                    appendElement(dataTypes);

                    //Files
                    foreach (string fileName in pack.Files) {
                        utill.AppendFileToManifest(fileName, localPath, _packageManifest);
                    }

                    //Load control on install...
                    if (!string.IsNullOrEmpty(pack.LoadControl)) {
                        XmlNode control = _packageManifest.CreateElement("control");
                        control.InnerText = pack.LoadControl;
                        utill.AppendFileToManifest(pack.LoadControl, localPath, _packageManifest);
                        appendElement(control);
                    }

                    //Actions
                    if (!string.IsNullOrEmpty(pack.Actions)) {
                        try {
                            XmlDocument xd_actions = new XmlDocument();
                            xd_actions.LoadXml("<Actions>" + pack.Actions + "</Actions>");
                            XmlNode actions = xd_actions.DocumentElement.SelectSingleNode(".");


                            if (actions != null) {
                                actions = _packageManifest.ImportNode(actions, true).Clone();
                                appendElement(actions);
                            }
                        } catch { }
                    }

                    string manifestFileName = localPath + "/package.xml";

                    if (System.IO.File.Exists(manifestFileName))
                        System.IO.File.Delete(manifestFileName);

                    _packageManifest.Save(manifestFileName);
                    _packageManifest = null;


                    string packPath = Settings.PackagerRoot.Replace(System.IO.Path.DirectorySeparatorChar.ToString(), "/") + "/" + pack.Name.Replace(' ', '_') + "_" + pack.Version.Replace(' ', '_') + "." + Settings.PackageFileExtension;
                    utill.ZipPackage(localPath, System.Web.HttpContext.Current.Server.MapPath(packPath));

                    pack.PackagePath = packPath;

                    if (pack.PackageGuid.Trim() == "")
                        pack.PackageGuid = Guid.NewGuid().ToString();

                    package.Save();

                    //Clean up..
                    System.IO.File.Delete(localPath + "/package.xml");
                    System.IO.Directory.Delete(localPath, true);

                    package.FireAfterPublish(e);
                }

            } catch (Exception ex) {
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Error, new BusinessLogic.User(0), -1, "CreatedPackage.cs " + ex.ToString());
            }
        }


        //EVENTS
        public delegate void SaveEventHandler(CreatedPackage sender, SaveEventArgs e);
        public delegate void NewEventHandler(CreatedPackage sender, NewEventArgs e);
        public delegate void PublishEventHandler(CreatedPackage sender, PublishEventArgs e);
        public delegate void DeleteEventHandler(CreatedPackage sender, DeleteEventArgs e);

        /// <summary>
        /// Occurs when a macro is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        public static event PublishEventHandler BeforePublish;
        protected virtual void FireBeforePublish(PublishEventArgs e) {
            if (BeforePublish != null)
                BeforePublish(this, e);
        }

        public static event PublishEventHandler AfterPublish;
        protected virtual void FireAfterPublish(PublishEventArgs e) {
            if (AfterPublish != null)
                AfterPublish(this, e);
        }

        
    }
}
