using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
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
        public void Saving_Culture()
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
            try
            {
                contentService.Save(document);
            }
            finally
            {
                ContentService.Saving -= OnSaving;
                ContentService.Saved -= OnSaved;
            }
        }

        [Test]
        public void Saving_Set_Value()
        {
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            IContent document = new Content("content", -1, contentType);
            
            void OnSaving(IContentService sender, ContentSavingEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.IsTrue(document.GetValue<string>("title").IsNullOrWhiteSpace());

                saved.SetValue("title", "title");
            }

            void OnSaved(IContentService sender, ContentSavedEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.AreSame("title", document.GetValue<string>("title"));

                //we're only dealing with invariant here
                var propValue = saved.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);
                
                Assert.AreEqual("title", propValue.EditedValue);
                Assert.IsNull(propValue.PublishedValue);
            }

            ContentService.Saving += OnSaving;
            ContentService.Saved += OnSaved;
            try
            {
                contentService.Save(document);
            }
            finally
            {
                ContentService.Saving -= OnSaving;
                ContentService.Saved -= OnSaved;
            }
            
        }

        [Test]
        public void Publishing_Culture()
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
            try
            {
                contentService.SaveAndPublish(document, "fr-FR");
            }
            finally
            {
                ContentService.Publishing -= OnPublishing;
                ContentService.Published -= OnPublished;
            }

            document = contentService.GetById(document.Id);

            // ensure it works and does not throw
            Assert.IsTrue(document.IsCulturePublished("fr-FR"));
            Assert.IsFalse(document.IsCulturePublished("en-US"));
        }

        [Test]
        public void Publishing_Set_Value()
        {
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            IContent document = new Content("content", -1, contentType);

            void OnSaving(IContentService sender, ContentSavingEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.IsTrue(document.GetValue<string>("title").IsNullOrWhiteSpace());

                saved.SetValue("title", "title");
            }

            void OnSaved(IContentService sender, ContentSavedEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.AreSame("title", document.GetValue<string>("title"));

                //we're only dealing with invariant here
                var propValue = saved.Properties["title"].Values.First(x => x.Culture == null && x.Segment == null);

                Assert.AreEqual("title", propValue.EditedValue);
                Assert.AreEqual("title", propValue.PublishedValue);
            }

            //We are binding to Saving (not Publishing), because the Publishing event is really just used for cancelling, it should not be 
            //used for setting values and it won't actually work! This is because the Publishing event is raised AFTER the values on the model
            //are published, but Saving is raised BEFORE.
            ContentService.Saving += OnSaving;
            ContentService.Saved += OnSaved;
            try
            {
                contentService.SaveAndPublish(document);
            }
            finally
            {
                ContentService.Saving -= OnSaving;
                ContentService.Saved -= OnSaved;
            }
        }

        [Test]
        public void Publishing_Set_Mandatory_Value()
        {
            var contentTypeService = ServiceContext.ContentTypeService;

            var contentType = MockedContentTypes.CreateTextPageContentType();
            var titleProperty = contentType.PropertyTypes.First(x => x.Alias == "title");
            titleProperty.Mandatory = true; // make this required!
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            contentTypeService.Save(contentType);

            var contentService = ServiceContext.ContentService;

            IContent document = new Content("content", -1, contentType);

            var result = contentService.SaveAndPublish(document);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("title", result.InvalidProperties.First().Alias);

            // when a service operation fails, the object is dirty and should not be re-used,
            // re-create it
            document = new Content("content", -1, contentType);

            void OnSaving(IContentService sender, ContentSavingEventArgs e)
            {
                var saved = e.SavedEntities.First();

                Assert.IsTrue(document.GetValue<string>("title").IsNullOrWhiteSpace());

                saved.SetValue("title", "title");
            }

            //We are binding to Saving (not Publishing), because the Publishing event is really just used for cancelling, it should not be 
            //used for setting values and it won't actually work! This is because the Publishing event is raised AFTER the values on the model
            //are published, but Saving is raised BEFORE.
            ContentService.Saving += OnSaving;
            try
            {
                result = contentService.SaveAndPublish(document);
                Assert.IsTrue(result.Success); //will succeed now because we were able to specify the required value in the Saving event
            }
            finally
            {
                ContentService.Saving -= OnSaving;
            }
        }

        [Test]
        public void Unpublishing_Culture()
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
            try
            {
                contentService.CommitDocumentChanges(document);
            }
            finally
            {
                ContentService.Publishing -= OnPublishing;
                ContentService.Published -= OnPublished;
            }

            document = contentService.GetById(document.Id);

            Assert.IsFalse(document.IsCulturePublished("fr-FR"));
            Assert.IsTrue(document.IsCulturePublished("en-US"));
        }
    }
}
