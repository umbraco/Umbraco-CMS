using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PackageInstallationTest : TestWithDatabaseBase
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
            var path = IOHelper.MapPath("~/" + _testBaseFolder);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private CompiledPackageXmlParser Parser => new CompiledPackageXmlParser(new ConflictingPackageData(ServiceContext.MacroService, ServiceContext.FileService));

        private PackageDataInstallation PackageDataInstallation => new PackageDataInstallation(
            Logger, ServiceContext.FileService, ServiceContext.MacroService, ServiceContext.LocalizationService,
            ServiceContext.DataTypeService, ServiceContext.EntityService,
            ServiceContext.ContentTypeService, ServiceContext.ContentService,
            Factory.GetInstance<PropertyEditorCollection>());

        private IPackageInstallation PackageInstallation => new PackageInstallation(
            PackageDataInstallation,
            new PackageFileInstallation(Parser, ProfilingLogger),
            Parser, Mock.Of<IPackageActionRunner>(),
            packagesFolderPath: "~/Packaging/packages",//this is where our test zip file is 
            applicationRootFolder: new DirectoryInfo(IOHelper.GetRootDirectorySafe()),
            packageExtractionFolder: new DirectoryInfo(IOHelper.MapPath("~/" + _testBaseFolder))); //we don't want to extract package files to the real root, so extract to a test folder

        const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

        //[Test]
        //public void PackagingService_Can_ImportPackage()
        //{
        //    const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

        //    string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

        //    InstallationSummary installationSummary = packagingService.InstallPackage(testPackagePath);

        //    Assert.IsNotNull(installationSummary);
        //}


        [Test]
        public void Can_Read_Compiled_Package()
        {
            var package = PackageInstallation.ReadPackage(documentTypePickerUmb);
            Assert.IsNotNull(package);
            Assert.AreEqual(1, package.Files.Count);
            Assert.AreEqual("095e064b-ba4d-442d-9006-3050983c13d8.dll", package.Files[0].UniqueFileName);
            Assert.AreEqual("/bin", package.Files[0].OriginalPath);
            Assert.AreEqual("Auros.DocumentTypePicker.dll", package.Files[0].OriginalName);
            Assert.AreEqual("Document Type Picker", package.Name);
            Assert.AreEqual("1.1", package.Version);
            Assert.AreEqual("http://www.opensource.org/licenses/mit-license.php", package.LicenseUrl);
            Assert.AreEqual("MIT", package.License);
            Assert.AreEqual(3, package.UmbracoVersion.Major);
            Assert.AreEqual(RequirementsType.Legacy, package.UmbracoVersionRequirementsType);
            Assert.AreEqual("@tentonipete", package.Author);
            Assert.AreEqual("auros.co.uk", package.AuthorUrl);
            Assert.AreEqual("Document Type Picker datatype that enables back office user to select one or many document types.", package.Readme);

        }

        [Test]
        public void Can_Read_Compiled_Package_Warnings()
        {
            

            var preInstallWarnings = PackageInstallation.ReadPackage(documentTypePickerUmb).Warnings;
            Assert.IsNotNull(preInstallWarnings);

            //TODO: Assert!
        }

        [Test]
        public void Install_Files()
        {
            var package = PackageInstallation.ReadPackage(documentTypePickerUmb);
            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();

            var result = PackageInstallation.InstallPackageFiles(def, package, -1).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bin\\Auros.DocumentTypePicker.dll", result[0]);
            Assert.IsTrue(File.Exists(Path.Combine(IOHelper.MapPath("~/" + _testBaseFolder), result[0])));
        }


    }
}
