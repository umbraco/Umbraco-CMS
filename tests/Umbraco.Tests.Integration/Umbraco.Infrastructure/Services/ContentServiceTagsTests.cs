// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(
        Database = UmbracoTestOptions.Database.NewSchemaPerTest,
        PublishedRepositoryEvents = true,
        WithApplication = true,
        Logger = UmbracoTestOptions.Logger.Console)]
    public class ContentServiceTagsTests : UmbracoIntegrationTest
    {
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        private IContentService ContentService => GetRequiredService<IContentService>();

        private ITagService TagService => GetRequiredService<ITagService>();

        private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

        private IFileService FileService => GetRequiredService<IFileService>();

        private IJsonSerializer Serializer => GetRequiredService<IJsonSerializer>();

        public PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

        [SetUp]
        public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

        [TearDown]
        public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

        [Test]
        public void TagsCanBeInvariant()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" });
            ContentService.SaveAndPublish(content1);

            content1 = ContentService.GetById(content1.Id);

            string[] enTags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer).ToArray();
            Assert.AreEqual(4, enTags.Length);
            Assert.Contains("one", enTags);
            Assert.AreEqual(-1, enTags.IndexOf("plus"));

            IEnumerable<IGrouping<int?, ITag>> tagGroups = TagService.GetAllTags().GroupBy(x => x.LanguageId);
            foreach (ITag tag in TagService.GetAllTags())
            {
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            }

            Assert.AreEqual(1, tagGroups.Count());
            IGrouping<int?, ITag> enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBeVariant()
        {
            ILocalizationService languageService = LocalizationService;
            ILanguage language = new LanguageBuilder()
                .WithCultureInfo("fr-FR")
                .Build();
            LocalizationService.Save(language); // en-US is already there

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            ContentService.SaveAndPublish(content1);

            content1 = ContentService.GetById(content1.Id);

            string[] frTags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "fr-FR").ToArray();
            Assert.AreEqual(5, frTags.Length);
            Assert.Contains("plus", frTags);
            Assert.AreEqual(-1, frTags.IndexOf("one"));

            string[] enTags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US").ToArray();
            Assert.AreEqual(4, enTags.Length);
            Assert.Contains("one", enTags);
            Assert.AreEqual(-1, enTags.IndexOf("plus"));

            IEnumerable<IGrouping<int?, ITag>> tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (ITag tag in TagService.GetAllTags())
            {
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            }

            Assert.AreEqual(2, tagGroups.Count());
            IGrouping<int?, ITag> frTagGroup = tagGroups.FirstOrDefault(x => x.Key == 2);
            Assert.IsNotNull(frTagGroup);
            Assert.AreEqual(5, frTagGroup.Count());
            Assert.IsTrue(frTagGroup.Any(x => x.Text == "plus"));
            Assert.IsFalse(frTagGroup.Any(x => x.Text == "one"));
            IGrouping<int?, ITag> enTagGroup = tagGroups.FirstOrDefault(x => x.Key == 1);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeVariant()
        {
            int enId = LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            PropertyType propertyType = CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" });
            ContentService.SaveAndPublish(content1);

            contentType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            // no changes
            content1 = ContentService.GetById(content1.Id);

            string[] tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer).ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            IEnumerable<IGrouping<int?, ITag>> tagGroups = TagService.GetAllTags().GroupBy(x => x.LanguageId);
            foreach (ITag tag in TagService.GetAllTags())
            {
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            }

            Assert.AreEqual(1, tagGroups.Count());
            IGrouping<int?, ITag> enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));

            propertyType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            // changes
            content1 = ContentService.GetById(content1.Id);

            // property value has been moved from invariant to en-US
            tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer).ToArray();
            Assert.IsEmpty(tags);

            tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US").ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            // tags have been copied from invariant to en-US
            tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (ITag tag in TagService.GetAllTags("*"))
            {
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            }

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
            ILanguage language = new LanguageBuilder()
                .WithCultureInfo("fr-FR")
                .Build();
            LocalizationService.Save(language); // en-US is already there

            int enId = LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            ContentService.SaveAndPublish(content1);

            contentType.Variations = ContentVariation.Nothing;
            ContentTypeService.Save(contentType);

            // changes
            content1 = ContentService.GetById(content1.Id);

            // property value has been moved from en-US to invariant, fr-FR tags are gone
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "fr-FR"));
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US"));

            string[] tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer).ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            // tags have been copied from en-US to invariant, fr-FR tags are gone
            IEnumerable<IGrouping<int?, ITag>> tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (ITag tag in TagService.GetAllTags("*"))
            {
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            }

            Assert.AreEqual(1, tagGroups.Count());

            IGrouping<int?, ITag> enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeInvariant2()
        {
            ILanguage language = new LanguageBuilder()
                .WithCultureInfo("fr-FR")
                .Build();
            LocalizationService.Save(language); // en-US is already there

            int enId = LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            PropertyType propertyType = CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            ContentService.SaveAndPublish(content1);

            IContent content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.SetCultureName("name-fr", "fr-FR");
            content2.SetCultureName("name-en", "en-US");
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            ContentService.SaveAndPublish(content2);

            //// pretend we already have invariant values
            // using (var scope = ScopeProvider.CreateScope())
            // {
            //    scope.Database.Execute("INSERT INTO [cmsTags] ([tag], [group], [languageId]) SELECT DISTINCT [tag], [group], NULL FROM [cmsTags] WHERE [languageId] IS NOT NULL");
            // }

            // this should work
            propertyType.Variations = ContentVariation.Nothing;
            Assert.DoesNotThrow(() => ContentTypeService.Save(contentType));
        }

        [Test]
        public void TagsCanBecomeInvariantByPropertyType()
        {
            ILanguage language = new LanguageBuilder()
                .WithCultureInfo("fr-FR")
                .Build();
            LocalizationService.Save(language); // en-US is already there

            int enId = LocalizationService.GetLanguageIdByIsoCode("en-US").Value;

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            PropertyType propertyType = CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            ContentService.SaveAndPublish(content1);

            propertyType.Variations = ContentVariation.Nothing;
            ContentTypeService.Save(contentType);

            // changes
            content1 = ContentService.GetById(content1.Id);

            // property value has been moved from en-US to invariant, fr-FR tags are gone
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "fr-FR"));
            Assert.IsEmpty(content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US"));

            string[] tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer).ToArray();
            Assert.AreEqual(4, tags.Length);
            Assert.Contains("one", tags);
            Assert.AreEqual(-1, tags.IndexOf("plus"));

            // tags have been copied from en-US to invariant, fr-FR tags are gone
            IEnumerable<IGrouping<int?, ITag>> tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
            foreach (ITag tag in TagService.GetAllTags("*"))
            {
                Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
            }

            Assert.AreEqual(1, tagGroups.Count());

            IGrouping<int?, ITag> enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
            Assert.IsNotNull(enTagGroup);
            Assert.AreEqual(4, enTagGroup.Count());
            Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
            Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
        }

        [Test]
        public void TagsCanBecomeInvariantByPropertyTypeAndBackToVariant()
        {
            ILanguage language = new LanguageBuilder()
                .WithCultureInfo("fr-FR")
                .Build();
            LocalizationService.Save(language); // en-US is already there

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            PropertyType propertyType = CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
            ContentTypeService.Save(contentType);

            IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.SetCultureName("name-fr", "fr-FR");
            content1.SetCultureName("name-en", "en-US");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "another", "one" }, culture: "en-US");
            ContentService.SaveAndPublish(content1);

            propertyType.Variations = ContentVariation.Nothing;
            ContentTypeService.Save(contentType);

            // FIXME: This throws due to index violations
            propertyType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            // TODO: Assert results
        }

        [Test]
        public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_One()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            Content content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" });
            ContentService.SaveAndPublish(content1);

            Content content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content2);

            // verify
            IEnumerable<ITag> tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());
            IEnumerable<ITag> allTags = TagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());

            ContentService.MoveToRecycleBin(content1);
        }

        [Test]
        public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_All()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            Content content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "bam" });
            ContentService.SaveAndPublish(content1);

            Content content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content2);

            // verify
            IEnumerable<ITag> tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());
            IEnumerable<ITag> allTags = TagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());

            ContentService.Unpublish(content1);
            ContentService.Unpublish(content2);
        }

        [Test]
        [Ignore("https://github.com/umbraco/Umbraco-CMS/issues/3821 (U4-8442), will need to be fixed.")]
        public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_Tree()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            Content content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "plus" });
            ContentService.SaveAndPublish(content1);

            Content content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", content1.Id);
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content2);

            // verify
            IEnumerable<ITag> tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());
            IEnumerable<ITag> allTags = TagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());

            ContentService.MoveToRecycleBin(content1);

            // no more tags
            tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());
            tags = TagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(0, tags.Count());

            // no more tags
            allTags = TagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());

            ContentService.Move(content1, -1);

            Assert.IsFalse(content1.Published);

            // no more tags
            tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());
            tags = TagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(0, tags.Count());

            // no more tags
            allTags = TagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());

            content1.PublishCulture(CultureImpact.Invariant);
            ContentService.SaveAndPublish(content1);

            Assert.IsTrue(content1.Published);

            // tags are back
            tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(5, tags.Count());

            // FIXME: tag & tree issue
            // when we publish, we 'just' publish the top one and not the ones below = fails
            // what we should do is... NOT clear tags when unpublishing or trashing or...
            // and just update the tag service to NOT return anything related to trashed or
            // unpublished entities (since trashed is set on ALL entities in the trashed branch)
            tags = TagService.GetTagsForEntity(content2.Id); // including that one!
            Assert.AreEqual(4, tags.Count());

            // tags are back
            allTags = TagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());
        }

        [Test]
        public void TagsAreUpdatedWhenContentIsUnpublishedAndRePublished()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            Content content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "bam" });
            ContentService.SaveAndPublish(content1);

            Content content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", -1);
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content2);

            ContentService.Unpublish(content1);
            ContentService.Unpublish(content2);
        }

        [Test]
        [Ignore("https://github.com/umbraco/Umbraco-CMS/issues/3821 (U4-8442), will need to be fixed.")]
        public void TagsAreUpdatedWhenContentIsUnpublishedAndRePublished_Tree()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            Content content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1", -1);
            content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags", "bam" });
            ContentService.SaveAndPublish(content1);

            Content content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", content1);
            content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content2);

            ContentService.Unpublish(content1);

            IEnumerable<ITag> tags = TagService.GetTagsForEntity(content1.Id);
            Assert.AreEqual(0, tags.Count());

            // FIXME: tag & tree issue
            // when we (un)publish, we 'just' publish the top one and not the ones below = fails
            // see similar note above
            tags = TagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(0, tags.Count());
            IEnumerable<ITag> allTags = TagService.GetAllContentTags();
            Assert.AreEqual(0, allTags.Count());

            content1.PublishCulture(CultureImpact.Invariant);
            ContentService.SaveAndPublish(content1);

            tags = TagService.GetTagsForEntity(content2.Id);
            Assert.AreEqual(4, tags.Count());
            allTags = TagService.GetAllContentTags();
            Assert.AreEqual(5, allTags.Count());
        }

        [Test]
        public void Create_Tag_Data_Bulk_Publish_Operation()
        {
            // Arrange
            // set configuration
            IDataType dataType = DataTypeService.GetDataType(1041);
            dataType.Configuration = new TagConfiguration
            {
                Group = "test",
                StorageType = TagsStorageType.Csv
            };

            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias) };

            Content content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.Save(content);

            Content child1 = ContentBuilder.CreateSimpleContent(contentType, "child 1 content", content.Id);
            child1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello1", "world1", "some1" });
            ContentService.Save(child1);

            Content child2 = ContentBuilder.CreateSimpleContent(contentType, "child 2 content", content.Id);
            child2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello2", "world2" });
            ContentService.Save(child2);

            // Act
            ContentService.SaveAndPublishBranch(content, true);

            // Assert
            int propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;

            using (IScope scope = ScopeProvider.CreateScope())
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
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            // create content type with a tag property
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            // create a content with tags and publish
            Content content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content);

            // edit tags and save
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "another", "world" }, merge: true);
            ContentService.Save(content);

            // the (edit) property does contain all tags
            Assert.AreEqual(5, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());

            // but the database still contains the initial two tags
            int propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (IScope scope = ScopeProvider.CreateScope())
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
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            // Arrange
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);

            Content content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content", -1);

            // Act
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content);

            // Assert
            Assert.AreEqual(4, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
            int propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (IScope scope = ScopeProvider.CreateScope())
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
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            // Arrange
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);
            Content content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content);

            // Act
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "another", "world" }, merge: true);
            ContentService.SaveAndPublish(content);

            // Assert
            Assert.AreEqual(5, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
            int propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (IScope scope = ScopeProvider.CreateScope())
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
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            // Arrange
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", mandatoryProperties: true, defaultTemplateId: template.Id);
            CreateAndAddTagsPropertyType(contentType);
            ContentTypeService.Save(contentType);
            Content content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content", -1);
            content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello", "world", "some", "tags" });
            ContentService.SaveAndPublish(content);

            // Act
            content.RemoveTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "some", "world" });
            ContentService.SaveAndPublish(content);

            // Assert
            Assert.AreEqual(2, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
            int propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                Assert.AreEqual(2, scope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content.Id, propTypeId = propertyTypeId }));

                scope.Complete();
            }
        }

        private PropertyType CreateAndAddTagsPropertyType(ContentType contentType, ContentVariation variations = ContentVariation.Nothing)
        {
            PropertyType propertyType = new PropertyTypeBuilder()
                .WithPropertyEditorAlias("test")
                .WithAlias("tags")
                .WithDataTypeId(1041)
                .WithVariations(variations)
                .Build();
            contentType.PropertyGroups.First().PropertyTypes.Add(propertyType);
            contentType.Variations = variations;
            return propertyType;
        }
    }
}
