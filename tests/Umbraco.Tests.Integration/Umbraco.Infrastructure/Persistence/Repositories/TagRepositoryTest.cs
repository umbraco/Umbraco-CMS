// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TagRepositoryTest : UmbracoIntegrationTest
    {
        private IFileService FileService => GetRequiredService<IFileService>();

        private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

        private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();

        private IMediaRepository MediaRepository => GetRequiredService<IMediaRepository>();

        private IMediaTypeRepository MediaTypeRepository => GetRequiredService<IMediaTypeRepository>();

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                TagRepository repository = CreateRepository(provider);

                var tag = new Tag
                {
                    Group = "Test",
                    Text = "Test"
                };

                repository.Save(tag);

                Assert.That(tag.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_Repository()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                TagRepository repository = CreateRepository(provider);

                var tag = new Tag
                {
                    Group = "Test",
                    Text = "Test"
                };

                repository.Save(tag);

                var tag2 = new Tag
                {
                    Group = "Test",
                    Text = "Test2"
                };

                repository.Save(tag2);

                Assert.That(tag.HasIdentity, Is.True);
                Assert.That(tag2.HasIdentity, Is.True);
                Assert.AreNotEqual(tag.Id, tag2.Id);
            }
        }

        [Test]
        public void Can_Create_Tag_Relations()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Assert.AreEqual(2, repository.GetTagsForEntity(content.Id).Count());
            }
        }

        [Test]
        public void Can_Append_Tag_Relations()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                    {
                        new Tag { Text = "tag3", Group = "test" },
                        new Tag { Text = "tag4", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags2,
                    false);

                Assert.AreEqual(4, repository.GetTagsForEntity(content.Id).Count());
            }
        }

        [Test]
        public void Can_Replace_Tag_Relations()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                    {
                        new Tag { Text = "tag3", Group = "test" },
                        new Tag { Text = "tag4", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags2,
                    true);

                ITag[] result = repository.GetTagsForEntity(content.Id).ToArray();
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual("tag3", result[0].Text);
                Assert.AreEqual("tag4", result[1].Text);
            }
        }

        [Test]
        public void Can_Merge_Tag_Relations()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                    {
                        new Tag { Text = "tag2", Group = "test" },
                        new Tag { Text = "tag3", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags2,
                    false);

                IEnumerable<ITag> result = repository.GetTagsForEntity(content.Id);
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Clear_Tag_Relations()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    Enumerable.Empty<ITag>(),
                    true);

                IEnumerable<ITag> result = repository.GetTagsForEntity(content.Id);
                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void Can_Remove_Specific_Tags_From_Property()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                        new Tag { Text = "tag3", Group = "test" },
                        new Tag { Text = "tag4", Group = "test" }
                    };
                repository.Assign(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tagsToRemove = new[]
                    {
                        new Tag { Text = "tag2", Group = "test" },
                        new Tag { Text = "tag3", Group = "test" }
                    };
                repository.Remove(
                    content.Id,
                    contentType.PropertyTypes.First().Id,
                    tagsToRemove);

                ITag[] result = repository.GetTagsForEntity(content.Id).ToArray();
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual("tag1", result[0].Text);
                Assert.AreEqual("tag4", result[1].Text);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_By_Id()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);
                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                        new Tag { Text = "tag3", Group = "test" },
                        new Tag { Text = "tag4", Group = "test" }
                    };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" }
                    };
                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.First().Id,
                    tags2,
                    false);

                IEnumerable<ITag> result = repository.GetTagsForEntity(content2.Id);
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_By_Key()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);
                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                        new Tag { Text = "tag3", Group = "test" },
                        new Tag { Text = "tag4", Group = "test" }
                    };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" }
                    };
                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.First().Id,
                    tags2,
                    false);

                // get by key
                IEnumerable<ITag> result = repository.GetTagsForEntity(content2.Key);
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_All()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);
                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                    {
                        new Tag { Text = "tag1", Group = "test" },
                        new Tag { Text = "tag2", Group = "test" },
                        new Tag { Text = "tag3", Group = "test" },
                        new Tag { Text = "tag4", Group = "test" }
                    };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                IEnumerable<ITag> result = repository.GetMany();
                Assert.AreEqual(4, result.Count());
            }
        }

        [Test]
        public void Can_Get_All_With_Ids()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);
                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                // TODO: This would be nice to be able to map the ids back but unfortunately we are not doing this
                // var result = repository.GetAll(new[] {tags[0].Id, tags[1].Id, tags[2].Id});
                ITag[] all = repository.GetMany().ToArray();

                IEnumerable<ITag> result = repository.GetMany(all[0].Id, all[1].Id, all[2].Id);
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Get_Tags_For_Content_For_Group()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);
                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test1" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" }
                };
                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.First().Id,
                    tags2,
                    false);

                IEnumerable<ITag> result = repository.GetTagsForEntity(content1.Id, "test1");
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_Tags_For_Property_By_Id()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                ITag[] result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias).ToArray();
                ITag[] result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias).ToArray();
                Assert.AreEqual(4, result1.Length);
                Assert.AreEqual(2, result2.Length);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Property_By_Key()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                ITag[] result1 = repository.GetTagsForProperty(content1.Key, contentType.PropertyTypes.First().Alias).ToArray();
                ITag[] result2 = repository.GetTagsForProperty(content1.Key, contentType.PropertyTypes.Last().Alias).ToArray();
                Assert.AreEqual(4, result1.Length);
                Assert.AreEqual(2, result2.Length);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Property_For_Group()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test1" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                ITag[] result1 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.First().Alias, "test1").ToArray();
                ITag[] result2 = repository.GetTagsForProperty(content1.Id, contentType.PropertyTypes.Last().Alias, "test1").ToArray();

                Assert.AreEqual(2, result1.Length);
                Assert.AreEqual(1, result2.Length);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("image2");
                MediaTypeRepository.Save(mediaType);

                Media media1 = MediaBuilder.CreateMediaImage(mediaType, -1);
                MediaRepository.Save(media1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                    new Tag { Text = "tag3", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag4", Group = "test1" }
                };
                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                ITag[] result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content).ToArray();
                ITag[] result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media).ToArray();
                ITag[] result3 = repository.GetTagsForEntityType(TaggableObjectTypes.All).ToArray();

                Assert.AreEqual(3, result1.Length);
                Assert.AreEqual(2, result2.Length);
                Assert.AreEqual(4, result3.Length);

                Assert.AreEqual(1, result1.Single(x => x.Text == "tag1").NodeCount);
                Assert.AreEqual(2, result3.Single(x => x.Text == "tag1").NodeCount);
                Assert.AreEqual(1, result3.Single(x => x.Text == "tag4").NodeCount);
            }
        }

        [Test]
        public void Can_Get_Tags_For_Entity_Type_For_Group()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("image2");
                MediaTypeRepository.Save(mediaType);

                Media media1 = MediaBuilder.CreateMediaImage(mediaType, -1);
                MediaRepository.Save(media1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test1" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" }
                };
                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                ITag[] result1 = repository.GetTagsForEntityType(TaggableObjectTypes.Content,  "test1").ToArray();
                ITag[] result2 = repository.GetTagsForEntityType(TaggableObjectTypes.Media, "test1").ToArray();

                Assert.AreEqual(2, result1.Length);
                Assert.AreEqual(1, result2.Length);
            }
        }

        [Test]
        public void Cascade_Deletes_Tag_Relations()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test" },
                    new Tag { Text = "tag3", Group = "test" },
                    new Tag { Text = "tag4", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                DocumentRepository.Delete(content1);

                Assert.AreEqual(0, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                    new { nodeId = content1.Id, propTypeId = contentType.PropertyTypes.First().Id }));
            }
        }

        [Test]
        public void Can_Get_Tagged_Entities_For_Tag_Group()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("image2");
                MediaTypeRepository.Save(mediaType);

                Media media1 = MediaBuilder.CreateMediaImage(mediaType, -1);
                MediaRepository.Save(media1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag {Text = "tag1", Group = "test"},
                    new Tag {Text = "tag2", Group = "test1"},
                    new Tag {Text = "tag3", Group = "test"}
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                    new Tag { Text = "tag3", Group = "test" }
                };
                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                Tag[] tags3 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" }
                };
                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    tags3,
                    false);

                TaggedEntity[] contentTestIds = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, "test").ToArray();

                // there are two content items tagged against the 'test' group
                Assert.AreEqual(2, contentTestIds.Length);

                // there are a total of two property types tagged against the 'test' group
                Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).Count());

                // there are a total of 2 tags tagged against the 'test' group
                Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                TaggedEntity[] contentTest1Ids = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, "test1").ToArray();

                // there are two content items tagged against the 'test1' group
                Assert.AreEqual(2, contentTest1Ids.Length);

                // there are a total of two property types tagged against the 'test1' group
                Assert.AreEqual(2, contentTest1Ids.SelectMany(x => x.TaggedProperties).Count());

                // there are a total of 1 tags tagged against the 'test1' group
                Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                IEnumerable<TaggedEntity> mediaTestIds = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Media, "test");
                Assert.AreEqual(1, mediaTestIds.Count());

                IEnumerable<TaggedEntity> mediaTest1Ids = repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Media, "test1");
                Assert.AreEqual(1, mediaTest1Ids.Count());
            }
        }

        [Test]
        public void Can_Get_Tagged_Entities_For_Tag()
        {
            IScopeProvider provider = ScopeProvider;
            using (ScopeProvider.CreateScope())
            {
                // create data to relate to
                // We have to create and save a template, otherwise we get an FK violation on contentType.
                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test", "Test", defaultTemplateId: template.Id);
                ContentTypeRepository.Save(contentType);

                Content content1 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content1);

                Content content2 = ContentBuilder.CreateSimpleContent(contentType);
                DocumentRepository.Save(content2);

                MediaType mediaType = MediaTypeBuilder.CreateImageMediaType("image2");
                MediaTypeRepository.Save(mediaType);

                Media media1 = MediaBuilder.CreateMediaImage(mediaType, -1);
                MediaRepository.Save(media1);

                TagRepository repository = CreateRepository(provider);
                Tag[] tags = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                    new Tag { Text = "tag3", Group = "test" }
                };
                repository.Assign(
                    content1.Id,
                    contentType.PropertyTypes.First().Id,
                    tags,
                    false);

                Tag[] tags2 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" },
                };
                repository.Assign(
                    content2.Id,
                    contentType.PropertyTypes.Last().Id,
                    tags2,
                    false);

                Tag[] tags3 = new[]
                {
                    new Tag { Text = "tag1", Group = "test" },
                    new Tag { Text = "tag2", Group = "test1" }
                };
                repository.Assign(
                    media1.Id,
                    mediaType.PropertyTypes.Last().Id,
                    tags3,
                    false);

                TaggedEntity[] contentTestIds = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, "tag1").ToArray();

                // there are two content items tagged against the 'tag1' tag
                Assert.AreEqual(2, contentTestIds.Length);

                // there are a total of two property types tagged against the 'tag1' tag
                Assert.AreEqual(2, contentTestIds.SelectMany(x => x.TaggedProperties).Count());

                // there are a total of 1 tags since we're only looking against one tag
                Assert.AreEqual(1, contentTestIds.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                TaggedEntity[] contentTest1Ids = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, "tag3").ToArray();

                // there are 1 content items tagged against the 'tag3' tag
                Assert.AreEqual(1, contentTest1Ids.Length);

                // there are a total of two property types tagged against the 'tag3' tag
                Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).Count());

                // there are a total of 1 tags since we're only looking against one tag
                Assert.AreEqual(1, contentTest1Ids.SelectMany(x => x.TaggedProperties).SelectMany(x => x.Tags).Select(x => x.Id).Distinct().Count());

                IEnumerable<TaggedEntity> mediaTestIds = repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Media, "tag1");
                Assert.AreEqual(1, mediaTestIds.Count());
            }
        }

        private TagRepository CreateRepository(IScopeProvider provider) =>
            new TagRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<TagRepository>());
    }
}
