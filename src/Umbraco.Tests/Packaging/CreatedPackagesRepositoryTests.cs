using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Services;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class CreatedPackagesRepositoryTests : TestWithDatabaseBase
    {
        private Guid _testBaseFolder;

        public override void SetUp()
        {
            base.SetUp();
            _testBaseFolder = Guid.NewGuid();
        }

        public override void TearDown()
        {
            base.TearDown();

            //clear out files/folders
            Directory.Delete(IOHelper.MapPath("~/" + _testBaseFolder), true);
        }

        public ICreatedPackagesRepository PackageBuilder => new PackagesRepository(
            ServiceContext.ContentService, ServiceContext.ContentTypeService, ServiceContext.DataTypeService,
            ServiceContext.FileService, ServiceContext.MacroService, ServiceContext.LocalizationService,
            Factory.GetInstance<IEntityXmlSerializer>(), Logger,
            "createdPackages.config",
            //temp paths
            tempFolderPath: "~/" + _testBaseFolder + "/temp",
            packagesFolderPath: "~/" + _testBaseFolder + "/packages",
            mediaFolderPath: "~/" + _testBaseFolder + "/media");

        [Test]
        public void Delete()
        {
            var def1 = new PackageDefinition
            {
                Name = "test",
                Url = "http://test.com",
                Author = "Someone",
                AuthorUrl = "http://test.com"
            };

            var result = PackageBuilder.SavePackage(def1);
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
                Url = "http://test.com",
                Author = "Someone",
                AuthorUrl = "http://test.com"
            };

            var result = PackageBuilder.SavePackage(def1);

            Assert.IsTrue(result);
            Assert.AreEqual(1, def1.Id);
            Assert.AreNotEqual(default(Guid).ToString(), def1.PackageId);

            var def2 = new PackageDefinition
            {
                Name = "test2",
                Url = "http://test2.com",
                Author = "Someone2",
                AuthorUrl = "http://test2.com"
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
                Id = 3, //doesn't exist
                Name = "test",
                Url = "http://test.com",
                Author = "Someone",
                AuthorUrl = "http://test.com"
            };

            var result = PackageBuilder.SavePackage(def);

            Assert.IsFalse(result);
        }

        [Test]
        public void Update()
        {
            var def = new PackageDefinition
            {
                Name = "test",
                Url = "http://test.com",
                Author = "Someone",
                AuthorUrl = "http://test.com"
            };
            var result = PackageBuilder.SavePackage(def);

            def.Name = "updated";
            def.Files = new List<string> {"hello.txt", "world.png"};
            result = PackageBuilder.SavePackage(def);
            Assert.IsTrue(result);

            //re-get
            def = PackageBuilder.GetById(def.Id);
            Assert.AreEqual("updated", def.Name);
            Assert.AreEqual(2, def.Files.Count);
            //TODO: There's a whole lot more assertions to be done

        }

        [Test]
        public void Export()
        {
            var file1 = $"~/{_testBaseFolder}/App_Plugins/MyPlugin/package.manifest";
            var file2 = $"~/{_testBaseFolder}/App_Plugins/MyPlugin/styles.css";
            var mappedFile1 = IOHelper.MapPath(file1);
            var mappedFile2 = IOHelper.MapPath(file2);
            Directory.CreateDirectory(Path.GetDirectoryName(mappedFile1));
            Directory.CreateDirectory(Path.GetDirectoryName(mappedFile2));
            File.WriteAllText(mappedFile1, "hello world");
            File.WriteAllText(mappedFile2, "hello world");

            var def = new PackageDefinition
            {
                Name = "test",
                Url = "http://test.com",
                Author = "Someone",
                AuthorUrl = "http://test.com",
                Files = new List<string> { file1, file2 },
                Actions = "<actions><Action alias='test' /></actions>"
            };
            var result = PackageBuilder.SavePackage(def);
            Assert.IsTrue(result);
            Assert.IsTrue(def.PackagePath.IsNullOrWhiteSpace());

            var zip = PackageBuilder.ExportPackage(def);

            def = PackageBuilder.GetById(def.Id); //re-get
            Assert.IsNotNull(def.PackagePath);

            using (var archive = ZipFile.OpenRead(IOHelper.MapPath(zip)))
            {
                Assert.AreEqual(3, archive.Entries.Count);

                //the 2 files we manually added
                Assert.IsNotNull(archive.Entries.Where(x => x.Name == "package.manifest"));
                Assert.IsNotNull(archive.Entries.Where(x => x.Name == "styles.css"));

                //this is the actual package definition/manifest (not the developer manifest!)
                var packageXml = archive.Entries.FirstOrDefault(x => x.Name == "package.xml");
                Assert.IsNotNull(packageXml);

                using (var stream = packageXml.Open())
                {
                    var xml = XDocument.Load(stream);
                    Assert.AreEqual("umbPackage", xml.Root.Name.ToString());
                    Assert.AreEqual(2, xml.Root.Element("files").Elements("file").Count());

                    Assert.AreEqual("<Actions><Action alias=\"test\" /></Actions>", xml.Element("umbPackage").Element("Actions").ToString(SaveOptions.DisableFormatting));

                    //TODO: There's a whole lot more assertions to be done
                }
            }
        }
    }
}
