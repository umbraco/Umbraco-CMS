using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_UpdateAndPublish_Invariant()
    {
        var content = await CreateInvariantContent();
        Assert.IsFalse(content.Published);

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.IsTrue(updatedContent.Published);
            Assert.AreEqual("Updated Name", updatedContent.Name);
            Assert.AreEqual("The updated title", updatedContent.GetValue<string>("title", published: true));
            Assert.AreEqual("The updated text", updatedContent.GetValue<string>("text", published: true));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Culture_Variant()
    {
        var content = await CreateCultureVariantContent();
        Assert.IsFalse(content.Published);

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "Updated invariant" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated Danish", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "en-US", "da-DK" }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.IsTrue(updatedContent.Published);
            Assert.IsTrue(updatedContent.IsCulturePublished("en-US"));
            Assert.IsTrue(updatedContent.IsCulturePublished("da-DK"));
            Assert.AreEqual("Updated English Name", updatedContent.GetPublishName("en-US"));
            Assert.AreEqual("Updated Danish Name", updatedContent.GetPublishName("da-DK"));
            Assert.AreEqual("Updated invariant", updatedContent.GetValue<string>("invariantTitle", published: true));
            Assert.AreEqual("Updated English", updatedContent.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreEqual("Updated Danish", updatedContent.GetValue<string>("variantTitle", "da-DK", published: true));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Single_Culture()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "Updated invariant" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated Danish", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyUpdateAndPublish(result.Result.Content);

        // re-get and re-test
        VerifyUpdateAndPublish(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdateAndPublish(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.IsTrue(updatedContent.IsCulturePublished("en-US"));
            Assert.IsFalse(updatedContent.IsCulturePublished("da-DK"));

            // both values should still be saved
            Assert.AreEqual("Updated English", updatedContent.GetValue<string>("variantTitle", "en-US", published: true));
            Assert.AreNotEqual("Updated Danish", updatedContent.GetValue<string>("variantTitle", "da-DK", published: true));
            Assert.AreEqual("Updated Danish", updatedContent.GetValue<string>("variantTitle", "da-DK", published: false));
        }
    }

    [Test]
    public async Task Can_UpdateAndPublish_Already_Published_Content()
    {
        var contentType = CreateInvariantContentType();

        // create and publish initially
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Original Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Original title" },
                new PropertyValueModel { Alias = "text", Value = "Original text" }
            ],
        };

        var createResult = await ContentEditingService.CreateAndPublishAsync(createModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        var content = createResult.Result.Content!;
        Assert.IsTrue(content.Published);

        // now update and republish
        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Republished Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Republished title" },
                new PropertyValueModel { Alias = "text", Value = "Republished text" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updatedContent);
        Assert.IsTrue(updatedContent.Published);
        Assert.AreEqual("Republished Name", updatedContent.Name);
        Assert.AreEqual("Republished title", updatedContent.GetValue<string>("title", published: true));
        Assert.AreEqual("Republished text", updatedContent.GetValue<string>("text", published: true));
    }

    [Test]
    public async Task Can_UpdateAndPublish_Template()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var template2 = TemplateBuilder.CreateTextPageTemplate("altTemplate");
        await TemplateService.CreateAsync(template2, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedTemplates = new[] { template, template2 };
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Name = "Template Test" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Title" },
                new PropertyValueModel { Alias = "bodyText", Value = "Body" }
            ],
        };

        var createResult = await ContentEditingService.CreateAndPublishAsync(createModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        var content = createResult.Result.Content!;
        Assert.AreEqual(template.Id, content.TemplateId);

        // update with different template
        var updateModel = new ContentUpdateModel
        {
            TemplateKey = template2.Key,
            Variants =
            [
                new VariantModel { Name = "Template Test Updated" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Updated Title" },
                new PropertyValueModel { Alias = "bodyText", Value = "Updated Body" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updatedContent);
        Assert.IsTrue(updatedContent.Published);
        Assert.AreEqual(template2.Id, updatedContent.TemplateId);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Non_Existing_Content()
    {
        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Test" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(Guid.NewGuid(), updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status.ContentEditingOperationStatus);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_With_Invalid_Culture()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "Invariant" },
                new PropertyValueModel { Alias = "variantTitle", Value = "English", Culture = "en-us" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-us", Name = "English" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "en-us" }, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status.ContentEditingOperationStatus);
    }

    [Test]
    public async Task Can_UpdateAndPublish_Readonly_Property_Is_Preserved()
    {
        var content = await CreateInvariantContent();
        var labelValue = content.GetValue<string>("label");

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Updated title" },
                new PropertyValueModel { Alias = "text", Value = "Updated text" },
                new PropertyValueModel { Alias = "label", Value = "Trying to change label" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Result.Content!.Published);

        // re-get and verify the label property was not changed
        var updatedContent = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updatedContent);
        Assert.AreEqual(labelValue, updatedContent.GetValue<string>("label"));
        Assert.AreEqual("Updated title", updatedContent.GetValue<string>("title"));
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Under_Unpublished_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        var (root, child) = await CreateRootAndChildAsync(contentType);
        Assert.IsFalse(root.Published);

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "The Updated Child" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(child.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.PathNotPublished, result.Status.ContentPublishingOperationStatus);
        });

        // the save half of the operation took effect, so the new name is persisted while the document stays unpublished
        var updated = await ContentEditingService.GetAsync(child.Key);
        Assert.IsNotNull(updated);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("The Updated Child", updated.Name);
            Assert.IsFalse(updated.Published);
        });
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Without_Mandatory_Culture()
    {
        var contentType = await CreateVariantContentType();

        // make the default language mandatory, so publishing only the other culture is rejected
        var defaultLanguage = await LanguageService.GetDefaultLanguageAsync();
        defaultLanguage!.IsMandatory = true;
        await LanguageService.UpdateAsync(defaultLanguage, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "English" },
                new VariantModel { Culture = "da-DK", Name = "Danish" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "variantTitle", Value = "English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Danish title", Culture = "da-DK" }
            ],
        };
        var content = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result.Content!;

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "English" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "variantTitle", Value = "English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "Updated Danish title", Culture = "da-DK" }
            ],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "da-DK" }, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.MandatoryCultureMissing, result.Status.ContentPublishingOperationStatus);
        });

        var updated = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Danish title", updated.GetValue<string>("variantTitle", "da-DK"));
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Expired_Content()
    {
        var content = await CreateInvariantContent();

        var schedule = new ContentScheduleCollection();
        schedule.Add(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
        ContentService.PersistContentSchedule(content, schedule);

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Name" }],
            Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.HasExpired, result.Status.ContentPublishingOperationStatus);
        });

        var updated = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Name", updated.Name);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Content_Awaiting_Release()
    {
        var content = await CreateInvariantContent();

        var schedule = new ContentScheduleCollection();
        schedule.Add(DateTime.UtcNow.AddDays(1), null);
        ContentService.PersistContentSchedule(content, schedule);

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Name" }],
            Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.AwaitingRelease, result.Status.ContentPublishingOperationStatus);
        });

        var updated = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updated);
        Assert.AreEqual("Updated Name", updated.Name);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Content_In_Recycle_Bin()
    {
        var content = await CreateInvariantContent();
        await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Name" }],
            Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.InTrash, result.Status.ContentPublishingOperationStatus);
        });
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_When_Nothing_Is_Left_To_Publish()
    {
        var content = await CreateCultureVariantContent();

        var publishModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Culture = "en-US", Name = "English" }],
            Properties = [new PropertyValueModel { Alias = "variantTitle", Value = "English title", Culture = "en-US" }],
        };
        var published = await ContentEditingService.UpdateAndPublishAsync(content.Key, publishModel, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);
        Assert.IsTrue(published.Success);

        // publishing no cultures at all on an already published document leaves nothing to do
        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Culture = "en-US", Name = "English" }],
            Properties = [new PropertyValueModel { Alias = "variantTitle", Value = "English title", Culture = "en-US" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status.ContentEditingOperationStatus);
            Assert.AreEqual(ContentPublishingOperationStatus.NothingToPublish, result.Status.ContentPublishingOperationStatus);
        });
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_When_Saving_Notification_Is_Cancelled()
    {
        var content = await CreateInvariantContent();
        var originalName = content.Name;
        ContentEditingNotificationHandler.SavingContent = notification => notification.Cancel = true;

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Name" }],
            Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.CancelledByNotification, result.Status.ContentEditingOperationStatus);
            Assert.IsNull(result.Status.ContentPublishingOperationStatus);
        });

        // nothing was persisted, so the outcome belongs to the save rather than the publish
        var updated = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updated);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(originalName, updated.Name);
            Assert.IsFalse(updated.Published);
        });
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_When_Publishing_Notification_Is_Cancelled()
    {
        var content = await CreateInvariantContent();
        var originalName = content.Name;
        ContentEditingNotificationHandler.PublishingContent = notification => notification.Cancel = true;

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Name" }],
            Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string>(), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            // the publishing notification is raised before the document is persisted, so a cancel here discards the
            // save as well - which is why both cancel points report against the editing status
            Assert.AreEqual(ContentEditingOperationStatus.CancelledByNotification, result.Status.ContentEditingOperationStatus);
            Assert.IsNull(result.Status.ContentPublishingOperationStatus);
        });

        var updated = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updated);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(originalName, updated.Name);
            Assert.IsFalse(updated.Published);
        });
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_Under_Unpublished_Parent_With_Obsolete_Overload()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        var (_, child) = await CreateRootAndChildAsync(contentType);

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "The Updated Child" }],
        };

#pragma warning disable CS0618 // Type or member is obsolete
        var result = await ContentEditingService.UpdateAndPublishAsync(child.Key, updateModel, [], Constants.Security.SuperUserKey);
#pragma warning restore CS0618 // Type or member is obsolete

        // the obsolete overload cannot express a publish failure, so it keeps collapsing to "unknown"
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Unknown, result.Status);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_With_Cultures_For_An_Invariant_Content_Type()
    {
        var content = await CreateInvariantContent();
        var originalName = content.Name;

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Name = "Updated Name" }],
            Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "en-US" }, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch, result.Status.ContentEditingOperationStatus);
            Assert.IsNull(result.Status.ContentPublishingOperationStatus);
        });

        // the request is rejected before anything is attempted, so the save must not have taken effect either
        var updated = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(updated);
        Assert.AreEqual(originalName, updated.Name);
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_With_A_Wildcard_Culture()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Culture = "en-US", Name = "English" }],
            Properties = [new PropertyValueModel { Alias = "variantTitle", Value = "English title", Culture = "en-US" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "*" }, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status.ContentEditingOperationStatus);
            Assert.IsNull(result.Status.ContentPublishingOperationStatus);
        });
    }

    [Test]
    public async Task Cannot_UpdateAndPublish_With_An_Unconfigured_Culture()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Variants = [new VariantModel { Culture = "en-US", Name = "English" }],
            Properties = [new PropertyValueModel { Alias = "variantTitle", Value = "English title", Culture = "en-US" }],
        };

        var result = await ContentEditingService.UpdateAndPublishAsync(content.Key, updateModel, new HashSet<string> { "zz-ZZ" }, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status.ContentEditingOperationStatus);
            Assert.IsNull(result.Status.ContentPublishingOperationStatus);
        });
    }
}
