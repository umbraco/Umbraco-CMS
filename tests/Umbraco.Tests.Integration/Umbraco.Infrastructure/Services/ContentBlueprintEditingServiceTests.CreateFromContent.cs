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
        var contentType = CreateInvariantContentType();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
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
        var content = await (variant ? CreateVariantContent() : CreateInvariantContent());

        var key = Guid.NewGuid();
        const string name = "Test Create From Content Blueprint";

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            name,
            key,
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
        var content = await (variant ? CreateVariantContent() : CreateInvariantContent());

        const string name = "Test Create From Content Blueprint";

        var result1 = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            name,
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
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result2.Success);
            Assert.AreEqual(ContentEditingOperationStatus.DuplicateName, result2.Status);
            Assert.IsNotNull(result2.Result);
        });
        Assert.IsNull(result2.Result.Content);
    }
}
