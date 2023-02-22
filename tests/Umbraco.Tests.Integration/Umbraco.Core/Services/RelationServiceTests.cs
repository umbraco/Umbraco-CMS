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
        });

        AssertRelationTypesAreSame(relationType, result.Result);

        var persistedRelationType = RelationService.GetRelationTypeById(result.Result.Key);

        Assert.AreEqual(result.Result, persistedRelationType);
    }


    [Test]
    [TestCase(Constants.ObjectTypes.Strings.Document, Constants.ObjectTypes.Strings.Media)]
    [TestCase(Constants.ObjectTypes.Strings.Member, Constants.ObjectTypes.Strings.DocumentType)]
    [TestCase(Constants.ObjectTypes.Strings.MediaType, Constants.ObjectTypes.Strings.MemberType)]
    [TestCase(Constants.ObjectTypes.Strings.DataType, Constants.ObjectTypes.Strings.MemberGroup)]
    [TestCase(Constants.ObjectTypes.Strings.Document, Constants.ObjectTypes.Strings.ContentRecycleBin)]
    [TestCase(Constants.ObjectTypes.Strings.Document, Constants.ObjectTypes.Strings.SystemRoot)]
    public async Task Can_Create_RelationTypes_With_Allowed_ObjectTypes(string childObjectTypeGuid, string parentObjectTypeGuid)
    {
        var relationType = new RelationTypeBuilder()
            .WithChildObjectType(new Guid(childObjectTypeGuid))
            .WithParentObjectType(new Guid(parentObjectTypeGuid))
            .Build();

        var result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserId);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RelationTypeOperationStatus.Success, result.Status);
        });
        AssertRelationTypesAreSame(relationType, result.Result);

        var persistedRelationType = RelationService.GetRelationTypeById(result.Result.Key);

        Assert.AreEqual(result.Result, persistedRelationType);
    }

    private void AssertRelationTypesAreSame(IRelationTypeWithIsDependency relationType, IRelationType result) =>
        Assert.Multiple(() =>
        {
            Assert.AreEqual(relationType.Name, result.Name);
            Assert.AreEqual(relationType.ParentObjectType, result.ParentObjectType);
            Assert.AreEqual(relationType.ChildObjectType, result.ChildObjectType);
            Assert.AreEqual(relationType.IsBidirectional, result.IsBidirectional);
            var asWithDependency = (IRelationTypeWithIsDependency)result;
            Assert.AreEqual(relationType.IsDependency, asWithDependency.IsDependency);
        });
}
