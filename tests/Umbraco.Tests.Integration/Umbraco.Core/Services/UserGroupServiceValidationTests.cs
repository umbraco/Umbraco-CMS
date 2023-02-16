using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserGroupServiceValidationTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [Test]
    public async Task Cannot_create_user_group_with_name_equals_null()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = null
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserId);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.MissingName, result.Status);
    }

    [Test]
    public async Task Cannot_create_user_group_with_name_longer_than_max_length()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Sed porttitor lectus nibh. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim."
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserId);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.NameTooLong, result.Status);
    }

    [Test]
    public async Task Cannot_create_user_group_with_alias_longer_than_max_length()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "Sed porttitor lectus nibh. Vivamus magna justo, lacinia eget consectetur sed, convallis at tellus. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim. Vivamus suscipit tortor eget felis porttitor volutpat. Quisque velit nisi, pretium ut lacinia in, elementum id enim."
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserId);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.AliasTooLong, result.Status);
    }

    [Test]
    public async Task Cannot_update_non_existing_user_group()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias"
        };

        var result = await UserGroupService.UpdateAsync(userGroup, Constants.Security.SuperUserId);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_create_existing_user_group()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Some Name",
            Alias = "someAlias"
        };

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserId);

        Assert.IsTrue(result.Success);

        result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserId);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserGroupOperationStatus.AlreadyExists, result.Status);
    }
}
