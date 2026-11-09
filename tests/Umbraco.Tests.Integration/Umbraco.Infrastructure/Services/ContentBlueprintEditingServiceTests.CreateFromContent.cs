using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Can_Create_From_Content()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var createContentResult = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(createContentResult.Success);
            Assert.IsNotNull(createContentResult.Result);
        });

        const string name = "Test Create From Content Blueprint";

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            createContentResult.Result.Content!.Key,
            name,
            null,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        });
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentBlueprintEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreate(IContent? createdBlueprint)
        {
            Assert.IsNotNull(createdBlueprint);
            Assert.Multiple(() =>
            {
                Assert.AreNotEqual(Guid.Empty, createdBlueprint.Key);
                Assert.IsTrue(createdBlueprint.HasIdentity);
                Assert.AreEqual(name, createdBlueprint.Name);
                Assert.AreEqual("The title value", createdBlueprint.GetValue<string>("title"));
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.IsNull(contentFound);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Create_From_Content_With_Explicit_Key(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());

        var key = Guid.NewGuid();
        const string name = "Test Create From Content Blueprint";

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            name,
            key,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result.Content);
        });
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Result.Content.HasIdentity);
            Assert.AreEqual(key, result.Result.Content.Key);
            Assert.AreEqual(name, result.Result.Content.Name);
        });

        // re-get and verify creation
        var blueprint = await ContentBlueprintEditingService.GetAsync(key);
        Assert.IsNotNull(blueprint);
        Assert.AreEqual(result.Result.Content.Id, blueprint.Id);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Create_From_Content_With_Duplicate_Name(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());

        const string name = "Test Create From Content Blueprint";

        var result1 = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            name,
            null,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result1.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result1.Status);
            Assert.IsNotNull(result1.Result);
        });

        // create another blueprint with the same name
        var result2 = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            name,
            null,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result2.Success);
            Assert.AreEqual(ContentEditingOperationStatus.DuplicateName, result2.Status);
            Assert.IsNotNull(result2.Result);
        });
        Assert.IsNull(result2.Result.Content);
    }

    [Test]
    public async Task Can_Create_From_Content_In_A_Folder()
    {
        var content = await CreateInvariantContent();

        var containerKey = Guid.NewGuid();
        var container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var blueprintKey = Guid.NewGuid();
        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            blueprintKey,
            containerKey,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        });

        // re-get to prove the placement was persisted
        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.IsNotNull(blueprint);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container!.Id, blueprint.ParentId);
            Assert.AreEqual($"{container.Path},{blueprint.Id}", blueprint.Path);
        });

        var children = GetBlueprintChildren(containerKey);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, children.Length);
            Assert.AreEqual(blueprintKey, children.First().Key);
        });
    }

    [Test]
    public async Task Can_Create_From_Content_At_Root_When_No_Parent_Is_Specified()
    {
        var content = await CreateInvariantContent();

        var blueprintKey = Guid.NewGuid();
        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            blueprintKey,
            null,
            Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.IsNotNull(blueprint);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.Root, blueprint.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{blueprint.Id}", blueprint.Path);
        });
    }

    [Test]
    public async Task Cannot_Create_From_Content_In_Non_Existent_Folder()
    {
        var content = await CreateInvariantContent();

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            null,
            Guid.NewGuid(),
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.ParentNotFound, result.Status);
        });
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Cannot_Create_From_Content_For_Content_Type_Excluded_By_Content_Type_Filter()
    {
        var content = await CreateInvariantContent();
        ExcludingContentTypeFilter.ExcludedContentTypeKey = content.ContentType.Key;

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            null,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        });
        Assert.IsNull(result.Result.Content);
    }
}
