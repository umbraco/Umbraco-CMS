﻿using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Umbraco.Cms.Api.Management.Controllers.User;
using Umbraco.Cms.Api.Management.ViewModels.User;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User;

public class ChangePasswordUserControllerTests : ManagementApiUserGroupTestBase<ChangePasswordUserController>
{
    protected override Expression<Func<ChangePasswordUserController, object>> MethodSelector => x => x.ChangePassword(Guid.Empty, null);

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
        ChangePasswordUserRequestModel changePasswordUserRequest = new()
        {
            NewPassword = "newPassword", OldPassword = "oldPassword"
        };

        return await Client.PostAsync(Url, JsonContent.Create(changePasswordUserRequest));
    }
}
