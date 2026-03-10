using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class GetMediaPermissionsCurrentUserControllerTests : ManagementApiUserGroupTestBase<GetMediaPermissionsCurrentUserController>
{
    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _mediaKey;

    [SetUp]
    public async Task SetUp()
    {
        var mediaTypeCreateAttempt = await MediaTypeEditingService.CreateAsync(
            MediaTypeEditingBuilder.CreateBasicMediaType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
            Constants.Security.SuperUserKey);
        Assert.IsTrue(mediaTypeCreateAttempt.Success);

        var response = await MediaEditingService.CreateAsync(
            MediaEditingBuilder.CreateBasicMedia(mediaTypeCreateAttempt.Result.Key, null),
            Constants.Security.SuperUserKey);
        Assert.IsTrue(response.Success);
        _mediaKey = response.Result.Content!.Key;
    }

    protected override Expression<Func<GetMediaPermissionsCurrentUserController, object>> MethodSelector
        => x => x.GetPermissions(CancellationToken.None, null!);

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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
        => await Client.GetAsync($"{Url}?id={_mediaKey}");
}
