using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services;

[TestFixture]
public partial class UserStartNodeEntitiesServiceMediaTests : UserStartNodeEntitiesServiceTestsBase
{
    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    protected override string SectionAlias => "media";

    protected override async Task CreateContentTypeAndHierarchy()
    {
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
            ItemsByName[root.Name!] = (root.Id, root.Key);

            foreach (var childNumber in Enumerable.Range(1, 10))
            {
                var child = new MediaBuilder()
                    .WithMediaType(mediaType)
                    .WithName($"{rootNumber}-{childNumber}")
                    .Build();
                child.SetParent(root);
                MediaService.Save(child);
                ItemsByName[child.Name!] = (child.Id, child.Key);

                foreach (var grandChildNumber in Enumerable.Range(1, 5))
                {
                    var grandchild = new MediaBuilder()
                        .WithMediaType(mediaType)
                        .WithName($"{rootNumber}-{childNumber}-{grandChildNumber}")
                        .Build();
                    grandchild.SetParent(child);
                    MediaService.Save(grandchild);
                    ItemsByName[grandchild.Name!] = (grandchild.Id, grandchild.Key);
                }
            }
        }
    }

    protected override void ClearUserGroupStartNode(IUserGroup userGroup)
        => userGroup.StartMediaId = null;

    protected override Core.Models.Membership.User BuildUserWithStartNodes(int[] startNodeIds)
        => new UserBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithStartMediaIds(startNodeIds)
            .Build();

    protected override string[]? GetStartNodePaths(Core.Models.Membership.User user)
        => user.GetMediaStartNodePaths(EntityService, AppCaches.NoCache);

    protected override int[]? CalculateStartNodeIds(Core.Models.Membership.User user)
        => user.CalculateMediaStartNodeIds(EntityService, AppCaches.NoCache);
}
