using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Can_Update_Invariant()
    {
        var blueprint = await CreateInvariantContentBlueprint();

        var updateModel = new ContentBlueprintUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Blueprint Name" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ],
        };

        var result = await ContentBlueprintEditingService.UpdateAsync(blueprint.Key, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentBlueprintEditingService.GetAsync(blueprint.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedContent.Name, Is.EqualTo("Updated Blueprint Name"));
                Assert.That(updatedContent.GetValue<string>("title"), Is.EqualTo("The updated title"));
                Assert.That(updatedContent.GetValue<string>("text"), Is.EqualTo("The updated text"));
            });
        }
    }

    [Test]
    public async Task Can_Update_Variant()
    {
        var blueprint = await CreateVariantContentBlueprint();

        var updateModel = new ContentBlueprintUpdateModel
        {
            Properties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated blueprint invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title", Culture = "da-DK" },
            },
            Variants = new[]
            {
                new VariantModel { Culture = "en-US", Name = "Updated Blueprint English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Blueprint Danish Name" },
            },
        };

        var result = await ContentBlueprintEditingService.UpdateAsync(blueprint.Key, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentBlueprintEditingService.GetAsync(blueprint.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedContent.GetCultureName("en-US"), Is.EqualTo("Updated Blueprint English Name"));
                Assert.That(updatedContent.GetCultureName("da-DK"), Is.EqualTo("Updated Blueprint Danish Name"));
                Assert.That(updatedContent.GetValue<string>("invariantTitle"), Is.EqualTo("The updated blueprint invariant title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", "en-US"), Is.EqualTo("The updated English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The updated Danish title"));
            });
        }
    }

    [Test]
    public async Task Cannot_Update_With_Duplicate_Name_For_The_Same_Content_Type()
    {
        var blueprintToUpdate = await CreateInvariantContentBlueprint();

        // create another blueprint of the same content type
        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = blueprintToUpdate.ContentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Blueprint" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var createResult = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(createResult.Success, Is.True);
            Assert.That(createResult.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(createResult.Result, Is.Not.Null);
        });

        // update a blueprint with the same name
        var updateModel = new ContentBlueprintUpdateModel
        {
            Variants = [new VariantModel { Name = "Test Blueprint" }]
        };

        var updateResult = await ContentBlueprintEditingService.UpdateAsync(blueprintToUpdate.Key, updateModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Status, Is.EqualTo(ContentEditingOperationStatus.DuplicateName));
            Assert.That(updateResult.Result, Is.Not.Null);
        });
        Assert.That(updateResult.Result.Content, Is.Null);
    }

    [Test]
    public async Task Can_Update_Blueprint_In_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, containerKey), Constants.Security.SuperUserKey);

        await ContentBlueprintEditingService.UpdateAsync(blueprintKey, SimpleContentBlueprintUpdateModel(), Constants.Security.SuperUserKey);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.That(blueprint, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blueprint.ParentId, Is.EqualTo(container.Id));
            Assert.That(blueprint.Path, Is.EqualTo($"{container.Path},{blueprint.Id}"));
        });

        var result = GetBlueprintChildren(containerKey);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo(blueprintKey));
        });

        blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.That(blueprint, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blueprint.Name, Is.EqualTo("Blueprint #1 updated"));
            Assert.That(blueprint.GetValue<string>("title"), Is.EqualTo("The title value updated"));
            Assert.That(blueprint.GetValue<string>("author"), Is.EqualTo("The author value updated"));
        });
    }
}
