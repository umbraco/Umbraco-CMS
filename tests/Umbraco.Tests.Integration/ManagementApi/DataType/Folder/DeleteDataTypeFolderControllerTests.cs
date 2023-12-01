using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Folder;

[TestFixture]
public class DeleteDataTypeFolderControllerTests : ManagementApiUserGroupTestBase<DeleteDataTypeFolderController>
{
    private readonly Guid _folderId = Guid.NewGuid();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    protected override Expression<Func<DeleteDataTypeFolderController, object>> MethodSelector =>
        x => x.Delete(_folderId);


    [SetUp]
    public void Setup() =>
        DataTypeContainerService.CreateAsync(_folderId, "FolderName", null, Constants.Security.SuperUserKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
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
        Allowed = true, ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        Allowed = false, ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };

    [Test]
    public override async Task As_Unauthorized_I_Dont_Have_Access()
    {
        var response = await Client.DeleteAsync(Url);

        Assert.AreEqual(UnauthorizedUserGroupAssertionModel.ExpectedStatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    protected override async Task<HttpResponseMessage> AuthorizedRequest(Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, UserEmail, UserPassword, userGroupKey);

        return await Client.DeleteAsync(Url);
    }
}
