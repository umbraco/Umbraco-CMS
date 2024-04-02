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
            InvariantName = "Updated Blueprint Name",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" },
            },
        };

        var result = await ContentBlueprintEditingService.UpdateAsync(blueprint.Key, updateModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentBlueprintEditingService.GetAsync(blueprint.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated Blueprint Name", updatedContent.Name);
            Assert.AreEqual("The updated title", updatedContent.GetValue<string>("title"));
            Assert.AreEqual("The updated text", updatedContent.GetValue<string>("text"));
        }
    }

    [Test]
    public async Task Can_Update_Variant()
    {
        var blueprint = await CreateVariantContentBlueprint();

        var updateModel = new ContentBlueprintUpdateModel
        {
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated blueprint invariant title" },
            },
            Variants = new[]
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated Blueprint English Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title" },
                    },
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Updated Blueprint Danish Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title" },
                    },
                },
            },
        };

        var result = await ContentBlueprintEditingService.UpdateAsync(blueprint.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentBlueprintEditingService.GetAsync(blueprint.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated Blueprint English Name", updatedContent.GetCultureName("en-US"));
            Assert.AreEqual("Updated Blueprint Danish Name", updatedContent.GetCultureName("da-DK"));
            Assert.AreEqual("The updated blueprint invariant title", updatedContent.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The updated English title", updatedContent.GetValue<string>("variantTitle", "en-US"));
            Assert.AreEqual("The updated Danish title", updatedContent.GetValue<string>("variantTitle", "da-DK"));
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
            InvariantName = "Test Blueprint",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        var createResult = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, createResult.Status);
        Assert.IsNotNull(createResult.Result);

        // update a blueprint with the same name
        var updateModel = new ContentBlueprintUpdateModel
        {
            InvariantName = "Test Blueprint",
        };

        var updateResult = await ContentBlueprintEditingService.UpdateAsync(blueprintToUpdate.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(updateResult.Success);
        Assert.AreEqual(ContentEditingOperationStatus.DuplicateName, updateResult.Status);
        Assert.IsNotNull(updateResult.Result);
        Assert.IsNull(updateResult.Result.Content);
    }
}
