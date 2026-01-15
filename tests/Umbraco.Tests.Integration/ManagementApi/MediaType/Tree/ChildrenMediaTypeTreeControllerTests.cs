using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType.Tree;

public class ChildrenMediaTypeTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenMediaTypeTreeController>
{
    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _parentKey;

    [SetUp]
    public async Task SetUp()
    {
        // Parent Media Type
        var mediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0, 100);
        var folderMediaType = mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));
        _parentKey = folderMediaType.Key;

        // Child Media Type
        MediaTypeCreateModel mediaTypeCreateModel = new() { Name = Guid.NewGuid().ToString(), Alias = Guid.NewGuid().ToString(), Key = Guid.NewGuid(), ContainerKey = _parentKey };
        await MediaTypeEditingService.CreateAsync(mediaTypeCreateModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenMediaTypeTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _parentKey, 0, 100, false);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
