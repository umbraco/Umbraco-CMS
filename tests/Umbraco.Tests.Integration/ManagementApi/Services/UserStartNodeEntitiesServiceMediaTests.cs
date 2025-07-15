using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public partial class UserStartNodeEntitiesServiceMediaTests : UmbracoIntegrationTest
{
    private Dictionary<string, IMedia> _mediaByName = new ();
    private IUserGroup _userGroup;

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private IUserStartNodeEntitiesService UserStartNodeEntitiesService => GetRequiredService<IUserStartNodeEntitiesService>();

    protected readonly Ordering BySortOrder = Ordering.By("sortOrder");

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.AddTransient<IUserStartNodeEntitiesService, UserStartNodeEntitiesService>();
    }

    [SetUp]
    public async Task SetUpTestAsync()
    {
        if (_mediaByName.Any())
        {
            return;
        }

        var mediaType = new MediaTypeBuilder()
            .WithAlias("theMediaType")
            .Build();
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        mediaType.AllowedContentTypes = [new() { Alias = mediaType.Alias, Key = mediaType.Key }];
        await MediaTypeService.UpdateAsync(mediaType, Constants.Security.SuperUserKey);

        foreach (var rootNumber in Enumerable.Range(1, 5))
        {
            var root = new MediaBuilder()
                .WithMediaType(mediaType)
                .WithName($"{rootNumber}")
                .Build();
            MediaService.Save(root);
            _mediaByName[root.Name!] = root;

            foreach (var childNumber in Enumerable.Range(1, 10))
            {
                var child = new MediaBuilder()
                    .WithMediaType(mediaType)
                    .WithName($"{rootNumber}-{childNumber}")
                    .Build();
                child.SetParent(root);
                MediaService.Save(child);
                _mediaByName[child.Name!] = child;

                foreach (var grandChildNumber in Enumerable.Range(1, 5))
                {
                    var grandchild = new MediaBuilder()
                        .WithMediaType(mediaType)
                        .WithName($"{rootNumber}-{childNumber}-{grandChildNumber}")
                        .Build();
                    grandchild.SetParent(child);
                    MediaService.Save(grandchild);
                    _mediaByName[grandchild.Name!] = grandchild;
                }
            }
        }

        _userGroup = new UserGroupBuilder()
            .WithAlias("theGroup")
            .WithAllowedSections(["media"])
            .Build();
        _userGroup.StartMediaId = null;
        await UserGroupService.CreateAsync(_userGroup, Constants.Security.SuperUserKey);
    }

    private async Task<string[]> CreateUserAndGetStartNodePaths(params int[] startNodeIds)
    {
        var user = await CreateUser(startNodeIds);

        var mediaStartNodePaths = user.GetMediaStartNodePaths(EntityService, AppCaches.NoCache);
        Assert.IsNotNull(mediaStartNodePaths);

        return mediaStartNodePaths;
    }

    private async Task<int[]> CreateUserAndGetStartNodeIds(params int[] startNodeIds)
    {
        var user = await CreateUser(startNodeIds);

        var mediaStartNodeIds = user.CalculateMediaStartNodeIds(EntityService, AppCaches.NoCache);
        Assert.IsNotNull(mediaStartNodeIds);

        return mediaStartNodeIds;
    }

    private async Task<User> CreateUser(int[] startNodeIds)
    {
        var user = new UserBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithStartMediaIds(startNodeIds)
            .Build();
        UserService.Save(user);

        var attempt = await UserGroupService.AddUsersToUserGroupAsync(
            new UsersToUserGroupManipulationModel(_userGroup.Key, [user.Key]),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(attempt.Success);
        return user;
    }
}
