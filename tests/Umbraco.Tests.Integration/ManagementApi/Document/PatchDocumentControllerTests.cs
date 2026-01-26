using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

[NonParallelizable]
public class PatchDocumentControllerTests : ManagementApiUserGroupTestBase<PatchDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private Guid _templateKey;
    private Guid _documentKey;

    [SetUp]
    public async Task Setup()
    {
        // Template
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        var templateResponse = await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        _templateKey = templateResponse.Result.Key;

        // Content Type
        var contentType = ContentTypeBuilder.CreateTextPageContentType(
            defaultTemplateId: template.Id,
            name: Guid.NewGuid().ToString(),
            alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = _templateKey,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel> { new() { Name = "Original Name" } },
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (response.Result.Content is null)
        {
            throw new ArgumentNullException(nameof(response.Result.Content), "Setup failed: No content returned from CreateAsync");
        }

        _documentKey = response.Result.Content.Key;
    }

    protected override Expression<Func<PatchDocumentController, object>> MethodSelector =>
        x => x.Patch(CancellationToken.None, _documentKey, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        PatchDocumentRequestModel patchModel = new()
        {
            Variants = new DocumentVariantPatchModel[]
            {
                new() { Culture = null, Segment = null, Name = "Updated Name" },
            },
        };

        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/merge-patch+json");
        return await Client.PatchAsync(Url, httpContent);
    }

    [Test]
    public async Task PatchDocument_SingleCulture_UpdatesOnlyThatCulture()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create languages
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        var langDaDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDaDk, Constants.Security.SuperUserKey);

        // Arrange - Create content type with culture variation
        var contentType = ContentTypeBuilder.CreateSimpleContentType("cultureTest", "Culture Test");
        contentType.Variations = ContentVariation.Culture;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with multiple cultures
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel>
            {
                new() { Name = "English Name", Culture = "en-US" },
                new() { Name = "Danish Name", Culture = "da-DK" },
            },
        };
        var createResponse = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createResponse.Result.Content is null)
        {
            throw new InvalidOperationException("Failed to create test content");
        }

        var documentKey = createResponse.Result.Content.Key;

        // Patch only en-US using authenticated admin client
        var patchModel = new PatchDocumentRequestModel
        {
            Variants = new DocumentVariantPatchModel[]
            {
                new() { Culture = "en-US", Segment = null, Name = "Updated English Name" }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/merge-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify only en-US was updated
        var content = await ContentEditingService.GetAsync(documentKey);
        Assert.IsNotNull(content);
        Assert.AreEqual("Updated English Name", content.GetCultureName("en-US"));
        Assert.AreEqual("Danish Name", content.GetCultureName("da-DK")); // Unchanged
    }

    [Test]
    public async Task PatchDocument_MultipleCultures_UpdatesBoth()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create languages
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        var langDaDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDaDk, Constants.Security.SuperUserKey);

        var langDeDe = new LanguageBuilder()
            .WithCultureInfo("de-DE")
            .Build();
        await LanguageService.CreateAsync(langDeDe, Constants.Security.SuperUserKey);

        // Arrange - Create content type with culture variation
        var contentType = ContentTypeBuilder.CreateSimpleContentType("multiCultureTest", "Multi Culture Test");
        contentType.Variations = ContentVariation.Culture;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with multiple cultures
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel>
            {
                new() { Name = "English Name", Culture = "en-US" },
                new() { Name = "Danish Name", Culture = "da-DK" },
                new() { Name = "German Name", Culture = "de-DE" },
            },
        };
        var createResponse = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createResponse.Result.Content is null)
        {
            throw new InvalidOperationException("Failed to create test content");
        }

        var documentKey = createResponse.Result.Content.Key;

        // Patch en-US and da-DK using authenticated admin client
        var patchModel = new PatchDocumentRequestModel
        {
            Variants = new DocumentVariantPatchModel[]
            {
                new() { Culture = "en-US", Segment = null, Name = "Updated English" },
                new() { Culture = "da-DK", Segment = null, Name = "Updated Danish" }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/merge-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify both cultures updated
        var content = await ContentEditingService.GetAsync(documentKey);
        Assert.IsNotNull(content);
        Assert.AreEqual("Updated English", content.GetCultureName("en-US"));
        Assert.AreEqual("Updated Danish", content.GetCultureName("da-DK"));
        Assert.AreEqual("German Name", content.GetCultureName("de-DE")); // Unchanged
    }

    [Test]
    public async Task PatchDocument_NonExistentCulture_ReturnsInvalidCulture()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create language (only en-US, not fr-FR)
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        // Arrange - Create content type with culture variation
        var contentType = ContentTypeBuilder.CreateSimpleContentType("invalidCultureTest", "Invalid Culture Test");
        contentType.Variations = ContentVariation.Culture;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with en-US
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel>
            {
                new() { Name = "English Name", Culture = "en-US" },
            },
        };
        var createResponse = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createResponse.Result.Content is null)
        {
            throw new InvalidOperationException("Failed to create test content");
        }

        var documentKey = createResponse.Result.Content.Key;

        // Try to patch non-existent culture using authenticated admin client
        var patchModel = new PatchDocumentRequestModel
        {
            Variants = new DocumentVariantPatchModel[]
            {
                new() { Culture = "fr-FR", Segment = null, Name = "French Name" } // fr-FR not enabled
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/merge-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
