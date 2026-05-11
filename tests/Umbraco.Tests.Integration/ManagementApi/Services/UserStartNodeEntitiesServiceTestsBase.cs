using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

/// <summary>
/// Abstract base class for UserStartNodeEntitiesService tests.
/// Provides common setup, services, and helper methods for testing start node access
/// across different content types (Document, Media, Element).
/// </summary>
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public abstract class UserStartNodeEntitiesServiceTestsBase : UmbracoIntegrationTest
{
    /// <summary>
    /// All items by name, storing just the Id and Key needed for tests.
    /// </summary>
    protected Dictionary<string, (int Id, Guid Key)> ItemsByName { get; } = new();

    protected IUserGroup UserGroup { get; set; } = null!;

    protected IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected IUserService UserService => GetRequiredService<IUserService>();

    protected IEntityService EntityService => GetRequiredService<IEntityService>();

    protected IUserStartNodeEntitiesService UserStartNodeEntitiesService => GetRequiredService<IUserStartNodeEntitiesService>();

    protected static readonly Ordering BySortOrder = Ordering.By("sortOrder");

    /// <summary>
    /// Gets the UmbracoObjectType for the content type being tested.
    /// </summary>
    protected abstract UmbracoObjectTypes ObjectType { get; }

    /// <summary>
    /// Gets the section alias for the user group (e.g., "content", "media", "element").
    /// </summary>
    protected abstract string SectionAlias { get; }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.AddTransient<IUserStartNodeEntitiesService, UserStartNodeEntitiesService>();
    }

    [SetUp]
    public async Task SetUpTestAsync()
    {
        if (ItemsByName.Any())
        {
            return;
        }

        await CreateContentTypeAndHierarchy();
        await CreateUserGroup();
    }

    /// <summary>
    /// Creates the content type and a hierarchy of items:
    /// 5 roots, each with 10 children, each with 5 grandchildren (300 total items).
    /// Items are named by their position: "1", "1-1", "1-1-1", etc.
    /// </summary>
    protected abstract Task CreateContentTypeAndHierarchy();

    /// <summary>
    /// Creates the user group for testing.
    /// </summary>
    protected virtual async Task CreateUserGroup()
    {
        UserGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithAllowedSections([SectionAlias])
            .Build();

        ClearUserGroupStartNode(UserGroup);
        await UserGroupService.CreateAsync(UserGroup, Constants.Security.SuperUserKey);
    }

    /// <summary>
    /// Clears the start node for the user group (type-specific implementation).
    /// </summary>
    protected abstract void ClearUserGroupStartNode(IUserGroup userGroup);

    /// <summary>
    /// Creates a user with the specified start node IDs and returns the calculated start node paths.
    /// </summary>
    protected async Task<string[]> CreateUserAndGetStartNodePaths(params int[] startNodeIds)
    {
        var user = await CreateUser(startNodeIds);
        var paths = GetStartNodePaths(user);
        Assert.IsNotNull(paths);
        return paths;
    }

    /// <summary>
    /// Creates a user with the specified start node IDs and returns the calculated start node IDs.
    /// </summary>
    protected async Task<int[]> CreateUserAndGetStartNodeIds(params int[] startNodeIds)
    {
        var user = await CreateUser(startNodeIds);
        var ids = CalculateStartNodeIds(user);
        Assert.IsNotNull(ids);
        return ids;
    }

    /// <summary>
    /// Creates a user with the specified start node IDs.
    /// </summary>
    protected async Task<Core.Models.Membership.User> CreateUser(int[] startNodeIds)
    {
        var user = BuildUserWithStartNodes(startNodeIds);
        UserService.Save(user);

        var attempt = await UserGroupService.AddUsersToUserGroupAsync(
            new UsersToUserGroupManipulationModel(UserGroup.Key, [user.Key]),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(attempt.Success);
        return user;
    }

    /// <summary>
    /// Builds a user with the specified start node IDs (type-specific implementation).
    /// </summary>
    protected abstract Cms.Core.Models.Membership.User BuildUserWithStartNodes(int[] startNodeIds);

    /// <summary>
    /// Gets the start node paths for the user (type-specific implementation).
    /// </summary>
    protected abstract string[]? GetStartNodePaths(Cms.Core.Models.Membership.User user);

    /// <summary>
    /// Calculates the start node IDs for the user (type-specific implementation).
    /// </summary>
    protected abstract int[]? CalculateStartNodeIds(Cms.Core.Models.Membership.User user);
}
