using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup;

public class UpdateUserGroupControllerTests : ManagementApiUserGroupTestBase<UpdateUserGroupController>
{
    protected override Expression<Func<UpdateUserGroupController, object>> MethodSelector => x => x.Update(CancellationToken.None, Guid.Empty, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateUserGroupRequestModel updateUserGroupRequest = new()
        {
            Name = "UpdatedTestGroup",
            Alias = "testAlias",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = [],
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel> { }
        };

        return await Client.PutAsync(Url, JsonContent.Create(updateUserGroupRequest));
    }
}
