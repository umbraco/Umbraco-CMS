﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using File = System.IO.File;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PackageInstallationTest : TestWithDatabaseBase
    {
        private DirectoryInfo _testBaseFolder;

        public override void SetUp()
        {
            base.SetUp();
            var path = Path.Combine(TestHelper.WorkingDirectory, Guid.NewGuid().ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            _testBaseFolder = new DirectoryInfo(path);
        }

        public override void TearDown()
        {
            base.TearDown();

            //clear out files/folders
            if (_testBaseFolder.Exists)
                _testBaseFolder.Delete(true);
        }

        private CompiledPackageXmlParser Parser => new CompiledPackageXmlParser(
            new ConflictingPackageData(
                ServiceContext.MacroService,
                ServiceContext.FileService),
                Microsoft.Extensions.Options.Options.Create(new GlobalSettings()));

        private PackageDataInstallation PackageDataInstallation => new PackageDataInstallation(
            NullLoggerFactory.Instance.CreateLogger<PackageDataInstallation>(), NullLoggerFactory.Instance, ServiceContext.FileService, ServiceContext.MacroService, ServiceContext.LocalizationService,
            ServiceContext.DataTypeService, ServiceContext.EntityService,
            ServiceContext.ContentTypeService, ServiceContext.ContentService,
            Factory.GetRequiredService<PropertyEditorCollection>(),
            Factory.GetRequiredService<IScopeProvider>(),
            Factory.GetRequiredService<IShortStringHelper>(),
            Microsoft.Extensions.Options.Options.Create(new GlobalSettings()),
            Factory.GetRequiredService<ILocalizedTextService>(),
            Factory.GetRequiredService<IConfigurationEditorJsonSerializer>());

        private IPackageInstallation PackageInstallation => new PackageInstallation(
            PackageDataInstallation,
            new PackageFileInstallation(Parser, IOHelper, ProfilingLogger),
            Parser, Mock.Of<IPackageActionRunner>(),
            //we don't want to extract package files to the real root, so extract to a test folder
            Mock.Of<IHostingEnvironment>(x => x.ApplicationPhysicalPath == _testBaseFolder.FullName));

        private const string DocumentTypePickerPackage = "Document_Type_Picker_1.1.umb";
        private const string HelloPackage = "Hello_1.0.0.zip";

        [Test]
        public void Can_Read_Compiled_Package_1()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage)));
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
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), HelloPackage)));
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

            var filePath = Path.Combine(_testBaseFolder.FullName, "bin", "Auros.DocumentTypePicker.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, "test");

            //this is where our test zip file is
            var packageFile = Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage);
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
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage)));

            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();
            def.Files = new List<string>(); //clear out the files of the def for testing, this should be populated by the install

            var result = PackageInstallation.InstallPackageFiles(def, package, -1).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bin\\Auros.DocumentTypePicker.dll", result[0]);
            Assert.IsTrue(File.Exists(Path.Combine(_testBaseFolder.FullName, result[0])));

            //make sure the def is updated too
            Assert.AreEqual(result.Count, def.Files.Count);
        }

        [Test]
        public void Install_Data()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage)));
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
