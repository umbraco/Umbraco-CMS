// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
public class ContentServiceNotificationTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetupTest()
    {
        ContentRepositoryBase.ThrowOnWarning = true;
        _globalSettings = new GlobalSettings();

        CreateTestData();
    }

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private GlobalSettings _globalSettings;
    private IContentType _contentType;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentSavedNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentPublishingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentPublishedNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishingNotification, ContentNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishedNotification, ContentNotificationHandler>();

    private void CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template); // else, FK violation on contentType!

        _contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(_contentType);
    }

    [Test]
    public void Saving_Culture()
    {
        LocalizationService.Save(new Language("fr-FR", "French (France)"));

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        ContentTypeService.Save(_contentType);

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

            Assert.AreSame(document, saved);

            Assert.IsTrue(notification.IsSavingCulture(saved, "en-US"));
            Assert.IsFalse(notification.IsSavingCulture(saved, "fr-FR"));

            savingWasCalled = true;
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.AreSame(document, saved);

            Assert.IsTrue(notification.HasSavedCulture(saved, "en-US"));
            Assert.IsFalse(notification.HasSavedCulture(saved, "fr-FR"));

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(savedWasCalled);
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

            Assert.IsTrue(document.GetValue<string>("title").IsNullOrWhiteSpace());

            saved.SetValue("title", "title");

            savingWasCalled = true;
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.AreSame("title", document.GetValue<string>("title"));

            // we're only dealing with invariant here
            var propValue = saved.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

            Assert.AreEqual("title", propValue.EditedValue);
            Assert.IsNull(propValue.PublishedValue);

            savedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Publishing_Culture()
    {
        LocalizationService.Save(new Language("fr-FR", "French (France)"));

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        ContentTypeService.Save(_contentType);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.Save(document);

        Assert.IsFalse(document.IsCulturePublished("fr-FR"));
        Assert.IsFalse(document.IsCulturePublished("en-US"));

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        var publishingWasCalled = false;
        var publishedWasCalled = false;

        ContentNotificationHandler.PublishingContent += notification =>
        {
            var publishing = notification.PublishedEntities.First();

            Assert.AreSame(document, publishing);

            Assert.IsFalse(notification.IsPublishingCulture(publishing, "en-US"));
            Assert.IsTrue(notification.IsPublishingCulture(publishing, "fr-FR"));

            publishingWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent += notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.AreSame(document, published);

            Assert.IsFalse(notification.HasPublishedCulture(published, "en-US"));
            Assert.IsTrue(notification.HasPublishedCulture(published, "fr-FR"));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.SaveAndPublish(document, "fr-FR");
            Assert.IsTrue(publishingWasCalled);
            Assert.IsTrue(publishedWasCalled);
        }
        finally
        {
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
        }

        document = ContentService.GetById(document.Id);

        // ensure it works and does not throw
        Assert.IsTrue(document.IsCulturePublished("fr-FR"));
        Assert.IsFalse(document.IsCulturePublished("en-US"));
    }

    [Test]
    public void Publishing_Set_Value()
    {
        IContent document = new Content("content", -1, _contentType);

        var savingWasCalled = false;
        var savedWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.IsTrue(document.GetValue<string>("title").IsNullOrWhiteSpace());

            saved.SetValue("title", "title");

            savingWasCalled = true;
        };

        ContentNotificationHandler.SavedContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.AreSame("title", document.GetValue<string>("title"));

            // We're only dealing with invariant here.
            var propValue = saved.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

            Assert.AreEqual("title", propValue.EditedValue);
            Assert.AreEqual("title", propValue.PublishedValue);

            savedWasCalled = true;
        };

        try
        {
            ContentService.SaveAndPublish(document);
            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(savedWasCalled);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
            ContentNotificationHandler.SavedContent = null;
        }
    }

    [Test]
    public void Publishing_Set_Mandatory_Value()
    {
        var titleProperty = _contentType.PropertyTypes.First(x => x.Alias == "title");
        titleProperty.Mandatory = true; // make this required!
        ContentTypeService.Save(_contentType);

        IContent document = new Content("content", -1, _contentType);

        var result = ContentService.SaveAndPublish(document);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("title", result.InvalidProperties.First().Alias);

        // when a service operation fails, the object is dirty and should not be re-used,
        // re-create it
        document = new Content("content", -1, _contentType);

        var savingWasCalled = false;

        ContentNotificationHandler.SavingContent = notification =>
        {
            var saved = notification.SavedEntities.First();

            Assert.IsTrue(document.GetValue<string>("title").IsNullOrWhiteSpace());

            saved.SetValue("title", "title");

            savingWasCalled = true;
        };

        try
        {
            result = ContentService.SaveAndPublish(document);
            Assert.IsTrue(result
                .Success); // will succeed now because we were able to specify the required value in the Saving event
            Assert.IsTrue(savingWasCalled);
        }
        finally
        {
            ContentNotificationHandler.SavingContent = null;
        }
    }

    [Test]
    public void Unpublishing_Culture()
    {
        LocalizationService.Save(new Language("fr-FR", "French (France)"));

        _contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in _contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        ContentTypeService.Save(_contentType);

        IContent document = new Content("content", -1, _contentType);
        document.SetCultureName("hello", "en-US");
        document.SetCultureName("bonjour", "fr-FR");
        ContentService.SaveAndPublish(document);

        Assert.IsTrue(document.IsCulturePublished("fr-FR"));
        Assert.IsTrue(document.IsCulturePublished("en-US"));

        // re-get - dirty properties need resetting
        document = ContentService.GetById(document.Id);

        document.UnpublishCulture("fr-FR");

        var publishingWasCalled = false;
        var publishedWasCalled = false;

        // TODO: revisit this - it was migrated when removing static events, but the expected result seems illogic - why does this test bind to Published and not Unpublished?

        ContentNotificationHandler.PublishingContent += notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.AreSame(document, published);

            Assert.IsFalse(notification.IsPublishingCulture(published, "en-US"));
            Assert.IsFalse(notification.IsPublishingCulture(published, "fr-FR"));

            Assert.IsFalse(notification.IsUnpublishingCulture(published, "en-US"));
            Assert.IsTrue(notification.IsUnpublishingCulture(published, "fr-FR"));

            publishingWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent += notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.AreSame(document, published);

            Assert.IsFalse(notification.HasPublishedCulture(published, "en-US"));
            Assert.IsFalse(notification.HasPublishedCulture(published, "fr-FR"));

            Assert.IsFalse(notification.HasUnpublishedCulture(published, "en-US"));
            Assert.IsTrue(notification.HasUnpublishedCulture(published, "fr-FR"));

            publishedWasCalled = true;
        };

        try
        {
            ContentService.CommitDocumentChanges(document);
            Assert.IsTrue(publishingWasCalled);
            Assert.IsTrue(publishedWasCalled);
        }
        finally
        {
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
        }

        document = ContentService.GetById(document.Id);

        Assert.IsFalse(document.IsCulturePublished("fr-FR"));
        Assert.IsTrue(document.IsCulturePublished("en-US"));
    }

    public class ContentNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentSavedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishingNotification>,
        INotificationHandler<ContentUnpublishedNotification>
    {
        public static Action<ContentSavingNotification> SavingContent { get; set; }

        public static Action<ContentSavedNotification> SavedContent { get; set; }

        public static Action<ContentPublishingNotification> PublishingContent { get; set; }

        public static Action<ContentPublishedNotification> PublishedContent { get; set; }

        public static Action<ContentUnpublishingNotification> UnpublishingContent { get; set; }

        public static Action<ContentUnpublishedNotification> UnpublishedContent { get; set; }

        public void Handle(ContentPublishedNotification notification) => PublishedContent?.Invoke(notification);

        public void Handle(ContentPublishingNotification notification) => PublishingContent?.Invoke(notification);

        public void Handle(ContentSavedNotification notification) => SavedContent?.Invoke(notification);
        public void Handle(ContentSavingNotification notification) => SavingContent?.Invoke(notification);

        public void Handle(ContentUnpublishedNotification notification) => UnpublishedContent?.Invoke(notification);

        public void Handle(ContentUnpublishingNotification notification) => UnpublishingContent?.Invoke(notification);
    }
}
