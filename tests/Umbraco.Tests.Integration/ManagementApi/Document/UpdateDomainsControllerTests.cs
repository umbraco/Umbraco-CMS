using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class UpdateDomainsControllerTests : ManagementApiUserGroupTestBase<UpdateDomainsController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private Guid _documentKey;

    [SetUp]
    public async Task Setup()
    {
        // Template
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // Content Type
        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id, name: Guid.NewGuid().ToString(), alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = Guid.NewGuid().ToString(),
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _documentKey = response.Result.Content.Key;

        // Publish
        var contentSchedule = new ContentScheduleCollection();
        var cultureAndSchedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = new HashSet<string> { "*" }, Schedules = contentSchedule,
        };
        await ContentPublishingService.PublishAsync(_documentKey, cultureAndSchedule, Constants.Security.SuperUserKey);

        // Language
        await LanguageService.CreateAsync(new Core.Models.Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

        // Domain
        var domainsUpdateModel = new DomainsUpdateModel
        {
            DefaultIsoCode = "en-US",
            Domains = new DomainModel { DomainName = Guid.NewGuid().ToString(), IsoCode = "da-DK" }.Yield(),
        };
        await DomainService.UpdateDomainsAsync(_documentKey, domainsUpdateModel);
    }

    protected override Expression<Func<UpdateDomainsController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _documentKey, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateDomainsRequestModel updateDomainsRequestModel = new()
        {
            DefaultIsoCode = "en-US",
            Domains = new DomainPresentationModel[]
            {
                new() { DomainName = Guid.NewGuid().ToString(), IsoCode = "en-US", },
            },
        };

        return await Client.PutAsync(Url, JsonContent.Create(updateDomainsRequestModel));
    }
}
