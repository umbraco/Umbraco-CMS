using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Language.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language.Item;

public class ItemsLanguageEntityControllerTests: ManagementApiUserGroupTestBase<ItemsLanguageEntityController>
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private const string IsoCode = "da";

    private readonly HashSet<string> _isoCode = new() { IsoCode };

    private readonly ILanguage _languageResponseModel = new Core.Models.Language(IsoCode, "Danish");

    protected override Expression<Func<ItemsLanguageEntityController, object>> MethodSelector => x => x.Items(_isoCode);

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.GetAsync(Url);
}
