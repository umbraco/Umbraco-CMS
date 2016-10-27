using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class UserRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        private UserRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out UserTypeRepository userTypeRepository)
        {
            userTypeRepository = new UserTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            var repository = new UserRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, userTypeRepository);
            return repository;
        }


        [Test]
        public void Can_Perform_Add_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {

                var user = MockedUser.CreateUser(CreateAndCommitUserType());

                // Act
                repository.AddOrUpdate(user);
                unitOfWork.Commit();

                // Assert
                Assert.That(user.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {

                var user1 = MockedUser.CreateUser(CreateAndCommitUserType(), "1");
                var use2 = MockedUser.CreateUser(CreateAndCommitUserType(), "2");

                // Act
                repository.AddOrUpdate(user1);
                unitOfWork.Commit();
                repository.AddOrUpdate(use2);
                unitOfWork.Commit();

                // Assert
                Assert.That(user1.HasIdentity, Is.True);
                Assert.That(use2.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                var user = MockedUser.CreateUser(CreateAndCommitUserType());
                repository.AddOrUpdate(user);
                unitOfWork.Commit();

                // Act
                var resolved = repository.Get((int)user.Id);
                bool dirty = ((User)resolved).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                var user = MockedUser.CreateUser(CreateAndCommitUserType());
                repository.AddOrUpdate(user);
                unitOfWork.Commit();

                // Act
                var resolved = (User)repository.Get((int)user.Id);

                resolved.Name = "New Name";
                //the db column is not used, default permissions are taken from the user type's permissions, this is a getter only
                //resolved.DefaultPermissions = "ZYX";
                resolved.Language = "fr";
                resolved.IsApproved = false;
                resolved.RawPasswordValue = "new";
                resolved.IsLockedOut = true;
                resolved.StartContentId = 10;
                resolved.StartMediaId = 11;
                resolved.Email = "new@new.com";
                resolved.Username = "newName";

                repository.AddOrUpdate(resolved);
                unitOfWork.Commit();
                var updatedItem = (User)repository.Get((int)user.Id);

                // Assert
                Assert.That(updatedItem.Id, Is.EqualTo(resolved.Id));
                Assert.That(updatedItem.Name, Is.EqualTo(resolved.Name));
                //Assert.That(updatedItem.DefaultPermissions, Is.EqualTo(resolved.DefaultPermissions));
                Assert.That(updatedItem.Language, Is.EqualTo(resolved.Language));
                Assert.That(updatedItem.IsApproved, Is.EqualTo(resolved.IsApproved));
                Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(resolved.RawPasswordValue));
                Assert.That(updatedItem.IsLockedOut, Is.EqualTo(resolved.IsLockedOut));
                Assert.That(updatedItem.StartContentId, Is.EqualTo(resolved.StartContentId));
                Assert.That(updatedItem.StartMediaId, Is.EqualTo(resolved.StartMediaId));
                Assert.That(updatedItem.Email, Is.EqualTo(resolved.Email));
                Assert.That(updatedItem.Username, Is.EqualTo(resolved.Username));
                Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(1));
                Assert.IsTrue(updatedItem.AllowedSections.Contains("media"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {

                var user = MockedUser.CreateUser(CreateAndCommitUserType());

                // Act
                repository.AddOrUpdate(user);
                unitOfWork.Commit();
                var id = user.Id;

                using (var utRepo = new UserTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
                using (var repository2 = new UserRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, utRepo))
                {
                    repository2.Delete(user);
                    unitOfWork.Commit();

                    var resolved = repository2.Get((int) id);

                    // Assert
                    Assert.That(resolved, Is.Null);
                }
            }
        }

        //[Test]
        //public void Can_Perform_Delete_On_UserRepository_With_Permissions_Assigned()
        //{
        //    // Arrange
        //    var provider = new PetaPocoUnitOfWorkProvider(Logger);
        //    var unitOfWork = provider.GetUnitOfWork();
        //    UserTypeRepository userTypeRepository;
        //using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
        //{

        //    var user = MockedUser.CreateUser(CreateAndCommitUserType());
        //    //repository.AssignPermissions()

        //    // Act
        //    repository.AddOrUpdate(user);
        //    unitOfWork.Commit();
        //    var id = user.Id;

        //    var repository2 = RepositoryResolver.Current.ResolveByType<IUserRepository>(unitOfWork);
        //    repository2.Delete(user);
        //    unitOfWork.Commit();

        //    var resolved = repository2.Get((int)id);

        //    // Assert
        //    Assert.That(resolved, Is.Null);
        //}

        //}

        [Test]
        public void Can_Perform_Get_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                var user = MockedUser.CreateUser(CreateAndCommitUserType());
                repository.AddOrUpdate(user);
                unitOfWork.Commit();

                // Act
                var updatedItem = repository.Get((int) user.Id);

                // Assert
                AssertPropertyValues(updatedItem, user);
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var query = Query<IUser>.Builder.Where(x => x.Username == "TestUser1");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                var users = CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var result = repository.GetAll((int) users[0].Id, (int) users[1].Id);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var result = repository.GetAll();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(3));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                var users = CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var exists = repository.Exists((int) users[0].Id);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Count_On_UserRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            UserTypeRepository userTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out userTypeRepository))
            {
                var users = CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var query = Query<IUser>.Builder.Where(x => x.Username == "TestUser1" || x.Username == "TestUser2");
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Default_User_Permissions_Based_On_User_Type()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var utRepo = new UserTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
            using (var repository = new UserRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax, utRepo))
            { 

                // Act
                var user1 = MockedUser.CreateUser(CreateAndCommitUserType(), "1");
                repository.AddOrUpdate(user1);
                unitOfWork.Commit();

                // Assert
                Assert.AreEqual(3, user1.DefaultPermissions.Count());
                Assert.AreEqual("A", user1.DefaultPermissions.ElementAt(0));
                Assert.AreEqual("B", user1.DefaultPermissions.ElementAt(1));
                Assert.AreEqual("C", user1.DefaultPermissions.ElementAt(2));
            }
        }

        private void AssertPropertyValues(IUser updatedItem, IUser originalUser)
        {
            Assert.That(updatedItem.Id, Is.EqualTo(originalUser.Id));
            Assert.That(updatedItem.Name, Is.EqualTo(originalUser.Name));
            Assert.That(updatedItem.DefaultPermissions, Is.EqualTo(originalUser.DefaultPermissions));
            Assert.That(updatedItem.Language, Is.EqualTo(originalUser.Language));
            Assert.That(updatedItem.IsApproved, Is.EqualTo(originalUser.IsApproved));
            Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(originalUser.RawPasswordValue));
            Assert.That(updatedItem.IsLockedOut, Is.EqualTo(originalUser.IsLockedOut));
            Assert.That(updatedItem.StartContentId, Is.EqualTo(originalUser.StartContentId));
            Assert.That(updatedItem.StartMediaId, Is.EqualTo(originalUser.StartMediaId));
            Assert.That(updatedItem.Email, Is.EqualTo(originalUser.Email));
            Assert.That(updatedItem.Username, Is.EqualTo(originalUser.Username));
            Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(2));
            Assert.IsTrue(updatedItem.AllowedSections.Contains("media"));
            Assert.IsTrue(updatedItem.AllowedSections.Contains("content"));
        }

        private IUser[] CreateAndCommitMultipleUsers(IUserRepository repository, IUnitOfWork unitOfWork)
        {
            var user1 = MockedUser.CreateUser(CreateAndCommitUserType(), "1");
            var user2 = MockedUser.CreateUser(CreateAndCommitUserType(), "2");
            var user3 = MockedUser.CreateUser(CreateAndCommitUserType(), "3");
            repository.AddOrUpdate(user1);
            repository.AddOrUpdate(user2);
            repository.AddOrUpdate(user3);
            unitOfWork.Commit();
            return new IUser[] { user1, user2, user3 };
        }

        private IUserType CreateAndCommitUserType()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new UserTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
            {
                var userType = MockedUserType.CreateUserType();
                repository.AddOrUpdate(userType);
                unitOfWork.Commit();
                return userType;
            }
        }
    }
}