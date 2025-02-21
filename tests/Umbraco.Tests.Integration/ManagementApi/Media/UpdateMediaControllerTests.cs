using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

[TestFixture]
public class UpdateMediaControllerTests : ManagementApiUserGroupTestBase<UpdateMediaController>
{
    private Guid _mediaKey = Guid.Empty;

    // We need to setup a media item. The reason for this is that the permission MediaPermissionByResource requires a media item to be present otherwise you will get the forbidden response.
    [SetUp]
    public async Task Setup()
    {
        var mediaService = GetRequiredService<IMediaService>();
        var media = mediaService.CreateMedia("Test", -1, Constants.Conventions.MediaTypes.Folder);
        mediaService.Save(media);
        _mediaKey = media.Key;
    }

    protected override Expression<Func<UpdateMediaController, object>> MethodSelector => x => x.Update(CancellationToken.None, _mediaKey, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
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
        IEnumerable<MediaValueModel> values = new List<MediaValueModel> { new() { Alias = "test", Value = "test" } };

        UpdateMediaRequestModel updateMediaModel = new() { Values = values };

        return await Client.PutAsync(Url, JsonContent.Create(updateMediaModel));
    }
}
