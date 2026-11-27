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

    [Test]
    public async Task Can_Create_From_Segment_Variant_Content_With_All_Segment_Values()
    {
        var content = await CreateSegmentVariantContent();

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            null,
            Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        VerifyValues(result.Result.Content);
        VerifyValues(await ContentBlueprintEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyValues(IContent? blueprint)
        {
            Assert.IsNotNull(blueprint);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The initial invariant title", blueprint.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The initial default title", blueprint.GetValue<string>("variantTitle"));
                Assert.AreEqual("The initial seg-1 title", blueprint.GetValue<string>("variantTitle", segment: "seg-1"));
                Assert.AreEqual("The initial seg-2 title", blueprint.GetValue<string>("variantTitle", segment: "seg-2"));
            });
        }
    }

    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Segment)]
    public async Task Can_Create_From_Culture_And_Segment_Variant_Content_With_All_Variant_Values(ContentVariation otherTitleVariation)
    {
        var content = await CreateCultureAndSegmentVariantContent(otherTitleVariation);

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            null,
            Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        VerifyValues(result.Result.Content);
        VerifyValues(await ContentBlueprintEditingService.GetAsync(result.Result.Content!.Key));

        void VerifyValues(IContent? blueprint)
        {
            Assert.IsNotNull(blueprint);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The initial title in English", blueprint.GetValue<string>("variantTitle", "en-US"));
                Assert.AreEqual("The initial seg-1 title in English", blueprint.GetValue<string>("variantTitle", "en-US", "seg-1"));
                Assert.AreEqual("The initial seg-2 title in English", blueprint.GetValue<string>("variantTitle", "en-US", "seg-2"));
                Assert.AreEqual("The initial title in Danish", blueprint.GetValue<string>("variantTitle", "da-DK"));
                Assert.AreEqual("The initial seg-1 title in Danish", blueprint.GetValue<string>("variantTitle", "da-DK", "seg-1"));
                Assert.AreEqual("The initial seg-2 title in Danish", blueprint.GetValue<string>("variantTitle", "da-DK", "seg-2"));

                if (otherTitleVariation is ContentVariation.Culture)
                {
                    Assert.AreEqual("The initial other English title", blueprint.GetValue<string>("otherTitle", "en-US"));
                    Assert.AreEqual("The initial other Danish title", blueprint.GetValue<string>("otherTitle", "da-DK"));
                }
                else
                {
                    Assert.AreEqual("The initial other default title", blueprint.GetValue<string>("otherTitle"));
                    Assert.AreEqual("The initial other seg-1 title", blueprint.GetValue<string>("otherTitle", segment: "seg-1"));
                    Assert.AreEqual("The initial other seg-2 title", blueprint.GetValue<string>("otherTitle", segment: "seg-2"));
                }
            });
        }
    }

    [Test]
    public async Task Can_Scaffold_Content_With_All_Segment_Values_From_Blueprint_Created_From_Content()
    {
        var content = await CreateSegmentVariantContent();

        var result = await ContentBlueprintEditingService.CreateFromContentAsync(
            content.Key,
            "Test Create From Content Blueprint",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var scaffold = await ContentBlueprintEditingService.GetScaffoldedAsync(result.Result.Content!.Key);

        Assert.IsNotNull(scaffold);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("The initial default title", scaffold.GetValue<string>("variantTitle"));
            Assert.AreEqual("The initial seg-1 title", scaffold.GetValue<string>("variantTitle", segment: "seg-1"));
            Assert.AreEqual("The initial seg-2 title", scaffold.GetValue<string>("variantTitle", segment: "seg-2"));
        });
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
