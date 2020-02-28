﻿using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using System;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
    public class UserRepositoryTest : TestWithDatabaseBase
    {
        private MediaRepository CreateMediaRepository(IScopeProvider provider, out IMediaTypeRepository mediaTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = new TemplateRepository(accessor, AppCaches.Disabled, Logger, TestObjects.GetFileSystemsMock());
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches);
            var languageRepository = new LanguageRepository(accessor, AppCaches, Logger);
            mediaTypeRepository = new MediaTypeRepository(accessor, AppCaches, Mock.Of<ILogger>(), commonRepository, languageRepository);
            var tagRepository = new TagRepository(accessor, AppCaches, Mock.Of<ILogger>());
            var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Logger);
            var entityRepository = new EntityRepository(accessor);
            var relationRepository = new RelationRepository(accessor, Logger, relationTypeRepository, entityRepository);
            var propertyEditors = new Lazy<PropertyEditorCollection>(() => new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>())));
            var dataValueReferences = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new MediaRepository(accessor, AppCaches, Mock.Of<ILogger>(), mediaTypeRepository, tagRepository, Mock.Of<ILanguageRepository>(), relationRepository, relationTypeRepository, propertyEditors, dataValueReferences);
            return repository;
        }

        private DocumentRepository CreateContentRepository(IScopeProvider provider, out IContentTypeRepository contentTypeRepository)
        {
            ITemplateRepository tr;
            return CreateContentRepository(provider, out contentTypeRepository, out tr);
        }

        private DocumentRepository CreateContentRepository(IScopeProvider provider, out IContentTypeRepository contentTypeRepository, out ITemplateRepository templateRepository)
        {
            var accessor = (IScopeAccessor) provider;
            templateRepository = new TemplateRepository(accessor, AppCaches, Logger, TestObjects.GetFileSystemsMock());
            var tagRepository = new TagRepository(accessor, AppCaches, Logger);
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches);
            var languageRepository = new LanguageRepository(accessor, AppCaches, Logger);
            contentTypeRepository = new ContentTypeRepository(accessor, AppCaches, Logger, commonRepository, languageRepository);
            var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Logger);
            var entityRepository = new EntityRepository(accessor);
            var relationRepository = new RelationRepository(accessor, Logger, relationTypeRepository, entityRepository);
            var propertyEditors = new Lazy<PropertyEditorCollection>(() => new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>())));
            var dataValueReferences = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new DocumentRepository(accessor, AppCaches, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository, relationRepository, relationTypeRepository, propertyEditors, dataValueReferences);
            return repository;
        }

        private UserRepository CreateRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor) provider;
            var repository = new UserRepository(accessor, AppCaches.Disabled, Logger, Mappers, TestObjects.GetGlobalSettings());
            return repository;
        }

        private UserGroupRepository CreateUserGroupRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor) provider;
            return new UserGroupRepository(accessor, AppCaches.Disabled, Logger);
        }

        [Test]
        public void Can_Perform_Add_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user = MockedUser.CreateUser();

                // Act
                repository.Save(user);


                // Assert
                Assert.That(user.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user1 = MockedUser.CreateUser("1");
                var use2 = MockedUser.CreateUser("2");

                // Act
                repository.Save(user1);

                repository.Save(use2);


                // Assert
                Assert.That(user1.HasIdentity, Is.True);
                Assert.That(use2.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user = MockedUser.CreateUser();
                repository.Save(user);


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
            var mt = MockedContentTypes.CreateSimpleMediaType("testmedia", "TestMedia");

            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var userRepository = CreateRepository(provider);
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepo);
                var mediaRepository = CreateMediaRepository(provider, out var mediaTypeRepo);
                var userGroupRepository = CreateUserGroupRepository(provider);

                contentTypeRepo.Save(ct);
                mediaTypeRepo.Save(mt);

                var content = MockedContent.CreateBasicContent(ct);
                var media = MockedMedia.CreateSimpleMedia(mt, "asdf", -1);

                contentRepository.Save(content);
                mediaRepository.Save(media);

                var user = CreateAndCommitUserWithGroup(userRepository, userGroupRepository);

                // Act
                var resolved = (User) userRepository.Get(user.Id);

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

                userRepository.Save(resolved);

                var updatedItem = (User) userRepository.Get(user.Id);

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
                Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(resolved.AllowedSections.Count()));
                foreach (var allowedSection in resolved.AllowedSections)
                    Assert.IsTrue(updatedItem.AllowedSections.Contains(allowedSection));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user = MockedUser.CreateUser();

                // Act
                repository.Save(user);

                var id = user.Id;

                var repository2 = new UserRepository((IScopeAccessor) provider, AppCaches.Disabled, Logger, Mock.Of<IMapperCollection>(),TestObjects.GetGlobalSettings());

                repository2.Delete(user);


                var resolved = repository2.Get((int) id);

                // Assert
                Assert.That(resolved, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);
                var userGroupRepository = CreateUserGroupRepository(provider);

                var user = CreateAndCommitUserWithGroup(repository, userGroupRepository);

                // Act
                var updatedItem = repository.Get(user.Id);

                // FIXME: this test cannot work, user has 2 sections but the way it's created,
                // they don't show, so the comparison with updatedItem fails - fix!

                // Assert
                AssertPropertyValues(updatedItem, user);
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                CreateAndCommitMultipleUsers(repository);

                // Act
                var query = scope.SqlContext.Query<IUser>().Where(x => x.Username == "TestUser1");
                var result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var users = CreateAndCommitMultipleUsers(repository);

                // Act
                var result = repository.GetMany((int) users[0].Id, (int) users[1].Id);

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                CreateAndCommitMultipleUsers(repository);

                // Act
                var result = repository.GetMany();

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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var users = CreateAndCommitMultipleUsers(repository);

                // Act
                var exists = repository.Exists(users[0].Id);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Count_On_UserRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var users = CreateAndCommitMultipleUsers(repository);

                // Act
                var query = scope.SqlContext.Query<IUser>().Where(x => x.Username == "TestUser1" || x.Username == "TestUser2");
                var result = repository.Count(query);

                // Assert
                Assert.AreEqual(2, result);
            }
        }

        [Test]
        public void Can_Get_Paged_Results_By_Query_And_Filter_And_Groups()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var users = CreateAndCommitMultipleUsers(repository);
                var query = provider.SqlContext.Query<IUser>().Where(x => x.Username == "TestUser1" || x.Username == "TestUser2");

                try
                {
                    scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    scope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    // Act
                    var result = repository.GetPagedResultsByQuery(query, 0, 10, out var totalRecs, user => user.Id, Direction.Ascending,
                            excludeUserGroups: new[] { Constants.Security.TranslatorGroupAlias },
                            filter: provider.SqlContext.Query<IUser>().Where(x => x.Id > -1));

                    // Assert
                    Assert.AreEqual(2, totalRecs);
                }
                finally
                {
                    scope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                    scope.Database.AsUmbracoDatabase().EnableSqlCount = false;
                }
            }

        }

        [Test]
        public void Can_Get_Paged_Results_With_Filter_And_Groups()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var users = CreateAndCommitMultipleUsers(repository);

                try
                {
                    scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    scope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    // Act
                    var result = repository.GetPagedResultsByQuery(null, 0, 10, out var totalRecs, user => user.Id, Direction.Ascending,
                        includeUserGroups: new[] { Constants.Security.AdminGroupAlias, Constants.Security.SensitiveDataGroupAlias },
                        excludeUserGroups: new[] { Constants.Security.TranslatorGroupAlias },
                        filter: provider.SqlContext.Query<IUser>().Where(x => x.Id == -1));

                    // Assert
                    Assert.AreEqual(1, totalRecs);
                }
                finally
                {
                    scope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                    scope.Database.AsUmbracoDatabase().EnableSqlCount = false;
                }
            }
        }

        [Test]
        public void Can_Invalidate_SecurityStamp_On_Username_Change()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);
                var userGroupRepository = CreateUserGroupRepository(provider);

                var user = CreateAndCommitUserWithGroup(repository, userGroupRepository);
                var originalSecurityStamp = user.SecurityStamp;

                // Ensure when user generated a security stamp is present
                Assert.That(user.SecurityStamp, Is.Not.Null);
                Assert.That(user.SecurityStamp, Is.Not.Empty);

                // Update username
                user.Username = user.Username + "UPDATED";
                repository.Save(user);

                // Get the user
                var updatedUser = repository.Get(user.Id);

                // Ensure the Security Stamp is invalidated & no longer the same
                Assert.AreNotEqual(originalSecurityStamp, updatedUser.SecurityStamp);
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
            Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(originalUser.AllowedSections.Count()));
            foreach (var allowedSection in originalUser.AllowedSections)
                Assert.IsTrue(updatedItem.AllowedSections.Contains(allowedSection));
        }

        private static User CreateAndCommitUserWithGroup(IUserRepository repository, IUserGroupRepository userGroupRepository)
        {
            var user = MockedUser.CreateUser();
            repository.Save(user);


            var group = MockedUserGroup.CreateUserGroup();
            userGroupRepository.AddOrUpdateGroupWithUsers(@group, new[] { user.Id });

            user.AddGroup(group);

            return user;
        }

        private IUser[] CreateAndCommitMultipleUsers(IUserRepository repository)
        {
            var user1 = MockedUser.CreateUser("1");
            var user2 = MockedUser.CreateUser("2");
            var user3 = MockedUser.CreateUser("3");
            repository.Save(user1);
            repository.Save(user2);
            repository.Save(user3);
            return new IUser[] { user1, user2, user3 };
        }
    }
}
