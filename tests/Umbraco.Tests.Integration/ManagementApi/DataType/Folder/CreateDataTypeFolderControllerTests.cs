using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Folder;

[TestFixture]
public class CreateDataTypeFolderControllerTests : ManagementApiUserGroupTestBase<CreateDataTypeFolderController>
{
    protected override Expression<Func<CreateDataTypeFolderController, object>> MethodSelector =>
        x => x.Create(null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.Created,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.Created,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.Created,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };

    [Test]
    public override async Task As_Unauthorized_I_Dont_Have_Access()
    {
        CreateFolderRequestModel createFolderModel =
            new() { Id = Guid.NewGuid(), ParentId = null, Name = "TestFolderName" };

        var response = await Client.PostAsync(Url, JsonContent.Create(createFolderModel));

        Assert.AreEqual(UnauthorizedUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    protected override async Task<HttpResponseMessage> AuthorizedRequest(Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, UserEmail, UserPassword, userGroupKey);

        CreateFolderRequestModel createFolderModel =
            new() { Id = Guid.NewGuid(), ParentId = null, Name = "TestFolderName" };

        return await Client.PostAsync(Url, JsonContent.Create(createFolderModel));
    }
}
