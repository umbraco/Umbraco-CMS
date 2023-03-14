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
    public async Task Can_Create_Relation_Types_With_Bi_Directional_And_Is_Dependency(bool isBiDirectional, bool isDependency)
    {
        IRelationTypeWithIsDependency relationType = new RelationTypeBuilder()
            .WithChildObjectType(Constants.ObjectTypes.DocumentType)
            .WithParentObjectType(Constants.ObjectTypes.MediaType)
            .WithIsBidirectional(isBiDirectional)
            .WithIsDependency(isDependency)
            .Build();

        Attempt<IRelationType, RelationTypeOperationStatus> result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RelationTypeOperationStatus.Success, result.Status);
        });

        AssertRelationTypesAreSame(relationType, result.Result);
    }

    [Test]
    [TestCase(Constants.ObjectTypes.Strings.Document, Constants.ObjectTypes.Strings.Media)]
    [TestCase(Constants.ObjectTypes.Strings.Member, Constants.ObjectTypes.Strings.DocumentType)]
    [TestCase(Constants.ObjectTypes.Strings.MediaType, Constants.ObjectTypes.Strings.MemberType)]
    [TestCase(Constants.ObjectTypes.Strings.DataType, Constants.ObjectTypes.Strings.MemberGroup)]
    [TestCase(Constants.ObjectTypes.Strings.Document, Constants.ObjectTypes.Strings.ContentRecycleBin)]
    [TestCase(Constants.ObjectTypes.Strings.Document, Constants.ObjectTypes.Strings.SystemRoot)]
    public async Task Can_Create_Relation_Types_With_Allowed_Object_Types(string childObjectTypeGuid, string parentObjectTypeGuid)
    {
        IRelationTypeWithIsDependency relationType = new RelationTypeBuilder()
            .WithChildObjectType(new Guid(childObjectTypeGuid))
            .WithParentObjectType(new Guid(parentObjectTypeGuid))
            .Build();

        Attempt<IRelationType, RelationTypeOperationStatus> result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RelationTypeOperationStatus.Success, result.Status);
        });
        AssertRelationTypesAreSame(relationType, result.Result);
    }

    [Test]
    [TestCase(Constants.ObjectTypes.Strings.Document, "53E492BD-F242-40A7-8F21-7D649463DD23", RelationTypeOperationStatus.InvalidChildObjectType)]
    [TestCase("E7524E34-F84F-43DE-92E2-25999785B7EA", Constants.ObjectTypes.Strings.DataType, RelationTypeOperationStatus.InvalidParentObjectType)]
    [TestCase("00000000-0000-0000-0000-000000000000", Constants.ObjectTypes.Strings.Document, RelationTypeOperationStatus.InvalidParentObjectType)]
    [TestCase(Constants.ObjectTypes.Strings.IdReservation, Constants.ObjectTypes.Strings.Document, RelationTypeOperationStatus.InvalidParentObjectType)]
    public async Task Cannot_Create_Relation_Types_With_Disallowed_Object_Types(string parentObjectTypeGuid, string childObjectTypeGuid, RelationTypeOperationStatus relationTypeOperationStatus)
    {
        IRelationTypeWithIsDependency relationType = new RelationTypeBuilder()
            .WithChildObjectType(new Guid(childObjectTypeGuid))
            .WithParentObjectType(new Guid(parentObjectTypeGuid))
            .Build();

        Attempt<IRelationType, RelationTypeOperationStatus> result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(relationTypeOperationStatus, result.Status);
        });
    }

    [Test]
    public async Task Can_Create_Relation_Type_With_Key()
    {
        const string key = "82E7631C-0417-460C-91C1-F65784627143";
        IRelationTypeWithIsDependency relationType = new RelationTypeBuilder()
            .WithChildObjectType(Constants.ObjectTypes.DocumentType)
            .WithParentObjectType(Constants.ObjectTypes.DocumentType)
            .WithKey(new Guid(key))
            .Build();

        Attempt<IRelationType, RelationTypeOperationStatus> result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(RelationTypeOperationStatus.Success, result.Status);
            Assert.IsTrue(string.Equals(key, result.Result.Key.ToString(), StringComparison.OrdinalIgnoreCase));
        });

        AssertRelationTypesAreSame(relationType, result.Result);
    }

    [Test]
    [TestCase(-1000000)]
    [TestCase(-1)]
    [TestCase(100)]
    [TestCase(10000000)]
    public async Task Cannot_Create_Relation_Types_With_Id(int id)
    {
        IRelationTypeWithIsDependency relationType = new RelationTypeBuilder()
            .WithChildObjectType(Constants.ObjectTypes.DocumentType)
            .WithParentObjectType(Constants.ObjectTypes.DocumentType)
            .WithId(id)
            .Build();

        Attempt<IRelationType, RelationTypeOperationStatus> result = await RelationService.CreateAsync(relationType, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(RelationTypeOperationStatus.InvalidId, result.Status);
        });
    }

    private void AssertRelationTypesAreSame(IRelationTypeWithIsDependency relationType, IRelationType result)
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual(relationType.Name, result.Name);
            Assert.AreEqual(relationType.ParentObjectType, result.ParentObjectType);
            Assert.AreEqual(relationType.ChildObjectType, result.ChildObjectType);
            Assert.AreEqual(relationType.IsBidirectional, result.IsBidirectional);
            var asWithDependency = (IRelationTypeWithIsDependency)result;
            Assert.AreEqual(relationType.IsDependency, asWithDependency.IsDependency);
        });
        var persistedRelationType = RelationService.GetRelationTypeById(result.Key);

        Assert.AreEqual(result, persistedRelationType);
    }

}
