using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UserIdKeyResolverTests : UmbracoIntegrationTest
{
    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserIdKeyResolver UserIdKeyResolver => GetRequiredService<IUserIdKeyResolver>();

    [Test]
    public async Task Can_Resolve_Id_To_Key()
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var userCreateModel = new UserCreateModel
        {
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var creationResult = await UserService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel);
        Assert.That(creationResult.Success, Is.True);
        var createdUser = creationResult.Result.CreatedUser;
        Assert.That(createdUser, Is.Not.Null);

        var resolvedKey = await UserIdKeyResolver.GetAsync(createdUser.Id);
        Assert.That(resolvedKey, Is.EqualTo(createdUser.Key));
    }

    [Test]
    public async Task Can_Resolve_Key_To_Id()
    {
        var userGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var userCreateModel = new UserCreateModel
        {
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test Mc. Gee",
            UserGroupKeys = new HashSet<Guid> { userGroup.Key }
        };

        var creationResult = await UserService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel);
        Assert.That(creationResult.Success, Is.True);
        var createdUser = creationResult.Result.CreatedUser;
        Assert.That(createdUser, Is.Not.Null);

        var resolvedId = await UserIdKeyResolver.GetAsync(createdUser.Key);
        Assert.That(resolvedId, Is.EqualTo(createdUser.Id));
    }

    [Test]
    public async Task Can_Resolve_Super_User_Key_To_Id()
    {
        var resolvedId = await UserIdKeyResolver.GetAsync(Constants.Security.SuperUserKey);
        Assert.That(resolvedId, Is.EqualTo(Constants.Security.SuperUserId));
    }

    [Test]
    public async Task Can_Resolve_Super_User_Id_To_Key()
    {
        var resolvedKey = await UserIdKeyResolver.GetAsync(Constants.Security.SuperUserId);
        Assert.That(resolvedKey, Is.EqualTo(Constants.Security.SuperUserKey));
    }

    [Test]
    public Task Unknown_Key_Throws()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await UserIdKeyResolver.GetAsync(Guid.NewGuid()));
        return Task.CompletedTask;
    }

    [Test]
    public Task Unknown_Id_Throws()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await UserIdKeyResolver.GetAsync(1234567890));
        return Task.CompletedTask;
    }
}
