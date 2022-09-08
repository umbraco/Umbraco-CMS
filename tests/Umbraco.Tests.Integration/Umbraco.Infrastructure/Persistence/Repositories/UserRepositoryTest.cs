// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
public class UserRepositoryTest : UmbracoIntegrationTest
{
    private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();

    private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

    private IMediaTypeRepository MediaTypeRepository => GetRequiredService<IMediaTypeRepository>();

    private IMediaRepository MediaRepository => GetRequiredService<IMediaRepository>();

    private UserRepository CreateRepository(ICoreScopeProvider provider)
    {
        var accessor = (IScopeAccessor)provider;
        var mockRuntimeState = CreateMockRuntimeState(RuntimeLevel.Run);

        var repository = new UserRepository(
            accessor,
            AppCaches.Disabled,
            LoggerFactory.CreateLogger<UserRepository>(),
            Mappers,
            Options.Create(GlobalSettings),
            Options.Create(new UserPasswordConfigurationSettings()),
            new JsonNetSerializer(),
            mockRuntimeState.Object);
        return repository;
    }

    private static Mock<IRuntimeState> CreateMockRuntimeState(RuntimeLevel runtimeLevel)
    {
        var mockRuntimeState = new Mock<IRuntimeState>();
        mockRuntimeState.SetupGet(x => x.Level).Returns(runtimeLevel);
        return mockRuntimeState;
    }

    private UserGroupRepository CreateUserGroupRepository(ICoreScopeProvider provider)
    {
        var accessor = (IScopeAccessor)provider;
        return new UserGroupRepository(accessor, AppCaches.Disabled, LoggerFactory.CreateLogger<UserGroupRepository>(), LoggerFactory, ShortStringHelper);
    }

    [Test]
    public void Can_Perform_Add_On_UserRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
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
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
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
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            var user = UserBuilderInstance.WithoutIdentity().Build();
            repository.Save(user);

            // Act
            var resolved = repository.Get(user.Id);
            var dirty = ((User)resolved).IsDirty();

            // Assert
            Assert.That(dirty, Is.False);
        }
    }

    [Test]
    public void Can_Perform_Delete_On_UserRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            var user = UserBuilderInstance.Build();

            // Act
            repository.Save(user);

            var id = user.Id;

            var mockRuntimeState = CreateMockRuntimeState(RuntimeLevel.Run);

            var repository2 = new UserRepository(
                (IScopeAccessor)provider,
                AppCaches.Disabled,
                LoggerFactory.CreateLogger<UserRepository>(),
                Mock.Of<IMapperCollection>(),
                Options.Create(GlobalSettings),
                Options.Create(new UserPasswordConfigurationSettings()),
                new JsonNetSerializer(),
                mockRuntimeState.Object);

            repository2.Delete(user);

            var resolved = repository2.Get(id);

            // Assert
            Assert.That(resolved, Is.Null);
        }
    }

    [Test]
    public void Can_Perform_Get_On_UserRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
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
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            CreateAndCommitMultipleUsers(repository);

            // Act
            var query = ScopeProvider.CreateQuery<IUser>().Where(x => x.Username == "TestUser1");
            var result = repository.Get(query);

            // Assert
            Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
        }
    }

    [Test]
    public void Can_Perform_GetAll_By_Param_Ids_On_UserRepository()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            var users = CreateAndCommitMultipleUsers(repository);

            // Act
            var result = repository.GetMany(users[0].Id, users[1].Id).ToArray();

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
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            CreateAndCommitMultipleUsers(repository);

            // Act
            var result = repository.GetMany().ToArray();

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
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
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
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            var users = CreateAndCommitMultipleUsers(repository);

            // Act
            var query = ScopeProvider.CreateQuery<IUser>()
                .Where(x => x.Username == "TestUser1" || x.Username == "TestUser2");
            var result = repository.Count(query);

            // Assert
            Assert.AreEqual(2, result);
        }
    }

    [Test]
    public void Can_Get_Paged_Results_By_Query_And_Filter_And_Groups()
    {
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            var users = CreateAndCommitMultipleUsers(repository);
            var query = provider.CreateQuery<IUser>()
                .Where(x => x.Username == "TestUser1" || x.Username == "TestUser2");

            try
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                // Act
                var result = repository.GetPagedResultsByQuery(
                    query,
                    0,
                    10,
                    out var totalRecs,
                    user => user.Id,
                    Direction.Ascending,
                    excludeUserGroups: new[] { Constants.Security.TranslatorGroupAlias },
                    filter: provider.CreateQuery<IUser>().Where(x => x.Id > -1));

                // Assert
                Assert.AreEqual(2, totalRecs);
            }
            finally
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = false;
            }
        }
    }

    [Test]
    public void Can_Get_Paged_Results_With_Filter_And_Groups()
    {
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);

            var users = CreateAndCommitMultipleUsers(repository);

            try
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                // Act
                var result = repository.GetPagedResultsByQuery(
                    null,
                    0,
                    10,
                    out var totalRecs,
                    user => user.Id,
                    Direction.Ascending,
                    new[] { Constants.Security.AdminGroupAlias, Constants.Security.SensitiveDataGroupAlias },
                    new[] { Constants.Security.TranslatorGroupAlias },
                    filter: provider.CreateQuery<IUser>().Where(x => x.Id == -1));

                // Assert
                Assert.AreEqual(1, totalRecs);
            }
            finally
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = false;
            }
        }
    }

    [Test]
    public void Can_Invalidate_SecurityStamp_On_Username_Change()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope())
        {
            var repository = CreateRepository(provider);
            var userGroupRepository = CreateUserGroupRepository(provider);

            var user = CreateAndCommitUserWithGroup(repository, userGroupRepository);
            var originalSecurityStamp = user.SecurityStamp;

            // Ensure when user generated a security stamp is present
            Assert.That(user.SecurityStamp, Is.Not.Null);
            Assert.That(user.SecurityStamp, Is.Not.Empty);

            // Update username
            user.Username += "UPDATED";
            repository.Save(user);

            // Get the user
            var updatedUser = repository.Get(user.Id);

            // Ensure the Security Stamp is invalidated & no longer the same
            Assert.AreNotEqual(originalSecurityStamp, updatedUser.SecurityStamp);
        }
    }

    [Test]
    public void Validate_Login_Session()
    {
        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        var user = UserBuilder.CreateUser();
        using (var scope = provider.CreateCoreScope(autoComplete: true))
        {
            var repository = CreateRepository(provider);
            repository.Save(user);
        }

        using (var scope = provider.CreateCoreScope(autoComplete: true))
        {
            var repository = CreateRepository(provider);
            var sessionId = repository.CreateLoginSession(user.Id, "1.2.3.4");

            // manually update this record to be in the past
            ScopeAccessor.AmbientScope.Database.Execute(ScopeAccessor.AmbientScope.SqlContext.Sql()
                .Update<UserLoginDto>(u => u.Set(x => x.LoggedOutUtc, DateTime.UtcNow.AddDays(-100)))
                .Where<UserLoginDto>(x => x.SessionId == sessionId));

            var isValid = repository.ValidateLoginSession(user.Id, sessionId);
            Assert.IsFalse(isValid);

            // create a new one
            sessionId = repository.CreateLoginSession(user.Id, "1.2.3.4");
            isValid = repository.ValidateLoginSession(user.Id, sessionId);
            Assert.IsTrue(isValid);
        }
    }

    [Test]
    public void Can_Perform_Update_On_UserRepository()
    {
        var ct = ContentTypeBuilder.CreateBasicContentType("test");
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testmedia", "TestMedia");

        // Arrange
        ICoreScopeProvider provider = ScopeProvider;
        using (var scope = provider.CreateCoreScope(autoComplete: true))
        {
            var userRepository = CreateRepository(provider);
            var userGroupRepository = CreateUserGroupRepository(provider);

            ContentTypeRepository.Save(ct);
            MediaTypeRepository.Save(mt);

            var content = ContentBuilder.CreateBasicContent(ct);
            var media = MediaBuilder.CreateSimpleMedia(mt, "asdf", -1);

            DocumentRepository.Save(content);
            MediaRepository.Save(media);

            var user = CreateAndCommitUserWithGroup(userRepository, userGroupRepository);

            // Act
            var resolved = (User)userRepository.Get(user.Id);

            resolved.Name = "New Name";

            // the db column is not used, default permissions are taken from the user type's permissions, this is a getter only
            //// resolved.DefaultPermissions = "ZYX";

            resolved.Language = "fr";
            resolved.IsApproved = false;
            resolved.RawPasswordValue = "new";
            resolved.IsLockedOut = true;
            resolved.StartContentIds = new[] { content.Id };
            resolved.StartMediaIds = new[] { media.Id };
            resolved.Email = "new@new.com";
            resolved.Username = "newName";

            userRepository.Save(resolved);

            var updatedItem = (User)userRepository.Get(user.Id);

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
            {
                Assert.IsTrue(updatedItem.AllowedSections.Contains(allowedSection));
            }
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
        {
            Assert.IsTrue(updatedItem.AllowedSections.Contains(allowedSection));
        }
    }

    private User CreateAndCommitUserWithGroup(IUserRepository repository, IUserGroupRepository userGroupRepository)
    {
        var user = UserBuilderInstance.WithoutIdentity().Build();
        repository.Save(user);

        var group = UserGroupBuilderInstance.Build();
        userGroupRepository.AddOrUpdateGroupWithUsers(group, new[] { user.Id });

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
