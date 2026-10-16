using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class CreateDocumentBlueprintFromDocumentControllerTests
    : ManagementApiUserGroupTestBase<CreateDocumentBlueprintFromDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _documentKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var document = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = $"Document {Guid.NewGuid()}" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(document.Success, $"Failed to create the document: {document.Status}");
        _documentKey = document.Result.Content!.Key;
    }

    protected override Expression<Func<CreateDocumentBlueprintFromDocumentController, object>> MethodSelector =>
        x => x.CreateFromDocument(CancellationToken.None, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    // Unlike the other blueprint endpoints, this one also authorizes the source document against
    // ActionCreateBlueprintFromContent, which writers are not granted.
    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var createModel = new CreateDocumentBlueprintFromDocumentRequestModel
        {
            Document = new ReferenceByIdModel(_documentKey),
            Id = Guid.NewGuid(),
            Name = $"Blueprint {Guid.NewGuid()}",
            Parent = null,
        };

        return await Client.PostAsync(Url, JsonContent.Create(createModel));
    }
}
