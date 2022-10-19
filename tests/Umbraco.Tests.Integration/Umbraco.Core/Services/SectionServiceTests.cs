// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the SectionService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class SectionServiceTests : UmbracoIntegrationTest
{
    private ISectionService SectionService => GetRequiredService<ISectionService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public void SectionService_Can_Get_Allowed_Sections_For_User()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = SectionService.GetAllowedSections(user.Id).ToList();

        // Assert
        Assert.AreEqual(3, result.Count);
    }

    private IUser CreateTestUser()
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        using var _ = scope.Notifications.Suppress();

        var globalSettings = new GlobalSettings();
        var user = new User(globalSettings) { Name = "Test user", Username = "testUser", Email = "testuser@test.com" };
        UserService.Save(user);

        var userGroupA = new UserGroup(ShortStringHelper) { Alias = "GroupA", Name = "Group A" };
        userGroupA.AddAllowedSection("media");
        userGroupA.AddAllowedSection("settings");

        // TODO: This is failing the test
        UserService.Save(userGroupA, new[] { user.Id });

        var userGroupB = new UserGroup(ShortStringHelper) { Alias = "GroupB", Name = "Group B" };
        userGroupB.AddAllowedSection("settings");
        userGroupB.AddAllowedSection("member");
        UserService.Save(userGroupB, new[] { user.Id });

        return UserService.GetUserById(user.Id);
    }
}
