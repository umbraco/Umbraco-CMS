using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class CreatedPackageSchemaTests : UmbracoIntegrationTest
{
    private ICreatedPackagesRepository CreatedPackageSchemaRepository =>
        GetRequiredService<ICreatedPackagesRepository>();

    private ICoreScopeProvider ScopeProvider => GetRequiredService<ICoreScopeProvider>();

    [Test]
    public void PackagesRepository_Can_Save_PackageDefinition()
    {
        using var scope = ScopeProvider.CreateCoreScope();
        var packageDefinition = new PackageDefinition { Name = "NewPack", DocumentTypes = new List<string> { "Root" } };
        var result = CreatedPackageSchemaRepository.SavePackage(packageDefinition);
        scope.Complete();
        Assert.IsTrue(result);
    }

    [Test]
    public void PackageRepository_GetAll_Returns_All_PackageDefinitions()
    {
        using var scope = ScopeProvider.CreateCoreScope();

        var packageDefinitionList = new List<PackageDefinition>
        {
            new() {Name = "PackOne"}, new() {Name = "PackTwo"}, new() {Name = "PackThree"}
        };
        foreach (var packageDefinition in packageDefinitionList)
        {
            CreatedPackageSchemaRepository.SavePackage(packageDefinition);
        }

        var loadedPackageDefinitions = CreatedPackageSchemaRepository.GetAll().ToList();
        scope.Complete();
        CollectionAssert.IsNotEmpty(loadedPackageDefinitions);
        CollectionAssert.AllItemsAreUnique(loadedPackageDefinitions);
        Assert.AreEqual(loadedPackageDefinitions.Count, 3);
    }

    [Test]
    public void PackageRepository_Can_Update_Package()
    {
        using var scope = ScopeProvider.CreateCoreScope();
        var packageDefinition = new PackageDefinition { Name = "TestPackage" };
        CreatedPackageSchemaRepository.SavePackage(packageDefinition);

        packageDefinition.Name = "UpdatedName";
        CreatedPackageSchemaRepository.SavePackage(packageDefinition);
        var results = CreatedPackageSchemaRepository.GetAll().ToList();
        scope.Complete();
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("UpdatedName", results.FirstOrDefault()?.Name);
    }
}
