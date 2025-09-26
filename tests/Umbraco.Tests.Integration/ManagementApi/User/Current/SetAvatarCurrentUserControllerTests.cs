using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class SetAvatarCurrentUserControllerTests : ManagementApiUserGroupTestBase<SetAvatarCurrentUserController>
{
    protected override Expression<Func<SetAvatarCurrentUserController, object>> MethodSelector => x => x.SetAvatar(CancellationToken.None, null);

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
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        SetAvatarRequestModel setAvatarRequestModel = new() { File = new ReferenceByIdModel(Guid.NewGuid()) };
        return await Client.PostAsync(Url, JsonContent.Create(setAvatarRequestModel));
    }
}
