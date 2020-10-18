using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Serialization;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
    public class UserRepositoryTest : UmbracoIntegrationTest
    {
        private UserRepository CreateRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor) provider;
            var repository = new UserRepository(accessor, AppCaches.Disabled, LoggerFactory.CreateLogger<UserRepository>(), Mappers, Options.Create(GlobalSettings), Options.Create(new UserPasswordConfigurationSettings()), new JsonNetSerializer());
            return repository;
        }

        private UserGroupRepository CreateUserGroupRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor) provider;
            return new UserGroupRepository(accessor, AppCaches.Disabled, LoggerFactory.CreateLogger<UserGroupRepository>(), LoggerFactory, ShortStringHelper);
        }

        [Test]
        public void Can_Perform_Add_On_UserRepository()
        {
            // Arrange
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user = UserBuilderInstance.Build();

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
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user1 = UserBuilderInstance.WithSuffix("1").Build();
                var use2 = UserBuilderInstance.WithSuffix("2").Build();

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
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user = UserBuilderInstance.WithoutIdentity().Build();
                repository.Save(user);


                // Act
                var resolved = repository.Get((int)user.Id);
                bool dirty = ((User)resolved).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserRepository()
        {
            // Arrange
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var user = UserBuilderInstance.Build();

                // Act
                repository.Save(user);

                var id = user.Id;

                var repository2 = new UserRepository((IScopeAccessor) provider, AppCaches.Disabled, LoggerFactory.CreateLogger<UserRepository>(), Mock.Of<IMapperCollection>(), Options.Create(GlobalSettings), Options.Create(new UserPasswordConfigurationSettings()), new JsonNetSerializer());

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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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
            var provider = ScopeProvider;
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

        private User CreateAndCommitUserWithGroup(IUserRepository repository, IUserGroupRepository userGroupRepository)
        {
            var user = UserBuilderInstance.WithoutIdentity().Build();
            repository.Save(user);

            var group = UserGroupBuilderInstance.Build();
            userGroupRepository.AddOrUpdateGroupWithUsers(@group, new[] { user.Id });

            user.AddGroup(UserGroupBuilderInstance.BuildReadOnly(group));

            return user;
        }

        private IUser[] CreateAndCommitMultipleUsers(IUserRepository repository)
        {
            var user1 = UserBuilderInstance.WithoutIdentity().WithSuffix("1").Build();
            var user2 = UserBuilderInstance.WithoutIdentity().WithSuffix("2").Build();
            var user3 = UserBuilderInstance.WithoutIdentity().WithSuffix("3").Build();
            repository.Save(user1);
            repository.Save(user2);
            repository.Save(user3);
            return new IUser[] { user1, user2, user3 };
        }
    }
}
