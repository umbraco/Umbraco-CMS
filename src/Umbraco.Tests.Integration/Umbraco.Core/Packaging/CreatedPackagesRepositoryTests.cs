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

        private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();
        private IContentService ContentService => GetRequiredService<IContentService>();

        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

        private IFileService FileService => GetRequiredService<IFileService>();

        private IMacroService MacroService => GetRequiredService<IMacroService>();

        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

        private IEntityXmlSerializer EntityXmlSerializer => GetRequiredService<IEntityXmlSerializer>();

        private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

        private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();

        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

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

            using (var packageXmlStream = File.OpenRead(packageXmlPath))
            {
                var xml = XDocument.Load(packageXmlStream);
                Assert.AreEqual("umbPackage", xml.Root.Name.ToString());

                Assert.AreEqual("<Templates><Template><Name>Text page</Name><Alias>textPage</Alias><Design><![CDATA[@using Umbraco.Cms.Web.Common.PublishedModels;\r\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@{\r\n\tLayout = null;\r\n}]]></Design></Template></Templates>", xml.Element("umbPackage").Element("Templates").ToString(SaveOptions.DisableFormatting));

                // TODO: There's a whole lot more assertions to be done

            }
        }
    }
}
