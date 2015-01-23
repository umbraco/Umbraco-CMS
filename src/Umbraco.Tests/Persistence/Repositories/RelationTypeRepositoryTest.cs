using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class RelationTypeRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        private RelationTypeRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            return new RelationTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
        }


        [Test]
        public void Can_Perform_Add_On_RelationTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var relateMemberToContent = new RelationType(new Guid(Constants.ObjectTypes.Member),
                                                             new Guid(Constants.ObjectTypes.Document),
                                                             "relateMemberToContent") { IsBidirectional = true, Name = "Relate Member to Content" };

                repository.AddOrUpdate(relateMemberToContent);
                unitOfWork.Commit();

                // Assert
                Assert.That(relateMemberToContent.HasIdentity, Is.True);
                Assert.That(repository.Exists(relateMemberToContent.Id), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_RelationTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var relationType = repository.Get(3);
                relationType.Alias = relationType.Alias + "Updated";
                relationType.Name = relationType.Name + " Updated";
                repository.AddOrUpdate(relationType);
                unitOfWork.Commit();

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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var relationType = repository.Get(3);
                repository.Delete(relationType);
                unitOfWork.Commit();

                var exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_RelationTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var relationTypes = repository.GetAll();

                // Assert
                Assert.That(relationTypes, Is.Not.Null);
                Assert.That(relationTypes.Any(), Is.True);
                Assert.That(relationTypes.Any(x => x == null), Is.False);
                Assert.That(relationTypes.Count(), Is.EqualTo(4));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_RelationTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var relationTypes = repository.GetAll(2, 3);

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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(5);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Count_On_RelationTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var query = Query<IRelationType>.Builder.Where(x => x.Alias.StartsWith("relate"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(4));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_RelationTypeRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                // Act
                var childObjType = new Guid(Constants.ObjectTypes.DocumentType);
                var query = Query<IRelationType>.Builder.Where(x => x.ChildObjectType == childObjType);
                var result = repository.GetByQuery(query);

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
            var relateContent = new RelationType(new Guid(Constants.ObjectTypes.Document), new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"), "relateContentOnCopy") { IsBidirectional = true, Name = "Relate Content on Copy" };
            var relateContentType = new RelationType(new Guid(Constants.ObjectTypes.DocumentType), new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"), "relateContentTypeOnCopy") { IsBidirectional = true, Name = "Relate ContentType on Copy" };

            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new RelationTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);

            repository.AddOrUpdate(relateContent);//Id 2
            repository.AddOrUpdate(relateContentType);//Id 3
            unitOfWork.Commit();
        }
    }
}