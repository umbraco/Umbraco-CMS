using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest,
        PublishedRepositoryEvents = true,
        WithApplication = true,
        Logger = UmbracoTestOptions.Logger.Console)]
    public class ContentServiceEventTests : TestWithSomeContentBase
    {
        public override void SetUp()
        {
            base.SetUp();
            ContentRepositoryBase.ThrowOnWarning = true;
        }

        public override void TearDown()
        {
            ContentRepositoryBase.ThrowOnWarning = false;
            base.TearDown();
        }

        [Test]
        public void SavingTest()
        {
            var languageService = ServiceContext.LocalizationService;

            languageService.Save(new Language("fr-FR"));

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentType.Variations = ContentVariation.Culture;
            foreach (var propertyType in contentType.PropertyTypes)
                propertyType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            var document = new Content("content", -1, contentType);
            document.SetCultureName("hello", "en-US");
            document.SetCultureName("bonjour", "fr-FR");
            contentService.Save(document);

            // properties: title, bodyText, keywords, description
            document.SetValue("title", "title-en", "en-US");

            // touch the culture - required for IsSaving/HasSaved to work
            document.TouchCulture("fr-FR");

            void OnSaving(IContentService sender, SaveEventArgs<IContent> e)
            {
                var saved = e.SavedEntities.First();

                Assert.AreSame(document, saved);

                Assert.IsTrue(saved.IsSavingCulture("fr-FR"));
                Assert.IsFalse(saved.IsSavingCulture("en-UK"));
            }

            void OnSaved(IContentService sender, SaveEventArgs<IContent> e)
            {
                var saved = e.SavedEntities.First();

                Assert.AreSame(document, saved);

                Assert.IsTrue(saved.HasSavedCulture("fr-FR"));
                Assert.IsFalse(saved.HasSavedCulture("en-UK"));
            }

            ContentService.Saving += OnSaving;
            ContentService.Saved += OnSaved;
            contentService.Save(document);
            ContentService.Saving -= OnSaving;
            ContentService.Saved -= OnSaved;
        }

        [Test]
        public void PublishingTest()
        {
            var languageService = ServiceContext.LocalizationService;

            languageService.Save(new Language("fr-FR"));

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentType.Variations = ContentVariation.Culture;
            foreach (var propertyType in contentType.PropertyTypes)
                propertyType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            var document = new Content("content", -1, contentType);
            document.SetCultureName("hello", "en-US");
            document.SetCultureName("bonjour", "fr-FR");
            contentService.Save(document);

            // ensure it works and does not throw
            Assert.IsFalse(document.WasCulturePublished("fr-FR"));
            Assert.IsFalse(document.WasCulturePublished("en-US"));
            Assert.IsFalse(document.IsCulturePublished("fr-FR"));
            Assert.IsFalse(document.IsCulturePublished("en-US"));

            void OnPublishing(IContentService sender, PublishEventArgs<IContent> e)
            {
                var publishing = e.PublishedEntities.First();

                Assert.AreSame(document, publishing);

                Assert.IsFalse(publishing.IsPublishingCulture("en-US"));
                Assert.IsTrue(publishing.IsPublishingCulture("fr-FR"));
            }

            void OnPublished(IContentService sender, PublishEventArgs<IContent> e)
            {
                var published = e.PublishedEntities.First();

                Assert.AreSame(document, published);

                Assert.IsFalse(published.HasPublishedCulture("en-US"));
                Assert.IsTrue(published.HasPublishedCulture("fr-FR"));
            }

            ContentService.Publishing += OnPublishing;
            ContentService.Published += OnPublished;
            contentService.SaveAndPublish(document, "fr-FR");
            ContentService.Publishing -= OnPublishing;
            ContentService.Published -= OnPublished;

            document = (Content) contentService.GetById(document.Id);

            // ensure it works and does not throw
            Assert.IsTrue(document.WasCulturePublished("fr-FR"));
            Assert.IsFalse(document.WasCulturePublished("en-US"));
            Assert.IsTrue(document.IsCulturePublished("fr-FR"));
            Assert.IsFalse(document.IsCulturePublished("en-US"));
        }

        [Test]
        public void UnpublishingTest()
        {
            var languageService = ServiceContext.LocalizationService;

            languageService.Save(new Language("fr-FR"));

            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentType.Variations = ContentVariation.Culture;
            foreach (var propertyType in contentType.PropertyTypes)
                propertyType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            var document = new Content("content", -1, contentType);
            document.SetCultureName("hello", "en-US");
            document.SetCultureName("bonjour", "fr-FR");
            contentService.SaveAndPublish(document);

            // ensure it works and does not throw
            Assert.IsTrue(document.WasCulturePublished("fr-FR"));
            Assert.IsTrue(document.WasCulturePublished("en-US"));
            Assert.IsTrue(document.IsCulturePublished("fr-FR"));
            Assert.IsTrue(document.IsCulturePublished("en-US"));

            void OnPublishing(IContentService sender, PublishEventArgs<IContent> e)
            {
                var publishing = e.PublishedEntities.First();

                Assert.AreSame(document, publishing);

                Assert.IsFalse(publishing.IsPublishingCulture("en-US"));
                Assert.IsFalse(publishing.IsPublishingCulture("fr-FR"));

                Assert.IsFalse(publishing.IsUnpublishingCulture("en-US"));
                Assert.IsTrue(publishing.IsUnpublishingCulture("fr-FR"));
            }

            void OnPublished(IContentService sender, PublishEventArgs<IContent> e)
            {
                var published = e.PublishedEntities.First();

                Assert.AreSame(document, published);

                Assert.IsFalse(published.HasPublishedCulture("en-US"));
                Assert.IsFalse(published.HasPublishedCulture("fr-FR"));

                Assert.IsFalse(published.HasUnpublishedCulture("en-US"));
                Assert.IsTrue(published.HasUnpublishedCulture("fr-FR"));
            }

            document.UnpublishCulture("fr-FR");

            ContentService.Publishing += OnPublishing;
            ContentService.Published += OnPublished;
            contentService.SavePublishing(document);
            ContentService.Publishing -= OnPublishing;
            ContentService.Published -= OnPublished;

            document = (Content) contentService.GetById(document.Id);

            // ensure it works and does not throw
            Assert.IsFalse(document.WasCulturePublished("fr-FR"));
            Assert.IsTrue(document.WasCulturePublished("en-US"));
            Assert.IsFalse(document.IsCulturePublished("fr-FR"));
            Assert.IsTrue(document.IsCulturePublished("en-US"));
        }
    }
}
