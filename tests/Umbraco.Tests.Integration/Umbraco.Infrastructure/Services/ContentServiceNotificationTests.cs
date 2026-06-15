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
    public async Task Saving_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

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
    public void Saving_Set_Value()
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
    public void Publishing_Invariant()
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
    public void Unpublishing_Invariant()
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
    public async Task Publishing_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

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
    public void Publishing_Set_Value()
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
    public async Task Publishing_Set_Mandatory_Value()
    {
        var titleProperty = _contentType.PropertyTypes.First(x => x.Alias == "title");
        titleProperty.Mandatory = true; // make this required!
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

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
    public async Task Unpublishing_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

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
