using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using UmbracoDataType = Umbraco.Cms.Core.Models.DataType;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class PatchDocumentControllerTests : ManagementApiUserGroupTestBase<PatchDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    private IContentService ContentService => GetRequiredService<IContentService>();

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
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == null && @.segment == null)].name",
                    Value = "Updated Name"
                }
            }
        };

        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
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
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == 'en-US' && @.segment == null)].name",
                    Value = "Updated English Name"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
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
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == 'en-US' && @.segment == null)].name",
                    Value = "Updated English"
                },
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == 'da-DK' && @.segment == null)].name",
                    Value = "Updated Danish"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
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
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == 'fr-FR' && @.segment == null)].name",
                    Value = "French Name" // fr-FR not enabled
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task PatchDocument_PropertyValue_UpdatesProperty()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create language
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        // Arrange - Create content type with properties and culture variation
        var contentType = new ContentTypeBuilder()
            .WithAlias("propertyTest")
            .WithName("Property Test")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with en-US variant using ContentBuilder and ContentService
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "English Name")
            .Build();

        // Set property value directly
        content.SetValue("title", "Original Title", culture: "en-US");
        ContentService.Save(content);

        var documentKey = content.Key;

        // Patch title property for en-US
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.values[?(@.alias == 'title' && @.culture == 'en-US' && @.segment == null)].value",
                    Value = "Updated Title"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify property was updated
        var updatedContent = await ContentEditingService.GetAsync(documentKey);
        Assert.IsNotNull(updatedContent);
        Assert.AreEqual("Updated Title", updatedContent.GetValue<string>("title", "en-US"));
    }

    [Test]
    public async Task PatchDocument_MultipleProperties_UpdatesAll()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create language
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        // Arrange - Create content type with multiple properties and culture variation
        var contentType = new ContentTypeBuilder()
            .WithAlias("multiPropertyTest")
            .WithName("Multi Property Test")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .AddPropertyType()
            .WithAlias("description")
            .WithName("Description")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with en-US variant using ContentBuilder and ContentService
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "English Name")
            .Build();

        // Set property values directly
        content.SetValue("title", "Original Title", culture: "en-US");
        content.SetValue("description", "Original Description", culture: "en-US");
        ContentService.Save(content);

        var documentKey = content.Key;

        // Patch both properties for en-US
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.values[?(@.alias == 'title' && @.culture == 'en-US' && @.segment == null)].value",
                    Value = "Updated Title"
                },
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.values[?(@.alias == 'description' && @.culture == 'en-US' && @.segment == null)].value",
                    Value = "Updated Description"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify both properties were updated
        var updatedContent = await ContentEditingService.GetAsync(documentKey);
        Assert.IsNotNull(updatedContent);
        Assert.AreEqual("Updated Title", updatedContent.GetValue<string>("title", "en-US"));
        Assert.AreEqual("Updated Description", updatedContent.GetValue<string>("description", "en-US"));
    }

    [Test]
    public async Task PatchDocument_PropertyAndVariant_UpdatesBoth()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create language
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        // Arrange - Create content type with properties and culture variation
        var contentType = new ContentTypeBuilder()
            .WithAlias("propertyAndVariantTest")
            .WithName("Property And Variant Test")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with en-US variant using ContentBuilder and ContentService
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Original Name")
            .Build();

        // Set property value directly
        content.SetValue("title", "Original Title", culture: "en-US");
        ContentService.Save(content);

        var documentKey = content.Key;

        // Patch both name and property for en-US
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == 'en-US' && @.segment == null)].name",
                    Value = "Updated Name"
                },
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.values[?(@.alias == 'title' && @.culture == 'en-US' && @.segment == null)].value",
                    Value = "Updated Title"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify both name and property were updated
        var updatedContent = await ContentEditingService.GetAsync(documentKey);
        Assert.IsNotNull(updatedContent);
        Assert.AreEqual("Updated Name", updatedContent.GetCultureName("en-US"));
        Assert.AreEqual("Updated Title", updatedContent.GetValue<string>("title", "en-US"));
    }

    [Test]
    public async Task PatchDocument_InvalidPropertyAlias_ReturnsBadRequest()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create language
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        // Arrange - Create content type with culture variation (but no custom properties)
        var contentType = ContentTypeBuilder.CreateSimpleContentType("invalidPropertyTest", "Invalid Property Test");
        contentType.Variations = ContentVariation.Culture;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with en-US variant
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

        // Try to patch non-existent property
        // When JSONPath matches no elements, it returns BadRequest (400) not UnprocessableEntity (422)
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.values[?(@.alias == 'nonExistentProperty' && @.culture == 'en-US' && @.segment == null)].value",
                    Value = "Some Value"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert - Returns BadRequest (400) because JSONPath expression matches no elements
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task PatchDocument_SegmentProperty_UpdatesCorrectSegment()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Arrange - Create language
        var langEnUs = new LanguageBuilder()
            .WithCultureInfo("en-US")
            .Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);

        // Arrange - Create content type with segment variation property
        var contentType = new ContentTypeBuilder()
            .WithAlias("segmentTest")
            .WithName("Segment Test")
            .WithContentVariation(ContentVariation.CultureAndSegment) // Content type must support segments
            .AddPropertyType()
            .WithAlias("price")
            .WithName("Price")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.CultureAndSegment)
            .Done()
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create document with en-US variant using ContentBuilder and ContentService
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "English Name")
            .Build();

        // Set property values for different segments (including default segment)
        content.SetValue("price", "75", culture: "en-US", segment: null); // Default segment
        content.SetValue("price", "100", culture: "en-US", segment: "standard");
        content.SetValue("price", "150", culture: "en-US", segment: "premium");
        ContentService.Save(content);

        var documentKey = content.Key;

        // Patch only premium segment
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.values[?(@.alias == 'price' && @.culture == 'en-US' && @.segment == 'premium')].value",
                    Value = "200"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error Response: {errorContent}");
        }
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify only premium segment was updated
        var updatedContent = await ContentEditingService.GetAsync(documentKey);
        Assert.IsNotNull(updatedContent);
        Assert.AreEqual("200", updatedContent.GetValue<string>("price", "en-US", "premium"));
        Assert.AreEqual("100", updatedContent.GetValue<string>("price", "en-US", "standard")); // Unchanged
    }

    [Test]
    public async Task PatchDocument_DocumentInRecycleBin_AllowsPatch()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Create simple content type
        var contentType = new ContentTypeBuilder()
            .WithAlias("simpleDoc")
            .WithName("Simple Document")
            .WithContentVariation(ContentVariation.Nothing)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create and immediately delete document (move to recycle bin)
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .Build();
        ContentService.Save(content);
        ContentService.MoveToRecycleBin(content);

        var documentKey = content.Key;

        // Try to patch document in recycle bin
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == null && @.segment == null)].name",
                    Value = "Updated Name"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert - Umbraco allows patching documents in recycle bin (they can be edited before permanent deletion)
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task PatchDocument_NonExistentDocument_ReturnsNotFound()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        var nonExistentKey = Guid.NewGuid();

        // Try to patch non-existent document
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "$.variants[?(@.culture == null && @.segment == null)].name",
                    Value = "Updated Name"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{nonExistentKey}", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task PatchDocument_BlockList_SingleBlock_UpdatesSingleProperty()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        // Create element type for blocks
        var elementType = new ContentTypeBuilder()
            .WithAlias("heroBlock")
            .WithName("Hero Block")
            .AddPropertyType()
            .WithAlias("headline")
            .WithName("Headline")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .Done()
            .AddPropertyType()
            .WithAlias("description")
            .WithName("Description")
            .WithDataTypeId(Constants.DataTypes.Textarea)
            .Done()
            .Build();
        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Create Block List data type
        var propertyEditorCollection = GetRequiredService<PropertyEditorCollection>();
        var configurationEditorJsonSerializer = GetRequiredService<IConfigurationEditorJsonSerializer>();

        var blockListDataType = new UmbracoDataType(
            propertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList],
            configurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new[]
                    {
                        new { contentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        var dataTypeService = GetRequiredService<IDataTypeService>();
        await dataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        // Create content type with Block List property
        var contentType = new ContentTypeBuilder()
            .WithAlias("blockListPage")
            .WithName("Block List Page")
            .AddPropertyType()
            .WithAlias("contentBlocks")
            .WithName("Content Blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create three blocks
        var block1Key = Guid.NewGuid();
        var block2Key = Guid.NewGuid();
        var block3Key = Guid.NewGuid();

        var jsonSerializer = GetRequiredService<IJsonSerializer>();
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = block1Key },
                        new BlockListLayoutItem { ContentKey = block2Key },
                        new BlockListLayoutItem { ContentKey = block3Key }
                    }
                }
            },
            ContentData = new List<BlockItemData>
            {
                new BlockItemData
                {
                    Key = block1Key,
                    ContentTypeKey = elementType.Key,
                    ContentTypeAlias = elementType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "headline", Value = "Block 1 Headline" },
                        new BlockPropertyValue { Alias = "description", Value = "Block 1 Description" }
                    }
                },
                new BlockItemData
                {
                    Key = block2Key,
                    ContentTypeKey = elementType.Key,
                    ContentTypeAlias = elementType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "headline", Value = "Block 2 Headline" },
                        new BlockPropertyValue { Alias = "description", Value = "Block 2 Description" }
                    }
                },
                new BlockItemData
                {
                    Key = block3Key,
                    ContentTypeKey = elementType.Key,
                    ContentTypeAlias = elementType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "headline", Value = "Block 3 Headline" },
                        new BlockPropertyValue { Alias = "description", Value = "Block 3 Description" }
                    }
                }
            },
            SettingsData = new List<BlockItemData>(),
            Expose = new List<BlockItemVariation>()
        };

        var blockListJson = jsonSerializer.Serialize(blockListValue);

        // Create document with Block List
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Blocks Document")
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        content.SetValue("contentBlocks", blockListJson);
        ContentService.Save(content);
        var documentKey = content.Key;

        // Act - Patch only the headline of block 2 using nested JSONPath
        // This demonstrates the TARGET API contract for Phase 8 implementation
        // The JSONPath expression navigates through the nested structure:
        //   1. Find the property with alias 'contentBlocks'
        //   2. Navigate into its .value (the BlockListValue object)
        //   3. Navigate into .contentData array
        //   4. Find the block with the specific key (block2Key)
        //   5. Navigate into its .values array
        //   6. Find the property with alias 'headline'
        //   7. Update its .value
        // This enables minimal payload updates (~100 bytes) vs full replacement (~2KB)
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = $"$.values[?(@.alias == 'contentBlocks' && @.culture == null && @.segment == null)].value.contentData[?(@.key == '{block2Key}')].values[?(@.alias == 'headline')].value",
                    Value = "Updated Block 2 Headline"
                }
            }
        };

        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}", httpContent);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {response.StatusCode}");
            Console.WriteLine($"Error body: {errorContent}");
        }
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify only block 2's headline was updated
        var updatedContent = ContentService.GetById(documentKey);
        Assert.IsNotNull(updatedContent);

        var updatedBlockListJson = updatedContent.GetValue<string>("contentBlocks");
        Assert.IsNotNull(updatedBlockListJson);

        var updatedBlockListValue = jsonSerializer.Deserialize<BlockListValue>(updatedBlockListJson);
        Assert.IsNotNull(updatedBlockListValue);

        // Find block 2 in the content data
        var block2Data = updatedBlockListValue.ContentData.FirstOrDefault(b => b.Key == block2Key);
        Assert.IsNotNull(block2Data);

        // Verify block 2 headline was updated
        var headlineValue = block2Data.Values.FirstOrDefault(v => v.Alias == "headline");
        Assert.IsNotNull(headlineValue);
        Assert.AreEqual("Updated Block 2 Headline", headlineValue.Value?.ToString());

        // Verify block 2 description was NOT updated
        var descriptionValue = block2Data.Values.FirstOrDefault(v => v.Alias == "description");
        Assert.IsNotNull(descriptionValue);
        Assert.AreEqual("Block 2 Description", descriptionValue.Value?.ToString());

        // Verify block 1 and block 3 were NOT updated
        var block1Data = updatedBlockListValue.ContentData.FirstOrDefault(b => b.Key == block1Key);
        Assert.IsNotNull(block1Data);
        Assert.AreEqual("Block 1 Headline", block1Data.Values.FirstOrDefault(v => v.Alias == "headline")?.Value?.ToString());
        Assert.AreEqual("Block 1 Description", block1Data.Values.FirstOrDefault(v => v.Alias == "description")?.Value?.ToString());

        var block3Data = updatedBlockListValue.ContentData.FirstOrDefault(b => b.Key == block3Key);
        Assert.IsNotNull(block3Data);
        Assert.AreEqual("Block 3 Headline", block3Data.Values.FirstOrDefault(v => v.Alias == "headline")?.Value?.ToString());
        Assert.AreEqual("Block 3 Description", block3Data.Values.FirstOrDefault(v => v.Alias == "description")?.Value?.ToString());
    }
}
