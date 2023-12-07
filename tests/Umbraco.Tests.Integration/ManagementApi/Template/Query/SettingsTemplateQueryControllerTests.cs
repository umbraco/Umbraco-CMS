﻿using System.Linq.Expressions;
using System.Net;
using Umbraco.Cms.Api.Management.Controllers.Template.Query;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template.Query;

public class SettingsTemplateQueryControllerTests : ManagementApiUserGroupTestBase<SettingsTemplateQueryController>
{
    protected override Expression<Func<SettingsTemplateQueryController, object>> MethodSelector => x => x.Settings();

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
}
