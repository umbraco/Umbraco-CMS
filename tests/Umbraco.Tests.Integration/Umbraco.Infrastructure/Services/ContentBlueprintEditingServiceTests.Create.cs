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
        var contentType = CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            InvariantName = "Test Create Blueprint",
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
                Assert.AreEqual("Test Create Blueprint", createdBlueprint.Name);
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.IsNull(contentFound);
    }

    [Test]
    public async Task Can_Create_At_Root()
    {
        var contentType = CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
                Assert.AreEqual("Test Create Blueprint", createdBlueprint.Name);
                Assert.AreEqual("The title value", createdBlueprint.GetValue<string>("title"));
            });
        }

        // ensures it's not found by normal content
        var contentFound = await ContentEditingService.GetAsync(result.Result.Content!.Key);
        Assert.IsNull(contentFound);
    }

    [Test]
    public async Task Can_Create_With_Explicit_Key()
    {
        var contentType = CreateInvariantContentType();

        var key = Guid.NewGuid();
        var createModel = new ContentBlueprintCreateModel
        {
            Key = key,
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

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
            Assert.AreEqual("The title value", result.Result.Content.GetValue<string>("title"));
        });

        // re-get and verify creation
        var blueprint = await ContentBlueprintEditingService.GetAsync(key);
        Assert.IsNotNull(blueprint);
        Assert.AreEqual(result.Result.Content.Id, blueprint.Id);
    }

    [Test]
    public async Task Cannot_Create_With_Duplicate_Name_For_The_Same_Content_Type()
    {
        var contentType = CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        var result1 = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result1.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result1.Status);
            Assert.IsNotNull(result1.Result);
        });

        // create another blueprint with the same name
        var result2 = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result2.Success);
            Assert.AreEqual(ContentEditingOperationStatus.DuplicateName, result2.Status);
            Assert.IsNotNull(result2.Result);
        });
        Assert.IsNull(result2.Result.Content);
    }

    [Test]
    public async Task Can_Create_With_Different_Name()
    {
        var contentType = CreateInvariantContentType();

        var createModel1 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint 1",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        var result1 = await ContentBlueprintEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result1.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result1.Status);
            Assert.IsNotNull(result1.Result);
        });

        var createModel2 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint 2",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        // create another blueprint
        var result2 = await ContentBlueprintEditingService.CreateAsync(createModel2, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result2.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result2.Status);
            Assert.IsNotNull(result2.Result);
        });
    }

    [Test]
    public async Task Can_Create_With_Duplicate_Name_For_Different_Content_Types()
    {
        var contentType1 = ContentTypeBuilder.CreateContentMetaContentType();
        contentType1.AllowedTemplates = null;
        contentType1.AllowedAsRoot = true;
        ContentTypeService.Save(contentType1);

        var createModel1 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType1.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        var result1 = await ContentBlueprintEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result1.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result1.Status);
            Assert.IsNotNull(result1.Result);
        });

        var contentType2 = CreateInvariantContentType();

        var createModel2 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType2.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
            },
        };

        // create another blueprint
        var result2 = await ContentBlueprintEditingService.CreateAsync(createModel2, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result2.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result2.Status);
            Assert.IsNotNull(result2.Result);
        });
    }

    [Test]
    public async Task Cannot_Create_When_Content_Type_Not_Found()
    {
        var createModel1 = new ContentBlueprintCreateModel
        {
            ContentTypeKey = Guid.NewGuid(),
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Create Blueprint",
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel1, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.ContentTypeNotFound, result.Status);
            Assert.IsNotNull(result.Result);
        });
        Assert.IsNull(result.Result.Content);
    }

    [Test]
    public async Task Can_Create_Blueprint_In_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, containerKey), Constants.Security.SuperUserKey);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(blueprint);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container.Id, blueprint.ParentId);
            Assert.AreEqual($"{container.Path},{blueprint.Id}", blueprint.Path);
        });

        var result = GetBlueprintChildren(containerKey);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(blueprintKey, result.First().Key);
        });
    }
}
