// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
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

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IFileService FileService => GetRequiredService<IFileService>();

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

    private void CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template); // else, FK violation on contentType!

        _contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(_contentType);
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
    public void Publishing_Invariant()
    {
        IContent document = new Content("content", -1, _contentType);
        ContentService.Save(document);

        var treeChangeWasCalled = false;

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            var publishedCultures = change?.PublishedCultures?.ToArray();
            Assert.IsNotNull(publishedCultures);
            Assert.AreEqual(1, publishedCultures.Length);
            Assert.IsTrue(publishedCultures.InvariantContains("*"));
            Assert.IsNull(change.UnpublishedCultures);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.Publish(document, ["*"]);
            Assert.IsTrue(treeChangeWasCalled);
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
            Assert.IsNull(change?.PublishedCultures);
            var unpublishedCultures = change?.UnpublishedCultures?.ToArray();
            Assert.IsNotNull(unpublishedCultures);
            Assert.AreEqual(1, unpublishedCultures.Length);
            Assert.IsTrue(unpublishedCultures.InvariantContains("*"));

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.Unpublish(document);
            Assert.IsTrue(treeChangeWasCalled);
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
        var treeChangeWasCalled = false;

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

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            var publishedCultures = change?.PublishedCultures?.ToArray();
            Assert.IsNotNull(publishedCultures);
            Assert.AreEqual(1, publishedCultures.Length);
            Assert.IsTrue(publishedCultures.InvariantContains("fr-FR"));
            Assert.IsNull(change.UnpublishedCultures);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.Publish(document, new[] { "fr-FR" });
            Assert.IsTrue(publishingWasCalled);
            Assert.IsTrue(publishedWasCalled);
            Assert.IsTrue(treeChangeWasCalled);
        }
        finally
        {
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
            ContentNotificationHandler.TreeChange = null;
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
        var publishingWasCalled = false;
        var publishedWasCalled = false;

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
            Assert.AreEqual(null, propValue.PublishedValue);

            savedWasCalled = true;
        };

        ContentNotificationHandler.PublishingContent = notification =>
        {
            var publishing = notification.PublishedEntities.First();

            Assert.AreEqual("title", publishing.GetValue<string>("title"));

            publishingWasCalled = true;
        };

        ContentNotificationHandler.PublishedContent = notification =>
        {
            var published = notification.PublishedEntities.First();

            Assert.AreSame("title", document.GetValue<string>("title"));

            // We're only dealing with invariant here.
            var propValue = published.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

            Assert.AreEqual("title", propValue.EditedValue);
            Assert.AreEqual("title", propValue.PublishedValue);

            publishedWasCalled = true;
        };

        try
        {
            ContentService.Save(document);
            ContentService.Publish(document, document.AvailableCultures.ToArray());
            Assert.IsTrue(savingWasCalled);
            Assert.IsTrue(savedWasCalled);
            Assert.IsTrue(publishingWasCalled);
            Assert.IsTrue(publishedWasCalled);
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
    public void Publishing_Set_Mandatory_Value()
    {
        var titleProperty = _contentType.PropertyTypes.First(x => x.Alias == "title");
        titleProperty.Mandatory = true; // make this required!
        ContentTypeService.Save(_contentType);

        IContent document = new Content("content", -1, _contentType);

        ContentService.Save(document);
        var result = ContentService.Publish(document, document.AvailableCultures.ToArray());
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
            ContentService.Save(document);
            result = ContentService.Publish(document, document.AvailableCultures.ToArray());
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
    [LongRunning]
    public async Task Unpublishing_Culture()
    {
        await LanguageService.CreateAsync(new Language("fr-FR", "French (France)"), Constants.Security.SuperUserKey);

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
        ContentService.Publish(document, document.AvailableCultures.ToArray());

        Assert.IsTrue(document.IsCulturePublished("fr-FR"));
        Assert.IsTrue(document.IsCulturePublished("en-US"));

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

        ContentNotificationHandler.TreeChange += notification =>
        {
            var change = notification.Changes.FirstOrDefault();
            var unpublishedCultures = change?.UnpublishedCultures?.ToArray();
            Assert.IsNotNull(unpublishedCultures);
            Assert.AreEqual(1, unpublishedCultures.Length);
            Assert.IsTrue(unpublishedCultures.InvariantContains("fr-FR"));
            Assert.IsNull(change.PublishedCultures);

            treeChangeWasCalled = true;
        };

        try
        {
            ContentService.CommitDocumentChanges(document);
            Assert.IsTrue(publishingWasCalled);
            Assert.IsTrue(publishedWasCalled);
            Assert.IsTrue(treeChangeWasCalled);
        }
        finally
        {
            ContentNotificationHandler.PublishingContent = null;
            ContentNotificationHandler.PublishedContent = null;
            ContentNotificationHandler.TreeChange = null;
        }

        document = ContentService.GetById(document.Id);

        Assert.IsFalse(document.IsCulturePublished("fr-FR"));
        Assert.IsTrue(document.IsCulturePublished("en-US"));
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
