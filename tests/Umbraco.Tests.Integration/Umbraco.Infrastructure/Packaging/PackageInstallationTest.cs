// Copyright (c) Umbraco.
// See LICENSE for more details.

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
internal sealed class PackageInstallationTest : UmbracoIntegrationTest
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
            Assert.That(package, Is.Not.Null);
            Assert.That(package.Name, Is.EqualTo("Document Type Picker"));
            Assert.That(package.DataTypes.Count(), Is.EqualTo(1));
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
            Assert.That(package, Is.Not.Null);
            Assert.That(package.Name, Is.EqualTo("Hello"));
            Assert.That(package.Documents.Count(), Is.EqualTo(1));
            Assert.That(package.DocumentTypes.Count(), Is.EqualTo(1));
            Assert.That(package.Templates.Count(), Is.EqualTo(1));
            Assert.That(package.DataTypes.Count(), Is.EqualTo(1));
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
            Assert.That(package.Name, Is.EqualTo("Document Type Picker"));
            Assert.That(dataType.LastAttribute?.Value, Is.EqualTo("3593d8e7-8b35-47b9-beda-5e830ca8c93c"));
            Assert.That(dataType.FirstAttribute?.Value, Is.EqualTo("Document Type Picker"));
            Assert.That(preInstallWarnings, Is.Not.Null);
            Assert.That(preInstallWarnings.ConflictingStylesheets.Count(), Is.EqualTo(0));
            Assert.That(preInstallWarnings.ConflictingTemplates.Count(), Is.EqualTo(0));
        });
    }

    [Test]
    public void Install_Data()
    {
        var testPackageFile = new FileInfo(Path.Combine(HostingEnvironment.MapPathContentRoot("~/TestData/Packages"), DocumentTypePickerPackage));
        using var fileStream = testPackageFile.OpenRead();
        var package = PackageInstallation.ReadPackage(XDocument.Load(fileStream));

        var summary = PackageInstallation.InstallPackageData(package, -1, out var def);

        Assert.That(summary.DataTypesInstalled.Count(), Is.EqualTo(1));

        // make sure the def is updated too
        Assert.That(def.DataTypes, Has.Count.EqualTo(summary.DataTypesInstalled.Count()));
    }
}
