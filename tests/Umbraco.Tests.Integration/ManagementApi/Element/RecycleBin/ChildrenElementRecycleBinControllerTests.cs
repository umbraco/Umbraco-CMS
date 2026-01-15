using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.RecycleBin;

public class ChildrenElementRecycleBinControllerTests : ElementRecycleBinControllerTestBase<ChildrenElementRecycleBinController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _folderKey;

    [SetUp]
    public async Task Setup()
    {
        // Folder
        var folderResult = await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        _folderKey = folderResult.Result!.Key;

        // Element Type
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Element inside folder
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = _folderKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };
        await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Move folder to recycle bin (this will move the element inside it too)
        await ElementContainerService.MoveToRecycleBinAsync(_folderKey, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenElementRecycleBinController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _folderKey, 0, 100);

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
