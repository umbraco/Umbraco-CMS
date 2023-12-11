using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.RelationType.Query;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.RelationType.Query;

public class UpdateRelationTypeControllerTests : ManagementApiUserGroupTestBase<UpdateRelationTypeController>
{
    protected override Expression<Func<UpdateRelationTypeController, object>> MethodSelector =>
        x => x.Update(Guid.NewGuid(), null);

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
        UpdateRelationTypeRequestModel updateRelationTypeRequestModel =
            new() { Name = "TestUpdateRelationType", IsBidirectional = true, ParentObjectType = null, ChildObjectType = Guid.NewGuid(), IsDependency = false };

        return await Client.PutAsync(Url, JsonContent.Create(updateRelationTypeRequestModel));
    }
}
