// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RelationTypeRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => CreateTestData();

    private RelationTypeRepository CreateRepository(ICoreScopeProvider provider) =>
        new((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());

    [Test]
    public void Can_Perform_Add_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var relateMemberToContent = new RelationType("Relate Member to Content", "relateMemberToContent", true, Constants.ObjectTypes.Member, Constants.ObjectTypes.Document, true);

            repository.Save(relateMemberToContent);

            // Assert
            Assert.That(relateMemberToContent.HasIdentity, Is.True);
            Assert.That(repository.Exists(relateMemberToContent.Id), Is.True);
        }
    }

    [Test]
    public void Can_Perform_Update_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var relationType = repository.Get(3);
            relationType.Alias += "Updated";
            relationType.Name += " Updated";
            repository.Save(relationType);

            var relationTypeUpdated = repository.Get(3);

            // Assert
            Assert.That(relationTypeUpdated, Is.Not.Null);
            Assert.That(relationTypeUpdated.HasIdentity, Is.True);
            Assert.That(relationTypeUpdated.Alias, Is.EqualTo(relationType.Alias));
            Assert.That(relationTypeUpdated.Name, Is.EqualTo(relationType.Name));
        }
    }

    [Test]
    public void Can_Perform_Delete_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var relationType = repository.Get(3);
            repository.Delete(relationType);

            var exists = repository.Exists(3);

            // Assert
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public void Can_Perform_Get_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var relationType = repository.Get(9) as IRelationTypeWithIsDependency;

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
    public void Can_Perform_GetAll_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var relationTypes = repository.GetMany().ToArray();

            // Assert
            Assert.That(relationTypes, Is.Not.Null);
            Assert.That(relationTypes.Any(), Is.True);
            Assert.That(relationTypes.Any(x => x == null), Is.False);
            Assert.That(relationTypes.Count(), Is.EqualTo(9));
        }
    }

    [Test]
    public void Can_Perform_GetAll_With_Params_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var relationTypes = repository.GetMany(2, 3).ToArray();

            // Assert
            Assert.That(relationTypes, Is.Not.Null);
            Assert.That(relationTypes.Any(), Is.True);
            Assert.That(relationTypes.Any(x => x == null), Is.False);
            Assert.That(relationTypes.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public void Can_Perform_Exists_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var exists = repository.Exists(3);
            var doesntExist = repository.Exists(99);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
        }
    }

    [Test]
    public void Can_Perform_Count_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var query = provider.CreateQuery<IRelationType>().Where(x => x.Alias.StartsWith("relate"));
            var count = repository.Count(query);

            // Assert
            Assert.That(count, Is.EqualTo(6));
        }
    }

    [Test]
    public void Can_Perform_GetByQuery_On_RelationTypeRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            // Act
            var childObjType = Constants.ObjectTypes.DocumentType;
            var query = provider.CreateQuery<IRelationType>().Where(x => x.ChildObjectType == childObjType);
            var result = repository.Get(query).ToArray();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
            Assert.That(result.Any(x => x == null), Is.False);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }

    public void CreateTestData()
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
            var repository = new RelationTypeRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());

            repository.Save(relateContent); // Id 2
            repository.Save(relateContentType); // Id 3
            repository.Save(relateContentMedia); // Id 4
            scope.Complete();
        }
    }

    [Test]
    public void Get_By_Guid_Returns_Deep_Clone_Not_Cached_Instance()
    {
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);
            var relationType = new RelationType("Test Clone", "testClone", true, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, false);
            repository.Save(relationType);

            var first = repository.Get(relationType.Key);
            var second = repository.Get(relationType.Key);

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(first!.Id, second!.Id);
            Assert.AreNotSame(first, second);
        }
    }

    [Test]
    public void Exists_By_Guid_Returns_Correct_Result()
    {
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);
            var relationType = new RelationType("Test Exists", "testExists", true, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, false);
            repository.Save(relationType);

            Assert.IsTrue(repository.Exists(relationType.Key));
            Assert.IsFalse(repository.Exists(Guid.NewGuid()));
        }
    }

    [Test]
    public void Get_By_Guid_Mutation_Does_Not_Affect_Subsequent_Get()
    {
        ICoreScopeProvider provider = ScopeProvider;
        using (provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);
            var relationType = new RelationType("Test Mutation", "testMutation", true, Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, false);
            repository.Save(relationType);

            var first = repository.Get(relationType.Key);
            Assert.IsNotNull(first);
            var originalName = first!.Name;
            first.Name = "MUTATED_" + Guid.NewGuid();

            var second = repository.Get(relationType.Key);
            Assert.IsNotNull(second);
            Assert.AreEqual(originalName, second!.Name, "Mutation of a returned entity should not affect the cached copy");
        }
    }
}
