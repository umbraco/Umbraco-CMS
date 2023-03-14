// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class PackageInstallationTest : UmbracoIntegrationTest
{
    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private PackageInstallation PackageInstallation => (PackageInstallation)GetRequiredService<IPackageInstallation>();

    private const string DocumentTypePickerPackage = "Document_Type_Picker_1.1.package.xml";
    private const string HelloPackage = "Hello_1.0.0.package.xml";

    [Test]
    public void Can_Read_Compiled_Package_1()
    {
        var testPackageFile = new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage));
        using var fileStream = testPackageFile.OpenRead();
        var package = PackageInstallation.ReadPackage(XDocument.Load(fileStream));
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(package);
            Assert.AreEqual("Document Type Picker", package.Name);
            Assert.AreEqual(1, package.DataTypes.Count());
        });
    }

    [Test]
    public void Can_Read_Compiled_Package_2()
    {
        var testPackageFile =
            new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), HelloPackage));
        using var fileStream = testPackageFile.OpenRead();
        var package = PackageInstallation.ReadPackage(XDocument.Load(fileStream));
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(package);
            Assert.AreEqual("Hello", package.Name);
            Assert.AreEqual(1, package.Documents.Count());
            Assert.AreEqual(1, package.DocumentTypes.Count());
            Assert.AreEqual(1, package.Templates.Count());
            Assert.AreEqual(1, package.DataTypes.Count());
        });
    }

    [Test]
    public void Can_Read_Compiled_Package_Warnings()
    {
        // Copy a file to the same path that the package will install so we can detect file conflicts.
        var filePath = Path.Combine(HostingEnvironment.MapPathContentRoot("~/"), "bin", "Auros.DocumentTypePicker.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, "test");

        // this is where our test zip file is
        var packageFile = Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage);
        Console.WriteLine(packageFile);

        using var fileStream = File.OpenRead(packageFile);
        var package = PackageInstallation.ReadPackage(XDocument.Load(fileStream));
        var preInstallWarnings = package.Warnings;
        var dataType = package.DataTypes.First();
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Document Type Picker", package.Name);
            Assert.AreEqual("3593d8e7-8b35-47b9-beda-5e830ca8c93c", dataType.LastAttribute?.Value);
            Assert.AreEqual("Document Type Picker", dataType.FirstAttribute?.Value);
            Assert.IsNotNull(preInstallWarnings);
            Assert.AreEqual(0, preInstallWarnings.ConflictingMacros.Count());
            Assert.AreEqual(0, preInstallWarnings.ConflictingStylesheets.Count());
            Assert.AreEqual(0, preInstallWarnings.ConflictingTemplates.Count());
        });
    }

    [Test]
    public void Install_Data()
    {
        var testPackageFile = new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage));
        using var fileStream = testPackageFile.OpenRead();
        var package = PackageInstallation.ReadPackage(XDocument.Load(fileStream));

        var summary = PackageInstallation.InstallPackageData(package, -1, out var def);

        Assert.AreEqual(1, summary.DataTypesInstalled.Count());

        // make sure the def is updated too
        Assert.AreEqual(summary.DataTypesInstalled.Count(), def.DataTypes.Count);
    }
}
