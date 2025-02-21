using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class UpdateUserGroupsUserControllerTests : ManagementApiUserGroupTestBase<UpdateUserGroupsUserController>
{
    protected override Expression<Func<UpdateUserGroupsUserController, object>> MethodSelector => x => x.UpdateUserGroups(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateUserGroupsOnUserRequestModel updateUserGroupsOnUserRequestModel = new()
        {
            UserIds = new HashSet<ReferenceByIdModel> { new ReferenceByIdModel(Guid.Empty) }, UserGroupIds = new HashSet<ReferenceByIdModel> { new ReferenceByIdModel(Guid.Empty) },
        };
        return await Client.PutAsync(Url, JsonContent.Create(updateUserGroupsOnUserRequestModel));
    }
}
