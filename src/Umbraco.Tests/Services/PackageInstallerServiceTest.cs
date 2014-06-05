using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class PackageInstallerServiceTest : BaseServiceTest
    {
        private const string DOCUMENT_TYPE_PICKER_UMB = "Document_Type_Picker_1.1.umb";
        private const string NETMESTER_BEST_PRACTICE_BASE_UMB = "Netmester.BestPractice.Base_0.0.0.1.umb";
        private const string TEST_PACKAGES_DIR_NAME = "Packages";

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void PackageInstallerService_TestSomething()
        {
            // Arrange
            var path = GetTestPackagePath(DOCUMENT_TYPE_PICKER_UMB);
            
            // Act
            var packageMetaData = ServiceContext.PackageInstallerService.GetMetaData(path);

            // Assert
            Assert.IsNotNull(packageMetaData);
        }

        [Test]
        public void PackageInstallerService_TestSomethingelse()
        {
            // Arrange
            var path = GetTestPackagePath(DOCUMENT_TYPE_PICKER_UMB);
            
            // Act
            var importIssues = ServiceContext.PackageInstallerService.GetPreInstallWarnings(path);

            // Assert
            Assert.IsNotNull(importIssues);
        }


        [Test]
        public void PackageInstallerService_TestSomethingnew()
        {
            // Arrange
            var path = GetTestPackagePath(NETMESTER_BEST_PRACTICE_BASE_UMB);

            // Act
            var importIssues = ServiceContext.PackageInstallerService.GetPreInstallWarnings(path);

            // Assert
            Assert.IsNotNull(importIssues);
        }


        
        [Test]
        public void PackageInstallerService_TestSomethingthered()
        {
            // Arrange
            var path = GetTestPackagePath(DOCUMENT_TYPE_PICKER_UMB);

            // Act
            var packageMetaData = ServiceContext.PackageInstallerService.InstallPackageFile(path, -1);
            // Assert
            IDataTypeDefinition dataTypeDefinitionById = ApplicationContext.Services.DataTypeService.GetDataTypeDefinitionById(
                packageMetaData.DataTypesInstalled.Single().Id);

            Assert.IsNotNull(dataTypeDefinitionById);

            foreach (var result in packageMetaData.FilesInstalled.Select(fi => fi.Key))
            {
                Assert.IsTrue(System.IO.File.Exists(result));
                System.IO.File.Delete(result);
            }
        }

        private static string GetTestPackagePath(string packageName)
        {
            string path = Path.Combine(Core.Configuration.GlobalSettings.FullpathToRoot, TEST_PACKAGES_DIR_NAME, packageName);
            return path;
        }
    }
}
