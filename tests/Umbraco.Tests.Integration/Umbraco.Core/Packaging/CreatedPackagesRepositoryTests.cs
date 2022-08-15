// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class CreatedPackagesRepositoryTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetupTestData() => _testBaseFolder = Guid.NewGuid();

    [TearDown]
    public void DeleteTestFolder() =>
        Directory.Delete(HostingEnvironment.MapPathContentRoot("~/" + _testBaseFolder), true);

    private Guid _testBaseFolder;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IMacroService MacroService => GetRequiredService<IMacroService>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private IEntityXmlSerializer EntityXmlSerializer => GetRequiredService<IEntityXmlSerializer>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private FileSystems FileSystems => GetRequiredService<FileSystems>();

    public ICreatedPackagesRepository PackageBuilder => new PackagesRepository(
        ContentService,
        ContentTypeService,
        DataTypeService,
        FileService,
        MacroService,
        LocalizationService,
        HostingEnvironment,
        EntityXmlSerializer,
        Options.Create(new GlobalSettings()),
        MediaService,
        MediaTypeService,
        MediaFileManager,
        FileSystems,
        "createdPackages.config",

        // temp paths
        "~/" + _testBaseFolder + "/temp",
        "~/" + _testBaseFolder + "/packages",
        "~/" + _testBaseFolder + "/media");

    [Test]
    public void Delete()
    {
        var def1 = new PackageDefinition { Name = "test" };

        var result = PackageBuilder.SavePackage(def1);
        Assert.IsTrue(result);

        PackageBuilder.Delete(def1.Id);

        def1 = PackageBuilder.GetById(def1.Id);
        Assert.IsNull(def1);
    }

    [Test]
    public void Create_New()
    {
        var def1 = new PackageDefinition { Name = "test" };

        var result = PackageBuilder.SavePackage(def1);

        Assert.IsTrue(result);
        Assert.AreEqual(1, def1.Id);
        Assert.AreNotEqual(default(Guid).ToString(), def1.PackageId);

        var def2 = new PackageDefinition { Name = "test2" };

        result = PackageBuilder.SavePackage(def2);

        Assert.IsTrue(result);
        Assert.AreEqual(2, def2.Id);
        Assert.AreNotEqual(default(Guid).ToString(), def2.PackageId);
    }

    [Test]
    public void Update_Not_Found()
    {
        var def = new PackageDefinition
        {
            Id = 3, // doesn't exist
            Name = "test"
        };

        var result = PackageBuilder.SavePackage(def);

        Assert.IsFalse(result);
    }

    [Test]
    public void Update()
    {
        var def = new PackageDefinition { Name = "test" };
        var result = PackageBuilder.SavePackage(def);
        //Update values and save
        def.Name = "updated";
        def.ContentNodeId = "test";
        def.Languages.Add("Danish");
        def.Languages.Add("English");
        def.Scripts.Add("TestScript1");
        def.Scripts.Add("TestScript2");
        result = PackageBuilder.SavePackage(def);
        Assert.IsTrue(result);
        // re-get
        def = PackageBuilder.GetById(def.Id);
        Assert.AreEqual("updated", def.Name);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("updated", def.Name);
            Assert.AreEqual("test", def.ContentNodeId);
            Assert.AreEqual(2, def.Languages.Count());
            Assert.AreEqual(2, def.Scripts.Count());
            Assert.AreEqual(0, def.DataTypes.Count());
            Assert.AreEqual(0, def.DictionaryItems.Count());
            Assert.AreEqual(0, def.DocumentTypes.Count());
            Assert.AreEqual(0, def.Macros.Count());
            Assert.AreEqual(0, def.MediaTypes.Count());
            Assert.AreEqual(0, def.MediaUdis.Count());
            Assert.AreEqual(0, def.PartialViews.Count());
            Assert.AreEqual(0, def.Stylesheets.Count());
            Assert.AreEqual(0, def.Templates.Count());
        });
    }

    [Test]
    public void GivenNestedDictionaryItems_WhenPackageExported_ThenTheXmlIsNested()
    {
        var parent = new DictionaryItem("Parent") { Key = Guid.NewGuid() };
        LocalizationService.Save(parent);
        var child1 = new DictionaryItem(parent.Key, "Child1") { Key = Guid.NewGuid() };
        LocalizationService.Save(child1);
        var child2 = new DictionaryItem(child1.Key, "Child2") { Key = Guid.NewGuid() };
        LocalizationService.Save(child2);
        var child3 = new DictionaryItem(child2.Key, "Child3") { Key = Guid.NewGuid() };
        LocalizationService.Save(child3);
        var child4 = new DictionaryItem(child3.Key, "Child4") { Key = Guid.NewGuid() };
        LocalizationService.Save(child4);

        var def = new PackageDefinition
        {
            Name = "test",

            // put these out of order to ensure that it doesn't matter.
            DictionaryItems = new List<string>
            {
                child2.Id.ToString(),
                child1.Id.ToString(),
                // we are missing 3 here so 4 will be orphaned and end up in the root
                child4.Id.ToString(),
                parent.Id.ToString()
            }
        };

        PackageBuilder.SavePackage(def);

        var packageXmlPath = PackageBuilder.ExportPackage(def);

        using (var packageXmlStream = File.OpenRead(packageXmlPath))
        {
            var packageXml = XDocument.Load(packageXmlStream);
            var dictionaryItems = packageXml.Root.Element("DictionaryItems");
            Assert.IsNotNull(dictionaryItems);
            var rootItems = dictionaryItems.Elements("DictionaryItem").ToList();
            Assert.AreEqual(2, rootItems.Count);
            Assert.AreEqual("Child4", rootItems[0].AttributeValue<string>("Name"));
            Assert.AreEqual("Parent", rootItems[1].AttributeValue<string>("Name"));
            var children = rootItems[1].Elements("DictionaryItem").ToList();
            Assert.AreEqual(1, children.Count);
            Assert.AreEqual("Child1", children[0].AttributeValue<string>("Name"));
            children = children[0].Elements("DictionaryItem").ToList();
            Assert.AreEqual(1, children.Count);
            Assert.AreEqual("Child2", children[0].AttributeValue<string>("Name"));
        }
    }

    [Test]
    public void Export_Zip()
    {
        var mt = MediaTypeBuilder.CreateImageMediaType("testImage");
        MediaTypeService.Save(mt);
        var m1 = MediaBuilder.CreateMediaFile(mt, -1);
        MediaService.Save(m1);

        //Ensure a file exist
        var fullPath =
            HostingEnvironment.MapPathWebRoot(m1.Properties[Constants.Conventions.Media.File].GetValue().ToString());
        using (var file1 = File.CreateText(fullPath))
        {
            file1.WriteLine("hello");
        }

        var def = new PackageDefinition { Name = "test", MediaUdis = new List<GuidUdi> { m1.GetUdi() } };

        var result = PackageBuilder.SavePackage(def);
        Assert.IsTrue(result);
        Assert.IsTrue(def.PackagePath.IsNullOrWhiteSpace());

        var packageXmlPath = PackageBuilder.ExportPackage(def);

        def = PackageBuilder.GetById(def.Id); // re-get
        Assert.IsNotNull(def.PackagePath);

        using (var packageZipStream = File.OpenRead(packageXmlPath))
        using (var zipArchive = PackageMigrationResource.GetPackageDataManifest(packageZipStream, out var packageXml))
        {
            var test = "test-file.txt";
            Assert.Multiple(() =>
            {
                var mediaEntry = zipArchive.GetEntry("media/media/test-file.txt");
                Assert.AreEqual("umbPackage", packageXml.Root.Name.ToString());
                Assert.IsNotNull(mediaEntry);
                Assert.AreEqual(test, mediaEntry.Name);
                Assert.IsNotNull(zipArchive.GetEntry("package.xml"));
                Assert.AreEqual(
                    $"<MediaItems><MediaSet><testImage id=\"{m1.Id}\" key=\"{m1.Key}\" parentID=\"-1\" level=\"1\" creatorID=\"-1\" sortOrder=\"0\" createDate=\"{m1.CreateDate:s}\" updateDate=\"{m1.UpdateDate:s}\" nodeName=\"Test File\" urlName=\"test-file\" path=\"{m1.Path}\" isDoc=\"\" nodeType=\"{mt.Id}\" nodeTypeAlias=\"testImage\" writerName=\"\" writerID=\"0\" udi=\"{m1.GetUdi()}\" mediaFilePath=\"/media/test-file.txt\"><umbracoFile><![CDATA[/media/test-file.txt]]></umbracoFile><umbracoBytes><![CDATA[100]]></umbracoBytes><umbracoExtension><![CDATA[png]]></umbracoExtension></testImage></MediaSet></MediaItems>",
                    packageXml.Element("umbPackage").Element("MediaItems").ToString(SaveOptions.DisableFormatting));
                Assert.AreEqual(2, zipArchive.Entries.Count());
                Assert.AreEqual(ZipArchiveMode.Read, zipArchive.Mode);
                Assert.IsNull(packageXml.DocumentType);
                Assert.IsNull(packageXml.NextNode);
                Assert.IsNull(packageXml.Parent);
                Assert.IsNull(packageXml.PreviousNode);
            });
        }
    }


    [Test]
    public void Export_Xml()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();

        FileService.SaveTemplate(template);

        var def = new PackageDefinition { Name = "test", Templates = new[] { template.Id.ToString() } };
        var result = PackageBuilder.SavePackage(def);
        Assert.IsTrue(result);
        Assert.IsTrue(def.PackagePath.IsNullOrWhiteSpace());

        var packageXmlPath = PackageBuilder.ExportPackage(def); // Get

        def = PackageBuilder.GetById(def.Id); // re-get
        Assert.IsNotNull(def.PackagePath);

        using (var packageXmlStream = File.OpenRead(packageXmlPath))
        {
            var xml = XDocument.Load(packageXmlStream);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("umbPackage", xml.Root.Name.ToString());
                Assert.AreEqual(
                    $"<Templates><Template><Name>Text page</Name><Key>{template.Key}</Key><Alias>textPage</Alias><Design><![CDATA[@using Umbraco.Cms.Web.Common.PublishedModels;{Environment.NewLine}@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage{Environment.NewLine}@{{{Environment.NewLine}\tLayout = null;{Environment.NewLine}}}]]></Design></Template></Templates>",
                    xml.Element("umbPackage").Element("Templates").ToString(SaveOptions.DisableFormatting));
                Assert.IsNull(xml.DocumentType);
                Assert.IsNull(xml.Parent);
                Assert.IsNull(xml.NextNode);
                Assert.IsNull(xml.PreviousNode);
            });
        }
    }
}
