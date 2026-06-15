using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Can_Create_With_Basic_Model()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
                Assert.That(createdBlueprint.Name, Is.EqualTo("Test Create Blueprint"));
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.That(contentFound, Is.Null);
    }

    [Test]
    public async Task Can_Create_At_Root()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
                Assert.That(createdBlueprint.Name, Is.EqualTo("Test Create Blueprint"));
                Assert.That(createdBlueprint.GetValue<string>("title"), Is.EqualTo("The title value"));
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.That(contentFound, Is.Null);
    }

    [Test]
    public async Task Can_Create_With_Explicit_Key()
    {
        var contentType = await CreateInvariantContentType();

        var key = Guid.NewGuid();
        var createModel = new ContentBlueprintCreateModel
        {
            Key = key,
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
            Assert.That(result.Result.Content.GetValue<string>("title"), Is.EqualTo("The title value"));
        });

        // re-get and verify creation
        var blueprint = await ContentBlueprintEditingService.GetAsync(key);
        Assert.That(blueprint, Is.Not.Null);
        Assert.That(blueprint.Id, Is.EqualTo(result.Result.Content.Id));
    }

    [Test]
    public async Task Cannot_Create_With_Duplicate_Name_For_The_Same_Content_Type()
    {
        var contentType = await CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var result1 = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result1.Success, Is.True);
            Assert.That(result1.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result1.Result, Is.Not.Null);
        });

        // create another blueprint with the same name
        var result2 = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result2.Success, Is.False);
            Assert.That(result2.Status, Is.EqualTo(ContentEditingOperationStatus.DuplicateName));
            Assert.That(result2.Result, Is.Not.Null);
        });
        Assert.That(result2.Result.Content, Is.Null);
    }

    [Test]
    public async Task Can_Create_With_Different_Name()
    {
        var contentType = await CreateInvariantContentType();

        var createModel1 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint 1" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var result1 = await ContentBlueprintEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result1.Success, Is.True);
            Assert.That(result1.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result1.Result, Is.Not.Null);
        });

        var createModel2 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint 2" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        // create another blueprint
        var result2 = await ContentBlueprintEditingService.CreateAsync(createModel2, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result2.Success, Is.True);
            Assert.That(result2.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result2.Result, Is.Not.Null);
        });
    }

    [Test]
    public async Task Can_Create_With_Duplicate_Name_For_Different_Content_Types()
    {
        var contentType1 = ContentTypeBuilder.CreateContentMetaContentType();
        contentType1.AllowedTemplates = null;
        contentType1.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType1, Constants.Security.SuperUserKey);

        var createModel1 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType1.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        var result1 = await ContentBlueprintEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result1.Success, Is.True);
            Assert.That(result1.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result1.Result, Is.Not.Null);
        });

        var contentType2 = await CreateInvariantContentType();

        var createModel2 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType2.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" }
            ],
        };

        // create another blueprint
        var result2 = await ContentBlueprintEditingService.CreateAsync(createModel2, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result2.Success, Is.True);
            Assert.That(result2.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result2.Result, Is.Not.Null);
        });
    }

    [Test]
    public async Task Cannot_Create_When_Content_Type_Not_Found()
    {
        var createModel1 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create Blueprint" }],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeNotFound));
            Assert.That(result.Result, Is.Not.Null);
        });
        Assert.That(result.Result.Content, Is.Null);
    }

    [Test]
    public async Task Can_Create_Blueprint_In_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, containerKey), Constants.Security.SuperUserKey);

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
    }

    [Test]
    public async Task Can_Create_Variant()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "English Blueprint", Culture = "en-US" },
                new VariantModel { Name = "Danish Blueprint", Culture = "da-DK" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The invariant title value" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The English title value", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The Danish title value", Culture = "da-DK" },
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
                Assert.That(createdBlueprint.GetCultureName("en-US"), Is.EqualTo("English Blueprint"));
                Assert.That(createdBlueprint.GetCultureName("da-DK"), Is.EqualTo("Danish Blueprint"));
                Assert.That(createdBlueprint.GetValue<string>("invariantTitle"), Is.EqualTo("The invariant title value"));
                Assert.That(createdBlueprint.GetValue<string>("variantTitle", culture: "en-US"), Is.EqualTo("The English title value"));
                Assert.That(createdBlueprint.GetValue<string>("variantTitle", culture: "da-DK"), Is.EqualTo("The Danish title value"));
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.That(contentFound, Is.Null);
    }

    [TestCase("English Blueprint", "Unique Danish Name")]
    [TestCase("Unique English Name", "Danish Blueprint")]
    [TestCase("English Blueprint", "Danish Blueprint")]
    public async Task Cannot_Create_With_Duplicate_Name_For_The_Same_Content_Type_Variant(string secondBlueprintNameInEnglish, string secondBlueprintNameInDanish)
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "English Blueprint", Culture = "en-US" },
                new VariantModel { Name = "Danish Blueprint", Culture = "da-DK" }
            ],
            Properties = []
        };

        Assert.That((await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Success, Is.True);

        createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = secondBlueprintNameInEnglish, Culture = "en-US" },
                new VariantModel { Name = secondBlueprintNameInDanish, Culture = "da-DK" }
            ],
            Properties = []
        };
        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.DuplicateName));
        });
    }
}
