using System.IO;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PackagingServiceTests : TestWithSomeContentBase
    {
        

        private static string GetTestPackagePath(string packageName)
        {
            const string testPackagesDirName = "Packaging\\Packages";
            string path = Path.Combine(IOHelper.GetRootDirectorySafe(), testPackagesDirName, packageName);
            return path;
        }


        [Test]
        public void PackagingService_Can_ImportPackage()
        {
            var packagingService = (PackagingService)ServiceContext.PackagingService;

            const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

            string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

            InstallationSummary installationSummary = packagingService.InstallPackage(testPackagePath);

            Assert.IsNotNull(installationSummary);
        }


        [Test]
        public void PackagingService_Can_GetPackageMetaData()
        {
            var packagingService = (PackagingService)ServiceContext.PackagingService;

            const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

            string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

            var packageMetaData = packagingService.GetPackageMetaData(testPackagePath);
            Assert.IsNotNull(packageMetaData);
        }

        [Test]
        public void PackagingService_Can_GetPackageWarnings()
        {
            var packagingService = (PackagingService)ServiceContext.PackagingService;

            const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

            string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

            PreInstallWarnings preInstallWarnings = packagingService.GetPackageWarnings(testPackagePath);
            Assert.IsNotNull(preInstallWarnings);
        }

        
    }
}
