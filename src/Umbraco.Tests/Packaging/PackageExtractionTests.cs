﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Packaging;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    class PackageExtractionTests
    {
        private const string PackageFileName = "Document_Type_Picker_1.1.umb";

        private static FileInfo GetTestPackagePath(string packageName)
        {
            const string testPackagesDirName = "Packaging\\Packages";
            string path = Path.Combine(IOHelper.GetRootDirectorySafe(), testPackagesDirName, packageName);
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
