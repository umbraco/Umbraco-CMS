// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ContentServiceNotificationTests : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;
        _globalSettings = new GlobalSettings();

        await CreateTestData();
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private GlobalSettings _globalSettings;
    private IContentType _contentType;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentSavedNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentPublishingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentPublishedNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishedNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentTreeChangeNotification, ContentNotificationHandler>();

    private async Task CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!

        _contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);
    }

    [Test]
    public async Task Can_Save_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        // properties: title, bodyText, keywords, description
        document.SetValue("title", "title-en", "en-US");

        var savingWasCalled = false;
        var savedWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(saved, Is.SameAs(document));

            Assert.That(notification.IsSavingCulture(saved, "en-US"), Is.True);
            Assert.That(notification.IsSavingCulture(saved, "fr-FR"), Is.False);

            savingWasCalled = true;
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(saved, Is.SameAs(document));

            Assert.That(notification.HasSavedCulture(saved, "en-US"), Is.True);
            Assert.That(notification.HasSavedCulture(saved, "fr-FR"), Is.False);

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savingWasCalled, Is.True);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Can_Set_Value_When_Saving()
    {
        IContent document = new Content("content", -1, _contentType);

        var savingWasCalled = false;
        var savedWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(document.GetValue<string>("title").IsNullOrWhiteSpace(), Is.True);

            saved.SetValue("title", "title");

            savingWasCalled = true;
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(document.GetValue<string>("title"), Is.SameAs("title"));

            // we're only dealing with invariant here
            var propValue = saved.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

            Assert.That(propValue.EditedValue, Is.EqualTo("title"));
            Assert.That(propValue.PublishedValue, Is.Null);

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savingWasCalled, Is.True);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Can_Publish_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);

        var treeChangeWasCalled = false;

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            var publishedCultures = change?.PublishedCultures?.ToArray();
            Assert.That(publishedCultures, Is.Not.Null);
            Assert.That(publishedCultures, Has.Length.EqualTo(1));
            Assert.That(publishedCultures.InvariantContains("*"), Is.True);
            Assert.That(change.UnpublishedCultures, Is.Null);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.Publish(document, ["*"]);
            Assert.That(treeChangeWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.TreeChange = null;
        }
    }

    [Test]
    public void Can_Unpublish_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);
        ContentService.Publish(document, ["*"]);

        var treeChangeWasCalled = false;

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            Assert.That(change?.PublishedCultures, Is.Null);
            var unpublishedCultures = change?.UnpublishedCultures?.ToArray();
            Assert.That(unpublishedCultures, Is.Not.Null);
            Assert.That(unpublishedCultures, Has.Length.EqualTo(1));
            Assert.That(unpublishedCultures.InvariantContains("*"), Is.True);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.Unpublish(document);
            Assert.That(treeChangeWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.TreeChange = null;
        }
    }

    [Test]
    public async Task Can_Publish_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);

        Assert.That(document.IsCulturePublished("fr-FR"), Is.False);
        Assert.That(document.IsCulturePublished("en-US"), Is.False);

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        var publishingWasCalled = false;
        var publishedWasCalled = false;
        var treeChangeWasCalled = false;

        ContentNotificationHandler.PublishingContent += notification =>
        {
            var publishing = notification.PublishedEntities.First();

            Assert.That(publishing, Is.SameAs(document));

            Assert.That(notification.IsPublishingCulture(publishing, "en-US"), Is.False);
            Assert.That(notification.IsPublishingCulture(publishing, "fr-FR"), Is.True);

            publishingWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent += notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.That(published, Is.SameAs(document));

            Assert.That(notification.HasPublishedCulture(published, "en-US"), Is.False);
            Assert.That(notification.HasPublishedCulture(published, "fr-FR"), Is.True);

            publishedWasCalled = true;
        };

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            var publishedCultures = change?.PublishedCultures?.ToArray();
            Assert.That(publishedCultures, Is.Not.Null);
            Assert.That(publishedCultures, Has.Length.EqualTo(1));
            Assert.That(publishedCultures.InvariantContains("fr-FR"), Is.True);
            Assert.That(change.UnpublishedCultures, Is.Null);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.Publish(document, new[] { "fr-FR" });
            Assert.That(publishingWasCalled, Is.True);
            Assert.That(publishedWasCalled, Is.True);
            Assert.That(treeChangeWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
            ContentNotificationHandler.TreeChange = null;
        }

        document = ContentService.GetById(document.Id);

        // ensure it works and does not throw
        Assert.That(document.IsCulturePublished("fr-FR"), Is.True);
        Assert.That(document.IsCulturePublished("en-US"), Is.False);
    }

    [Test]
    public void Can_Set_Value_When_Publishing()
    {
        IContent document = new Content("content", -1, _contentType);

        var savingWasCalled = false;
        var savedWasCalled = false;
        var publishingWasCalled = false;
        var publishedWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(document.GetValue<string>("title").IsNullOrWhiteSpace(), Is.True);

            saved.SetValue("title", "title");

            savingWasCalled = true;
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(document.GetValue<string>("title"), Is.SameAs("title"));

            // We're only dealing with invariant here.
            var propValue = saved.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

            Assert.That(propValue.EditedValue, Is.EqualTo("title"));
            Assert.That(propValue.PublishedValue, Is.EqualTo(null));

            savedWasCalled = true;
        };

        ContentNotificationHandler.PublishingContent = notification =>
        {
            var publishing = notification.PublishedEntities.First();

            Assert.That(publishing.GetValue<string>("title"), Is.EqualTo("title"));

            publishingWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent = notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.That(document.GetValue<string>("title"), Is.SameAs("title"));

            // We're only dealing with invariant here.
            var propValue = published.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

            Assert.That(propValue.EditedValue, Is.EqualTo("title"));
            Assert.That(propValue.PublishedValue, Is.EqualTo("title"));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            ContentService.Publish(document, document.AvailableCultures.ToArray());
            Assert.That(savingWasCalled, Is.True);
            Assert.That(savedWasCalled, Is.True);
            Assert.That(publishingWasCalled, Is.True);
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public void Can_Read_Saved_Notification_When_Save_And_Publishing_Invariant()
    {
        // A combined save-and-publish must still raise the paired Saved notification, just like a plain Save does
        // (https://github.com/umbraco/Umbraco-CMS/issues/23523).
        Content document = new Content("content", -1, _contentType);

        var savingWasCalled = false;
        var savedWasCalled = false;
        var publishingWasCalled = false;
        var publishedWasCalled = false;

        ContentNotificationHandler.SavingContent = _ => savingWasCalled = true;
        ContentNotificationHandler.SavedContent = notification =>
        {
            IContent saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "*" }));

            savedWasCalled = true;
        };
        ContentNotificationHandler.PublishingContent = _ => publishingWasCalled = true;
        ContentNotificationHandler.PublishedContent = _ => publishedWasCalled = true;

        try
        {
            var result = ContentService.SaveAndPublish(document, []);
            Assert.That(result.Success, Is.True);
            Assert.That(savingWasCalled, Is.True);
            Assert.That(savedWasCalled, Is.True, "ContentSavedNotification should fire when saving and publishing.");
            Assert.That(publishingWasCalled, Is.True);
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_Saved_Notification_When_Save_And_Publishing_Variant()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        Content document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            IContent saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);

            // both cultures were changed as part of the save-and-publish, so both are reported as saved
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        try
        {
            var result = ContentService.SaveAndPublish(document, ["en-US", "fr-FR"]);
            Assert.That(result.Success, Is.True);
            Assert.That(savedWasCalled, Is.True, "ContentSavedNotification should fire when saving and publishing.");
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_All_Changed_Cultures_As_Saved_When_Publishing_A_Subset()
    {
        // The saved cultures must reflect what was *changed*, not what was *published*: both cultures are edited here
        // but only one is published, so the Saved notification reports both while the Published notification reports one.
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        Content document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");

        var savedWasCalled = false;
        var publishedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            IContent saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);

            // both cultures were changed, so both are reported as saved - even though only en-US is being published
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent = notification =>
        {
            IContent published = notification.PublishedEntities.First();

            Assert.That(notification.PublishedCultures, Is.Not.Null);
            Assert.That(notification.PublishedCultures.ContainsKey(published.Key), Is.True);

            // only en-US was published
            Assert.That(notification.PublishedCultures[published.Key], Is.EquivalentTo(new[] { "en-US" }));

            publishedWasCalled = true;
        };

        try
        {
            var result = ContentService.SaveAndPublish(document, ["en-US"]);
            Assert.That(result.Success, Is.True);
            Assert.That(savedWasCalled, Is.True, "ContentSavedNotification should fire when saving and publishing.");
            Assert.That(publishedWasCalled, Is.True, "ContentPublishedNotification should fire when saving and publishing.");
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public async Task Can_Set_Mandatory_Value_When_Publishing()
    {
        var titleProperty = _contentType.PropertyTypes.First(x => x.Alias == "title");
        titleProperty.Mandatory = true; // make this required!
        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);

        ContentService.Save(document);
        var result = ContentService.Publish(document, document.AvailableCultures.ToArray());
        Assert.That(result.Success, Is.False);
        Assert.That(result.InvalidProperties.First().Alias, Is.EqualTo("title"));

        // when a service operation fails, the object is dirty and should not be re-used,
        // re-create it
        document = new Content("content", -1, _contentType);

        var savingWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.That(document.GetValue<string>("title").IsNullOrWhiteSpace(), Is.True);

            saved.SetValue("title", "title");

            savingWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            result = ContentService.Publish(document, document.AvailableCultures.ToArray());
            Assert.That(result
                .Success, Is.True); // will succeed now because we were able to specify the required value in the Saving event
            Assert.That(savingWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Unpublish_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);
        ContentService.Publish(document, document.AvailableCultures.ToArray());

        Assert.That(document.IsCulturePublished("fr-FR"), Is.True);
        Assert.That(document.IsCulturePublished("en-US"), Is.True);

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        document.UnpublishCulture("fr-FR");

        var publishingWasCalled = false;
        var publishedWasCalled = false;
        var treeChangeWasCalled = false;

        // TODO: revisit this - it was migrated when removing static events, but the expected result seems illogic - why does this test bind to Published and not Unpublished?

        ContentNotificationHandler.PublishingContent += notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.That(published, Is.SameAs(document));

            Assert.That(notification.IsPublishingCulture(published, "en-US"), Is.False);
            Assert.That(notification.IsPublishingCulture(published, "fr-FR"), Is.False);

            Assert.That(notification.IsUnpublishingCulture(published, "en-US"), Is.False);
            Assert.That(notification.IsUnpublishingCulture(published, "fr-FR"), Is.True);

            publishingWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent += notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.That(published, Is.SameAs(document));

            Assert.That(notification.HasPublishedCulture(published, "en-US"), Is.False);
            Assert.That(notification.HasPublishedCulture(published, "fr-FR"), Is.False);

            Assert.That(notification.HasUnpublishedCulture(published, "en-US"), Is.False);
            Assert.That(notification.HasUnpublishedCulture(published, "fr-FR"), Is.True);

            publishedWasCalled = true;
        };

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            var unpublishedCultures = change?.UnpublishedCultures?.ToArray();
            Assert.That(unpublishedCultures, Is.Not.Null);
            Assert.That(unpublishedCultures, Has.Length.EqualTo(1));
            Assert.That(unpublishedCultures.InvariantContains("fr-FR"), Is.True);
            Assert.That(change.PublishedCultures, Is.Null);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.CommitDocumentChanges(document);
            Assert.That(publishingWasCalled, Is.True);
            Assert.That(publishedWasCalled, Is.True);
            Assert.That(treeChangeWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
            ContentNotificationHandler.TreeChange = null;
        }

        document = ContentService.GetById(document.Id);

        Assert.That(document.IsCulturePublished("fr-FR"), Is.False);
        Assert.That(document.IsCulturePublished("en-US"), Is.True);
    }

    [Test]
    public void Can_Read_Saved_Cultures_For_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            IContent saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "*" }));

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_Only_Changed_Saved_Cultures()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        // only change the en-US culture
        document.SetValue("title", "title-en", "en-US");

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            IContent saved = notification.SavedEntities.First();

            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(saved.Key), Is.True);

            // captured at raise-time even though the entity's change tracking has been reset by persistence
            Assert.That(notification.SavedCultures[saved.Key], Is.EquivalentTo(new[] { "en-US" }));

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Can_Read_Published_Cultures_For_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);

        var publishedWasCalled = false;

        ContentNotificationHandler.PublishedContent = notification =>
        {
            IContent published = notification.PublishedEntities.First();

            Assert.That(notification.PublishedCultures, Is.Not.Null);
            Assert.That(notification.PublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.PublishedCultures[published.Key], Is.EquivalentTo(new[] { "*" }));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.Publish(document, ["*"]);
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_Only_Published_Cultures()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        var publishedWasCalled = false;

        ContentNotificationHandler.PublishedContent = notification =>
        {
            IContent published = notification.PublishedEntities.First();

            Assert.That(notification.PublishedCultures, Is.Not.Null);
            Assert.That(notification.PublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.PublishedCultures[published.Key], Is.EquivalentTo(new[] { "fr-FR" }));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.Publish(document, new[] { "fr-FR" });
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public void Can_Read_Unpublished_Cultures_For_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);
        ContentService.Publish(document, ["*"]);

        var unpublishedWasCalled = false;

        ContentNotificationHandler.UnpublishedContent = notification =>
        {
            IContent unpublished = notification.UnpublishedEntities.First();

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "*" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ContentService.Unpublish(document);
            Assert.That(unpublishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.UnpublishedContent = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Unpublished_Cultures_When_Unpublishing_A_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);
        ContentService.Publish(document, document.AvailableCultures.ToArray());

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);
        document.UnpublishCulture("fr-FR");

        var publishedWasCalled = false;

        ContentNotificationHandler.PublishedContent = notification =>
        {
            IContent published = notification.PublishedEntities.First();

            // unpublishing a single culture is performed as a publish operation
            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(published.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[published.Key], Is.EquivalentTo(new[] { "fr-FR" }));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.CommitDocumentChanges(document);
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_Per_Document_Saved_Cultures_For_Bulk_Save()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent documentOne = new Content("one", -1, _contentType);
        documentOne.SetCultureName("one-en", "en-US");

        IContent documentTwo = new Content("two", -1, _contentType);
        documentTwo.SetCultureName("two-en", "en-US");
        documentTwo.SetCultureName("two-fr", "fr-FR");

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            Assert.That(notification.SavedCultures, Is.Not.Null);

            // the culture map is keyed per document, so each document reports only its own changed cultures
            Assert.That(notification.SavedCultures.ContainsKey(documentOne.Key), Is.True);
            Assert.That(notification.SavedCultures.ContainsKey(documentTwo.Key), Is.True);
            Assert.That(notification.SavedCultures[documentOne.Key], Is.EquivalentTo(new[] { "en-US" }));
            Assert.That(notification.SavedCultures[documentTwo.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(new[] { documentOne, documentTwo });
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_Empty_Not_Null_Saved_Cultures_For_No_Op_Variant_Re_Save()
    {
        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        ContentService.Save(document);

        // re-get so nothing is dirty, then re-save without changes
        document = ContentService.GetById(document.Id);

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            // the save tracked cultures and found none changed, so the map is present but empty - not null
            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures, Is.Empty);

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_All_Unpublished_Cultures_When_Unpublishing_Whole_Variant_Document()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);
        ContentService.Publish(document, document.AvailableCultures.ToArray());

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        var unpublishedWasCalled = false;

        ContentNotificationHandler.UnpublishedContent = notification =>
        {
            IContent unpublished = notification.UnpublishedEntities.First();

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);

            // unpublishing the whole document reports every culture that was published, not an empty/partial set
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ContentService.Unpublish(document, "*");
            Assert.That(unpublishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.UnpublishedContent = null;
        }
    }

    [Test]
    public async Task Can_Read_Separate_Saved_Cultures_Per_Document_For_Mixed_Variance_Bulk_Save()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        // _contentType stays invariant; add a second, culture-variant content type so the bulk save is heterogeneous
        IContentType variantContentType = ContentTypeBuilder.CreateBasicContentType("variantPage", "Variant Page");
        variantContentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        IContent invariantDocument = new Content("invariant", -1, _contentType);

        IContent variantDocument = new Content("variant", -1, variantContentType);
        variantDocument.SetCultureName("hello", "en-US");
        variantDocument.SetCultureName("bonjour", "fr-FR");

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            Assert.That(notification.SavedCultures, Is.Not.Null);

            // the per-document map keeps each document's cultures separate - the invariant "*" marker is not
            // conflated with the variant document's specific cultures (which a flat list could not represent)
            Assert.That(notification.SavedCultures.ContainsKey(invariantDocument.Key), Is.True);
            Assert.That(notification.SavedCultures.ContainsKey(variantDocument.Key), Is.True);
            Assert.That(notification.SavedCultures[invariantDocument.Key], Is.EquivalentTo(new[] { "*" }));
            Assert.That(notification.SavedCultures[variantDocument.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(new[] { invariantDocument, variantDocument });
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Can_Read_Empty_Saved_Cultures_For_No_Op_Invariant_Re_Save()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);

        // re-get so nothing is dirty, then re-save without changes
        document = ContentService.GetById(document.Id);

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            // invariant content reports the "*" marker only when it changed; a no-op re-save reports nothing
            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures, Is.Empty);

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Can_Read_Star_Marker_Saved_Cultures_For_Changed_Invariant_Save()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);

        // re-get so nothing is dirty, then make a genuine change before re-saving
        document = ContentService.GetById(document.Id);
        document.SetValue("title", "changed");

        var savedWasCalled = false;

        ContentNotificationHandler.SavedContent = notification =>
        {
            Assert.That(notification.SavedCultures, Is.Not.Null);
            Assert.That(notification.SavedCultures.ContainsKey(document.Key), Is.True);
            Assert.That(notification.SavedCultures[document.Key], Is.EquivalentTo(new[] { "*" }));

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.That(savedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    [LongRunning]
    public void Can_Read_Per_Document_Published_Cultures_For_Branch_Publish()
    {
        IContent root = new Content("root", -1, _contentType);
        ContentService.Save(root);
        ContentService.Publish(root, ["*"]);

        IContent child = new Content("child", root.Id, _contentType);
        ContentService.Save(child);
        ContentService.Publish(child, ["*"]);

        // re-get - dirty properties need resetting
        root = ContentService.GetById(root.Id);

        var publishedWasCalled = false;

        ContentNotificationHandler.PublishedContent = notification =>
        {
            // the branch publish raises a single notification covering every published document
            Assert.That(notification.IncludeDescendants, Is.True);
            Assert.That(notification.PublishedCultures, Is.Not.Null);

            // invariant content, so each published document reports the "*" marker under its own key
            Assert.That(notification.PublishedCultures, Has.Count.EqualTo(2));
            Assert.That(notification.PublishedCultures.ContainsKey(root.Key), Is.True, "missing entry for root");
            Assert.That(notification.PublishedCultures.ContainsKey(child.Key), Is.True, "missing entry for child");
            Assert.That(notification.PublishedCultures[root.Key], Is.EquivalentTo(new[] { "*" }));
            Assert.That(notification.PublishedCultures[child.Key], Is.EquivalentTo(new[] { "*" }));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.PublishBranch(root, PublishBranchFilter.ForceRepublish, ["*"]);
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Per_Document_Published_Cultures_For_Branch_Publish_With_Mixed_Variance()
    {
        // invariant root
        IContent root = new Content("root", -1, _contentType);
        ContentService.Save(root);
        ContentService.Publish(root, ["*"]);

        // variant descendant of the invariant root - a branch can legitimately mix content type variance
        IContentType variantContentType = ContentTypeBuilder.CreateBasicContentType("variantPage", "Variant Page");
        variantContentType.Variations = ContentVariation.Culture;
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        IContent child = new Content("child", root.Id, variantContentType);
        child.SetCultureName("child-en", "en-US");
        ContentService.Save(child);
        ContentService.Publish(child, ["en-US"]);

        // re-get - dirty properties need resetting
        root = ContentService.GetById(root.Id);

        var publishedWasCalled = false;

        ContentNotificationHandler.PublishedContent = notification =>
        {
            Assert.That(notification.PublishedCultures, Is.Not.Null);

            // variance is per document: the invariant root reports "*", the variant descendant reports its culture
            Assert.That(notification.PublishedCultures, Has.Count.EqualTo(2));
            Assert.That(notification.PublishedCultures.ContainsKey(root.Key), Is.True);
            Assert.That(notification.PublishedCultures.ContainsKey(child.Key), Is.True);
            Assert.That(notification.PublishedCultures[root.Key], Is.EquivalentTo(new[] { "*" }));
            Assert.That(notification.PublishedCultures[child.Key], Is.EquivalentTo(new[] { "en-US" }));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.PublishBranch(root, PublishBranchFilter.ForceRepublish, ["*"]);
            Assert.That(publishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.PublishedContent = null;
        }
    }

    [Test]
    public void Can_Read_Unpublished_Cultures_When_Deleting_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);
        ContentService.Publish(document, ["*"]);

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        var unpublishedWasCalled = false;

        ContentNotificationHandler.UnpublishedContent = notification =>
        {
            IContent unpublished = notification.UnpublishedEntities.First();

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "*" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ContentService.Delete(document);
            Assert.That(unpublishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.UnpublishedContent = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Unpublished_Cultures_When_Deleting_Variant()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);
        ContentService.Publish(document, document.AvailableCultures.ToArray());

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        var unpublishedWasCalled = false;

        ContentNotificationHandler.UnpublishedContent = notification =>
        {
            IContent unpublished = notification.UnpublishedEntities.First();

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);

            // deleting a published document reports every culture that was published
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ContentService.Delete(document);
            Assert.That(unpublishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.UnpublishedContent = null;
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Read_Unpublished_Cultures_When_Deleting_Of_Types()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (IPropertyType propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(_contentType, Constants.Security.SuperUserKey);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);
        ContentService.Publish(document, document.AvailableCultures.ToArray());

        var unpublishedWasCalled = false;

        ContentNotificationHandler.UnpublishedContent = notification =>
        {
            IContent unpublished = notification.UnpublishedEntities.First();

            Assert.That(notification.UnpublishedCultures, Is.Not.Null);
            Assert.That(notification.UnpublishedCultures.ContainsKey(unpublished.Key), Is.True);
            Assert.That(notification.UnpublishedCultures[unpublished.Key], Is.EquivalentTo(new[] { "en-US", "fr-FR" }));

            unpublishedWasCalled = true;
        };

        try
        {
            ContentService.DeleteOfTypes(new[] { _contentType.Id });
            Assert.That(unpublishedWasCalled, Is.True);
        }
        finally
        {
            ContentNotificationHandler.UnpublishedContent = null;
        }
    }

    internal sealed class ContentNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentSavedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishingNotification>,
        INotificationHandler<ContentUnpublishedNotification>,
        INotificationHandler<ContentTreeChangeNotification>
    {
        public static Action<ContentSavingNotification> SavingContent { get; set; }

        public static Action<ContentSavedNotification> SavedContent { get; set; }

        public static Action<ContentPublishingNotification> PublishingContent { get; set; }

        public static Action<ContentPublishedNotification> PublishedContent { get; set; }

        public static Action<ContentUnpublishingNotification> UnpublishingContent { get; set; }

        public static Action<ContentUnpublishedNotification> UnpublishedContent { get; set; }

        public static Action<ContentTreeChangeNotification> TreeChange { get; set; }

        public void Handle(ContentPublishedNotification notification) => PublishedContent?.Invoke(notification);

        public void Handle(ContentPublishingNotification notification) => PublishingContent?.Invoke(notification);

        public void Handle(ContentSavedNotification notification) => SavedContent?.Invoke(notification);
        public void Handle(ContentSavingNotification notification) => SavingContent?.Invoke(notification);

        public void Handle(ContentUnpublishedNotification notification) => UnpublishedContent?.Invoke(notification);

        public void Handle(ContentUnpublishingNotification notification) => UnpublishingContent?.Invoke(notification);

        public void Handle(ContentTreeChangeNotification notification) => TreeChange?.Invoke(notification);
    }
}
