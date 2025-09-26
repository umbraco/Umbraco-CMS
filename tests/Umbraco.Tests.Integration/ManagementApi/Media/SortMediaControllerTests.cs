using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

public class SortMediaControllerTests : ManagementApiUserGroupTestBase<SortMediaController>
{
    protected override Expression<Func<SortMediaController, object>> MethodSelector => x => x.Sort(CancellationToken.None, null);

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
        SortingRequestModel sortingRequestModel = new()
        {
            Parent = null, Sorting = new[] { new ItemSortingRequestModel { Id = Guid.Empty, SortOrder = 1 } }
        };
        return await Client.PutAsync(Url, JsonContent.Create(sortingRequestModel));
    }
}
