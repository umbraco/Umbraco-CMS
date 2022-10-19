// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.Extensions.Logging;
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
public class RelationTypeRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => CreateTestData();

    private RelationTypeRepository CreateRepository(ICoreScopeProvider provider) =>
        new((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>());

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
            var relationType = repository.Get(8) as IRelationTypeWithIsDependency;

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
            Assert.That(relationTypes.Count(), Is.EqualTo(8));
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
            var doesntExist = repository.Exists(9);

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
            var repository = new RelationTypeRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<RelationTypeRepository>());

            repository.Save(relateContent); // Id 2
            repository.Save(relateContentType); // Id 3
            repository.Save(relateContentMedia); // Id 4
            scope.Complete();
        }
    }
}
