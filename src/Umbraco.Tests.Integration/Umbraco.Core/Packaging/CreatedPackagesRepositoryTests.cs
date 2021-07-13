// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class CreatedPackagesRepositoryTests : UmbracoIntegrationTest
    {
        private Guid _testBaseFolder;

        [SetUp]
        public void SetupTestData() => _testBaseFolder = Guid.NewGuid();

        [TearDown]
        public void DeleteTestFolder() =>
            Directory.Delete(HostingEnvironment.MapPathContentRoot("~/" + _testBaseFolder), true);
        
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
            Microsoft.Extensions.Options.Options.Create(new GlobalSettings()),
            MediaService,
            MediaTypeService,
            MediaFileManager,
            FileSystems,
            "createdPackages.config",

            // temp paths
            tempFolderPath: "~/" + _testBaseFolder + "/temp",
            packagesFolderPath: "~/" + _testBaseFolder + "/packages",
            mediaFolderPath: "~/" + _testBaseFolder + "/media");

        [Test]
        public void Delete()
        {
            var def1 = new PackageDefinition
            {
                Name = "test",
            };

            bool result = PackageBuilder.SavePackage(def1);
            Assert.IsTrue(result);

            PackageBuilder.Delete(def1.Id);

            def1 = PackageBuilder.GetById(def1.Id);
            Assert.IsNull(def1);
        }

        [Test]
        public void Create_New()
        {
            var def1 = new PackageDefinition
            {
                Name = "test",
            };

            bool result = PackageBuilder.SavePackage(def1);

            Assert.IsTrue(result);
            Assert.AreEqual(1, def1.Id);
            Assert.AreNotEqual(default(Guid).ToString(), def1.PackageId);

            var def2 = new PackageDefinition
            {
                Name = "test2",
            };

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
                Name = "test",
            };

            bool result = PackageBuilder.SavePackage(def);

            Assert.IsFalse(result);
        }

        [Test]
        public void Update()
        {
            var def = new PackageDefinition
            {
                Name = "test",
            };
            bool result = PackageBuilder.SavePackage(def);

            def.Name = "updated";
            result = PackageBuilder.SavePackage(def);
            Assert.IsTrue(result);

            // re-get
            def = PackageBuilder.GetById(def.Id);
            Assert.AreEqual("updated", def.Name);

            // TODO: There's a whole lot more assertions to be done
        }

        [Test]
        public void GivenNestedDictionaryItems_WhenPackageExported_ThenTheXmlIsNested()
        {
            var parent = new DictionaryItem("Parent")
            {
                Key = Guid.NewGuid()                
            };
            LocalizationService.Save(parent);
            var child1 = new DictionaryItem(parent.Key, "Child1")
            {
                Key = Guid.NewGuid()
            };
            LocalizationService.Save(child1);
            var child2 = new DictionaryItem(child1.Key, "Child2")
            {
                Key = Guid.NewGuid()
            };
            LocalizationService.Save(child2);
            var child3 = new DictionaryItem(child2.Key, "Child3")
            {
                Key = Guid.NewGuid()
            };
            LocalizationService.Save(child3);
            var child4 = new DictionaryItem(child3.Key, "Child4")
            {
                Key = Guid.NewGuid()
            };
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
            
            string packageXmlPath = PackageBuilder.ExportPackage(def);

            using (var packageZipStream = File.OpenRead(packageXmlPath))
            using (ZipArchive zipArchive = PackageMigrationResource.GetPackageDataManifest(packageZipStream, out XDocument packageXml))
            {
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
        public void Export()
        {

            var template = TemplateBuilder.CreateTextPageTemplate();

            FileService.SaveTemplate(template);

            var def = new PackageDefinition
            {
                Name = "test",
                Templates = new []{template.Id.ToString()}
            };
            bool result = PackageBuilder.SavePackage(def);
            Assert.IsTrue(result);
            Assert.IsTrue(def.PackagePath.IsNullOrWhiteSpace());

            string packageXmlPath = PackageBuilder.ExportPackage(def);

            def = PackageBuilder.GetById(def.Id); // re-get
            Assert.IsNotNull(def.PackagePath);

            using (FileStream packageZipStream = File.OpenRead(packageXmlPath))
            using (ZipArchive zipArchive = PackageMigrationResource.GetPackageDataManifest(packageZipStream, out XDocument packageXml))
            {
                Assert.AreEqual("umbPackage", packageXml.Root.Name.ToString());

                Assert.AreEqual(
                    $"<Templates><Template><Name>Text page</Name><Alias>textPage</Alias><Design><![CDATA[@using Umbraco.Cms.Web.Common.PublishedModels;{Environment.NewLine}@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage{Environment.NewLine}@{{{Environment.NewLine}\tLayout = null;{Environment.NewLine}}}]]></Design></Template></Templates>",
                    packageXml.Element("umbPackage").Element("Templates").ToString(SaveOptions.DisableFormatting));

                // TODO: There's a whole lot more assertions to be done

            }
        }
    }
}
