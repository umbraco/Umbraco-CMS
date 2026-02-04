// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.RecycleBin;

public class OriginalParentElementRecycleBinControllerTests : ElementRecycleBinControllerTestBase<OriginalParentElementRecycleBinController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _elementKey;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ElementMovedNotification, RelateOnTrashNotificationHandler>()
            .AddNotificationAsyncHandler<ElementMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>();
    }

    [SetUp]
    public async Task Setup()
    {
        // Create a parent folder
        var folderResult = await ElementContainerService.CreateAsync(
            null,
            $"Parent Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(folderResult.Success, $"Folder create failed with status: {folderResult.Status}");
        var parentFolderKey = folderResult.Result!.Key;

        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName($"Test Element {Guid.NewGuid()}")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Create element inside the folder
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = parentFolderKey,
            Variants = [new VariantModel { Name = "Test Element Instance" }],
        };
        var response = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(response.Success, $"Create failed with status: {response.Status}");
        _elementKey = response.Result!.Content!.Key;

        var moveResult = await ElementEditingService.MoveToRecycleBinAsync(_elementKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success, $"Failed to move element to recycle bin: {moveResult.Result}");
    }

    protected override Expression<Func<OriginalParentElementRecycleBinController, object>> MethodSelector =>
        x => x.OriginalParent(CancellationToken.None, _elementKey);

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
