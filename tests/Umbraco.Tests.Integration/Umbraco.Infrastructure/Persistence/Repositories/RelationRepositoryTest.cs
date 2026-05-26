// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RelationRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetUp() => await CreateTestDataAsync();

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

    private RelationRepository CreateRepository(out RelationTypeRepository relationTypeRepository)
    {
        relationTypeRepository = (RelationTypeRepository)GetRequiredService<IRelationTypeRepository>();
        return (RelationRepository)GetRequiredService<IRelationRepository>();
    }

    [Test]
    public async Task Can_Perform_Add_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relationType = await repositoryType.GetAsync(1, CancellationToken.None);
            var relation = new Relation(_textpage.Id, _subpage.Id, relationType);
            await repository.SaveAsync(relation, CancellationToken.None);

            // Assert
            Assert.That(relation, Is.Not.Null);
            Assert.That(relation.HasIdentity, Is.True);
        }
    }

    [Test]
    public async Task Can_Perform_Update_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relation = await repository.GetAsync(1, CancellationToken.None);
            relation.Comment = "This relation has been updated";
            await repository.SaveAsync(relation, CancellationToken.None);

            var relationUpdated = await repository.GetAsync(1, CancellationToken.None);

            // Assert
            Assert.That(relationUpdated, Is.Not.Null);
            Assert.That(relationUpdated.Comment, Is.EqualTo("This relation has been updated"));
            Assert.AreNotEqual(relationUpdated.UpdateDate, relation.UpdateDate);
        }
    }

    [Test]
    public async Task Can_Perform_Delete_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relation = await repository.GetAsync(2, CancellationToken.None);
            await repository.DeleteAsync(relation, CancellationToken.None);

            var exists = await repository.ExistsAsync(2, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public async Task Can_Perform_Get_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relation = await repository.GetAsync(1, CancellationToken.None);

            // Assert
            Assert.That(relation, Is.Not.Null);
            Assert.That(relation.HasIdentity, Is.True);
            Assert.That(relation.ChildId, Is.EqualTo(_subpage.Id));
            Assert.That(relation.ParentId, Is.EqualTo(_textpage.Id));
            Assert.That(relation.RelationType.Alias, Is.EqualTo("relateContentOnCopy"));
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relations = (await repository.GetAllAsync(CancellationToken.None)).ToArray();

            // Assert
            Assert.That(relations, Is.Not.Null);
            Assert.That(relations.Any(), Is.True);
            Assert.That(relations.Any(x => x == null), Is.False);
            Assert.That(relations.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_With_Params_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relations = (await repository.GetManyAsync(new[] { 1, 2 }, CancellationToken.None)).ToArray();

            // Assert
            Assert.That(relations, Is.Not.Null);
            Assert.That(relations.Any(), Is.True);
            Assert.That(relations.Any(x => x == null), Is.False);
            Assert.That(relations.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    [Ignore("Deferred until EntityRepository is migrated to EF Core - GetPagedParentEntitiesByChildIdAsync throws NotImplementedException")]
    public async Task Get_Paged_Parent_Entities_By_Child_Id()
    {
        var (createdContent, createdMembers, createdMedia) = await CreateTestDataForPagingTestsAsync();

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var relationTypeRepository);

            // Get parent entities for child id
            var firstPage = await repository.GetPagedParentEntitiesByChildIdAsync(createdMedia[0].Id, 0, 11);
            Assert.AreEqual(9, firstPage.Total);
            Assert.AreEqual(9, firstPage.Items.Count());

            // Add the next page
            var secondPage = await repository.GetPagedParentEntitiesByChildIdAsync(createdMedia[0].Id, 11, 11);
            Assert.AreEqual(9, secondPage.Total);

            var allParents = firstPage.Items.Concat(secondPage.Items).ToList();
            var contentEntities = allParents.OfType<IDocumentEntitySlim>().ToList();
            var mediaEntities = allParents.OfType<IMediaEntitySlim>().ToList();
            var memberEntities = allParents.OfType<IMemberEntitySlim>().ToList();

            Assert.AreEqual(3, contentEntities.Count);
            Assert.AreEqual(3, mediaEntities.Count);
            Assert.AreEqual(3, memberEntities.Count);

            // Only of a certain type
            var documents = await repository.GetPagedParentEntitiesByChildIdAsync(createdMedia[0].Id, 0, 100, [UmbracoObjectTypes.Document.GetGuid()]);
            Assert.AreEqual(3, documents.Total);

            var members = await repository.GetPagedParentEntitiesByChildIdAsync(createdMedia[0].Id, 0, 100, [UmbracoObjectTypes.Member.GetGuid()]);
            Assert.AreEqual(3, members.Total);

            var media = await repository.GetPagedParentEntitiesByChildIdAsync(createdMedia[0].Id, 0, 100, [UmbracoObjectTypes.Media.GetGuid()]);
            Assert.AreEqual(3, media.Total);

            // Test relations on content
            var contentParents = await repository.GetPagedParentEntitiesByChildIdAsync(createdContent[0].Id, 0, int.MaxValue);
            Assert.AreEqual(6, contentParents.Total);
            Assert.AreEqual(6, contentParents.Items.Count());
        }
    }

    [Test]
    [Ignore("Deferred until EntityRepository is migrated to EF Core - GetPagedParentEntitiesByChildIdAsync/GetPagedChildEntitiesByParentIdAsync throw NotImplementedException")]
    public async Task Get_Paged_Parent_Child_Entities_With_Same_Entity_Relation()
    {
        // Create a media item and create a relationship between itself (parent -> child)
        var imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
        await MediaTypeService.CreateAsync(imageType, Constants.Security.SuperUserKey);
        var media = MediaBuilder.CreateMediaImage(imageType, -1);
        MediaService.Save(media);
        var relType = await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelatedMediaAlias);
        await RelationService.RelateAsync(media.Id, media.Id, relType!);

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var relationTypeRepository);

            // Get parent entities for child id
            var parents = await repository.GetPagedParentEntitiesByChildIdAsync(media.Id, 0, 10);
            Assert.AreEqual(1, parents.Total);
            Assert.AreEqual(1, parents.Items.Count());

            // Get child entities for parent id
            var children = await repository.GetPagedChildEntitiesByParentIdAsync(media.Id, 0, 10);
            Assert.AreEqual(1, children.Total);
            Assert.AreEqual(1, children.Items.Count());
        }
    }

    [Test]
    [Ignore("Deferred until EntityRepository is migrated to EF Core - GetPagedChildEntitiesByParentIdAsync throws NotImplementedException")]
    public async Task Get_Paged_Child_Entities_By_Parent_Id()
    {
        var (createdContent, createdMembers, _) = await CreateTestDataForPagingTestsAsync();

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out _);

            // Get child entities for parent id
            var firstPage = await repository.GetPagedChildEntitiesByParentIdAsync(createdContent[0].Id, 0, 9);
            Assert.AreEqual(9, firstPage.Total);
            Assert.AreEqual(9, firstPage.Items.Count());

            // Add the next page
            var secondPage = await repository.GetPagedChildEntitiesByParentIdAsync(createdContent[0].Id, 9, 9);
            Assert.AreEqual(9, secondPage.Total);

            var allChildren = firstPage.Items.Concat(secondPage.Items).ToList();
            var contentEntities = allChildren.OfType<IDocumentEntitySlim>().ToList();
            var mediaEntities = allChildren.OfType<IMediaEntitySlim>().ToList();
            var memberEntities = allChildren.OfType<IMemberEntitySlim>().ToList();

            Assert.AreEqual(3, contentEntities.Count);
            Assert.AreEqual(3, mediaEntities.Count);
            Assert.AreEqual(3, memberEntities.Count);

            // only of a certain type
            var media = await repository.GetPagedChildEntitiesByParentIdAsync(createdContent[0].Id, 0, 100, [UmbracoObjectTypes.Media.GetGuid()]);
            Assert.AreEqual(3, media.Total);

            var membersFromMembers = await repository.GetPagedChildEntitiesByParentIdAsync(createdMembers[0].Id, 0, 100, [UmbracoObjectTypes.Media.GetGuid()]);
            Assert.AreEqual(3, membersFromMembers.Total);

            var membersFromContent = await repository.GetPagedChildEntitiesByParentIdAsync(createdContent[0].Id, 0, 100, [UmbracoObjectTypes.Member.GetGuid()]);
            Assert.AreEqual(3, membersFromContent.Total);
        }
    }

    private async Task<(List<IContent> createdContent, List<IMember> createdMembers, List<IMedia> createdMedia)> CreateTestDataForPagingTestsAsync()
    {
        // Create content
        var createdContent = new List<IContent>();
        var createdMembers = new List<IMember>();
        var createdMedia = new List<IMedia>();
        var contentType = ContentTypeBuilder.CreateBasicContentType("blah");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        for (var i = 0; i < 3; i++)
        {
            var c1 = ContentBuilder.CreateBasicContent(contentType);
            ContentService.Save(c1);
            createdContent.Add(c1);
        }

        // Create related content
        var relatedContent = new List<IContent>();
        for (var i = 0; i < 3; i++)
        {
            var c1 = ContentBuilder.CreateBasicContent(contentType);
            ContentService.Save(c1);
            relatedContent.Add(c1);
        }

        // Create media
        var createdMedia = new List<IMedia>();
        var imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
        await MediaTypeService.CreateAsync(imageType, Constants.Security.SuperUserKey);
        for (var i = 0; i < 3; i++)
        {
            var c1 = MediaBuilder.CreateMediaImage(imageType, -1);
            MediaService.Save(c1);
            createdMedia.Add(c1);
        }

        // Create members
        var memberType = MemberTypeBuilder.CreateSimpleMemberType("simple");
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        var createdMembers.AddRange(MemberBuilder.CreateSimpleMembers(memberType, 3));
        GetMemberService().Save(createdMembers);

        var relatedMediaRelType =
            await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelatedMediaAlias);
        var relatedContentRelType =
            await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelatedDocumentAlias);
        var relatedMemberRelType =
            await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelatedMemberAlias);

        // Relate content to media
        foreach (var content in createdContent)
        {
            foreach (var media in createdMedia)
            {
                await RelationService.RelateAsync(content.Id, media.Id, relatedMediaRelType!);
            }
        }

        // Relate content to content
        foreach (var relContent in relatedContent)
        {
            foreach (var content in createdContent)
            {
                await RelationService.RelateAsync(relContent.Id, content.Id, relatedContentRelType!);
            }
        }

        // Relate content to member
        foreach (var content in createdContent)
        {
            foreach (var member in createdMembers)
            {
                await RelationService.RelateAsync(content.Id, member.Id, relatedMemberRelType!);
            }
        }

        // Relate members to media
        foreach (var member in createdMembers)
        {
            foreach (var media in createdMedia)
            {
                await RelationService.RelateAsync(member.Id, media.Id, relatedMediaRelType!);
            }
        }

        // Create copied content
        var copiedContent = new List<IContent>();
        for (var i = 0; i < 3; i++)
        {
            var c1 = ContentBuilder.CreateBasicContent(contentType);
            ContentService.Save(c1);
            copiedContent.Add(c1);
        }

        var copiedContentRelType =
            await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes.RelateDocumentOnCopyAlias);

        // Relate content to content (mimics copy)
        foreach (var content in createdContent)
        {
            foreach (var cpContent in copiedContent)
            {
                await RelationService.RelateAsync(content.Id, cpContent.Id, copiedContentRelType!);
            }
        }

        // Create trashed content
        var trashedContent = new List<IContent>();
        for (var i = 0; i < 3; i++)
        {
            var c1 = ContentBuilder.CreateBasicContent(contentType);
            ContentService.Save(c1);
            trashedContent.Add(c1);
        }

        var trashedRelType =
            await RelationService.GetRelationTypeByAliasAsync(
                Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias);

        // Relate to trashed content
        foreach (var trContent in trashedContent)
        {
            foreach (var content in createdContent)
            {
                await RelationService.RelateAsync(trContent.Id, content.Id, trashedRelType!);
            }
        }

        // Create trashed media
        var trashedMedia = new List<IMedia>();
        for (var i = 0; i < 3; i++)
        {
            var m1 = MediaBuilder.CreateMediaImage(imageType, -1);
            MediaService.Save(m1);
            trashedMedia.Add(m1);
        }

        var trashedMediaRelType =
            await RelationService.GetRelationTypeByAliasAsync(Constants.Conventions.RelationTypes
                .RelateParentMediaFolderOnDeleteAlias);

        // Relate to trashed media
        foreach (var trMedia in trashedMedia)
        {
            foreach (var media in createdMedia)
            {
                await RelationService.RelateAsync(trMedia.Id, media.Id, trashedMediaRelType!);
            }
        }

        return (createdContent, createdMembers, createdMedia);
    }

    [Test]
    public async Task Can_Perform_Exists_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var exists = await repository.ExistsAsync(2, CancellationToken.None);
            var doesntExist = await repository.ExistsAsync(5, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
        }
    }

    [Test]
    public async Task Can_Get_By_Parent_Id_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relations = (await repository.GetByParentIdAsync(_textpage.Id)).ToArray();

            // Assert
            Assert.That(relations.Length, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task Can_Get_By_Relation_Type_Id_On_RelationRepository()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            // Act
            var relations = (await repository.GetByRelationTypeIdAsync(_relateContent.Id)).ToArray();

            // Assert
            Assert.That(relations, Is.Not.Null);
            Assert.That(relations.Any(), Is.True);
            Assert.That(relations.Any(x => x == null), Is.False);
            Assert.That(relations.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public async Task Can_Delete_Content_And_Verify_Relation_Is_Removed()
    {
        // Arrange
        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = CreateRepository(out var repositoryType);

            var content = ContentService.GetById(_subpage.Id);
            ContentService.Delete(content, -1);

            // Act
            var shouldntExist = await repository.ExistsAsync(1, CancellationToken.None);
            var shouldExist = await repository.ExistsAsync(2, CancellationToken.None);

            // Assert
            Assert.That(shouldntExist, Is.False);
            Assert.That(shouldExist, Is.True);
        }
    }

    public async Task CreateTestDataAsync()
    {
        _relateContent = new RelationType(
            "Relate Content on Copy",
            "relateContentOnCopy",
            true,
            Constants.ObjectTypes.Document,
            new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"),
            false);

        _relateContentType = new RelationType(
            "Relate ContentType on Copy",
            "relateContentTypeOnCopy",
            true,
            Constants.ObjectTypes.DocumentType,
            new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"),
            false);

        using (var scope = ScopeProvider.CreateScope())
        {
            var relationTypeRepository = GetRequiredService<IRelationTypeRepository>();
            var relationRepository = GetRequiredService<IRelationRepository>();

            await relationTypeRepository.SaveAsync(_relateContent, CancellationToken.None);
            await relationTypeRepository.SaveAsync(_relateContentType, CancellationToken.None);

            var templateService = GetRequiredService<ITemplateService>();
            var template = TemplateBuilder.CreateTextPageTemplate();
            await templateService.CreateAsync(template, Constants.Security.SuperUserKey);

            // Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            _contentType =
                ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);

            await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

            // Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
            _textpage = ContentBuilder.CreateSimpleContent(_contentType);
            ContentService.Save(_textpage, -1);

            // Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
            _subpage = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 1", _textpage.Id);
            ContentService.Save(_subpage, -1);

            // Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
            _subpage2 = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 2", _textpage.Id);
            ContentService.Save(_subpage2, -1);

            _relation = new Relation(_textpage.Id, _subpage.Id, _relateContent) { Comment = string.Empty };
            _relation2 = new Relation(_textpage.Id, _subpage2.Id, _relateContent) { Comment = string.Empty };
            await relationRepository.SaveAsync(_relation, CancellationToken.None);
            await relationRepository.SaveAsync(_relation2, CancellationToken.None);
            scope.Complete();
        }
    }
}
