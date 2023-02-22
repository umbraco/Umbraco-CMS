using Castle.Components.DictionaryAdapter.Xml;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class RelationServiceTests : UmbracoIntegrationTest
{
    private IRelationService RelationService => GetRequiredService<IRelationService>();

    [Test]
    [TestCase(true, true)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    public async Task Can_Create_RelationTypes_With_BiDirectional_And_IsDependency(bool isBiDirectional, bool isDependency)
    {
        var relationType = new RelationTypeBuilder()
            .WithChildObjectType(Constants.ObjectTypes.DocumentType)
            .WithParentObjectType(Constants.ObjectTypes.MediaType)
            .WithIsBidirectional(isBiDirectional)
            .WithIsDependency(isDependency)
            .Build();

        var result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserId);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RelationTypeOperationStatus.Success, result.Status);
            Assert.AreEqual(relationType.Name, result.Result.Name);
            Assert.AreEqual(relationType.ParentObjectType, result.Result.ParentObjectType);
            Assert.AreEqual(relationType.ChildObjectType, result.Result.ChildObjectType);
            Assert.AreEqual(relationType.IsBidirectional, result.Result.IsBidirectional);
            var asWithDependency = (IRelationTypeWithIsDependency)result.Result;
            Assert.AreEqual(relationType.IsDependency, asWithDependency.IsDependency);
        });

        var persistedRelationType = RelationService.GetRelationTypeById(result.Result.Key);

        Assert.AreEqual(result.Result, persistedRelationType);
    }

}
