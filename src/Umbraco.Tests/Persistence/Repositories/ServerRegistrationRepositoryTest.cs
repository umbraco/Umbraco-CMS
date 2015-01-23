using System;
using System.Data.SqlServerCe;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class ServerRegistrationRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        private ServerRegistrationRepository CreateRepositor(IDatabaseUnitOfWork unitOfWork)
        {
            return new ServerRegistrationRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
        }

        [Test]
        public void Cannot_Add_Duplicate_Computer_Names()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = CreateRepositor(unitOfWork))
            {
                var server = new ServerRegistration("http://shazwazza.com", "COMPUTER1", DateTime.Now);
                repository.AddOrUpdate(server);

                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Cannot_Update_To_Duplicate_Computer_Names()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = CreateRepositor(unitOfWork))
            {
                var server = repository.Get(1);
                server.ComputerName = "COMPUTER2";
                repository.AddOrUpdate(server);
                Assert.Throws<SqlCeException>(unitOfWork.Commit);
            }

        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Assert
                Assert.That(repository, Is.Not.Null);    
            }
        }

        [Test]
        public void Can_Perform_Get_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var server = repository.Get(1);

                // Assert
                Assert.That(server, Is.Not.Null);
                Assert.That(server.HasIdentity, Is.True);
                Assert.That(server.ServerAddress, Is.EqualTo("http://localhost"));    
            }

            
        }

        [Test]
        public void Can_Perform_GetAll_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var servers = repository.GetAll();

                // Assert
                Assert.That(servers.Count(), Is.EqualTo(3));    
            }
            
        }

        [Test]
        public void Can_Perform_GetByQuery_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var query = Query<ServerRegistration>.Builder.Where(x => x.ComputerName.ToUpper() == "COMPUTER3");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.AreEqual(1, result.Count());    
            }
        }

        [Test]
        public void Can_Perform_Count_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var query = Query<ServerRegistration>.Builder.Where(x => x.ServerAddress.StartsWith("http://"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(2));    
            }
        }

        [Test]
        public void Can_Perform_Add_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var server = new ServerRegistration("http://shazwazza.com", "COMPUTER4", DateTime.Now);
                repository.AddOrUpdate(server);
                unitOfWork.Commit();

                // Assert
                Assert.That(server.HasIdentity, Is.True);
                Assert.That(server.Id, Is.EqualTo(4));//With 3 existing entries the Id should be 4   
            }            
        }

        [Test]
        public void Can_Perform_Update_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var server = repository.Get(2);
                server.ServerAddress = "https://umbraco.com";
                server.IsActive = true;

                repository.AddOrUpdate(server);
                unitOfWork.Commit();

                var serverUpdated = repository.Get(2);

                // Assert
                Assert.That(serverUpdated, Is.Not.Null);
                Assert.That(serverUpdated.ServerAddress, Is.EqualTo("https://umbraco.com"));
                Assert.That(serverUpdated.IsActive, Is.EqualTo(true));   
            }            
        }

        [Test]
        public void Can_Perform_Delete_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var server = repository.Get(3);
                Assert.IsNotNull(server);
                repository.Delete(server);
                unitOfWork.Commit();

                var exists = repository.Exists(3);

                // Assert
                Assert.That(exists, Is.False);   
            }            
        }

        [Test]
        public void Can_Perform_Exists_On_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepositor(unitOfWork))
            {
                // Act
                var exists = repository.Exists(3);
                var doesntExist = repository.Exists(10);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);    
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public void CreateTestData()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.GetUnitOfWork())
            using (var repository = new ServerRegistrationRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax))
            {
                repository.AddOrUpdate(new ServerRegistration("http://localhost", "COMPUTER1", DateTime.Now) { IsActive = true });
                repository.AddOrUpdate(new ServerRegistration("http://www.mydomain.com", "COMPUTER2", DateTime.Now));
                repository.AddOrUpdate(new ServerRegistration("https://www.another.domain.com", "Computer3", DateTime.Now));
                unitOfWork.Commit();
            }

        }
    }
}