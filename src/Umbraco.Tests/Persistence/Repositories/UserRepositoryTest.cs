using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UserRepositoryTest : TestWithDatabaseBase
    {
        private MediaRepository CreateMediaRepository(IScopeUnitOfWork unitOfWork, out IMediaTypeRepository mediaTypeRepository)
        {
            mediaTypeRepository = new MediaTypeRepository(unitOfWork, CacheHelper, Mock.Of<ILogger>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper, Mock.Of<ILogger>());
            var repository = new MediaRepository(unitOfWork, CacheHelper, Mock.Of<ILogger>(), mediaTypeRepository, tagRepository, Mock.Of<IContentSection>());
            return repository;
        }

        private ContentRepository CreateContentRepository(IScopeUnitOfWork unitOfWork, out IContentTypeRepository contentTypeRepository)
        {
            ITemplateRepository tr;
            return CreateContentRepository(unitOfWork, out contentTypeRepository, out tr);
        }

        private ContentRepository CreateContentRepository(IScopeUnitOfWork unitOfWork, out IContentTypeRepository contentTypeRepository, out ITemplateRepository templateRepository)
        {
            templateRepository = new TemplateRepository(unitOfWork, CacheHelper, Logger, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>());
            var tagRepository = new TagRepository(unitOfWork, CacheHelper, Logger);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, CacheHelper, Logger, templateRepository);
            var repository = new ContentRepository(unitOfWork, CacheHelper, Logger, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>());
            return repository;
        }

        private UserRepository CreateRepository(IScopeUnitOfWork unitOfWork)
        {
            var repository = new UserRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), Mock.Of<IMapperCollection>());
            return repository;
        }

        private UserGroupRepository CreateUserGroupRepository(IScopeUnitOfWork unitOfWork)
        {
            return new UserGroupRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>());
        }

        [Test]
        public void Can_Perform_Add_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var user = MockedUser.CreateUser();

                // Act
                repository.AddOrUpdate(user);
                unitOfWork.Flush();

                // Assert
                Assert.That(user.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var user1 = MockedUser.CreateUser("1");
                var use2 = MockedUser.CreateUser("2");

                // Act
                repository.AddOrUpdate(user1);
                unitOfWork.Flush();
                repository.AddOrUpdate(use2);
                unitOfWork.Flush();

                // Assert
                Assert.That(user1.HasIdentity, Is.True);
                Assert.That(use2.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var user = MockedUser.CreateUser();
                repository.AddOrUpdate(user);
                unitOfWork.Flush();

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
            var ct = MockedContentTypes.CreateBasicContentType("test");
            var content = MockedContent.CreateBasicContent(ct);
            var mt = MockedContentTypes.CreateSimpleMediaType("testmedia", "TestMedia");
            var media = MockedMedia.CreateSimpleMedia(mt, "asdf", -1);

            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var userRepository = CreateRepository(unitOfWork);
                var contentRepository = CreateContentRepository(unitOfWork, out var contentTypeRepo);
                var mediaRepository = CreateMediaRepository(unitOfWork, out var mediaTypeRepo);
                var userGroupRepository = CreateUserGroupRepository(unitOfWork);

                contentTypeRepo.AddOrUpdate(ct);
                mediaTypeRepo.AddOrUpdate(mt);
                unitOfWork.Flush();

                contentRepository.AddOrUpdate(content);
                mediaRepository.AddOrUpdate(media);
                unitOfWork.Flush();

                var user = CreateAndCommitUserWithGroup(userRepository, userGroupRepository, unitOfWork);

                // Act
                var resolved = (User)userRepository.Get((int)user.Id);

                resolved.Name = "New Name";
                //the db column is not used, default permissions are taken from the user type's permissions, this is a getter only
                //resolved.DefaultPermissions = "ZYX";
                resolved.Language = "fr";
                resolved.IsApproved = false;
                resolved.RawPasswordValue = "new";
                resolved.IsLockedOut = true;
                resolved.StartContentIds = new[] { content.Id };
                resolved.StartMediaIds = new[] { media.Id };
                resolved.Email = "new@new.com";
                resolved.Username = "newName";

                userRepository.AddOrUpdate(resolved);
                unitOfWork.Flush();
                var updatedItem = (User)userRepository.Get((int)user.Id);

                // Assert
                Assert.That(updatedItem.Id, Is.EqualTo(resolved.Id));
                Assert.That(updatedItem.Name, Is.EqualTo(resolved.Name));
                Assert.That(updatedItem.Language, Is.EqualTo(resolved.Language));
                Assert.That(updatedItem.IsApproved, Is.EqualTo(resolved.IsApproved));
                Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(resolved.RawPasswordValue));
                Assert.That(updatedItem.IsLockedOut, Is.EqualTo(resolved.IsLockedOut));
                Assert.IsTrue(updatedItem.StartContentIds.UnsortedSequenceEqual(resolved.StartContentIds));
                Assert.IsTrue(updatedItem.StartMediaIds.UnsortedSequenceEqual(resolved.StartMediaIds));
                Assert.That(updatedItem.Email, Is.EqualTo(resolved.Email));
                Assert.That(updatedItem.Username, Is.EqualTo(resolved.Username));
                Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(1));
                Assert.IsTrue(updatedItem.AllowedSections.Contains("content"));
                Assert.IsTrue(updatedItem.AllowedSections.Contains("media"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var user = MockedUser.CreateUser();

                // Act
                repository.AddOrUpdate(user);
                unitOfWork.Flush();
                var id = user.Id;

                var repository2 = new UserRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger, Mock.Of<IMapperCollection>());

                repository2.Delete(user);
                unitOfWork.Flush();

                var resolved = repository2.Get((int) id);

                // Assert
                Assert.That(resolved, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);
                var userGroupRepository = CreateUserGroupRepository(unitOfWork);

                var user = CreateAndCommitUserWithGroup(repository, userGroupRepository, unitOfWork);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var query = unitOfWork.Query<IUser>().Where(x => x.Username == "TestUser1");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var users = CreateAndCommitMultipleUsers(repository, unitOfWork);

                // Act
                var query = unitOfWork.Query<IUser>().Where(x => x.Username == "TestUser1" || x.Username == "TestUser2");
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        private void AssertPropertyValues(IUser updatedItem, IUser originalUser)
        {
            Assert.That(updatedItem.Id, Is.EqualTo(originalUser.Id));
            Assert.That(updatedItem.Name, Is.EqualTo(originalUser.Name));
            Assert.That(updatedItem.Language, Is.EqualTo(originalUser.Language));
            Assert.That(updatedItem.IsApproved, Is.EqualTo(originalUser.IsApproved));
            Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(originalUser.RawPasswordValue));
            Assert.That(updatedItem.IsLockedOut, Is.EqualTo(originalUser.IsLockedOut));
            Assert.IsTrue(updatedItem.StartContentIds.UnsortedSequenceEqual(originalUser.StartContentIds));
            Assert.IsTrue(updatedItem.StartMediaIds.UnsortedSequenceEqual(originalUser.StartMediaIds));
            Assert.That(updatedItem.Email, Is.EqualTo(originalUser.Email));
            Assert.That(updatedItem.Username, Is.EqualTo(originalUser.Username));
            Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(2));
            Assert.IsTrue(updatedItem.AllowedSections.Contains("media"));
            Assert.IsTrue(updatedItem.AllowedSections.Contains("content"));
        }

        private static User CreateAndCommitUserWithGroup(IUserRepository repository, IUserGroupRepository userGroupRepository, IScopeUnitOfWork unitOfWork)
        {

            var user = MockedUser.CreateUser();
            repository.AddOrUpdate(user);
            unitOfWork.Flush();

            var group = MockedUserGroup.CreateUserGroup();
            userGroupRepository.AddOrUpdateGroupWithUsers(@group, new[] { user.Id });
            unitOfWork.Flush();

            return user;
        }

        private IUser[] CreateAndCommitMultipleUsers(IUserRepository repository, IUnitOfWork unitOfWork)
        {
            var user1 = MockedUser.CreateUser("1");
            var user2 = MockedUser.CreateUser("2");
            var user3 = MockedUser.CreateUser("3");
            repository.AddOrUpdate(user1);
            repository.AddOrUpdate(user2);
            repository.AddOrUpdate(user3);
            unitOfWork.Complete();
            return new IUser[] { user1, user2, user3 };
        }
    }
}
