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
            UserGroups = new HashSet<IUserGroup> { userGroup! }
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
            UserGroups = new HashSet<IUserGroup> { userGroup! }
        };

        var creationResult = await UserService.CreateAsync(Constants.Security.SuperUserKey, userCreateModel);
        Assert.IsTrue(creationResult.Success);
        var createdUser = creationResult.Result.CreatedUser;
        Assert.IsNotNull(createdUser);

        var resolvedId = await UserIdKeyResolver.GetAsync(createdUser.Key);
        Assert.AreEqual(createdUser.Id, resolvedId);
    }

    [Test]
    public async Task Unknown_Key_Resolves_To_Null()
    {
        var resolvedId = await UserIdKeyResolver.GetAsync(Guid.NewGuid());
        Assert.IsNull(resolvedId);
    }

    [Test]
    public async Task Unknown_Id_Resolves_To_Null()
    {
        var resolvedKey = await UserIdKeyResolver.GetAsync(1234567890);
        Assert.IsNull(resolvedKey);
    }
}
