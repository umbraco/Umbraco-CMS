using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

public class CreateMediaControllerTests : ManagementApiUserGroupTestBase<CreateMediaController>
{
    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _folderMediaTypeKey;

    [SetUp]
    public async Task SetUp()
    {
        // Media Folder Type
        var mediaTypes =await MediaTypeEditingService.GetFolderMediaTypes(0, 100);
        var folderMediaType = mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));
        _folderMediaTypeKey = folderMediaType.Key;
    }

    protected override Expression<Func<CreateMediaController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        CreateMediaRequestModel createMediaModel =
            new()
            {
                MediaType = new ReferenceByIdModel(_folderMediaTypeKey),  Variants = new MediaVariantRequestModel[]
                {
                    new() { Culture = null, Segment = null, Name = "The en-US name", },
                },
            };

        return await Client.PostAsync(Url, JsonContent.Create(createMediaModel));
    }
}
