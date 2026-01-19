using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.UserGroup;

public class CreateUserGroupControllerTests : ManagementApiUserGroupTestBase<CreateUserGroupController>
{
    protected override Expression<Func<CreateUserGroupController, object>> MethodSelector => x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        CreateUserGroupRequestModel createUserGroupRequest = new()
        {
            Name = "CreatedTestGroup",
            Alias = "testAlias",
            Description = "Test group description",
            FallbackPermissions = new HashSet<string>(),
            HasAccessToAllLanguages = true,
            Languages = [],
            Sections = ["Umb.Section.Content"],
            Permissions = new HashSet<IPermissionPresentationModel> { },
        };

        return await Client.PostAsync(Url, JsonContent.Create(createUserGroupRequest));
    }
}
