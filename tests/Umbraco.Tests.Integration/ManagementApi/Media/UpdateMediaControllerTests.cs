using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

public class UpdateMediaControllerTests : ManagementApiUserGroupTestBase<UpdateMediaController>
{
    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _mediaKey;

    [SetUp]
    public async Task SetUp()
    {
        // Media Folder Type
        var mediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0, 100);
        var folderMediaType =
            mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));

        // Media Folder
        MediaCreateModel mediaCreateModel = new()
        {
            Variants = new List<VariantModel> { new() { Name = "MediaTest" } },
            ContentTypeKey = folderMediaType.Key,
        };
        var response = await MediaEditingService.CreateAsync(mediaCreateModel, Constants.Security.SuperUserKey);
        _mediaKey = response.Result.Content.Key;
    }

    protected override Expression<Func<UpdateMediaController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _mediaKey, null);

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
        UpdateMediaRequestModel updateMediaModel = new()
        {
            Variants = new MediaVariantRequestModel[]
            {
                new() { Culture = null, Segment = null, Name = "NewName", },
            },
        };

        return await Client.PutAsync(Url, JsonContent.Create(updateMediaModel));
    }
}
