using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Lucene.Net.Documents;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using File = System.IO.File;


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
            var pack = new CreatedPackage
            {
                Data = data.MakeNew(name, IOHelper.MapPath(Settings.CreatedPackagesSettings))
            };


            return pack;
        }

        public void Save()
        {
            data.Save(this.Data, IOHelper.MapPath(Settings.CreatedPackagesSettings));
        }

        public void Delete()
        {
            data.Delete(this.Data.Id, IOHelper.MapPath(Settings.CreatedPackagesSettings));
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

            var outInt = 0;

            //Path checking...
            var localPath = IOHelper.MapPath(SystemDirectories.Media + "/" + pack.Folder);

            if (Directory.Exists(localPath) == false)
                Directory.CreateDirectory(localPath);

            //Init package file...
            CreatePackageManifest();
            //Info section..
            AppendElement(PackagerUtility.PackageInfo(pack, _packageManifest));

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
                    //var umbDocument = new Document(contentNodeId);
                    //var x = umbDocument.ToXml(_packageManifest, pack.ContentLoadChildNodes);
                    var udoc = Current.Services.ContentService.GetById(contentNodeId);
                    var xe = pack.ContentLoadChildNodes ? udoc.ToDeepXml(Current.Services.PackagingService) : udoc.ToXml(Current.Services.PackagingService);
                    var x = xe.GetXmlNode(_packageManifest);
                    documentSet.AppendChild(x);

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
                    //        Current.Services.TagService.GetTagsForEntity(exportedId));
                    //}

                    ////This is pretty round-about but it works. Essentially we need to get the properties that are tagged
                    //// but to do that we need to lookup by a tag (string)
                    //var allTaggedEntities = new List<TaggedEntity>();
                    //foreach (var group in allContentTags.Select(x => x.Group).Distinct())
                    //{
                    //    allTaggedEntities.AddRange(
                    //        Current.Services.TagService.GetTaggedContentByTagGroup(group));
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
            var dtl = new List<IContentType>();
            var docTypes = _packageManifest.CreateElement("DocumentTypes");
            foreach (var dtId in pack.Documenttypes)
            {
                if (int.TryParse(dtId, out outInt))
                {
                    var docT = Current.Services.ContentTypeService.Get(outInt);
                    //DocumentType docT = new DocumentType(outInt);

                    AddDocumentType(docT, ref dtl);

                }
            }

            var exporter = new EntityXmlSerializer();

            foreach (var d in dtl)
            {
                var xml = exporter.Serialize(Current.Services.DataTypeService, Current.Services.ContentTypeService, d);
                var xNode = xml.GetXmlNode();
                var n = (XmlElement) _packageManifest.ImportNode(xNode, true);
                docTypes.AppendChild(n);
            }

            AppendElement(docTypes);

            //Templates
            var templates = _packageManifest.CreateElement("Templates");
            foreach (var templateId in pack.Templates)
            {
                if (int.TryParse(templateId, out outInt))
                {
                    var t = Current.Services.FileService.GetTemplate(outInt);

                    var serializer = new EntityXmlSerializer();
                    var serialized = serializer.Serialize(t);
                    var n = serialized.GetXmlNode(_packageManifest);


                    templates.AppendChild(n);
                }
            }
            AppendElement(templates);

            //Stylesheets
            var stylesheets = _packageManifest.CreateElement("Stylesheets");
            foreach (var stylesheetName in pack.Stylesheets)
            {
                if (stylesheetName.IsNullOrWhiteSpace()) continue;
                var stylesheetXmlNode = PackagerUtility.Stylesheet(stylesheetName, true, _packageManifest);
                if (stylesheetXmlNode != null)
                    stylesheets.AppendChild(stylesheetXmlNode);
            }
            AppendElement(stylesheets);

            //Macros
            var macros = _packageManifest.CreateElement("Macros");
            foreach (var macroId in pack.Macros)
            {
                if (int.TryParse(macroId, out outInt))
                {
                    macros.AppendChild(PackagerUtility.Macro(int.Parse(macroId), true, localPath, _packageManifest));
                }
            }
            AppendElement(macros);

            //Dictionary Items
            var dictionaryItems = _packageManifest.CreateElement("DictionaryItems");
            foreach (var dictionaryId in pack.DictionaryItems)
            {
                if (int.TryParse(dictionaryId, out outInt))
                {
                    var di = Current.Services.LocalizationService.GetDictionaryItemById(outInt);
                    var entitySerializer = new EntityXmlSerializer();
                    var xmlNode = entitySerializer.Serialize(di).GetXmlNode(_packageManifest);
                    dictionaryItems.AppendChild(xmlNode);
                }
            }
            AppendElement(dictionaryItems);

            //Languages
            var languages = _packageManifest.CreateElement("Languages");
            foreach (var langId in pack.Languages)
            {
                if (int.TryParse(langId, out outInt))
                {
                    var lang = Current.Services.LocalizationService.GetLanguageById(outInt);

                    var serializer = new EntityXmlSerializer();
                    var xml = serializer.Serialize(lang);
                    var n = xml.GetXmlNode(_packageManifest);

                    languages.AppendChild(n);
                }
            }
            AppendElement(languages);

            //TODO: Fix this! ... actually once we use the new packager we don't need to

            ////Datatypes
            //var dataTypes = _packageManifest.CreateElement("DataTypes");
            //foreach (var dtId in pack.DataTypes)
            //{
            //    if (int.TryParse(dtId, out outInt))
            //    {
            //        datatype.DataTypeDefinition dtd = new datatype.DataTypeDefinition(outInt);
            //        dataTypes.AppendChild(dtd.ToXml(_packageManifest));
            //    }
            //}
            //AppendElement(dataTypes);

            //Files
            foreach (var fileName in pack.Files)
            {
                PackagerUtility.AppendFileToManifest(fileName, localPath, _packageManifest);
            }

            //Load control on install...
            if (string.IsNullOrEmpty(pack.LoadControl) == false)
            {
                XmlNode control = _packageManifest.CreateElement("control");
                control.InnerText = pack.LoadControl;
                PackagerUtility.AppendFileToManifest(pack.LoadControl, localPath, _packageManifest);
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
            PackagerUtility.ZipPackage(localPath, IOHelper.MapPath(packPath));

            pack.PackagePath = packPath;

            if (pack.PackageGuid.Trim() == "")
                pack.PackageGuid = Guid.NewGuid().ToString();

            package.Save();

            //Clean up..
            File.Delete(localPath + "/package.xml");
            Directory.Delete(localPath, true);
        }

        private void AddDocumentType(IContentType dt, ref List<IContentType> dtl)
        {
            if (dt.ParentId > 0)
            {
                var parent = Current.Services.ContentTypeService.Get(dt.ParentId);
                if (parent != null) // could be a container
                {
                    AddDocumentType(parent, ref dtl);
                }
            }

            if (dtl.Contains(dt) == false)
            {
                dtl.Add(dt);
            }
        }



    }
}
