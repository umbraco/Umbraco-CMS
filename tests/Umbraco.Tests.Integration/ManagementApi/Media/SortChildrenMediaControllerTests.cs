using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

public class SortChildrenMediaControllerTests : ManagementApiUserGroupTestBase<SortChildrenMediaController>
{
    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _parentFolderKey;

    [SetUp]
    public async Task SetUp()
    {
        var mediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0, 100);
        var folderMediaType = mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));

        MediaCreateModel parentCreateModel = new()
        {
            Variants = new List<VariantModel> { new() { Name = "MediaParentFolder" } },
            ContentTypeKey = folderMediaType.Key,
            ParentKey = Constants.System.RootKey,
        };
        var responseParent = await MediaEditingService.CreateAsync(parentCreateModel, Constants.Security.SuperUserKey);
        _parentFolderKey = responseParent.Result.Content.Key;
    }

    protected override Expression<Func<SortChildrenMediaController, object>> MethodSelector =>
        x => x.SortChildren(CancellationToken.None, _parentFolderKey, null);

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
        SortMediaChildrenByFieldRequestModel requestModel = new()
        {
            Field = ContentSortField.Name,
            Direction = Direction.Ascending,
        };

        return await Client.PutAsync(Url, JsonContent.Create(requestModel));
    }
}
