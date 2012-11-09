using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class RelationRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repositoryType = new RelationTypeRepository(unitOfWork);
            var repository = new RelationRepository(unitOfWork, NullCacheProvider.Current, repositoryType);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Perform_Add_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_Update_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_Delete_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_Get_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_GetAll_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_Exists_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_Count_On_RelationRepository()
        { }

        [Test]
        public void Can_Perform_GetByQuery_On_RelationRepository()
        { }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var relateContent = new RelationType(new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"), new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"), "relateContentOnCopy") { IsBidirectional = true, Name = "Relate Content on Copy" };
            var relateContentType = new RelationType(new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"), new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"), "relateContentTypeOnCopy") { IsBidirectional = true, Name = "Relate ContentType on Copy" };

            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            var repository = new RelationTypeRepository(unitOfWork);

            repository.AddOrUpdate(relateContent);
            repository.AddOrUpdate(relateContentType);
            unitOfWork.Commit();
        }
    }
}