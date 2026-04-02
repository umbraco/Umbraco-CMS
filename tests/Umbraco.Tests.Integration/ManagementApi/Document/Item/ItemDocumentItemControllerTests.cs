using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document.Item;

public class ItemDocumentItemControllerTests : ManagementApiUserGroupTestBase<ItemDocumentItemController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _contentKey;

    [SetUp]
    public async Task Setup()
    {
        // Template
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // ContentType
        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id, name: Guid.NewGuid().ToString(), alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel> { new() { Name = Guid.NewGuid().ToString() } },
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _contentKey = response.Result.Content.Key;
    }

    protected override Expression<Func<ItemDocumentItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _contentKey });

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
