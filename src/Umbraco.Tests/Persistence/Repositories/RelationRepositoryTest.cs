using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RelationRepositoryTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            CreateTestData();
        }

        private RelationRepository CreateRepository(IScopeProvider provider, out RelationTypeRepository relationTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Mock.Of<ILogger<RelationTypeRepository>>());
            var entityRepository = new EntityRepository(accessor);
            var repository = new RelationRepository(accessor, Mock.Of<ILogger<RelationRepository>>(), relationTypeRepository, entityRepository);
            return repository;
        }

        [Test]
        public void Can_Perform_Add_On_RelationRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var relationType = repositoryType.Get(1);
                var relation = new Relation(NodeDto.NodeIdSeed + 2, NodeDto.NodeIdSeed + 3, relationType);
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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var relation = repository.Get(1);
                relation.Comment = "This relation has been updated";
                repository.Save(relation);


                var relationUpdated = repository.Get(1);

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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var relation = repository.Get(2);
                repository.Delete(relation);


                var exists = repository.Exists(2);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_RelationRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var relation = repository.Get(1);

                // Assert
                Assert.That(relation, Is.Not.Null);
                Assert.That(relation.HasIdentity, Is.True);
                Assert.That(relation.ChildId, Is.EqualTo(NodeDto.NodeIdSeed + 3));
                Assert.That(relation.ParentId, Is.EqualTo(NodeDto.NodeIdSeed + 2));
                Assert.That(relation.RelationType.Alias, Is.EqualTo("relateContentOnCopy"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_RelationRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var relations = repository.GetMany();

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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var relations = repository.GetMany(1, 2);

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
            CreateTestDataForPagingTests(out var createdContent, out var createdMembers, out var createdMedia);

            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out var relationTypeRepository);

                // Get parent entities for child id
                var parents = repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 11, out var totalRecords).ToList();
                Assert.AreEqual(20, totalRecords);
                Assert.AreEqual(11, parents.Count);

                //add the next page
                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 1, 11, out totalRecords));
                Assert.AreEqual(20, totalRecords);
                Assert.AreEqual(20, parents.Count);

                var contentEntities = parents.OfType<IDocumentEntitySlim>().ToList();
                var mediaEntities = parents.OfType<IMediaEntitySlim>().ToList();
                var memberEntities = parents.OfType<IMemberEntitySlim>().ToList();

                Assert.AreEqual(10, contentEntities.Count);
                Assert.AreEqual(0, mediaEntities.Count);
                Assert.AreEqual(10, memberEntities.Count);

                //only of a certain type
                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Document.GetGuid()));
                Assert.AreEqual(10, totalRecords);

                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Member.GetGuid()));
                Assert.AreEqual(10, totalRecords);

                parents.AddRange(repository.GetPagedParentEntitiesByChildId(createdMedia[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Media.GetGuid()));
                Assert.AreEqual(0, totalRecords);
            }
        }

        [Test]
        public void Get_Paged_Parent_Child_Entities_With_Same_Entity_Relation()
        {
            //Create a media item and create a relationship between itself (parent -> child)
            var imageType = MockedContentTypes.CreateImageMediaType("myImage");
            ServiceContext.MediaTypeService.Save(imageType);
            var media = MockedMedia.CreateMediaImage(imageType, -1);
            ServiceContext.MediaService.Save(media);
            var relType = ServiceContext.RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);
            ServiceContext.RelationService.Relate(media.Id, media.Id, relType);

            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out var relationTypeRepository);

                // Get parent entities for child id
                var parents = repository.GetPagedParentEntitiesByChildId(media.Id, 0, 10, out var totalRecords).ToList();
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
            CreateTestDataForPagingTests(out var createdContent, out var createdMembers, out var createdMedia);

            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider, out var relationTypeRepository);

                // Get parent entities for child id
                var parents = repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 0, 6, out var totalRecords).ToList();
                Assert.AreEqual(10, totalRecords);
                Assert.AreEqual(6, parents.Count);

                //add the next page
                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 1, 6, out totalRecords));
                Assert.AreEqual(10, totalRecords);
                Assert.AreEqual(10, parents.Count);

                var contentEntities = parents.OfType<IDocumentEntitySlim>().ToList();
                var mediaEntities = parents.OfType<IMediaEntitySlim>().ToList();
                var memberEntities = parents.OfType<IMemberEntitySlim>().ToList();

                Assert.AreEqual(0, contentEntities.Count);
                Assert.AreEqual(10, mediaEntities.Count);
                Assert.AreEqual(0, memberEntities.Count);

                //only of a certain type
                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Media.GetGuid()));
                Assert.AreEqual(10, totalRecords);

                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdMembers[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Media.GetGuid()));
                Assert.AreEqual(10, totalRecords);

                parents.AddRange(repository.GetPagedChildEntitiesByParentId(createdContent[0].Id, 0, 100, out totalRecords, UmbracoObjectTypes.Member.GetGuid()));
                Assert.AreEqual(0, totalRecords);
            }
        }

        private void CreateTestDataForPagingTests(out List<IContent> createdContent, out List<IMember> createdMembers, out List<IMedia> createdMedia)
        {
            //Create content
            createdContent = new List<IContent>();
            var contentType = MockedContentTypes.CreateBasicContentType("blah");
            ServiceContext.ContentTypeService.Save(contentType);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateBasicContent(contentType);
                ServiceContext.ContentService.Save(c1);
                createdContent.Add(c1);
            }

            //Create media
            createdMedia = new List<IMedia>();
            var imageType = MockedContentTypes.CreateImageMediaType("myImage");
            ServiceContext.MediaTypeService.Save(imageType);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageType, -1);
                ServiceContext.MediaService.Save(c1);
                createdMedia.Add(c1);
            }

            // Create members
            var memberType = MockedContentTypes.CreateSimpleMemberType("simple");
            ServiceContext.MemberTypeService.Save(memberType);
            createdMembers = MockedMember.CreateSimpleMember(memberType, 10).ToList();
            ServiceContext.MemberService.Save(createdMembers);

            var relType = ServiceContext.RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);

            // Relate content to media
            foreach (var content in createdContent)
                foreach (var media in createdMedia)
                    ServiceContext.RelationService.Relate(content.Id, media.Id, relType);
            // Relate members to media
            foreach (var member in createdMembers)
                foreach (var media in createdMedia)
                    ServiceContext.RelationService.Relate(member.Id, media.Id, relType);
        }

        [Test]
        public void Can_Perform_Exists_On_RelationRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var exists = repository.Exists(2);
                var doesntExist = repository.Exists(5);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Count_On_RelationRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var query = scope.SqlContext.Query<IRelation>().Where(x => x.ParentId == NodeDto.NodeIdSeed + 2);
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_RelationRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                // Act
                var query = scope.SqlContext.Query<IRelation>().Where(x => x.RelationTypeId == RelationTypeDto.NodeIdSeed);
                var relations = repository.Get(query);

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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                RelationTypeRepository repositoryType;
                var repository = CreateRepository(provider, out repositoryType);

                var content = ServiceContext.ContentService.GetById(NodeDto.NodeIdSeed + 3);
                ServiceContext.ContentService.Delete(content, 0);

                // Act
                var shouldntExist = repository.Exists(1);
                var shouldExist = repository.Exists(2);

                // Assert
                Assert.That(shouldntExist, Is.False);
                Assert.That(shouldExist, Is.True);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var relateContent = new RelationType(
                "Relate Content on Copy", "relateContentOnCopy", true,
                Constants.ObjectTypes.Document,
                new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"));

            var relateContentType = new RelationType("Relate ContentType on Copy",
                "relateContentTypeOnCopy",
                true,
                Constants.ObjectTypes.DocumentType,
                new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"));

            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                var accessor = (IScopeAccessor)provider;
                var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Mock.Of<ILogger<RelationTypeRepository>>());
                var entityRepository = new EntityRepository(accessor);
                var relationRepository = new RelationRepository(accessor, Mock.Of<ILogger<RelationRepository>>(), relationTypeRepository, entityRepository);

                relationTypeRepository.Save(relateContent);
                relationTypeRepository.Save(relateContentType);

                //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
                ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                ServiceContext.ContentTypeService.Save(contentType);

                //Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
                Content textpage = MockedContent.CreateSimpleContent(contentType);
                ServiceContext.ContentService.Save(textpage, 0);

                //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
                Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
                ServiceContext.ContentService.Save(subpage, 0);

                //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
                Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
                ServiceContext.ContentService.Save(subpage2, 0);

                var relation = new Relation(textpage.Id, subpage.Id, relateContent) { Comment = string.Empty };
                var relation2 = new Relation(textpage.Id, subpage2.Id, relateContent) { Comment = string.Empty };
                relationRepository.Save(relation);
                relationRepository.Save(relation2);
                scope.Complete();
            }
        }
    }
}
