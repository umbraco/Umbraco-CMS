using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests : DocumentNavigationServiceTestsBase
{
    [SetUp]
    public async Task Setup()
    {
        // Root
        //    - Child 1
        //      - Grandchild 1
        //      - Grandchild 2
        //    - Child 2
        //      - Grandchild 3
        //        - Great-grandchild 1
        //    - Child 3
        //      - Grandchild 4

        // Doc Type
        ContentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page");
        ContentType.Key = new Guid("DD72B8A6-2CE3-47F0-887E-B695A1A5D086");
        ContentType.AllowedAsRoot = true;
        ContentType.AllowedTemplates = null;
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias) };
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var rootModel = CreateContentCreateModel("Root", new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF"));
        var rootCreateAttempt = await ContentEditingService.CreateAsync(rootModel, Constants.Security.SuperUserKey);
        Root = rootCreateAttempt.Result.Content!;

        var child1Model = CreateContentCreateModel("Child 1", new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE"), Root.Key);
        var child1CreateAttempt = await ContentEditingService.CreateAsync(child1Model, Constants.Security.SuperUserKey);
        Child1 = child1CreateAttempt.Result.Content!;

        var grandchild1Model = CreateContentCreateModel("Grandchild 1", new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573"), Child1.Key);
        var grandchild1CreateAttempt = await ContentEditingService.CreateAsync(grandchild1Model, Constants.Security.SuperUserKey);
        Grandchild1 = grandchild1CreateAttempt.Result.Content!;

        var grandchild2Model = CreateContentCreateModel("Grandchild 2", new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB"), Child1.Key);
        var grandchild2CreateAttempt = await ContentEditingService.CreateAsync(grandchild2Model, Constants.Security.SuperUserKey);
        Grandchild2 = grandchild2CreateAttempt.Result.Content!;

        var child2Model = CreateContentCreateModel("Child 2", new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96"), Root.Key);
        var child2CreateAttempt = await ContentEditingService.CreateAsync(child2Model, Constants.Security.SuperUserKey);
        Child2 = child2CreateAttempt.Result.Content!;

        var grandchild3Model = CreateContentCreateModel("Grandchild 3", new Guid("D63C1621-C74A-4106-8587-817DEE5FB732"), Child2.Key);
        var grandchild3CreateAttempt = await ContentEditingService.CreateAsync(grandchild3Model, Constants.Security.SuperUserKey);
        Grandchild3 = grandchild3CreateAttempt.Result.Content!;

        var greatGrandchild1Model = CreateContentCreateModel("Great-grandchild 1", new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7"), Grandchild3.Key);
        var greatGrandchild1CreateAttempt = await ContentEditingService.CreateAsync(greatGrandchild1Model, Constants.Security.SuperUserKey);
        GreatGrandchild1 = greatGrandchild1CreateAttempt.Result.Content!;

        var child3Model = CreateContentCreateModel("Child 3", new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF"), Root.Key);
        var child3CreateAttempt = await ContentEditingService.CreateAsync(child3Model, Constants.Security.SuperUserKey);
        Child3 = child3CreateAttempt.Result.Content!;

        var grandchild4Model = CreateContentCreateModel("Grandchild 4", new Guid("F381906C-223C-4466-80F7-B63B4EE073F8"), Child3.Key);
        var grandchild4CreateAttempt = await ContentEditingService.CreateAsync(grandchild4Model, Constants.Security.SuperUserKey);
        Grandchild4 = grandchild4CreateAttempt.Result.Content!;
    }

    [Test]
    public async Task Structure_Does_Not_Update_When_Scope_Is_Not_Completed()
    {
        // Arrange
        Guid notCreatedRootKey = new Guid("516927E5-8574-497B-B45B-E27EFAB47DE4");

        // Create node at content root
        var createModel = CreateContentCreateModel("Root 2", notCreatedRootKey);

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        // Act
        var nodeExists = DocumentNavigationQueryService.TryGetParentKey(notCreatedRootKey, out _);

        // Assert
        Assert.IsFalse(nodeExists);
    }
}
