using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class CreatedPackageSchemaTests : UmbracoIntegrationTest
{
    private ICreatedPackagesRepository CreatedPackageSchemaRepository =>
        GetRequiredService<ICreatedPackagesRepository>();

    [Test]
    public void PackagesRepository_Can_Save_PackageDefinition()
    {
        var packageDefinition = new PackageDefinition { Name = "NewPack", DocumentTypes = new List<string> { "Root" } };
        var result = CreatedPackageSchemaRepository.SavePackage(packageDefinition);
        Assert.IsTrue(result);
    }

    [Test]
    public void PackageRepository_GetAll_Returns_All_PackageDefinitions()
    {
        var packageDefinitionList = new List<PackageDefinition>
        {
            new() {Name = "PackOne"}, new() {Name = "PackTwo"}, new() {Name = "PackThree"}
        };
        foreach (var packageDefinition in packageDefinitionList)
        {
            CreatedPackageSchemaRepository.SavePackage(packageDefinition);
        }

        var loadedPackageDefinitions = CreatedPackageSchemaRepository.GetAll().ToList();
        CollectionAssert.IsNotEmpty(loadedPackageDefinitions);
        CollectionAssert.AllItemsAreUnique(loadedPackageDefinitions);
        Assert.AreEqual(loadedPackageDefinitions.Count, 3);
    }

    [Test]
    public void PackageRepository_Can_Update_Package()
    {
        var packageDefinition = new PackageDefinition { Name = "TestPackage" };
        CreatedPackageSchemaRepository.SavePackage(packageDefinition);

        packageDefinition.Name = "UpdatedName";
        CreatedPackageSchemaRepository.SavePackage(packageDefinition);
        var results = CreatedPackageSchemaRepository.GetAll().ToList();

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("UpdatedName", results.FirstOrDefault()?.Name);
    }
}
