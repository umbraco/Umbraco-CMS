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
    public class UserGroupRepositoryTest : BaseDatabaseFactoryTest
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

        private UserGroupRepository CreateRepository(IScopeUnitOfWork unitOfWork)
        {
            return new UserGroupRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);            
        }

        [Test]
        public void Can_Perform_Add_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                var userGroup = MockedUserGroup.CreateUserGroup();

                // Act
                repository.AddOrUpdate(userGroup);
                unitOfWork.Commit();

                // Assert
                Assert.That(userGroup.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                var userGroup1 = MockedUserGroup.CreateUserGroup("1");
                var userGroup2 = MockedUserGroup.CreateUserGroup("2");

                // Act
                repository.AddOrUpdate(userGroup1);
                unitOfWork.Commit();
                repository.AddOrUpdate(userGroup2);
                unitOfWork.Commit();

                // Assert
                Assert.That(userGroup1.HasIdentity, Is.True);
                Assert.That(userGroup2.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var userGroup = MockedUserGroup.CreateUserGroup();
                repository.AddOrUpdate(userGroup);
                unitOfWork.Commit();

                // Act
                var resolved = repository.Get(userGroup.Id);
                bool dirty = ((UserGroup) resolved).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var userGroup = MockedUserGroup.CreateUserGroup();
                repository.AddOrUpdate(userGroup);
                unitOfWork.Commit();

                // Act
                var resolved = repository.Get(userGroup.Id);
                resolved.Name = "New Name";
                resolved.Permissions = new[]{"Z", "Y", "X"};
                repository.AddOrUpdate(resolved);
                unitOfWork.Commit();
                var updatedItem = repository.Get(userGroup.Id);

                // Assert
                Assert.That(updatedItem.Id, Is.EqualTo(resolved.Id));
                Assert.That(updatedItem.Name, Is.EqualTo(resolved.Name));
                Assert.That(updatedItem.Permissions, Is.EqualTo(resolved.Permissions));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                var userGroup = MockedUserGroup.CreateUserGroup();

                // Act
                repository.AddOrUpdate(userGroup);
                unitOfWork.Commit();
                var id = userGroup.Id;

                using (var repository2 = new UserGroupRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax))
                {
                    repository2.Delete(userGroup);
                    unitOfWork.Commit();

                    var resolved = repository2.Get(id);

                    // Assert
                    Assert.That(resolved, Is.Null);    
                }
                
            }
        }

        [Test]
        public void Can_Perform_Get_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var userGroup = MockedUserGroup.CreateUserGroup();
                repository.AddOrUpdate(userGroup);
                unitOfWork.Commit();

                // Act
                var resolved = repository.Get(userGroup.Id);

                // Assert
                Assert.That(resolved.Id, Is.EqualTo(userGroup.Id));
                //Assert.That(resolved.CreateDate, Is.GreaterThan(DateTime.MinValue));
                //Assert.That(resolved.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(resolved.Name, Is.EqualTo(userGroup.Name));
                Assert.That(resolved.Alias, Is.EqualTo(userGroup.Alias));
                Assert.That(resolved.Permissions, Is.EqualTo(userGroup.Permissions));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act
                var query = Query<IUserGroup>.Builder.Where(x => x.Alias == "testUserGroup1");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var userGroups = CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act
                var result = repository.GetAll(userGroups[0].Id, userGroups[1].Id);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act
                var result = repository.GetAll();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(3));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var userGroups = CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act
                var exists = repository.Exists(userGroups[0].Id);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Count_On_UserGroupRepository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var userGroups = CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act
                var query = Query<IUserGroup>.Builder.Where(x => x.Alias == "testUserGroup1" || x.Alias == "testUserGroup2");
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Can_Remove_Section_For_Group()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var groups = CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act

                //add and remove a few times, this tests the internal collection
                groups[0].RemoveAllowedSection("content");
                groups[0].RemoveAllowedSection("content");
                groups[0].AddAllowedSection("content");
                groups[0].RemoveAllowedSection("content");

                groups[1].RemoveAllowedSection("media");
                groups[1].RemoveAllowedSection("media");

                repository.AddOrUpdate(groups[0]);
                repository.AddOrUpdate(groups[1]);
                unitOfWork.Commit();

                // Assert
                var result = repository.GetAll((int)groups[0].Id, (int)groups[1].Id).ToArray();
                Assert.AreEqual(1, result[0].AllowedSections.Count());
                Assert.AreEqual("media", result[0].AllowedSections.First());
                Assert.AreEqual(1, result[1].AllowedSections.Count());
                Assert.AreEqual("content", result[1].AllowedSections.First());
            }
        }

        [Test]
        public void Can_Add_Section_ForGroup()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var groups = CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act

                //add and remove a few times, this tests the internal collection
                groups[0].ClearAllowedSections();
                groups[0].AddAllowedSection("content");
                groups[0].AddAllowedSection("media");
                groups[0].RemoveAllowedSection("content");
                groups[0].AddAllowedSection("content");
                groups[0].AddAllowedSection("settings");

                //add the same even though it's already there
                groups[0].AddAllowedSection("content");

                groups[1].ClearAllowedSections();
                groups[1].AddAllowedSection("developer");

                groups[2].ClearAllowedSections();

                repository.AddOrUpdate(groups[0]);
                repository.AddOrUpdate(groups[1]);
                repository.AddOrUpdate(groups[2]);
                unitOfWork.Commit();

                // Assert
                var result = repository.GetAll((int)groups[0].Id, (int)groups[1].Id, (int)groups[2].Id).ToArray();
                Assert.AreEqual(3, result[0].AllowedSections.Count());
                Assert.IsTrue(result[0].AllowedSections.Contains("content"));
                Assert.IsTrue(result[0].AllowedSections.Contains("media"));
                Assert.IsTrue(result[0].AllowedSections.Contains("settings"));
                Assert.AreEqual(1, result[1].AllowedSections.Count());
                Assert.IsTrue(result[1].AllowedSections.Contains("developer"));
                Assert.AreEqual(0, result[2].AllowedSections.Count());
            }
        }

        [Test]
        public void Can_Update_Section_For_Group()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var groups = CreateAndCommitMultipleUserGroups(repository, unitOfWork);

                // Act

                groups[0].RemoveAllowedSection("content");
                groups[0].AddAllowedSection("settings");

                repository.AddOrUpdate(groups[0]);
                unitOfWork.Commit();

                // Assert
                var result = repository.Get((int)groups[0].Id);
                Assert.AreEqual(2, result.AllowedSections.Count());
                Assert.IsTrue(result.AllowedSections.Contains("settings"));
                Assert.IsTrue(result.AllowedSections.Contains("media"));
            }
        }


        [Test]
        public void Get_Groups_Assigned_To_Section()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var user1 = MockedUserGroup.CreateUserGroup("1", allowedSections: new[] { "test1" });
                var user2 = MockedUserGroup.CreateUserGroup("2", allowedSections: new[] { "test2" });
                var user3 = MockedUserGroup.CreateUserGroup("3", allowedSections: new[] { "test1" });
                repository.AddOrUpdate(user1);
                repository.AddOrUpdate(user2);
                repository.AddOrUpdate(user3);
                unitOfWork.Commit();

                // Act
                var groups = repository.GetGroupsAssignedToSection("test1");

                // Assert            
                Assert.AreEqual(2, groups.Count());
                var names = groups.Select(x => x.Name).ToArray();
                Assert.IsTrue(names.Contains("TestUserGroup1"));
                Assert.IsFalse(names.Contains("TestUserGroup2"));
                Assert.IsTrue(names.Contains("TestUserGroup3"));
            }
        }

        private IUserGroup[] CreateAndCommitMultipleUserGroups(IUserGroupRepository repository, IUnitOfWork unitOfWork)
        {
            var userGroup1 = MockedUserGroup.CreateUserGroup("1");
            var userGroup2 = MockedUserGroup.CreateUserGroup("2");
            var userGroup3 = MockedUserGroup.CreateUserGroup("3");
            repository.AddOrUpdate(userGroup1);
            repository.AddOrUpdate(userGroup2);
            repository.AddOrUpdate(userGroup3);
            unitOfWork.Commit();
            return new IUserGroup[] { userGroup1, userGroup2, userGroup3 };
        }
    }
}