using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class CreateDocumentBlueprintControllerTests : ManagementApiUserGroupTestBase<CreateDocumentBlueprintController>
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _contentTypeKey;

    [SetUp]
    public async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        _contentTypeKey = contentType.Key;
    }

    protected override Expression<Func<CreateDocumentBlueprintController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var createModel = new CreateDocumentBlueprintRequestModel
        {
            DocumentType = new ReferenceByIdModel(_contentTypeKey),
            Parent = null,
            Id = Guid.NewGuid(),
            Values = [],
            Variants = [new DocumentVariantRequestModel { Culture = null, Segment = null, Name = $"Blueprint {Guid.NewGuid()}" }],
        };

        return await Client.PostAsync(Url, JsonContent.Create(createModel));
    }
}
