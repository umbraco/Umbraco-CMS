using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.macro;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using File = System.IO.File;
using Template = umbraco.cms.businesslogic.template.Template;


namespace umbraco.cms.businesslogic.packager
{
    public class CreatedPackage
    {

        public static CreatedPackage GetById(int id)
        {
            var pack = new CreatedPackage();
            pack.Data = data.Package(id, IOHelper.MapPath(Settings.CreatedPackagesSettings));
            return pack;
        }

        public static CreatedPackage MakeNew(string name)
        {
            var pack = new CreatedPackage();
            pack.Data = data.MakeNew(name, IOHelper.MapPath(Settings.CreatedPackagesSettings));

            var e = new NewEventArgs();
            pack.OnNew(e);

            return pack;
        }

        public void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                data.Save(this.Data, IOHelper.MapPath(Settings.CreatedPackagesSettings));
                FireAfterSave(e);
            }
        }

        public void Delete()
        {
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                data.Delete(this.Data.Id, IOHelper.MapPath(Settings.CreatedPackagesSettings));
                FireAfterDelete(e);
            }
        }

        public PackageInstance Data { get; set; }

        public static List<CreatedPackage> GetAllCreatedPackages()
        {
            var val = new List<CreatedPackage>();

            foreach (var pack in data.GetAllPackages(IOHelper.MapPath(Settings.CreatedPackagesSettings)))
            {
                var crPack = new CreatedPackage();
                crPack.Data = pack;
                val.Add(crPack);
            }

            return val;
        }

        private static XmlDocument _packageManifest;
        private static void CreatePackageManifest()
        {
            _packageManifest = new XmlDocument();
            var xmldecl = _packageManifest.CreateXmlDeclaration("1.0", "UTF-8", "no");

            _packageManifest.AppendChild(xmldecl);

            //root node
            XmlNode umbPackage = _packageManifest.CreateElement("umbPackage");
            _packageManifest.AppendChild(umbPackage);
            //Files node
            umbPackage.AppendChild(_packageManifest.CreateElement("files"));
        }

        private static void AppendElement(XmlNode node)
        {
            var root = _packageManifest.SelectSingleNode("/umbPackage");
            root.AppendChild(node);
        }


        public void Publish()
        {

            var package = this;
            var pack = package.Data;

            var e = new PublishEventArgs();
            package.FireBeforePublish(e);

            if (e.Cancel == false)
            {
                var outInt = 0;

                //Path checking...
                var localPath = IOHelper.MapPath(SystemDirectories.Media + "/" + pack.Folder);

                if (Directory.Exists(localPath) == false)
                    Directory.CreateDirectory(localPath);

                //Init package file...
                CreatePackageManifest();
                //Info section..
                AppendElement(utill.PackageInfo(pack, _packageManifest));

                //Documents and tags...
                var contentNodeId = 0;
                if (string.IsNullOrEmpty(pack.ContentNodeId) == false && int.TryParse(pack.ContentNodeId, out contentNodeId))
                {
                    if (contentNodeId > 0)
                    {
                        //Create the Documents/DocumentSet node
                        XmlNode documents = _packageManifest.CreateElement("Documents");
                        XmlNode documentSet = _packageManifest.CreateElement("DocumentSet");
                        XmlAttribute importMode = _packageManifest.CreateAttribute("importMode", "");
                        importMode.Value = "root";
                        documentSet.Attributes.Append(importMode);
                        documents.AppendChild(documentSet);

                        //load content from umbraco.
                        var umbDocument = new Document(contentNodeId);
                        
                        documentSet.AppendChild(umbDocument.ToXml(_packageManifest, pack.ContentLoadChildNodes));

                        AppendElement(documents);

                        ////Create the TagProperties node - this is used to store a definition for all
                        //// document properties that are tags, this ensures that we can re-import tags properly
                        //XmlNode tagProps = _packageManifest.CreateElement("TagProperties");

                        ////before we try to populate this, we'll do a quick lookup to see if any of the documents
                        //// being exported contain published tags. 
                        //var allExportedIds = documents.SelectNodes("//@id").Cast<XmlNode>()
                        //    .Select(x => x.Value.TryConvertTo<int>())
                        //    .Where(x => x.Success)
                        //    .Select(x => x.Result)
                        //    .ToArray();
                        //var allContentTags = new List<ITag>();
                        //foreach (var exportedId in allExportedIds)
                        //{                            
                        //    allContentTags.AddRange(
                        //        ApplicationContext.Current.Services.TagService.GetTagsForEntity(exportedId));
                        //}

                        ////This is pretty round-about but it works. Essentially we need to get the properties that are tagged
                        //// but to do that we need to lookup by a tag (string)
                        //var allTaggedEntities = new List<TaggedEntity>();
                        //foreach (var group in allContentTags.Select(x => x.Group).Distinct())
                        //{
                        //    allTaggedEntities.AddRange(
                        //        ApplicationContext.Current.Services.TagService.GetTaggedContentByTagGroup(group));
                        //}

                        ////Now, we have all property Ids/Aliases and their referenced document Ids and tags
                        //var allExportedTaggedEntities = allTaggedEntities.Where(x => allExportedIds.Contains(x.EntityId))
                        //    .DistinctBy(x => x.EntityId)
                        //    .OrderBy(x => x.EntityId);

                        //foreach (var taggedEntity in allExportedTaggedEntities)
                        //{
                        //    foreach (var taggedProperty in taggedEntity.TaggedProperties.Where(x => x.Tags.Any()))
                        //    {
                        //        XmlNode tagProp = _packageManifest.CreateElement("TagProperty");
                        //        var docId = _packageManifest.CreateAttribute("docId", "");
                        //        docId.Value = taggedEntity.EntityId.ToString(CultureInfo.InvariantCulture);
                        //        tagProp.Attributes.Append(docId);

                        //        var propertyAlias = _packageManifest.CreateAttribute("propertyAlias", "");
                        //        propertyAlias.Value = taggedProperty.PropertyTypeAlias;
                        //        tagProp.Attributes.Append(propertyAlias);
                                
                        //        var group = _packageManifest.CreateAttribute("group", "");
                        //        group.Value = taggedProperty.Tags.First().Group;
                        //        tagProp.Attributes.Append(group);

                        //        tagProp.AppendChild(_packageManifest.CreateCDataSection(
                        //            JsonConvert.SerializeObject(taggedProperty.Tags.Select(x => x.Text).ToArray())));

                        //        tagProps.AppendChild(tagProp);
                        //    }
                        //}

                        //AppendElement(tagProps);

                    }
                }

                //Document types..
                var dtl = new List<DocumentType>();
                var docTypes = _packageManifest.CreateElement("DocumentTypes");
                foreach (var dtId in pack.Documenttypes)
                {
                    if (int.TryParse(dtId, out outInt))
                    {
                        DocumentType docT = new DocumentType(outInt);

                        AddDocumentType(docT, ref dtl);

                    }
                }
                foreach (DocumentType d in dtl)
                {
                    docTypes.AppendChild(d.ToXml(_packageManifest));
                }

                AppendElement(docTypes);

                //Templates
                var templates = _packageManifest.CreateElement("Templates");
                foreach (var templateId in pack.Templates)
                {
                    if (int.TryParse(templateId, out outInt))
                    {
                        var t = new Template(outInt);
                        templates.AppendChild(t.ToXml(_packageManifest));
                    }
                }
                AppendElement(templates);

                //Stylesheets
                var stylesheets = _packageManifest.CreateElement("Stylesheets");
                foreach (var ssId in pack.Stylesheets)
                {
                    if (int.TryParse(ssId, out outInt))
                    {
                        var s = new StyleSheet(outInt);
                        stylesheets.AppendChild(s.ToXml(_packageManifest));
                    }
                }
                AppendElement(stylesheets);

                //Macros
                var macros = _packageManifest.CreateElement("Macros");
                foreach (var macroId in pack.Macros)
                {
                    if (int.TryParse(macroId, out outInt))
                    {
                        macros.AppendChild(utill.Macro(int.Parse(macroId), true, localPath, _packageManifest));
                    }
                }
                AppendElement(macros);

                //Dictionary Items
                var dictionaryItems = _packageManifest.CreateElement("DictionaryItems");
                foreach (var dictionaryId in pack.DictionaryItems)
                {
                    if (int.TryParse(dictionaryId, out outInt))
                    {
                        var di = new Dictionary.DictionaryItem(outInt);
                        dictionaryItems.AppendChild(di.ToXml(_packageManifest));
                    }
                }
                AppendElement(dictionaryItems);

                //Languages
                var languages = _packageManifest.CreateElement("Languages");
                foreach (var langId in pack.Languages)
                {
                    if (int.TryParse(langId, out outInt))
                    {
                        var lang = new language.Language(outInt);

                        languages.AppendChild(lang.ToXml(_packageManifest));
                    }
                }
                AppendElement(languages);

                //Datatypes
                var dataTypes = _packageManifest.CreateElement("DataTypes");
                foreach (var dtId in pack.DataTypes)
                {
                    if (int.TryParse(dtId, out outInt))
                    {
                        datatype.DataTypeDefinition dtd = new datatype.DataTypeDefinition(outInt);
                        dataTypes.AppendChild(dtd.ToXml(_packageManifest));
                    }
                }
                AppendElement(dataTypes);

                //Files
                foreach (var fileName in pack.Files)
                {
                    utill.AppendFileToManifest(fileName, localPath, _packageManifest);
                }

                //Load control on install...
                if (string.IsNullOrEmpty(pack.LoadControl) == false)
                {
                    XmlNode control = _packageManifest.CreateElement("control");
                    control.InnerText = pack.LoadControl;
                    utill.AppendFileToManifest(pack.LoadControl, localPath, _packageManifest);
                    AppendElement(control);
                }

                //Actions
                if (string.IsNullOrEmpty(pack.Actions) == false)
                {
                    try
                    {
                        var xdActions = new XmlDocument();
                        xdActions.LoadXml("<Actions>" + pack.Actions + "</Actions>");
                        var actions = xdActions.DocumentElement.SelectSingleNode(".");


                        if (actions != null)
                        {
                            actions = _packageManifest.ImportNode(actions, true).Clone();
                            AppendElement(actions);
                        }
                    }
                    catch { }
                }

                var manifestFileName = localPath + "/package.xml";

                if (File.Exists(manifestFileName))
                    File.Delete(manifestFileName);

                _packageManifest.Save(manifestFileName);
                _packageManifest = null;


                //string packPath = Settings.PackagerRoot.Replace(System.IO.Path.DirectorySeparatorChar.ToString(), "/") + "/" + pack.Name.Replace(' ', '_') + "_" + pack.Version.Replace(' ', '_') + "." + Settings.PackageFileExtension;

                // check if there's a packages directory below media
                var packagesDirectory = SystemDirectories.Media + "/created-packages";
                if (Directory.Exists(IOHelper.MapPath(packagesDirectory)) == false)
                {
                    Directory.CreateDirectory(IOHelper.MapPath(packagesDirectory));
                }


                var packPath = packagesDirectory + "/" + (pack.Name + "_" + pack.Version).Replace(' ', '_') + "." + Settings.PackageFileExtension;
                utill.ZipPackage(localPath, IOHelper.MapPath(packPath));

                pack.PackagePath = packPath;

                if (pack.PackageGuid.Trim() == "")
                    pack.PackageGuid = Guid.NewGuid().ToString();

                package.Save();

                //Clean up..
                File.Delete(localPath + "/package.xml");
                Directory.Delete(localPath, true);

                package.FireAfterPublish(e);
            }

        }

        private void AddDocumentType(DocumentType dt, ref List<DocumentType> dtl)
        {
            if (dt.MasterContentType != 0)
            {
                //first add masters
                var mDocT = new DocumentType(dt.MasterContentType);

                AddDocumentType(mDocT, ref dtl);

            }

            if (dtl.Contains(dt) == false)
            {
                dtl.Add(dt);
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
        protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        public static event PublishEventHandler BeforePublish;
        protected virtual void FireBeforePublish(PublishEventArgs e)
        {
            if (BeforePublish != null)
                BeforePublish(this, e);
        }

        public static event PublishEventHandler AfterPublish;
        protected virtual void FireAfterPublish(PublishEventArgs e)
        {
            if (AfterPublish != null)
                AfterPublish(this, e);
        }


    }
}
