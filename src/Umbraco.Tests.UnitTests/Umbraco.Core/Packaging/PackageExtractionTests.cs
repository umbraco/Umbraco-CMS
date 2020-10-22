using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Hosting;
using Umbraco.Core.Packaging;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    public class PackageExtractionTests
    {
        private const string PackageFileName = "Document_Type_Picker_1.1.umb";

        private static FileInfo GetTestPackagePath(string packageName)
        {
            var testPackagesDirName = Path.Combine("Umbraco.Core","Packaging","Packages");
            var testDir = TestContext.CurrentContext.TestDirectory.Split("bin")[0];
            var path = Path.Combine(testDir, testPackagesDirName, packageName);
            return new FileInfo(path);
        }

        [Test]
        public void ReadFilesFromArchive_NumberOfFilesIs1_SearchingForPackageXmlFile()
        {
            // Arrange
            var sut = new PackageExtraction();

            // Act
            var result = sut.ReadFilesFromArchive(GetTestPackagePath(PackageFileName), new[] { "Package.xml" });

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void FindMissingFiles_1_UnknownFile()
        {
            // Arrange
            var sut = new PackageExtraction();

            // Act
            var result = sut.FindMissingFiles(GetTestPackagePath(PackageFileName), new[] { "DoesNotExists.XYZ" });

            // Assert
            Assert.AreEqual(1, result.Count());
        }
    }
}
