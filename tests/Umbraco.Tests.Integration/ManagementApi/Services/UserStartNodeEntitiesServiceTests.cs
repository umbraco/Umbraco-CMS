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
public partial class UserStartNodeEntitiesServiceTests : UmbracoIntegrationTest
{
    private Dictionary<string, IContent> _contentByName = new ();
    private IUserGroup _userGroup;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

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
        if (_contentByName.Any())
        {
            return;
        }

        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new() { Alias = contentType.Alias, Key = contentType.Key }];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        foreach (var rootNumber in Enumerable.Range(1, 5))
        {
            var root = new ContentBuilder()
                .WithContentType(contentType)
                .WithName($"{rootNumber}")
                .Build();
            ContentService.Save(root);
            _contentByName[root.Name!] = root;

            foreach (var childNumber in Enumerable.Range(1, 10))
            {
                var child = new ContentBuilder()
                    .WithContentType(contentType)
                    .WithParent(root)
                    .WithName($"{rootNumber}-{childNumber}")
                    .Build();
                ContentService.Save(child);
                _contentByName[child.Name!] = child;

                foreach (var grandChildNumber in Enumerable.Range(1, 5))
                {
                    var grandchild = new ContentBuilder()
                        .WithContentType(contentType)
                        .WithParent(child)
                        .WithName($"{rootNumber}-{childNumber}-{grandChildNumber}")
                        .Build();
                    ContentService.Save(grandchild);
                    _contentByName[grandchild.Name!] = grandchild;
                }
            }
        }

        _userGroup = new UserGroupBuilder()
            .WithAlias("theGroup")
            .WithAllowedSections(["content"])
            .Build();
        _userGroup.StartContentId = null;
        await UserGroupService.CreateAsync(_userGroup, Constants.Security.SuperUserKey);
    }

    private async Task<string[]> CreateUserAndGetStartNodePaths(params int[] startNodeIds)
    {
        var user = await CreateUser(startNodeIds);

        var contentStartNodePaths = user.GetContentStartNodePaths(EntityService, AppCaches.NoCache);
        Assert.IsNotNull(contentStartNodePaths);

        return contentStartNodePaths;
    }

    private async Task<int[]> CreateUserAndGetStartNodeIds(params int[] startNodeIds)
    {
        var user = await CreateUser(startNodeIds);

        var contentStartNodeIds = user.CalculateContentStartNodeIds(EntityService, AppCaches.NoCache);
        Assert.IsNotNull(contentStartNodeIds);

        return contentStartNodeIds;
    }

    private async Task<User> CreateUser(int[] startNodeIds)
    {
        var user = new UserBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithStartContentIds(startNodeIds)
            .Build();
        UserService.Save(user);

        var attempt = await UserGroupService.AddUsersToUserGroupAsync(
            new UsersToUserGroupManipulationModel(_userGroup.Key, [user.Key]),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(attempt.Success);
        return user;
    }
}
