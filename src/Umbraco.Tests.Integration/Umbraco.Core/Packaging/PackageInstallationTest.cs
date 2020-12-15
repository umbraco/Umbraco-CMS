using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PackageInstallationTest : UmbracoIntegrationTest
    {
        private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();
        private IPackageInstallation PackageInstallation => GetRequiredService<IPackageInstallation>();

        private const string DocumentTypePickerPackage = "Document_Type_Picker_1.1.umb";
        private const string HelloPackage = "Hello_1.0.0.zip";

        [Test]
        public void Can_Read_Compiled_Package_1()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage)));
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
            Assert.AreEqual(1, package.DataTypes.Count());
        }

        [Test]
        public void Can_Read_Compiled_Package_2()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), HelloPackage)));
            Assert.IsNotNull(package);
            Assert.AreEqual(0, package.Files.Count);
            Assert.AreEqual("Hello", package.Name);
            Assert.AreEqual("1.0.0", package.Version);
            Assert.AreEqual("http://opensource.org/licenses/MIT", package.LicenseUrl);
            Assert.AreEqual("MIT License", package.License);
            Assert.AreEqual(8, package.UmbracoVersion.Major);
            Assert.AreEqual(0, package.UmbracoVersion.Minor);
            Assert.AreEqual(0, package.UmbracoVersion.Build);
            Assert.AreEqual(RequirementsType.Strict, package.UmbracoVersionRequirementsType);
            Assert.AreEqual("asdf", package.Author);
            Assert.AreEqual("http://hello.com", package.AuthorUrl);
            Assert.AreEqual("asdf", package.Readme);
            Assert.AreEqual(1, package.Documents.Count());
            Assert.AreEqual("root", package.Documents.First().ImportMode);
            Assert.AreEqual(1, package.DocumentTypes.Count());
            Assert.AreEqual(1, package.Templates.Count());
            Assert.AreEqual(1, package.DataTypes.Count());
        }

        [Test]
        public void Can_Read_Compiled_Package_Warnings()
        {
            //copy a file to the same path that the package will install so we can detect file conflicts

            var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "bin", "Auros.DocumentTypePicker.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, "test");

            //this is where our test zip file is
            var packageFile = Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage);
            Console.WriteLine(packageFile);

            var package = PackageInstallation.ReadPackage(new FileInfo(packageFile));
            var preInstallWarnings = package.Warnings;
            Assert.IsNotNull(preInstallWarnings);

            Assert.AreEqual(1, preInstallWarnings.FilesReplaced.Count());
            Assert.AreEqual("bin\\Auros.DocumentTypePicker.dll", preInstallWarnings.FilesReplaced.First());

            // TODO: More Asserts
        }

        [Test]
        public void Install_Files()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage)));

            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();
            def.Files = new List<string>(); //clear out the files of the def for testing, this should be populated by the install

            var result = PackageInstallation.InstallPackageFiles(def, package, -1).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bin\\Auros.DocumentTypePicker.dll", result[0]);
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, result[0])));

            //make sure the def is updated too
            Assert.AreEqual(result.Count, def.Files.Count);
        }

        [Test]
        public void Install_Data()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage)));
            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();

            var summary = PackageInstallation.InstallPackageData(def, package, -1);

            Assert.AreEqual(1, summary.DataTypesInstalled.Count());


            //make sure the def is updated too
            Assert.AreEqual(summary.DataTypesInstalled.Count(), def.DataTypes.Count);
        }
    }
}
