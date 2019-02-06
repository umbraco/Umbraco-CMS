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

            IContent document = new Content("content", -1, contentType);
            document.SetCultureName("hello", "en-US");
            document.SetCultureName("bonjour", "fr-FR");
            contentService.Save(document);

            //re-get - dirty properties need resetting
            document = contentService.GetById(document.Id);

            // properties: title, bodyText, keywords, description
            document.SetValue("title", "title-en", "en-US");

            void OnSaving(IContentService sender, ContentSavingEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.AreSame(document, saved);

                Assert.IsTrue(e.IsSavingCulture(saved, "en-US"));
                Assert.IsFalse(e.IsSavingCulture(saved, "fr-FR"));
            }

            void OnSaved(IContentService sender, ContentSavedEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.AreSame(document, saved);

                Assert.IsTrue(e.HasSavedCulture(saved, "en-US"));
                Assert.IsFalse(e.HasSavedCulture(saved, "fr-FR"));
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

            IContent document = new Content("content", -1, contentType);
            document.SetCultureName("hello", "en-US");
            document.SetCultureName("bonjour", "fr-FR");
            contentService.Save(document);

            Assert.IsFalse(document.IsCulturePublished("fr-FR"));
            Assert.IsFalse(document.IsCulturePublished("en-US"));

            //re-get - dirty properties need resetting
            document = contentService.GetById(document.Id);

            void OnPublishing(IContentService sender, ContentPublishingEventArgs e)
            {
                var publishing = e.PublishedEntities.First();

                Assert.AreSame(document, publishing);

                Assert.IsFalse(e.IsPublishingCulture(publishing, "en-US"));
                Assert.IsTrue(e.IsPublishingCulture(publishing, "fr-FR"));
            }

            void OnPublished(IContentService sender, ContentPublishedEventArgs e)
            {
                var published = e.PublishedEntities.First();

                Assert.AreSame(document, published);

                Assert.IsFalse(e.HasPublishedCulture(published, "en-US"));
                Assert.IsTrue(e.HasPublishedCulture(published, "fr-FR"));
            }

            ContentService.Publishing += OnPublishing;
            ContentService.Published += OnPublished;
            contentService.SaveAndPublish(document, "fr-FR");
            ContentService.Publishing -= OnPublishing;
            ContentService.Published -= OnPublished;

            document = contentService.GetById(document.Id);

            // ensure it works and does not throw
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

            var contentService = (ContentService)ServiceContext.ContentService;

            IContent document = new Content("content", -1, contentType);
            document.SetCultureName("hello", "en-US");
            document.SetCultureName("bonjour", "fr-FR");
            contentService.SaveAndPublish(document);

            Assert.IsTrue(document.IsCulturePublished("fr-FR"));
            Assert.IsTrue(document.IsCulturePublished("en-US"));

            //re-get - dirty properties need resetting
            document = contentService.GetById(document.Id);

            void OnPublishing(IContentService sender, ContentPublishingEventArgs e)
            {
                var publishing = e.PublishedEntities.First();

                Assert.AreSame(document, publishing);

                Assert.IsFalse(e.IsPublishingCulture(publishing, "en-US"));
                Assert.IsFalse(e.IsPublishingCulture(publishing, "fr-FR"));

                Assert.IsFalse(e.IsUnpublishingCulture(publishing, "en-US"));
                Assert.IsTrue(e.IsUnpublishingCulture(publishing, "fr-FR"));
            }

            void OnPublished(IContentService sender, ContentPublishedEventArgs e)
            {
                var published = e.PublishedEntities.First();

                Assert.AreSame(document, published);

                Assert.IsFalse(e.HasPublishedCulture(published, "en-US"));
                Assert.IsFalse(e.HasPublishedCulture(published, "fr-FR"));

                Assert.IsFalse(e.HasUnpublishedCulture(published, "en-US"));
                Assert.IsTrue(e.HasUnpublishedCulture(published, "fr-FR"));
            }

            document.UnpublishCulture("fr-FR");

            ContentService.Publishing += OnPublishing;
            ContentService.Published += OnPublished;
            contentService.CommitDocumentChanges(document);
            ContentService.Publishing -= OnPublishing;
            ContentService.Published -= OnPublished;

            document = contentService.GetById(document.Id);

            Assert.IsFalse(document.IsCulturePublished("fr-FR"));
            Assert.IsTrue(document.IsCulturePublished("en-US"));
        }
    }
}
