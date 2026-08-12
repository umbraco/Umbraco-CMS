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
public partial class UserStartNodeEntitiesServiceTests : UserStartNodeEntitiesServiceTestsBase
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Document;

    protected override string SectionAlias => "content";

    protected override async Task CreateContentTypeAndHierarchy()
    {
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
            ItemsByName[root.Name!] = (root.Id, root.Key);

            foreach (var childNumber in Enumerable.Range(1, 10))
            {
                var child = new ContentBuilder()
                    .WithContentType(contentType)
                    .WithParent(root)
                    .WithName($"{rootNumber}-{childNumber}")
                    .Build();
                ContentService.Save(child);
                ItemsByName[child.Name!] = (child.Id, child.Key);

                foreach (var grandChildNumber in Enumerable.Range(1, 5))
                {
                    var grandchild = new ContentBuilder()
                        .WithContentType(contentType)
                        .WithParent(child)
                        .WithName($"{rootNumber}-{childNumber}-{grandChildNumber}")
                        .Build();
                    ContentService.Save(grandchild);
                    ItemsByName[grandchild.Name!] = (grandchild.Id, grandchild.Key);
                }
            }
        }
    }

    protected override void ClearUserGroupStartNode(IUserGroup userGroup)
        => userGroup.StartContentId = null;

    protected override Cms.Core.Models.Membership.User BuildUserWithStartNodes(int[] startNodeIds)
        => new UserBuilder()
            .WithName(Guid.NewGuid().ToString("N"))
            .WithStartContentIds(startNodeIds)
            .Build();

    protected override string[]? GetStartNodePaths(Cms.Core.Models.Membership.User user)
        => user.GetContentStartNodePaths(EntityService, AppCaches.NoCache);

    protected override int[]? CalculateStartNodeIds(Cms.Core.Models.Membership.User user)
        => user.CalculateContentStartNodeIds(EntityService, AppCaches.NoCache);
}
