// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UserGroupRepositoryTest : UmbracoIntegrationTest
    {
        private UserGroupRepository CreateRepository(IScopeProvider provider) =>
            new UserGroupRepository((IScopeAccessor)provider, global::Umbraco.Cms.Core.Cache.AppCaches.Disabled, LoggerFactory.CreateLogger<UserGroupRepository>(), LoggerFactory, ShortStringHelper);

        [Test]
        public void Can_Perform_Add_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup userGroup = UserGroupBuilder.CreateUserGroup();

                // Act
                repository.Save(userGroup);
                scope.Complete();

                // Assert
                Assert.That(userGroup.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup userGroup1 = UserGroupBuilder.CreateUserGroup(suffix: "1");
                UserGroup userGroup2 = UserGroupBuilder.CreateUserGroup(suffix: "2");

                // Act
                repository.Save(userGroup1);

                repository.Save(userGroup2);
                scope.Complete();

                // Assert
                Assert.That(userGroup1.HasIdentity, Is.True);
                Assert.That(userGroup2.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup userGroup = UserGroupBuilder.CreateUserGroup();
                repository.Save(userGroup);
                scope.Complete();

                // Act
                IUserGroup resolved = repository.Get((int)userGroup.Id);
                bool dirty = ((UserGroup)resolved).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup userGroup = UserGroupBuilder.CreateUserGroup();
                repository.Save(userGroup);

                // Act
                IUserGroup resolved = repository.Get((int)userGroup.Id);
                resolved.Name = "New Name";
                resolved.Permissions = new[] { "Z", "Y", "X" };
                repository.Save(resolved);
                scope.Complete();
                IUserGroup updatedItem = repository.Get((int)userGroup.Id);

                // Assert
                Assert.That(updatedItem.Id, Is.EqualTo(resolved.Id));
                Assert.That(updatedItem.Name, Is.EqualTo(resolved.Name));
                Assert.That(updatedItem.Permissions, Is.EqualTo(resolved.Permissions));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserGroupRepository()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup userGroup = UserGroupBuilder.CreateUserGroup();

                // Act
                repository.Save(userGroup);

                int id = userGroup.Id;

                var repository2 = new UserGroupRepository((IScopeAccessor)provider, global::Umbraco.Cms.Core.Cache.AppCaches.Disabled, LoggerFactory.CreateLogger<UserGroupRepository>(), LoggerFactory, ShortStringHelper);
                repository2.Delete(userGroup);
                scope.Complete();

                IUserGroup resolved = repository2.Get((int)id);

                // Assert
                Assert.That(resolved, Is.Null);
            }
        }

        [Test]
        public void Can_Perform_Get_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup userGroup = UserGroupBuilder.CreateUserGroup();
                repository.Save(userGroup);
                scope.Complete();

                // Act
                IUserGroup resolved = repository.Get((int)userGroup.Id);

                // Assert
                Assert.That(resolved.Id, Is.EqualTo(userGroup.Id));
                //// Assert.That(resolved.CreateDate, Is.GreaterThan(DateTime.MinValue));
                //// Assert.That(resolved.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(resolved.Name, Is.EqualTo(userGroup.Name));
                Assert.That(resolved.Alias, Is.EqualTo(userGroup.Alias));
                Assert.That(resolved.Permissions, Is.EqualTo(userGroup.Permissions));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                CreateAndCommitMultipleUserGroups(repository);

                // Act
                IQuery<IUserGroup> query = scope.SqlContext.Query<IUserGroup>().Where(x => x.Alias == "testGroup1");
                IEnumerable<IUserGroup> result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                IUserGroup[] userGroups = CreateAndCommitMultipleUserGroups(repository);

                // Act
                IEnumerable<IUserGroup> result = repository.GetMany(userGroups[0].Id, userGroups[1].Id);

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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                CreateAndCommitMultipleUserGroups(repository);

                // Act
                IEnumerable<IUserGroup> result = repository.GetMany();

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
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                IUserGroup[] userGroups = CreateAndCommitMultipleUserGroups(repository);

                // Act
                bool exists = repository.Exists(userGroups[0].Id);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Count_On_UserGroupRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                IUserGroup[] userGroups = CreateAndCommitMultipleUserGroups(repository);

                // Act
                IQuery<IUserGroup> query = scope.SqlContext.Query<IUserGroup>().Where(x => x.Alias == "testGroup1" || x.Alias == "testGroup2");
                int result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void Can_Remove_Section_For_Group()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                IUserGroup[] groups = CreateAndCommitMultipleUserGroups(repository);

                // Act

                // add and remove a few times, this tests the internal collection
                groups[0].RemoveAllowedSection("content");
                groups[0].RemoveAllowedSection("content");
                groups[0].AddAllowedSection("content");
                groups[0].RemoveAllowedSection("content");

                groups[1].RemoveAllowedSection("media");
                groups[1].RemoveAllowedSection("media");

                repository.Save(groups[0]);
                repository.Save(groups[1]);
                scope.Complete();

                // Assert
                IUserGroup[] result = repository.GetMany((int)groups[0].Id, (int)groups[1].Id).ToArray();
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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                IUserGroup[] groups = CreateAndCommitMultipleUserGroups(repository);

                // Act

                // add and remove a few times, this tests the internal collection
                groups[0].ClearAllowedSections();
                groups[0].AddAllowedSection("content");
                groups[0].AddAllowedSection("media");
                groups[0].RemoveAllowedSection("content");
                groups[0].AddAllowedSection("content");
                groups[0].AddAllowedSection("settings");

                // add the same even though it's already there
                groups[0].AddAllowedSection("content");

                groups[1].ClearAllowedSections();
                groups[1].AddAllowedSection("developer");

                groups[2].ClearAllowedSections();

                repository.Save(groups[0]);
                repository.Save(groups[1]);
                repository.Save(groups[2]);
                scope.Complete();

                for (int i = 0; i < 3; i++)
                {
                    Assert.IsNotNull(repository.Get(groups[i].Id));
                }

                // Assert
                IUserGroup[] result = repository.GetMany(groups[0].Id, groups[1].Id, groups[2].Id).ToArray();
                Assert.AreEqual(3, result.Length);

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                IUserGroup[] groups = CreateAndCommitMultipleUserGroups(repository);

                // Act
                groups[0].RemoveAllowedSection("content");
                groups[0].AddAllowedSection("settings");

                repository.Save(groups[0]);
                scope.Complete();

                // Assert
                IUserGroup result = repository.Get(groups[0].Id);
                Assert.AreEqual(2, result.AllowedSections.Count());
                Assert.IsTrue(result.AllowedSections.Contains("settings"));
                Assert.IsTrue(result.AllowedSections.Contains("media"));
            }
        }

        [Test]
        public void Get_Groups_Assigned_To_Section()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                UserGroupRepository repository = CreateRepository(provider);

                UserGroup user1 = UserGroupBuilder.CreateUserGroup(suffix: "1", allowedSections: new[] { "test1" });
                UserGroup user2 = UserGroupBuilder.CreateUserGroup(suffix: "2", allowedSections: new[] { "test2" });
                UserGroup user3 = UserGroupBuilder.CreateUserGroup(suffix: "3", allowedSections: new[] { "test1" });
                repository.Save(user1);
                repository.Save(user2);
                repository.Save(user3);
                scope.Complete();

                // Act
                IEnumerable<IUserGroup> groups = repository.GetGroupsAssignedToSection("test1");

                // Assert
                Assert.AreEqual(2, groups.Count());
                string[] names = groups.Select(x => x.Name).ToArray();
                Assert.IsTrue(names.Contains("Test Group1"));
                Assert.IsFalse(names.Contains("Test Group2"));
                Assert.IsTrue(names.Contains("Test Group3"));
            }
        }

        private IUserGroup[] CreateAndCommitMultipleUserGroups(IUserGroupRepository repository)
        {
            UserGroup userGroup1 = UserGroupBuilder.CreateUserGroup(suffix: "1");
            UserGroup userGroup2 = UserGroupBuilder.CreateUserGroup(suffix: "2");
            UserGroup userGroup3 = UserGroupBuilder.CreateUserGroup(suffix: "3");
            repository.Save(userGroup1);
            repository.Save(userGroup2);
            repository.Save(userGroup3);

            return new IUserGroup[] { userGroup1, userGroup2, userGroup3 };
        }
    }
}
