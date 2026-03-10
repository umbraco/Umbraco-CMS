using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

public class SortMediaControllerTests : ManagementApiUserGroupTestBase<SortMediaController>
{
    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _parentFolderKey;
    private Guid _childFolderKey;

    [SetUp]
    public async Task SetUp()
    {
        // Media Folder Type
        var mediaTypes = await MediaTypeEditingService.GetFolderMediaTypes(0,100);
        var folderMediaType = mediaTypes.Items.FirstOrDefault(x => x.Name.Contains("Folder", StringComparison.OrdinalIgnoreCase));

        // Media ParentFolder
        MediaCreateModel parentCreateModel = new()
        {
            Variants = new List<VariantModel> { new() { Name = "MediaParentFolder" } },
            ContentTypeKey = folderMediaType.Key, ParentKey = Constants.System.RootKey
        };
        var responseParent = await MediaEditingService.CreateAsync(parentCreateModel, Constants.Security.SuperUserKey);
        _parentFolderKey = responseParent.Result.Content.Key;

        // Media ChildFolder
        MediaCreateModel childCreateModel = new()
        {
            Variants = new List<VariantModel> { new() { Name = "MediaChildFolder" } },
            ContentTypeKey = folderMediaType.Key, ParentKey = _parentFolderKey
        };
        var responseChild = await MediaEditingService.CreateAsync(childCreateModel, Constants.Security.SuperUserKey);
        _childFolderKey = responseChild.Result.Content.Key;
    }

    protected override Expression<Func<SortMediaController, object>> MethodSelector => x => x.Sort(CancellationToken.None, null);

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
        SortingRequestModel sortingRequestModel = new()
        {
            Parent = new ReferenceByIdModel(_parentFolderKey),
            Sorting = new[] { new ItemSortingRequestModel { Id = _childFolderKey, SortOrder = 0 } },
        };
        return await Client.PutAsync(Url, JsonContent.Create(sortingRequestModel));
    }
}
