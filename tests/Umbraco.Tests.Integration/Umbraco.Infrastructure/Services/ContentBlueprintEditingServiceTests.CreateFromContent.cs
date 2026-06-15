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
            Assert.That(createContentResult.Success, Is.True);
            Assert.That(createContentResult.Result, Is.Not.Null);
        });

        const string name = "Test Create From Content Blueprint";

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            createContentResult.Result.Content!.Key,
            name,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });
        VerifyCreate(result.Result.Content);

        // re-get and re-test
        VerifyCreate(await ContentBlueprintEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyCreate(IContent? createdBlueprint)
        {
            Assert.That(createdBlueprint, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createdBlueprint.Key, Is.Not.EqualTo(Guid.Empty));
                Assert.That(createdBlueprint.HasIdentity, Is.True);
                Assert.That(createdBlueprint.Name, Is.EqualTo(name));
                Assert.That(createdBlueprint.GetValue<string>("title"), Is.EqualTo("The title value"));
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.That(contentFound, Is.Null);
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
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result.Result.Content, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Result.Content.HasIdentity, Is.True);
            Assert.That(result.Result.Content.Key, Is.EqualTo(key));
            Assert.That(result.Result.Content.Name, Is.EqualTo(name));
        });

        // re-get and verify creation
        var blueprint = await ContentBlueprintEditingService.GetAsync(key);
        Assert.That(blueprint, Is.Not.Null);
        Assert.That(blueprint.Id, Is.EqualTo(result.Result.Content.Id));
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
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result1.Success, Is.True);
            Assert.That(result1.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result1.Result, Is.Not.Null);
        });

        // create another blueprint with the same name
        var result2 = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            name,
            null,
            Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result2.Success, Is.False);
            Assert.That(result2.Status, Is.EqualTo(ContentEditingOperationStatus.DuplicateName));
            Assert.That(result2.Result, Is.Not.Null);
        });
        Assert.That(result2.Result.Content, Is.Null);
    }
}
