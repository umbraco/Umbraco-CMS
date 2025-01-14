using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserIdKeyResolverTests : UmbracoIntegrationTest
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
        Assert.IsTrue(creationResult.Success);
        var createdUser = creationResult.Result.CreatedUser;
        Assert.IsNotNull(createdUser);

        var resolvedKey = await UserIdKeyResolver.GetAsync(createdUser.Id);
        Assert.AreEqual(createdUser.Key, resolvedKey);
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
        Assert.IsTrue(creationResult.Success);
        var createdUser = creationResult.Result.CreatedUser;
        Assert.IsNotNull(createdUser);

        var resolvedId = await UserIdKeyResolver.GetAsync(createdUser.Key);
        Assert.AreEqual(createdUser.Id, resolvedId);
    }

    [Test]
    public async Task Can_Resolve_Super_User_Key_To_Id()
    {
        var resolvedId = await UserIdKeyResolver.GetAsync(Constants.Security.SuperUserKey);
        Assert.AreEqual(Constants.Security.SuperUserId, resolvedId);
    }

    [Test]
    public async Task Can_Resolve_Super_User_Id_To_Key()
    {
        var resolvedKey = await UserIdKeyResolver.GetAsync(Constants.Security.SuperUserId);
        Assert.AreEqual(Constants.Security.SuperUserKey, resolvedKey);
    }

    [Test]
    public async Task Unknown_Key_Throws()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await UserIdKeyResolver.GetAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task Unknown_Id_Throws()
    {
         Assert.ThrowsAsync<InvalidOperationException>(async () => await UserIdKeyResolver.GetAsync(1234567890));
    }
}
