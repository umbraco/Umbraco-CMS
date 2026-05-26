// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RelationTypeRepositoryTest : UmbracoIntegrationTest
{
    private IRelationType _relateContentToMedia;

    [SetUp]
    public async Task SetUp() => await CreateTestDataAsync();

    private RelationTypeRepository CreateRepository() =>
        (RelationTypeRepository)GetRequiredService<IRelationTypeRepository>();

    [Test]
    public async Task Can_Perform_Add_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var relateMemberToContent = new RelationType("Relate Member to Content", "relateMemberToContent", true, Constants.ObjectTypes.Member, Constants.ObjectTypes.Document, true);

            await repository.SaveAsync(relateMemberToContent, CancellationToken.None);

            // Assert
            Assert.That(relateMemberToContent.HasIdentity, Is.True);
            Assert.That(await repository.ExistsAsync(relateMemberToContent.Id, CancellationToken.None), Is.True);
        }
    }

    [Test]
    public async Task Can_Perform_Update_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var relationType = await repository.GetAsync(3, CancellationToken.None);
            relationType.Alias += "Updated";
            relationType.Name += " Updated";
            await repository.SaveAsync(relationType, CancellationToken.None);

            var relationTypeUpdated = await repository.GetAsync(3, CancellationToken.None);

            // Assert
            Assert.That(relationTypeUpdated, Is.Not.Null);
            Assert.That(relationTypeUpdated.HasIdentity, Is.True);
            Assert.That(relationTypeUpdated.Alias, Is.EqualTo(relationType.Alias));
            Assert.That(relationTypeUpdated.Name, Is.EqualTo(relationType.Name));
        }
    }

    [Test]
    public async Task Can_Perform_Delete_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var relationType = await repository.GetAsync(3, CancellationToken.None);
            await repository.DeleteAsync(relationType, CancellationToken.None);

            var exists = await repository.ExistsAsync(3, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public async Task Can_Perform_Get_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var relationType = await repository.GetAsync(_relateContentToMedia.Id, CancellationToken.None) as IRelationTypeWithIsDependency;

            // Assert
            Assert.That(relationType, Is.Not.Null);
            Assert.That(relationType.HasIdentity, Is.True);
            Assert.That(relationType.IsBidirectional, Is.True);
            Assert.That(relationType.IsDependency, Is.True);
            Assert.That(relationType.Alias, Is.EqualTo("relateContentToMedia"));
            Assert.That(relationType.Name, Is.EqualTo("Relate Content to Media"));
            Assert.That(relationType.ChildObjectType, Is.EqualTo(Constants.ObjectTypes.Media));
            Assert.That(relationType.ParentObjectType, Is.EqualTo(Constants.ObjectTypes.Document));
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var relationTypes = (await repository.GetAllAsync(CancellationToken.None)).ToArray();

            // Assert
            Assert.That(relationTypes, Is.Not.Null);
            Assert.That(relationTypes.Length, Is.EqualTo(12));
            Assert.That(relationTypes.Any(x => x == null), Is.False);
        }
    }

    [Test]
    public async Task Can_Perform_GetMany_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var relationTypes = (await repository.GetManyAsync(new[] { 2, 3 }, CancellationToken.None)).ToArray();

            // Assert
            Assert.That(relationTypes, Is.Not.Null);
            Assert.That(relationTypes.Any(), Is.True);
            Assert.That(relationTypes.Any(x => x == null), Is.False);
            Assert.That(relationTypes.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public async Task Can_Perform_Exists_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var exists = await repository.ExistsAsync(3, CancellationToken.None);
            var doesntExist = await repository.ExistsAsync(99, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
        }
    }

    [Test]
    public async Task Can_Get_By_Alias_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var result = await repository.GetByAliasAsync("relateContentToMedia");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Alias, Is.EqualTo("relateContentToMedia"));
            Assert.That(result.ChildObjectType, Is.EqualTo(Constants.ObjectTypes.Media));
        }
    }

    [Test]
    public async Task Can_Get_By_Key_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository();

            // Act
            var result = await repository.GetAsync(_relateContentToMedia.Key);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_relateContentToMedia.Id));
            Assert.That(result.Alias, Is.EqualTo("relateContentToMedia"));
        }
    }

    public async Task CreateTestDataAsync()
    {
        var relateContent = new RelationType(
            "Relate Content on Copy",
            "relateContentOnCopy",
            true,
            Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Document,
            false);
        var relateContentType = new RelationType(
            "Relate ContentType on Copy",
            "relateContentTypeOnCopy",
            true,
            Constants.ObjectTypes.DocumentType,
            Constants.ObjectTypes.DocumentType,
            false);
        var relateContentMedia = new RelationType(
            "Relate Content to Media",
            "relateContentToMedia",
            true,
            Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Media,
            true);

        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = GetRequiredService<IRelationTypeRepository>();

            await repository.SaveAsync(relateContent, CancellationToken.None);
            await repository.SaveAsync(relateContentType, CancellationToken.None);
            await repository.SaveAsync(relateContentMedia, CancellationToken.None);
            scope.Complete();
        }

        _relateContentToMedia = relateContentMedia;
    }

    [Test]
    public async Task Get_By_Guid_Returns_Deep_Clone_Not_Cached_Instance()
    {
        using (ScopeProvider.CreateCoreScope())
        {
            var repository = CreateRepository();
            var relationType = new RelationType("Test Clone", "testClone", true, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, false);
            await repository.SaveAsync(relationType, CancellationToken.None);

            var first = await repository.GetAsync(relationType.Key, CancellationToken.None);
            var second = await repository.GetAsync(relationType.Key, CancellationToken.None);

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(first!.Id, second!.Id);
            Assert.AreNotSame(first, second);
        }
    }

    [Test]
    public async Task Exists_By_Guid_Returns_Correct_Result()
    {
        using (ScopeProvider.CreateCoreScope())
        {
            var repository = CreateRepository();
            var relationType = new RelationType("Test Exists", "testExists", true, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, false);
            await repository.SaveAsync(relationType, CancellationToken.None);

            Assert.IsTrue(await repository.ExistsAsync(relationType.Id, CancellationToken.None));
            Assert.IsNull(await repository.GetAsync(Guid.NewGuid(), CancellationToken.None));
        }
    }

    [Test]
    public async Task Get_By_Guid_Mutation_Does_Not_Affect_Subsequent_Get()
    {
        using (ScopeProvider.CreateCoreScope())
        {
            var repository = CreateRepository();
            var relationType = new RelationType("Test Mutation", "testMutation", true, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, false);
            await repository.SaveAsync(relationType, CancellationToken.None);

            var first = await repository.GetAsync(relationType.Key, CancellationToken.None);
            Assert.IsNotNull(first);
            var originalName = first!.Name;
            first.Name = "MUTATED_" + Guid.NewGuid();

            var second = await repository.GetAsync(relationType.Key, CancellationToken.None);
            Assert.IsNotNull(second);
            Assert.AreEqual(originalName, second!.Name, "Mutation of a returned entity should not affect the cached copy");
        }
    }
}
