using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RelationTypeRepositoryTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            CreateTestData();
        }

        private RelationTypeRepository CreateRepository(IScopeProvider provider)
        {
            return new RelationTypeRepository((IScopeAccessor) provider, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>());
        }


        [Test]
        public void Can_Perform_Add_On_RelationTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var relateMemberToContent = new RelationType(Constants.ObjectTypes.Member,
                                                            Constants.ObjectTypes.Document,
                                                            "relateMemberToContent") { IsBidirectional = true, Name = "Relate Member to Content" };

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var relationType = repository.Get(3);
                relationType.Alias = relationType.Alias + "Updated";
                relationType.Name = relationType.Name + " Updated";
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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var relationType = repository.Get(RelationTypeDto.NodeIdSeed);

                // Assert
                Assert.That(relationType, Is.Not.Null);
                Assert.That(relationType.HasIdentity, Is.True);
                Assert.That(relationType.Alias, Is.EqualTo("relateContentOnCopy"));
                Assert.That(relationType.Name, Is.EqualTo("Relate Content on Copy"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_RelationTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var relationTypes = repository.GetMany();

                // Assert
                Assert.That(relationTypes, Is.Not.Null);
                Assert.That(relationTypes.Any(), Is.True);
                Assert.That(relationTypes.Any(x => x == null), Is.False);
                Assert.That(relationTypes.Count(), Is.EqualTo(5));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_RelationTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var relationTypes = repository.GetMany(2, 3);

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(6);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Count_On_RelationTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var query = scope.SqlContext.Query<IRelationType>().Where(x => x.Alias.StartsWith("relate"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(5));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_RelationTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                var childObjType = Constants.ObjectTypes.DocumentType;
                var query = scope.SqlContext.Query<IRelationType>().Where(x => x.ChildObjectType == childObjType);
                var result = repository.Get(query);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Any(x => x == null), Is.False);
                Assert.That(result.Count(), Is.EqualTo(1));
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var relateContent = new RelationType(Constants.ObjectTypes.Document, new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"), "relateContentOnCopy") { IsBidirectional = true, Name = "Relate Content on Copy" };
            var relateContentType = new RelationType(Constants.ObjectTypes.DocumentType, new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"), "relateContentTypeOnCopy") { IsBidirectional = true, Name = "Relate ContentType on Copy" };

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repository = new RelationTypeRepository((IScopeAccessor) provider, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>());

                repository.Save(relateContent);//Id 2
                repository.Save(relateContentType);//Id 3
                scope.Complete();
            }
        }
    }
}
