using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Folder;

[TestFixture]
public class UpdateDataTypeFolderControllerTests : ManagementApiUserGroupTestBase<UpdateDataTypeFolderController>
{
    private readonly Guid _folderId = Guid.NewGuid();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    protected override Expression<Func<UpdateDataTypeFolderController, object>> MethodSelector =>
        x => x.Update(_folderId, null);

    [SetUp]
    public void Setup() => DataTypeContainerService.CreateAsync(_folderId, "FolderName", null, Constants.Security.SuperUserKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    [Test]
    public override async Task As_Unauthorized_I_Have_Specified_Access()
    {
        UpdateFolderResponseModel updateFolderModel = new() { Name = "UpdatedName" };

        var response = await Client.PutAsync(Url, JsonContent.Create(updateFolderModel));

        Assert.AreEqual(UnauthorizedUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    protected override async Task<HttpResponseMessage> AuthorizedRequest(Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, UserEmail, UserPassword, userGroupKey);

        UpdateFolderResponseModel updateFolderModel = new() { Name = "UpdatedName" };

        return await Client.PutAsync(Url, JsonContent.Create(updateFolderModel));
    }
}
