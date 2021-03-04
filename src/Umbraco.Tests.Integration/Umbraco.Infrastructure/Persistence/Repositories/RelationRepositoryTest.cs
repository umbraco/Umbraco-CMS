// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RelationRepositoryTest : UmbracoIntegrationTest
    {
        private RelationType _relateContent;
        private RelationType _relateContentType;
        private ContentType _contentType;
        private Content _textpage;
        private Content _subpage;
        private Content _subpage2;
        private Relation _relation;
        private Relation _relation2;

        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        private IContentService ContentService => GetRequiredService<IContentService>();

        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

        private IMemberService GetMemberService() => GetRequiredService<IMemberService>();

        private IRelationService RelationService => GetRequiredService<IRelationService>();

        private IFileService FileService => GetRequiredService<IFileService>();

        [SetUp]
        public void SetUp() => CreateTestData();

        private RelationRepository CreateRepository(IScopeProvider provider, out RelationTypeRepository relationTypeRepository)
        {
            relationTypeRepository = (RelationTypeRepository)GetRequiredService<IRelationTypeRepository>();
            return (RelationRepository)GetRequiredService<IRelationRepository>();
        }

        [Test]
        public void Can_Perform_Add_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IRelationType relationType = repositoryType.Get(1);
                var relation = new Relation(_textpage.Id, _subpage.Id, relationType);
                repository.Save(relation);

                // Assert
                Assert.That(relation, Is.Not.Null);
                Assert.That(relation.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IRelation relation = repository.Get(1);
                relation.Comment = "This relation has been updated";
                repository.Save(relation);

                IRelation relationUpdated = repository.Get(1);

                // Assert
                Assert.That(relationUpdated, Is.Not.Null);
                Assert.That(relationUpdated.Comment, Is.EqualTo("This relation has been updated"));
                Assert.AreNotEqual(relationUpdated.UpdateDate, relation.UpdateDate);
            }
        }

        [Test]
        public void Can_Perform_Delete_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IRelation relation = repository.Get(2);
                repository.Delete(relation);

                bool exists = repository.Exists(2);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IRelation relation = repository.Get(1);

                // Assert
                Assert.That(relation, Is.Not.Null);
                Assert.That(relation.HasIdentity, Is.True);
                Assert.That(relation.ChildId, Is.EqualTo(_subpage.Id));
                Assert.That(relation.ParentId, Is.EqualTo(_textpage.Id));
                Assert.That(relation.RelationType.Alias, Is.EqualTo("relateContentOnCopy"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IEnumerable<IRelation> relations = repository.GetMany();

                // Assert
                Assert.That(relations, Is.Not.Null);
                Assert.That(relations.Any(), Is.True);
                Assert.That(relations.Any(x => x == null), Is.False);
                Assert.That(relations.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IEnumerable<IRelation> relations = repository.GetMany(1, 2);

                // Assert
                Assert.That(relations, Is.Not.Null);
                Assert.That(relations.Any(), Is.True);
                Assert.That(relations.Any(x => x == null), Is.False);
                Assert.That(relations.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Get_Paged_Parent_Entities_By_Child_Id()
        {
            CreateTestDataForPagingTests(out List<IContent> createdContent, out List<IMember> createdMembers, out List<IMedia> createdMedia);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository relationTypeRepository);

                // Get parent entities for child id
                var parents = repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 11, out long totalRecords).ToList();
                Assert.AreEqual(6, totalRecords);
                Assert.AreEqual(6, parents.Count);

                // add the next page
                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 1, 11, out totalRecords));
                Assert.AreEqual(6, totalRecords);
                Assert.AreEqual(6, parents.Count);

                var contentEntities = parents.OfType<IDocumentEntitySlim>().ToList();
                var mediaEntities = parents.OfType<IMediaEntitySlim>().ToList();
                var memberEntities = parents.OfType<IMemberEntitySlim>().ToList();

                Assert.AreEqual(3, contentEntities.Count);
                Assert.AreEqual(0, mediaEntities.Count);
                Assert.AreEqual(3, memberEntities.Count);

                // only of a certain type
                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Document.GetGuid()));
                Assert.AreEqual(3, totalRecords);

                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Member.GetGuid()));
                Assert.AreEqual(3, totalRecords);

                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Media.GetGuid()));
                Assert.AreEqual(0, totalRecords);
            }
        }

        [Test]
        public void Get_Paged_Parent_Child_Entities_With_Same_Entity_Relation()
        {
            // Create a media item and create a relationship between itself (parent -> child)
            MediaType imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
            MediaTypeService.Save(imageType);
            Media media = MediaBuilder.CreateMediaImage(imageType, -1);
            MediaService.Save(media);
            IRelationType relType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);
            RelationService.Relate(media.Id, media.Id, relType);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository relationTypeRepository);

                // Get parent entities for child id
                var parents = repository.GetPagedParentEntitiesByChildId(media.Id, 0, 10, out long totalRecords).ToList();
                Assert.AreEqual(1, totalRecords);
                Assert.AreEqual(1, parents.Count);

                // Get child entities for parent id
                var children = repository.GetPagedChildEntitiesByParentId(media.Id, 0, 10, out totalRecords).ToList();
                Assert.AreEqual(1, totalRecords);
                Assert.AreEqual(1, children.Count);
            }
        }

        [Test]
        public void Get_Paged_Child_Entities_By_Parent_Id()
        {
            CreateTestDataForPagingTests(out List<IContent> createdContent, out List<IMember> createdMembers, out _);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out _);

                // Get parent entities for child id
                var parents = repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 0, 6, out long totalRecords).ToList();
                Assert.AreEqual(3, totalRecords);
                Assert.AreEqual(3, parents.Count);

                // add the next page
                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 1, 6, out totalRecords));
                Assert.AreEqual(3, totalRecords);
                Assert.AreEqual(3, parents.Count);

                var contentEntities = parents.OfType<IDocumentEntitySlim>().ToList();
                var mediaEntities = parents.OfType<IMediaEntitySlim>().ToList();
                var memberEntities = parents.OfType<IMemberEntitySlim>().ToList();

                Assert.AreEqual(0, contentEntities.Count);
                Assert.AreEqual(3, mediaEntities.Count);
                Assert.AreEqual(0, memberEntities.Count);

                // only of a certain type
                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Media.GetGuid()));
                Assert.AreEqual(3, totalRecords);

                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdMembers[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Media.GetGuid()));
                Assert.AreEqual(3, totalRecords);

                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Member.GetGuid()));
                Assert.AreEqual(0, totalRecords);
            }
        }

        private void CreateTestDataForPagingTests(out List<IContent> createdContent, out List<IMember> createdMembers, out List<IMedia> createdMedia)
        {
            // Create content
            createdContent = new List<IContent>();
            ContentType contentType = ContentTypeBuilder.CreateBasicContentType("blah");
            ContentTypeService.Save(contentType);
            for (int i = 0; i < 3; i++)
            {
                Content c1 = ContentBuilder.CreateBasicContent(contentType);
                ContentService.Save(c1);
                createdContent.Add(c1);
            }

            // Create media
            createdMedia = new List<IMedia>();
            MediaType imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
            MediaTypeService.Save(imageType);
            for (int i = 0; i < 3; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageType, -1);
                MediaService.Save(c1);
                createdMedia.Add(c1);
            }

            // Create members
            MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("simple");
            MemberTypeService.Save(memberType);
            createdMembers = MemberBuilder.CreateSimpleMembers(memberType, 3).ToList();
            GetMemberService().Save(createdMembers);

            IRelationType relType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);

            // Relate content to media
            foreach (IContent content in createdContent)
            {
                foreach (IMedia media in createdMedia)
                {
                    RelationService.Relate(content.Id, media.Id, relType);
                }
            }

            // Relate members to media
            foreach (IMember member in createdMembers)
            {
                foreach (IMedia media in createdMedia)
                {
                    RelationService.Relate(member.Id, media.Id, relType);
                }
            }
        }

        [Test]
        public void Can_Perform_Exists_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                bool exists = repository.Exists(2);
                bool doesntExist = repository.Exists(5);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Count_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IQuery<IRelation> query = scope.SqlContext.Query<IRelation>().Where(x => x.ParentId == _textpage.Id);
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_RelationRepository()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                // Act
                IQuery<IRelation> query = scope.SqlContext.Query<IRelation>().Where(x => x.RelationTypeId == _relateContent.Id);
                IEnumerable<IRelation> relations = repository.Get(query);

                // Assert
                Assert.That(relations, Is.Not.Null);
                Assert.That(relations.Any(), Is.True);
                Assert.That(relations.Any(x => x == null), Is.False);
                Assert.That(relations.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Delete_Content_And_Verify_Relation_Is_Removed()
        {
            // Arrange
            using (IScope scope = ScopeProvider.CreateScope())
            {
                RelationRepository repository = CreateRepository(ScopeProvider, out RelationTypeRepository repositoryType);

                IContent content = ContentService.GetById(_subpage.Id);
                ContentService.Delete(content, 0);

                // Act
                bool shouldntExist = repository.Exists(1);
                bool shouldExist = repository.Exists(2);

                // Assert
                Assert.That(shouldntExist, Is.False);
                Assert.That(shouldExist, Is.True);
            }
        }

        public void CreateTestData()
        {
            _relateContent = new RelationType(
                "Relate Content on Copy",
                "relateContentOnCopy",
                true,
                Constants.ObjectTypes.Document,
                new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"));

            _relateContentType = new RelationType(
                "Relate ContentType on Copy",
                "relateContentTypeOnCopy",
                true,
                Constants.ObjectTypes.DocumentType,
                new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"));

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var accessor = (IScopeAccessor)ScopeProvider;
                var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Mock.Of<ILogger<RelationTypeRepository>>());
                var entityRepository = new EntityRepository(accessor, AppCaches.Disabled);
                var relationRepository = new RelationRepository(accessor, Mock.Of<ILogger<RelationRepository>>(), relationTypeRepository, entityRepository);

                relationTypeRepository.Save(_relateContent);
                relationTypeRepository.Save(_relateContentType);

                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template);

                // Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
                _contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);

                ContentTypeService.Save(_contentType);

                // Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
                _textpage = ContentBuilder.CreateSimpleContent(_contentType);
                ContentService.Save(_textpage, 0);

                // Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
                _subpage = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 1", _textpage.Id);
                ContentService.Save(_subpage, 0);

                // Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
                _subpage2 = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 2", _textpage.Id);
                ContentService.Save(_subpage2, 0);

                _relation = new Relation(_textpage.Id, _subpage.Id, _relateContent) { Comment = string.Empty };
                _relation2 = new Relation(_textpage.Id, _subpage2.Id, _relateContent) { Comment = string.Empty };
                relationRepository.Save(_relation);
                relationRepository.Save(_relation2);
                scope.Complete();
            }
        }
    }
}
