using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Language;

public class ByIsoCodeLanguageControllerTests : ManagementApiUserGroupTestBase<ByIsoCodeLanguageController>
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private const string IsoCode = "da";

    private readonly ILanguage _languageResponseModel = new Core.Models.Language(IsoCode, "Danish");

    [SetUp]
    public void Setup() => LanguageService.CreateAsync(_languageResponseModel, Constants.Security.SuperUserKey);

    protected override Expression<Func<ByIsoCodeLanguageController, object>> MethodSelector => x => x.ByIsoCode(IsoCode);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
