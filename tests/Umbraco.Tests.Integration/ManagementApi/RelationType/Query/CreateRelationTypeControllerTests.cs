using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.RelationType.Query;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RelationType.Query;

public class CreateRelationTypeControllerTests : ManagementApiUserGroupTestBase<CreateRelationTypeController>
{
    protected override Expression<Func<CreateRelationTypeController, object>> MethodSelector =>
        x => x.Create(null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
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
        CreateRelationTypeRequestModel createRelationTypeRequestModel =
            new() { Name = "TestCreateRelationType", IsBidirectional = true, ParentObjectType = null, ChildObjectType = Guid.NewGuid(), IsDependency = true, Id = Guid.NewGuid() };

        return await Client.PostAsync(Url, JsonContent.Create(createRelationTypeRequestModel));
    }
}
