using System;
using System.Linq;
using Umbraco.Core.Composing;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest,
        PublishedRepositoryEvents = true,
        WithApplication = true,
        Logger = UmbracoTestOptions.Logger.Console)]
    public class ContentServiceTagsTests : TestWithSomeContentBase
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

        protected override void Compose()
        {
            base.Compose();

            // FIXME: do it differently
            Composition.Register(factory => factory.GetInstance<ServiceContext>().TextService);
        }

        [Test]
        public void TagsCanBeInvariant()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" });
            contentService.SaveAndPublish(content1);

            content1 = contentService.GetById(content1.Id);

            var enTags = content1.Properties["tags"].GetTagsValue().ToArray();
            Assert.AreEqual(4, enTags.Length);
            Assert.Contains("one", enTags);
            Assert.AreEqual(-1, enTags.IndexOf("plus"));

            var tagGroups = tagService.GetAllTags().GroupBy(x => x.LanguageId);
            foreach (var tag in tagService.GetAllTags())
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            Assert.AreEqual(1, tagGroups.Count());
            var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBeVariant()
        {
            var languageService = ServiceContext.LocalizationService;
            languageService.Save(new Language("fr-FR")); // en-US is already there

            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041,
                    Variations = ContentVariation.Culture
                });
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            contentService.SaveAndPublish(content1);

            content1 = contentService.GetById(content1.Id);

            var frTags = content1.Properties["tags"].GetTagsValue("fr-FR").ToArray();
            Assert.AreEqual(5, frTags.Length);
            Assert.Contains("plus", frTags);
            Assert.AreEqual(-1, frTags.IndexOf("one"));

            var enTags = content1.Properties["tags"].GetTagsValue("en-US").ToArray();
            Assert.AreEqual(4, enTags.Length);
            Assert.Contains("one", enTags);
            Assert.AreEqual(-1, enTags.IndexOf("plus"));

            var tagGroups = tagService.GetAllTags(culture:"*").GroupBy(x => x.LanguageId);
            foreach (var tag in tagService.GetAllTags())
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            Assert.AreEqual(2, tagGroups.Count());
            var frTagGroup = tagGroups.FirstOrDefault(x => x.Key == 2);
            Assert.IsNotNull(frTagGroup);
            Assert.AreEqual(5, frTagGroup.Count());
            Assert.IsTrue(frTagGroup.Any(x => x.Text == "plus"));
            Assert.IsFalse(frTagGroup.Any(x => x.Text == "one"));
            var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == 1);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeVariant()
        {
            var enId = ServiceContext.LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            PropertyType propertyType;
            contentType.PropertyGroups.First().PropertyTypes.Add(
                propertyType = new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" });
            contentService.SaveAndPublish(content1);

            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            // no changes
            content1 = contentService.GetById(content1.Id);

            var tags = content1.Properties["tags"].GetTagsValue().ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            var tagGroups = tagService.GetAllTags().GroupBy(x => x.LanguageId);
            foreach (var tag in tagService.GetAllTags())
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            Assert.AreEqual(1, tagGroups.Count());
            var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));

            propertyType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            // changes
            content1 = contentService.GetById(content1.Id);

            // property value has been moved from invariant to en-US
            tags = content1.Properties["tags"].GetTagsValue().ToArray();
            Assert.IsEmpty(tags);

            tags = content1.Properties["tags"].GetTagsValue("en-US").ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            // tags have been copied from invariant to en-US
            tagGroups = tagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (var tag in tagService.GetAllTags("*"))
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            Assert.AreEqual(1, tagGroups.Count());

            enTagGroup = tagGroups.FirstOrDefault(x => x.Key == enId);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeInvariant()
        {
            var languageService = ServiceContext.LocalizationService;
            languageService.Save(new Language("fr-FR")); // en-US is already there

            var enId = ServiceContext.LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            PropertyType propertyType;
            contentType.PropertyGroups.First().PropertyTypes.Add(
                propertyType = new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041,
                    Variations = ContentVariation.Culture
                });
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            contentService.SaveAndPublish(content1);

            contentType.Variations = ContentVariation.Nothing;
            contentTypeService.Save(contentType);

            // changes
            content1 = contentService.GetById(content1.Id);

            // property value has been moved from en-US to invariant, fr-FR tags are gone
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue("fr-FR"));
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue("en-US"));

            var tags = content1.Properties["tags"].GetTagsValue().ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            // tags have been copied from en-US to invariant, fr-FR tags are gone
            var tagGroups = tagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (var tag in tagService.GetAllTags("*"))
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            Assert.AreEqual(1, tagGroups.Count());

            var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeInvariant2()
        {
            var languageService = ServiceContext.LocalizationService;
            languageService.Save(new Language("fr-FR")); // en-US is already there

            var enId = ServiceContext.LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            PropertyType propertyType;
            contentType.PropertyGroups.First().PropertyTypes.Add(
                propertyType = new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041,
                    Variations = ContentVariation.Culture
                });
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            contentService.SaveAndPublish(content1);

            IContent content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetCultureName("name-fr", "fr-FR");
            content2.SetCultureName("name-en", "en-US");
            content2.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content2.AssignTags("tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            contentService.SaveAndPublish(content2);

            //// pretend we already have invariant values
            //using (var scope = ScopeProvider.CreateScope())
            //{
            //    scope.Database.Execute("INSERT INTO [cmsTags] ([tag], [group], [languageId]) SELECT DISTINCT [tag], [group], NULL FROM [cmsTags] WHERE [languageId] IS NOT NULL");
            //}

            // this should work
            propertyType.Variations = ContentVariation.Nothing;
            Assert.DoesNotThrow(() => contentTypeService.Save(contentType));
        }

        [Test]
        public void TagsCanBecomeInvariantByPropertyType()
        {
            var languageService = ServiceContext.LocalizationService;
            languageService.Save(new Language("fr-FR")); // en-US is already there

            var enId = ServiceContext.LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            PropertyType propertyType;
            contentType.PropertyGroups.First().PropertyTypes.Add(
                propertyType = new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041,
                    Variations = ContentVariation.Culture
                });
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            contentService.SaveAndPublish(content1);

            propertyType.Variations = ContentVariation.Nothing;
            contentTypeService.Save(contentType);

            // changes
            content1 = contentService.GetById(content1.Id);

            // property value has been moved from en-US to invariant, fr-FR tags are gone
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue("fr-FR"));
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue("en-US"));

            var tags = content1.Properties["tags"].GetTagsValue().ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            // tags have been copied from en-US to invariant, fr-FR tags are gone
            var tagGroups = tagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (var tag in tagService.GetAllTags("*"))
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            Assert.AreEqual(1, tagGroups.Count());

            var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeInvariantByPropertyTypeAndBackToVariant()
        {
            var languageService = ServiceContext.LocalizationService;
            languageService.Save(new Language("fr-FR")); // en-US is already there

            var enId = ServiceContext.LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            PropertyType propertyType;
            contentType.PropertyGroups.First().PropertyTypes.Add(
                propertyType = new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041,
                    Variations = ContentVariation.Culture
                });
            contentType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            IContent content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags("tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            contentService.SaveAndPublish(content1);

            propertyType.Variations = ContentVariation.Nothing;
            contentTypeService.Save(contentType);

            // FIXME: This throws due to index violations
            propertyType.Variations = ContentVariation.Culture;
            contentTypeService.Save(contentType);

            // TODO: Assert results
        }

        [Test]
        public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_One()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" });
            contentService.SaveAndPublish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content2);

            // verify
            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());

            contentService.MoveToRecycleBin(content1);
        }

        [Test]
        public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_All()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "bam" });
            contentService.SaveAndPublish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content2);

            // verify
            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());

            contentService.Unpublish(content1);
            contentService.Unpublish(content2);
        }

        [Test]
        [Ignore("https://github.com/umbraco/Umbraco-CMS/issues/3821 (U4-8442), will need to be fixed.")]
        public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_Tree()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "plus" });
            contentService.SaveAndPublish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", content1.Id);
            content2.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content2);

            // verify
            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());

            contentService.MoveToRecycleBin(content1);

            // no more tags
            tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());
            tags = tagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(0, tags.Count());

            // no more tags
            allTags = tagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());

            contentService.Move(content1, -1);

            Assert.IsFalse(content1.Published);

            // no more tags
            tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());
            tags = tagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(0, tags.Count());

            // no more tags
            allTags = tagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());

            content1.PublishCulture(CultureType.Invariant);
            contentService.SaveAndPublish(content1);

            Assert.IsTrue(content1.Published);

            // tags are back
            tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());

            // FIXME: tag & tree issue
            // when we publish, we 'just' publish the top one and not the ones below = fails
            // what we should do is... NOT clear tags when unpublishing or trashing or...
            // and just update the tag service to NOT return anything related to trashed or
            // unpublished entities (since trashed is set on ALL entities in the trashed branch)
            tags = tagService.GetTagsForEntity(content2.Id); // including that one!
            Assert.AreEqual(4, tags.Count());

            // tags are back
            allTags = tagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());
        }

        [Test]
        public void TagsAreUpdatedWhenContentIsUnpublishedAndRePublished()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "bam" });
            contentService.SaveAndPublish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content2);

            contentService.Unpublish(content1);
            contentService.Unpublish(content2);
        }

        [Test]
        [Ignore("https://github.com/umbraco/Umbraco-CMS/issues/3821 (U4-8442), will need to be fixed.")]
        public void TagsAreUpdatedWhenContentIsUnpublishedAndRePublished_Tree()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var tagService = ServiceContext.TagService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            var content1 = MockedContent.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags("tags", new[] { "hello", "world", "some", "tags", "bam" });
            contentService.SaveAndPublish(content1);

            var content2 = MockedContent.CreateSimpleContent(contentType, "Tagged content 2", content1);
            content2.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content2);

            contentService.Unpublish(content1);

            var tags = tagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());

            // FIXME: tag & tree issue
            // when we (un)publish, we 'just' publish the top one and not the ones below = fails
            // see similar note above
            tags = tagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(0, tags.Count());
            var allTags = tagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());

            content1.PublishCulture(CultureType.Invariant);
            contentService.SaveAndPublish(content1);

            tags = tagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(4, tags.Count());
            allTags = tagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());
        }

        [Test]
        public void Create_Tag_Data_Bulk_Publish_Operation()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var dataTypeService = ServiceContext.DataTypeService;

            //set configuration
            var dataType = dataTypeService.GetDataType(1041);
            dataType.Configuration = new TagConfiguration
            {
                Group = "test",
                StorageType = TagsStorageType.Csv
            };

            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias) };

            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.Save(content);

            var child1 = MockedContent.CreateSimpleContent(contentType, "child 1 content", content.Id);
            child1.AssignTags("tags", new[] { "hello1", "world1", "some1" });
            contentService.Save(child1);

            var child2 = MockedContent.CreateSimpleContent(contentType, "child 2 content", content.Id);
            child2.AssignTags("tags", new[] { "hello2", "world2" });
            contentService.Save(child2);

            // Act
            contentService.SaveAndPublishBranch(content, true);

            // Assert
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;

            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.AreEqual(4, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content.Id, propTypeId = propertyTypeId }));

                Assert.AreEqual(3, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = child1.Id, propTypeId = propertyTypeId }));

                Assert.AreEqual(2, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = child2.Id, propTypeId = propertyTypeId }));

                scope.Complete();
            }
        }

        [Test]
        public void Does_Not_Create_Tag_Data_For_Non_Published_Version()
        {
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            // create content type with a tag property
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(new PropertyType("test", ValueStorageType.Ntext, "tags") { DataTypeId = 1041 });
            contentTypeService.Save(contentType);

            // create a content with tags and publish
            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content);

            // edit tags and save
            content.AssignTags("tags", new[] { "another", "world" }, merge: true);
            contentService.Save(content);

            // the (edit) property does contain all tags
            Assert.AreEqual(5, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());

            // but the database still contains the initial two tags
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.AreEqual(4, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content.Id, propTypeId = propertyTypeId }));
                scope.Complete();
            }
        }

        [Test]
        public void Can_Replace_Tag_Data_To_Published_Content()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);

            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);


            // Act
            content.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content);

            // Assert
            Assert.AreEqual(4, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.AreEqual(4, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content.Id, propTypeId = propertyTypeId }));

                scope.Complete();
            }
        }

        [Test]
        public void Can_Append_Tag_Data_To_Published_Content()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);
            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content);

            // Act
            content.AssignTags("tags", new[] { "another", "world" }, merge: true);
            contentService.SaveAndPublish(content);

            // Assert
            Assert.AreEqual(5, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.AreEqual(5, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content.Id, propTypeId = propertyTypeId }));

                scope.Complete();
            }
        }

        [Test]
        public void Can_Remove_Tag_Data_To_Published_Content()
        {
            //Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            contentType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Ntext, "tags")
                {
                    DataTypeId = 1041
                });
            contentTypeService.Save(contentType);
            var content = MockedContent.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags("tags", new[] { "hello", "world", "some", "tags" });
            contentService.SaveAndPublish(content);

            // Act
            content.RemoveTags("tags", new[] { "some", "world" });
            contentService.SaveAndPublish(content);

            // Assert
            Assert.AreEqual(2, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
            var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (var scope = ScopeProvider.CreateScope())
            {
                Assert.AreEqual(2, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content.Id, propTypeId = propertyTypeId }));

                scope.Complete();
            }
        }

    }
}
