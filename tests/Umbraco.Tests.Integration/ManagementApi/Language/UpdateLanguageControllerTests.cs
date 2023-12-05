using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Language;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language;

public class UpdateLanguageControllerTests : ManagementApiUserGroupTestBase<UpdateLanguageController>
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private const string IsoCode = "da";

    private readonly ILanguage _languageResponseModel = new Core.Models.Language(IsoCode, "Danish");

    protected override Expression<Func<UpdateLanguageController, object>> MethodSelector => x => x.Update(IsoCode, null);

    [SetUp]
    public void Setup() => LanguageService.CreateAsync(_languageResponseModel, Constants.Security.SuperUserKey);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateLanguageRequestModel updateLanguageModel = new() { IsMandatory = true };

        return await Client.PutAsync(Url, JsonContent.Create(updateLanguageModel));
    }
}
