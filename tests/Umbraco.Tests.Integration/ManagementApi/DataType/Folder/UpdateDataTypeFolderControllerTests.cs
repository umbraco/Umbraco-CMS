using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Folder;

[TestFixture]
public class UpdateDataTypeFolderControllerTests : ManagementApiTest<UpdateDataTypeFolderController>
{
    protected override Expression<Func<UpdateDataTypeFolderController, object>> MethodSelector =>
        x => x.Update(Guid.NewGuid(), null);

    private readonly List<HttpStatusCode> _authenticatedStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound
        };

    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        var response = await SendUpdateDataTypeFolderRequestAsync("admin@umbraco.com", "1234567890", Constants.Security.AdminGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Editor_I_Have_Access()
    {
        var response = await SendUpdateDataTypeFolderRequestAsync("editor@umbraco.com", "1234567890", Constants.Security.EditorGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Sensitive_Data_I_Have_No_Access()
    {
        var response = await SendUpdateDataTypeFolderRequestAsync("sensitiveData@umbraco.com", "1234567890", Constants.Security.SensitiveDataGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Translator_I_Have_No_Access()
    {
        var response = await SendUpdateDataTypeFolderRequestAsync("translator@umbraco.com", "1234567890", Constants.Security.TranslatorGroupKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public virtual async Task As_Writer_I_Have_Access()
    {
        var response = await SendUpdateDataTypeFolderRequestAsync("writer@umbraco.com", "1234567890", Constants.Security.WriterGroupKey);

        Assert.Contains(response.StatusCode, _authenticatedStatusCodes, await response.Content.ReadAsStringAsync());
    }


    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var updateFolderModel = GenerateUpdateFolderResponseModel();

        var response = await Client.PutAsync(Url, JsonContent.Create(updateFolderModel));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> SendUpdateDataTypeFolderRequestAsync(string userEmail, string userPassword, Guid userGroupKey)
    {
        await AuthenticateClientAsync(Client, userEmail, userPassword, userGroupKey);

        var updateFolderModel = GenerateUpdateFolderResponseModel();

        return await Client.PutAsync(Url, JsonContent.Create(updateFolderModel));
    }

    private UpdateFolderResponseModel GenerateUpdateFolderResponseModel()
    {
        return new UpdateFolderResponseModel
        {
            Name = "TestUpdateFolder"
        };
    }

}
