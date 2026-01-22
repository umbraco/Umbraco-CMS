// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.RecycleBin;

public class OriginalParentFolderElementRecycleBinControllerTests : ElementRecycleBinControllerTestBase<OriginalParentElementFolderRecycleBinController>
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private Guid _folderKey;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<EntityContainerMovedNotification, RelateOnTrashNotificationHandler>()
            .AddNotificationAsyncHandler<EntityContainerMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>();
    }

    [SetUp]
    public async Task Setup()
    {
        var folderResult = await ElementContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(folderResult.Success, $"Folder create failed with status: {folderResult.Status}");
        _folderKey = folderResult.Result!.Key;

        var moveResult = await ElementContainerService.MoveToRecycleBinAsync(_folderKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success, $"Failed to move folder to recycle bin: {moveResult.Status}");
    }

    protected override Expression<Func<OriginalParentElementFolderRecycleBinController, object>> MethodSelector =>
        x => x.OriginalParentFolder(CancellationToken.None, _folderKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };
}
