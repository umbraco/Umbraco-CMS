// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
internal sealed class CreatedPackagesRepositoryTests : UmbracoIntegrationTest
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

    private IStylesheetService StylesheetService => GetRequiredService<IStylesheetService>();

    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private IEntityXmlSerializer EntityXmlSerializer => GetRequiredService<IEntityXmlSerializer>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private FileSystems FileSystems => GetRequiredService<FileSystems>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    public ICreatedPackagesRepository PackageBuilder => new PackagesRepository(
        ContentService,
        ContentTypeService,
        DataTypeService,
        TemplateService,
        StylesheetService,
        GetRequiredService<ILanguageRepository>(),
        GetRequiredService<IDictionaryRepository>(),
        GetRequiredService<IScopeProvider>(),
        HostingEnvironment,
        EntityXmlSerializer,
        MediaService,
        MediaTypeService,
        MediaFileManager,
        FileSystems,
        GetRequiredService<IIdKeyMap>(),
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
        Assert.That(result, Is.True);

        PackageBuilder.Delete(def1.Id);

        def1 = PackageBuilder.GetById(def1.Id);
        Assert.That(def1, Is.Null);
    }

    [Test]
    public void Create_New()
    {
        var def1 = new PackageDefinition { Name = "test" };

        var result = PackageBuilder.SavePackage(def1);

        Assert.That(result, Is.True);
        Assert.That(def1.Id, Is.EqualTo(1));
        Assert.That(def1.PackageId, Is.Not.EqualTo(default(Guid).ToString()));

        var def2 = new PackageDefinition { Name = "test2" };

        result = PackageBuilder.SavePackage(def2);

        Assert.That(result, Is.True);
        Assert.That(def2.Id, Is.EqualTo(2));
        Assert.That(def2.PackageId, Is.Not.EqualTo(default(Guid).ToString()));
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

        Assert.That(result, Is.False);
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
        Assert.That(result, Is.True);
        // re-get
        def = PackageBuilder.GetById(def.Id);
        Assert.That(def.Name, Is.EqualTo("updated"));
        Assert.Multiple(() =>
        {
            Assert.That(def.Name, Is.EqualTo("updated"));
            Assert.That(def.ContentNodeId, Is.EqualTo("test"));
            Assert.That(def.Languages.Count(), Is.EqualTo(2));
            Assert.That(def.Scripts.Count(), Is.EqualTo(2));
            Assert.That(def.DataTypes.Count(), Is.EqualTo(0));
            Assert.That(def.DictionaryItems.Count(), Is.EqualTo(0));
            Assert.That(def.DocumentTypes.Count(), Is.EqualTo(0));
            Assert.That(def.MediaTypes.Count(), Is.EqualTo(0));
            Assert.That(def.MediaUdis.Count(), Is.EqualTo(0));
            Assert.That(def.PartialViews.Count(), Is.EqualTo(0));
            Assert.That(def.Stylesheets.Count(), Is.EqualTo(0));
            Assert.That(def.Templates.Count(), Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GivenNestedDictionaryItems_WhenPackageExported_ThenTheXmlIsNested()
    {
        var parent = (await DictionaryItemService.CreateAsync(new DictionaryItem("Parent"), Constants.Security.SuperUserKey)).Result;
        var child1 = (await DictionaryItemService.CreateAsync(new DictionaryItem(parent.Key, "Child1"), Constants.Security.SuperUserKey)).Result;
        var child2 = (await DictionaryItemService.CreateAsync(new DictionaryItem(child1.Key, "Child2"), Constants.Security.SuperUserKey)).Result;
        var child3 = (await DictionaryItemService.CreateAsync(new DictionaryItem(child2.Key, "Child3"), Constants.Security.SuperUserKey)).Result;
        var child4 = (await DictionaryItemService.CreateAsync(new DictionaryItem(child3.Key, "Child4"), Constants.Security.SuperUserKey)).Result;

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
            Assert.That(dictionaryItems, Is.Not.Null);
            var rootItems = dictionaryItems.Elements("DictionaryItem").ToList();
            Assert.That(rootItems, Has.Count.EqualTo(2));
            Assert.That(rootItems[0].AttributeValue<string>("Name"), Is.EqualTo("Child4"));
            Assert.That(rootItems[1].AttributeValue<string>("Name"), Is.EqualTo("Parent"));
            var children = rootItems[1].Elements("DictionaryItem").ToList();
            Assert.That(children, Has.Count.EqualTo(1));
            Assert.That(children[0].AttributeValue<string>("Name"), Is.EqualTo("Child1"));
            children = children[0].Elements("DictionaryItem").ToList();
            Assert.That(children, Has.Count.EqualTo(1));
            Assert.That(children[0].AttributeValue<string>("Name"), Is.EqualTo("Child2"));
        }
    }

    [Test]
    [LongRunning]
    public async Task Export_Zip()
    {
        var mt = MediaTypeBuilder.CreateImageMediaType("testImage");
        await MediaTypeService.CreateAsync(mt, Constants.Security.SuperUserKey);
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
        Assert.That(result, Is.True);
        Assert.That(def.PackagePath.IsNullOrWhiteSpace(), Is.True);

        var packageXmlPath = PackageBuilder.ExportPackage(def);

        def = PackageBuilder.GetById(def.Id); // re-get
        Assert.That(def.PackagePath, Is.Not.Null);

        using (var packageZipStream = File.OpenRead(packageXmlPath))
        using (var zipArchive = PackageMigrationResource.GetPackageDataManifest(packageZipStream, out var packageXml))
        {
            var test = "test-file.txt";
            Assert.Multiple(() =>
            {
                var mediaEntry = zipArchive.GetEntry("media/media/test-file.txt");
                Assert.That(packageXml.Root.Name.ToString(), Is.EqualTo("umbPackage"));
                Assert.That(mediaEntry, Is.Not.Null);
                Assert.That(mediaEntry.Name, Is.EqualTo(test));
                Assert.That(zipArchive.GetEntry("package.xml"), Is.Not.Null);
                Assert.That(
                    packageXml.Element("umbPackage").Element("MediaItems").ToString(SaveOptions.DisableFormatting), Is.EqualTo($"<MediaItems><MediaSet><testImage id=\"{m1.Id}\" key=\"{m1.Key}\" parentID=\"-1\" level=\"1\" creatorID=\"-1\" sortOrder=\"0\" createDate=\"{m1.CreateDate:s}\" updateDate=\"{m1.UpdateDate:s}\" nodeName=\"Test File\" urlName=\"test-file\" path=\"{m1.Path}\" isDoc=\"\" nodeType=\"{mt.Id}\" nodeTypeAlias=\"testImage\" writerName=\"Administrator\" writerID=\"-1\" udi=\"{m1.GetUdi()}\" mediaFilePath=\"/media/test-file.txt\"><umbracoFile><![CDATA[/media/test-file.txt]]></umbracoFile><umbracoBytes><![CDATA[100]]></umbracoBytes><umbracoExtension><![CDATA[png]]></umbracoExtension></testImage></MediaSet></MediaItems>"));
                Assert.That(zipArchive.Entries.Count(), Is.EqualTo(2));
                Assert.That(zipArchive.Mode, Is.EqualTo(ZipArchiveMode.Read));
                Assert.That(packageXml.DocumentType, Is.Null);
                Assert.That(packageXml.NextNode, Is.Null);
                Assert.That(packageXml.Parent, Is.Null);
                Assert.That(packageXml.PreviousNode, Is.Null);
            });
        }
    }


    [Test]
    public async Task Export_Xml()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();

        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var def = new PackageDefinition { Name = "test", Templates = new[] { template.Id.ToString() } };
        var result = PackageBuilder.SavePackage(def);
        Assert.That(result, Is.True);
        Assert.That(def.PackagePath.IsNullOrWhiteSpace(), Is.True);

        var packageXmlPath = PackageBuilder.ExportPackage(def); // Get

        def = PackageBuilder.GetById(def.Id); // re-get
        Assert.That(def.PackagePath, Is.Not.Null);

        using (var packageXmlStream = File.OpenRead(packageXmlPath))
        {
            var xml = XDocument.Load(packageXmlStream);
            Assert.Multiple(() =>
            {
                Assert.That(xml.Root.Name.ToString(), Is.EqualTo("umbPackage"));
                Assert.That(
                    xml.Element("umbPackage").Element("Templates").ToString(SaveOptions.DisableFormatting), Is.EqualTo($"<Templates><Template><Name>Text page</Name><Key>{template.Key}</Key><Alias>textPage</Alias><Design><![CDATA[@using Umbraco.Cms.Web.Common.PublishedModels;{Environment.NewLine}@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage{Environment.NewLine}@{{{Environment.NewLine}\tLayout = null;{Environment.NewLine}}}]]></Design></Template></Templates>"));
                Assert.That(xml.DocumentType, Is.Null);
                Assert.That(xml.Parent, Is.Null);
                Assert.That(xml.NextNode, Is.Null);
                Assert.That(xml.PreviousNode, Is.Null);
            });
        }
    }
}
