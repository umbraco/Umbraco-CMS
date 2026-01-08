using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

public class MoveMediaControllerTests : ManagementApiUserGroupTestBase<MoveMediaController>
{
    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _moveFolderKey;
    private Guid _targetFolderKey;

    [SetUp]
    public async Task SetUp()
    {
        // Media Folder Type
        var mediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0, 100);
        var folderMediaType = mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));

        // Media MoveFolder
        MediaCreateModel moveCreateModel = new() { InvariantName = "MediaTest", ContentTypeKey = folderMediaType.Key, ParentKey = Constants.System.RootKey};
        var responseMove = await MediaEditingService.CreateAsync(moveCreateModel, Constants.Security.SuperUserKey);
        _moveFolderKey = responseMove.Result.Content.Key;

        // Media TargetFolder
        MediaCreateModel targetCreateModel = new() { InvariantName = "MediaFolder", ContentTypeKey = folderMediaType.Key,  ParentKey = Constants.System.RootKey };
        var responseTarget = await MediaEditingService.CreateAsync(targetCreateModel, Constants.Security.SuperUserKey);
        _targetFolderKey = responseTarget.Result.Content.Key;
    }

    protected override Expression<Func<MoveMediaController, object>> MethodSelector => x => x.Move(CancellationToken.None, _moveFolderKey, null);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveMediaRequestModel updateMediaModel = new() { Target = new ReferenceByIdModel(_targetFolderKey) };
        return await Client.PutAsync(Url, JsonContent.Create(updateMediaModel));
    }
}
