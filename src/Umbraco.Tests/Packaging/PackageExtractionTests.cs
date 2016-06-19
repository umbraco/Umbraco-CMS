using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Packaging;

namespace Umbraco.Tests.Packaging
{

    [TestFixture]
    public class PackageExtractionTests
    {
        private const string PackageFileName = "Packages/Document_Type_Picker_1.1.umb";
        [Test]
        public void ReadFilesFromArchive_NumberOfFilesIs1_SearchingForPackageXmlFile()
        {
            // Arrange
            var sut = new PackageExtraction();

            // Act
            var result = sut.ReadFilesFromArchive(PackageFileName, new string[] { "Package.xml" });

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void FindMissingFiles_1_UnknownFile()
        {
            // Arrange
            var sut = new PackageExtraction();

            // Act
            var result = sut.FindMissingFiles(PackageFileName, new string[] { "DoesNotExists.XYZ" });

            // Assert
            Assert.AreEqual(1, result.Count());
        }
    }
}
