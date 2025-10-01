using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media.Tree;

public class ChildrenMediaTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenMediaTreeController>
{
    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _parentKey;

    [SetUp]
    public async Task SetUp()
    {
        // Media Folder Types
        var mediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0, 100);
        var folderMediaType = mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));

        // Parent Media Folder
        MediaCreateModel parentCreateModel = new() { InvariantName = "MediaTest", ContentTypeKey = folderMediaType.Key, ParentKey = Constants.System.RootKey};
        var responseParent = await MediaEditingService.CreateAsync(parentCreateModel, Constants.Security.SuperUserKey);
        _parentKey = responseParent.Result.Content.Key;

        // Child Media Folder
        MediaCreateModel childCreateModel = new() { InvariantName = "MediaTest", ContentTypeKey = folderMediaType.Key, ParentKey = _parentKey};
        await MediaEditingService.CreateAsync(childCreateModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenMediaTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _parentKey, 0, 100, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
