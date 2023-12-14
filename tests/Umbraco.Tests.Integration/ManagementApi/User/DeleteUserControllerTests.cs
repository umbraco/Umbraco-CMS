﻿using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class DeleteUserControllerTests : ManagementApiUserGroupTestBase<DeleteUserController>
{
    protected override Expression<Func<DeleteUserController, object>> MethodSelector => x => x.DeleteUser(Constants.Security.SuperUserKey);

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
