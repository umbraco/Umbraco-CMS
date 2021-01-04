// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Packaging;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Packaging
{
    [TestFixture]
    public class PackageExtractionTests
    {
        private const string PackageFileName = "Document_Type_Picker_1.1.umb";

        private static FileInfo GetTestPackagePath(string packageName)
        {
            var testPackagesDirName = Path.Combine("Umbraco.Core", "Packaging", "Packages");
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
            IEnumerable<byte[]> result = sut.ReadFilesFromArchive(GetTestPackagePath(PackageFileName), new[] { "Package.xml" });

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void FindMissingFiles_1_UnknownFile()
        {
            // Arrange
            var sut = new PackageExtraction();

            // Act
            IEnumerable<string> result = sut.FindMissingFiles(GetTestPackagePath(PackageFileName), new[] { "DoesNotExists.XYZ" });

            // Assert
            Assert.AreEqual(1, result.Count());
        }
    }
}
