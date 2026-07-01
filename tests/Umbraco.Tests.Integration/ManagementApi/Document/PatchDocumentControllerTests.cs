using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
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
                    Path = "/variants[culture=null,segment=null]/name",
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
                    Path = "/variants[culture=en-US,segment=null]/name",
                    Value = "Updated English Name"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/variants[culture=en-US,segment=null]/name",
                    Value = "Updated English"
                },
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "/variants[culture=da-DK,segment=null]/name",
                    Value = "Updated Danish"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/variants[culture=fr-FR,segment=null]/name",
                    Value = "French Name" // fr-FR not enabled
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/values[alias=title,culture=en-US,segment=null]/value",
                    Value = "Updated Title"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/values[alias=title,culture=en-US,segment=null]/value",
                    Value = "Updated Title"
                },
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "/values[alias=description,culture=en-US,segment=null]/value",
                    Value = "Updated Description"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/variants[culture=en-US,segment=null]/name",
                    Value = "Updated Name"
                },
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "/values[alias=title,culture=en-US,segment=null]/value",
                    Value = "Updated Title"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
        // When path filter matches no elements, it returns BadRequest (400)
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = "/values[alias=nonExistentProperty,culture=en-US,segment=null]/value",
                    Value = "Some Value"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

        // Assert - Returns BadRequest (400) because path filter matches no elements
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
                    Path = "/values[alias=price,culture=en-US,segment=premium]/value",
                    Value = "200"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/variants[culture=null,segment=null]/name",
                    Value = "Updated Name"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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
                    Path = "/variants[culture=null,segment=null]/name",
                    Value = "Updated Name"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{nonExistentKey}/patch", httpContent);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task PatchDocument_BlockList_SingleBlock_UpdatesSingleProperty()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create element type for blocks
        var elementType = new ContentTypeBuilder()
            .WithAlias($"heroBlock{suffix}")
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
            .WithAlias($"blockListPage{suffix}")
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

        // Act - Patch only the headline of block 2 using nested path
        // The path expression navigates through the nested structure:
        //   1. Find the property with alias 'contentBlocks'
        //   2. Navigate into its .value (the BlockListValue object)
        //   3. Navigate into .contentData array
        //   4. Find the block with the specific key (block2Key)
        //   5. Navigate into its .values array
        //   6. Find the property with alias 'headline'
        //   7. Update its .value
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = $"/values[alias=contentBlocks,culture=null,segment=null]/value/contentData[key={block2Key}]/values[alias=headline]/value",
                    Value = "Updated Block 2 Headline"
                }
            }
        };

        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

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

    [Test]
    public async Task PatchDocument_BlockList_AddBlock_AppendsToExistingBlockList()
    {
        // Arrange - Authenticate as admin
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create element type for blocks
        var elementType = new ContentTypeBuilder()
            .WithAlias($"heroBlock{suffix}")
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
            .WithAlias($"blockListPage{suffix}")
            .WithName("Block List Page")
            .AddPropertyType()
            .WithAlias("contentBlocks")
            .WithName("Content Blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Create two existing blocks
        var block1Key = Guid.NewGuid();
        var block2Key = Guid.NewGuid();

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
                        new BlockListLayoutItem { ContentKey = block2Key }
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
                }
            },
            SettingsData = new List<BlockItemData>(),
            Expose = new List<BlockItemVariation>()
        };

        var blockListJson = jsonSerializer.Serialize(blockListValue);

        // Create document with Block List containing 2 blocks
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Blocks Document")
            .WithCreatorId(Constants.Security.SuperUserId)
            .Build();

        content.SetValue("contentBlocks", blockListJson);
        ContentService.Save(content);
        var documentKey = content.Key;

        // Act - Add a new block to the block list using two add operations:
        //   1. Append block data to contentData array
        //   2. Append layout item to layout array (so the block is rendered)
        var newBlockKey = Guid.NewGuid();

        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = "/values[alias=contentBlocks,culture=null,segment=null]/value/contentData/-",
                    Value = new
                    {
                        key = newBlockKey,
                        contentTypeKey = elementType.Key,
                        contentTypeAlias = elementType.Alias,
                        values = new[]
                        {
                            new { alias = "headline", value = "New Block Headline" },
                            new { alias = "description", value = "New Block Description" }
                        }
                    }
                },
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"/values[alias=contentBlocks,culture=null,segment=null]/value/layout/Umbraco.BlockList/-",
                    Value = new { contentKey = newBlockKey }
                }
            }
        };

        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{documentKey}/patch", httpContent);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {response.StatusCode}");
            Console.WriteLine($"Error body: {errorContent}");
        }
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify the block list now has 3 blocks
        var updatedContent = ContentService.GetById(documentKey);
        Assert.IsNotNull(updatedContent);

        var updatedBlockListJson = updatedContent.GetValue<string>("contentBlocks");
        Assert.IsNotNull(updatedBlockListJson);

        var updatedBlockListValue = jsonSerializer.Deserialize<BlockListValue>(updatedBlockListJson);
        Assert.IsNotNull(updatedBlockListValue);

        // Verify contentData has 3 blocks
        Assert.AreEqual(3, updatedBlockListValue.ContentData.Count);

        // Verify the new block was added with correct values
        var newBlockData = updatedBlockListValue.ContentData.FirstOrDefault(b => b.Key == newBlockKey);
        Assert.IsNotNull(newBlockData);
        Assert.AreEqual("New Block Headline", newBlockData.Values.FirstOrDefault(v => v.Alias == "headline")?.Value?.ToString());
        Assert.AreEqual("New Block Description", newBlockData.Values.FirstOrDefault(v => v.Alias == "description")?.Value?.ToString());

        // Verify original blocks were NOT changed
        var block1Data = updatedBlockListValue.ContentData.FirstOrDefault(b => b.Key == block1Key);
        Assert.IsNotNull(block1Data);
        Assert.AreEqual("Block 1 Headline", block1Data.Values.FirstOrDefault(v => v.Alias == "headline")?.Value?.ToString());
        Assert.AreEqual("Block 1 Description", block1Data.Values.FirstOrDefault(v => v.Alias == "description")?.Value?.ToString());

        var block2Data = updatedBlockListValue.ContentData.FirstOrDefault(b => b.Key == block2Key);
        Assert.IsNotNull(block2Data);
        Assert.AreEqual("Block 2 Headline", block2Data.Values.FirstOrDefault(v => v.Alias == "headline")?.Value?.ToString());
        Assert.AreEqual("Block 2 Description", block2Data.Values.FirstOrDefault(v => v.Alias == "description")?.Value?.ToString());

        // Verify layout was also updated with the new block
        var layoutItems = updatedBlockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].ToList();
        Assert.AreEqual(3, layoutItems.Count);
    }

    // ── Deeply nested block editor tests ──────────────────────────────────────
    //
    // Structure: Document → RTE → BlockGrid (with areas) → BlockList → textBlock
    //
    // These tests prove that PatchEngine can navigate through multiple layers of
    // recursively-expanded block editor values (RTE > Block Grid > Block List).

    /// <summary>
    /// Builds the full deeply-nested block editor structure used by the deep nesting tests.
    /// Returns all keys and identifiers needed to construct patch paths.
    /// </summary>
    private async Task<DeepNestedSetup> CreateDeeplyNestedBlockDocument()
    {
        var jsonSerializer = GetRequiredService<IJsonSerializer>();
        var propertyEditorCollection = GetRequiredService<PropertyEditorCollection>();
        var configEditorJsonSerializer = GetRequiredService<IConfigurationEditorJsonSerializer>();
        var dataTypeService = GetRequiredService<IDataTypeService>();

        // ── Languages ──
        var langEnUs = new LanguageBuilder().WithCultureInfo("en-US").WithIsDefault(true).Build();
        await LanguageService.CreateAsync(langEnUs, Constants.Security.SuperUserKey);
        var langDaDk = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(langDaDk, Constants.Security.SuperUserKey);

        // ── Element type: textBlock (culture-variant, has "text" property) ──
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var textBlockType = new ContentTypeBuilder()
            .WithAlias($"textBlock{suffix}")
            .WithName("Text Block")
            .AddPropertyType()
                .WithAlias("text")
                .WithName("Text")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .Done()
            .Build();
        textBlockType.IsElement = true;
        textBlockType.Variations = ContentVariation.Culture;
        foreach (var pt in textBlockType.PropertyTypes)
        {
            pt.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(textBlockType, Constants.Security.SuperUserKey);

        // ── Element type: listContainerBlock (has "blockList" property) ──
        // First create the Block List data type configured with textBlock
        var blockListDataType = new UmbracoDataType(
            propertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList],
            configEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { "blocks", new[] { new { contentElementTypeKey = textBlockType.Key } } }
            },
            Name = "Test Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        var listContainerType = new ContentTypeBuilder()
            .WithAlias($"listContainerBlock{suffix}")
            .WithName("List Container Block")
            .AddPropertyType()
                .WithAlias("blockList")
                .WithName("Block List")
                .WithDataTypeId(blockListDataType.Id)
                .Done()
            .Build();
        listContainerType.IsElement = true;
        await ContentTypeService.CreateAsync(listContainerType, Constants.Security.SuperUserKey);

        // ── Element type: areaBlock (empty container, holds areas) ──
        var areaBlockType = new ContentTypeBuilder()
            .WithAlias($"areaBlock{suffix}")
            .WithName("Area Block")
            .Build();
        areaBlockType.IsElement = true;
        await ContentTypeService.CreateAsync(areaBlockType, Constants.Security.SuperUserKey);

        // ── Block Grid data type ──
        var areaKey = Guid.NewGuid();
        var blockGridDataType = new UmbracoDataType(
            propertyEditorCollection[Constants.PropertyEditors.Aliases.BlockGrid],
            configEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks", new object[]
                    {
                        new
                        {
                            contentElementTypeKey = areaBlockType.Key,
                            allowAtRoot = true,
                            allowInAreas = false,
                            areaGridColumns = 12,
                            areas = new[]
                            {
                                new
                                {
                                    key = areaKey,
                                    alias = "content",
                                    columnSpan = 12,
                                    rowSpan = 1,
                                    minAllowed = 0,
                                    maxAllowed = 10
                                }
                            }
                        },
                        new
                        {
                            contentElementTypeKey = listContainerType.Key,
                            allowAtRoot = false,
                            allowInAreas = true,
                            areas = Array.Empty<object>()
                        }
                    }
                }
            },
            Name = "Test Block Grid",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(blockGridDataType, Constants.Security.SuperUserKey);

        // ── Element type: gridContainerBlock (has "blockGrid" property) ──
        var gridContainerType = new ContentTypeBuilder()
            .WithAlias($"gridContainerBlock{suffix}")
            .WithName("Grid Container Block")
            .AddPropertyType()
                .WithAlias("blockGrid")
                .WithName("Block Grid")
                .WithDataTypeId(blockGridDataType.Id)
                .Done()
            .Build();
        gridContainerType.IsElement = true;
        await ContentTypeService.CreateAsync(gridContainerType, Constants.Security.SuperUserKey);

        // ── RTE data type ──
        var rteDataType = new UmbracoDataType(
            propertyEditorCollection[Constants.PropertyEditors.Aliases.RichText],
            configEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { "blocks", new[] { new { contentElementTypeKey = gridContainerType.Key } } }
            },
            Name = "Test RTE",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await dataTypeService.CreateAsync(rteDataType, Constants.Security.SuperUserKey);

        // ── Document content type ──
        var contentType = new ContentTypeBuilder()
            .WithAlias($"deepNestedPage{suffix}")
            .WithName("Deep Nested Page")
            .AddPropertyType()
                .WithAlias("rte")
                .WithName("Rich Text")
                .WithDataTypeId(rteDataType.Id)
                .Done()
            .Build();
        contentType.AllowedAsRoot = true;
        contentType.Variations = ContentVariation.Culture;
        foreach (var pt in contentType.PropertyTypes)
        {
            pt.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // ── Build nested block values (inside-out) ──
        var textBlock1Key = Guid.NewGuid();
        var textBlock2Key = Guid.NewGuid();
        var listContainerKey = Guid.NewGuid();
        var areaBlockKey = Guid.NewGuid();
        var gridContainerKey = Guid.NewGuid();

        // Layer 1: Block List value with 2 text blocks (culture-variant)
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    new IBlockLayoutItem[]
                    {
                        new BlockListLayoutItem { ContentKey = textBlock1Key },
                        new BlockListLayoutItem { ContentKey = textBlock2Key }
                    }
                }
            },
            ContentData = new List<BlockItemData>
            {
                new BlockItemData
                {
                    Key = textBlock1Key,
                    ContentTypeKey = textBlockType.Key,
                    ContentTypeAlias = textBlockType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "text", Culture = "en-US", Value = "original en" },
                        new BlockPropertyValue { Alias = "text", Culture = "da-DK", Value = "original da" }
                    }
                },
                new BlockItemData
                {
                    Key = textBlock2Key,
                    ContentTypeKey = textBlockType.Key,
                    ContentTypeAlias = textBlockType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "text", Culture = "en-US", Value = "second block en" },
                        new BlockPropertyValue { Alias = "text", Culture = "da-DK", Value = "second block da" }
                    }
                }
            },
            SettingsData = new List<BlockItemData>(),
            Expose = new List<BlockItemVariation>
            {
                new BlockItemVariation(textBlock1Key, "en-US", null),
                new BlockItemVariation(textBlock1Key, "da-DK", null),
                new BlockItemVariation(textBlock2Key, "en-US", null),
                new BlockItemVariation(textBlock2Key, "da-DK", null)
            }
        };
        var blockListJson = jsonSerializer.Serialize(blockListValue);

        // Layer 2: Block Grid value with areaBlock (root) + listContainerBlock (in area)
        var blockGridValue = new BlockGridValue(new[]
        {
            new BlockGridLayoutItem(areaBlockKey)
            {
                ColumnSpan = 12,
                RowSpan = 1,
                Areas = new[]
                {
                    new BlockGridLayoutAreaItem(areaKey)
                    {
                        Items = new[]
                        {
                            new BlockGridLayoutItem(listContainerKey)
                            {
                                ColumnSpan = 12,
                                RowSpan = 1,
                                Areas = Array.Empty<BlockGridLayoutAreaItem>()
                            }
                        }
                    }
                }
            }
        })
        {
            ContentData = new List<BlockItemData>
            {
                new BlockItemData
                {
                    Key = areaBlockKey,
                    ContentTypeKey = areaBlockType.Key,
                    ContentTypeAlias = areaBlockType.Alias,
                    Values = new List<BlockPropertyValue>()
                },
                new BlockItemData
                {
                    Key = listContainerKey,
                    ContentTypeKey = listContainerType.Key,
                    ContentTypeAlias = listContainerType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "blockList", Value = blockListJson }
                    }
                }
            },
            SettingsData = new List<BlockItemData>(),
            Expose = new List<BlockItemVariation>
            {
                new BlockItemVariation(areaBlockKey, null, null),
                new BlockItemVariation(listContainerKey, null, null)
            }
        };
        var blockGridJson = jsonSerializer.Serialize(blockGridValue);

        // Layer 3: RTE value with gridContainerBlock
        var rteBlockValue = new RichTextBlockValue(new[]
        {
            new RichTextBlockLayoutItem(gridContainerKey)
        })
        {
            ContentData = new List<BlockItemData>
            {
                new BlockItemData
                {
                    Key = gridContainerKey,
                    ContentTypeKey = gridContainerType.Key,
                    ContentTypeAlias = gridContainerType.Alias,
                    Values = new List<BlockPropertyValue>
                    {
                        new BlockPropertyValue { Alias = "blockGrid", Value = blockGridJson }
                    }
                }
            },
            SettingsData = new List<BlockItemData>(),
            Expose = new List<BlockItemVariation>
            {
                new BlockItemVariation(gridContainerKey, null, null)
            }
        };

        var rteEditorValue = new RichTextEditorValue
        {
            Markup = "<p>Hello</p>",
            Blocks = rteBlockValue
        };
        var rteJson = RichTextPropertyEditorHelper.SerializeRichTextEditorValue(rteEditorValue, jsonSerializer);

        // ── Create document ──
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel>
            {
                new() { Name = "English Page", Culture = "en-US" },
                new() { Name = "Danish Page", Culture = "da-DK" },
            },
            Properties = new List<PropertyValueModel>
            {
                new() { Alias = "rte", Value = rteJson, Culture = "en-US" },
                new() { Alias = "rte", Value = rteJson, Culture = "da-DK" }
            }
        };
        var createResult = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        return new DeepNestedSetup
        {
            DocumentKey = createResult.Result.Content!.Key,
            GridContainerKey = gridContainerKey,
            ListContainerKey = listContainerKey,
            TextBlock1Key = textBlock1Key,
            TextBlock2Key = textBlock2Key,
            TextBlockTypeKey = textBlockType.Key,
            JsonSerializer = jsonSerializer
        };
    }

    private record DeepNestedSetup
    {
        public Guid DocumentKey { get; init; }
        public Guid GridContainerKey { get; init; }
        public Guid ListContainerKey { get; init; }
        public Guid TextBlock1Key { get; init; }
        public Guid TextBlock2Key { get; init; }
        public Guid TextBlockTypeKey { get; init; }
        public IJsonSerializer JsonSerializer { get; init; } = null!;

        /// <summary>
        /// Builds the common path prefix to reach the nested block list value.
        /// </summary>
        public string BlockListPathPrefix(string culture) =>
            $"/values[alias=rte,culture={culture},segment=null]/value/blocks" +
            $"/contentData[key={GridContainerKey}]/values[alias=blockGrid]/value" +
            $"/contentData[key={ListContainerKey}]/values[alias=blockList]/value";
    }

    [Test]
    public async Task PatchDocument_DeeplyNestedBlocks_ReplacesPropertyAtDeepestLevel()
    {
        // Arrange
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
        var setup = await CreateDeeplyNestedBlockDocument();

        // Path to the first text block's text property for en-US culture
        var path = setup.BlockListPathPrefix("en-US")
            + $"/contentData[key={setup.TextBlock1Key}]/values[alias=text,culture=en-US]/value";

        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "replace",
                    Path = path,
                    Value = "updated deep value"
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{setup.DocumentKey}/patch", httpContent);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {response.StatusCode}");
            Console.WriteLine($"Error body: {errorContent}");
        }

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify the deeply nested value was updated for en-US
        var updatedContent = await ContentEditingService.GetAsync(setup.DocumentKey);
        Assert.IsNotNull(updatedContent);

        var rteValue = updatedContent.GetValue<string>("rte", "en-US");
        Assert.IsNotNull(rteValue);

        // Parse through the nested structure to verify the patched value
        RichTextPropertyEditorHelper.TryParseRichTextEditorValue(rteValue, setup.JsonSerializer, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, out var parsedRte);
        Assert.IsNotNull(parsedRte?.Blocks);

        var gridContainerData = parsedRte!.Blocks!.ContentData.FirstOrDefault(b => b.Key == setup.GridContainerKey);
        Assert.IsNotNull(gridContainerData);

        var blockGridRaw = gridContainerData!.Values.FirstOrDefault(v => v.Alias == "blockGrid")?.Value?.ToString();
        Assert.IsNotNull(blockGridRaw);

        var blockGridVal = setup.JsonSerializer.Deserialize<BlockGridValue>(blockGridRaw!);
        Assert.IsNotNull(blockGridVal);

        var listContainerData = blockGridVal!.ContentData.FirstOrDefault(b => b.Key == setup.ListContainerKey);
        Assert.IsNotNull(listContainerData);

        var blockListRaw = listContainerData!.Values.FirstOrDefault(v => v.Alias == "blockList")?.Value?.ToString();
        Assert.IsNotNull(blockListRaw);

        var blockListVal = setup.JsonSerializer.Deserialize<BlockListValue>(blockListRaw!);
        Assert.IsNotNull(blockListVal);

        // Verify the patched text block
        var textBlock1 = blockListVal!.ContentData.FirstOrDefault(b => b.Key == setup.TextBlock1Key);
        Assert.IsNotNull(textBlock1);
        Assert.AreEqual("updated deep value",
            textBlock1!.Values.FirstOrDefault(v => v.Alias == "text" && v.Culture == "en-US")?.Value?.ToString());

        // Verify da-DK text was NOT changed
        Assert.AreEqual("original da",
            textBlock1.Values.FirstOrDefault(v => v.Alias == "text" && v.Culture == "da-DK")?.Value?.ToString());

        // Verify the second text block was NOT changed
        var textBlock2 = blockListVal.ContentData.FirstOrDefault(b => b.Key == setup.TextBlock2Key);
        Assert.IsNotNull(textBlock2);
        Assert.AreEqual("second block en",
            textBlock2!.Values.FirstOrDefault(v => v.Alias == "text" && v.Culture == "en-US")?.Value?.ToString());
    }

    [Test]
    public async Task PatchDocument_DeeplyNestedBlocks_AddsBlockToNestedBlockList()
    {
        // Arrange
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
        var setup = await CreateDeeplyNestedBlockDocument();

        var prefix = setup.BlockListPathPrefix("en-US");
        var newBlockKey = Guid.NewGuid();

        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"{prefix}/contentData/-",
                    Value = new
                    {
                        key = newBlockKey,
                        contentTypeKey = setup.TextBlockTypeKey,
                        values = new[]
                        {
                            new { alias = "text", culture = "en-US", value = "new block en" },
                            new { alias = "text", culture = "da-DK", value = "new block da" }
                        }
                    }
                },
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"{prefix}/layout/Umbraco.BlockList/-",
                    Value = new { contentKey = newBlockKey }
                },
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"{prefix}/expose/-",
                    Value = new { contentKey = newBlockKey, culture = "en-US" }
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{setup.DocumentKey}/patch", httpContent);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {response.StatusCode}");
            Console.WriteLine($"Error body: {errorContent}");
        }

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Parse through the nested structure to the block list
        var updatedContent = await ContentEditingService.GetAsync(setup.DocumentKey);
        Assert.IsNotNull(updatedContent);
        var rteValue = updatedContent.GetValue<string>("rte", "en-US");
        RichTextPropertyEditorHelper.TryParseRichTextEditorValue(rteValue, setup.JsonSerializer, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, out var parsedRte);
        var gridContainerData = parsedRte!.Blocks!.ContentData.First(b => b.Key == setup.GridContainerKey);
        var blockGridVal = setup.JsonSerializer.Deserialize<BlockGridValue>(gridContainerData.Values.First(v => v.Alias == "blockGrid").Value!.ToString()!);
        var listContainerData = blockGridVal!.ContentData.First(b => b.Key == setup.ListContainerKey);
        var blockListVal = setup.JsonSerializer.Deserialize<BlockListValue>(listContainerData.Values.First(v => v.Alias == "blockList").Value!.ToString()!);

        // Verify 3 blocks in contentData
        Assert.AreEqual(3, blockListVal!.ContentData.Count);

        // Verify new block was appended
        var newBlock = blockListVal.ContentData.FirstOrDefault(b => b.Key == newBlockKey);
        Assert.IsNotNull(newBlock);
        Assert.AreEqual("new block en", newBlock!.Values.FirstOrDefault(v => v.Alias == "text" && v.Culture == "en-US")?.Value?.ToString());

        // Verify originals unchanged
        Assert.AreEqual("original en", blockListVal.ContentData.First(b => b.Key == setup.TextBlock1Key).Values.First(v => v.Alias == "text" && v.Culture == "en-US").Value?.ToString());
        Assert.AreEqual("second block en", blockListVal.ContentData.First(b => b.Key == setup.TextBlock2Key).Values.First(v => v.Alias == "text" && v.Culture == "en-US").Value?.ToString());

        // Verify layout has 3 entries
        var layoutItems = blockListVal.Layout[Constants.PropertyEditors.Aliases.BlockList].ToList();
        Assert.AreEqual(3, layoutItems.Count);
    }

    [Test]
    public async Task PatchDocument_DeeplyNestedBlocks_InsertsBlockAtSpecificPosition()
    {
        // Arrange
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
        var setup = await CreateDeeplyNestedBlockDocument();

        var prefix = setup.BlockListPathPrefix("en-US");
        var newBlockKey = Guid.NewGuid();

        // Insert at index 1 (between the two existing blocks)
        var patchModel = new PatchDocumentRequestModel
        {
            Operations = new[]
            {
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"{prefix}/contentData/1",
                    Value = new
                    {
                        key = newBlockKey,
                        contentTypeKey = setup.TextBlockTypeKey,
                        values = new[]
                        {
                            new { alias = "text", culture = "en-US", value = "inserted block en" },
                            new { alias = "text", culture = "da-DK", value = "inserted block da" }
                        }
                    }
                },
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"{prefix}/layout/Umbraco.BlockList/1",
                    Value = new { contentKey = newBlockKey }
                },
                new PatchOperationRequestModel
                {
                    Op = "add",
                    Path = $"{prefix}/expose/1",
                    Value = new { contentKey = newBlockKey, culture = "en-US" }
                }
            }
        };

        // Act
        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json-patch+json");
        var response = await Client.PatchAsync($"/umbraco/management/api/v1/document/{setup.DocumentKey}/patch", httpContent);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {response.StatusCode}");
            Console.WriteLine($"Error body: {errorContent}");
        }

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Parse through the nested structure to the block list
        var updatedContent = await ContentEditingService.GetAsync(setup.DocumentKey);
        var rteValue = updatedContent.GetValue<string>("rte", "en-US");
        RichTextPropertyEditorHelper.TryParseRichTextEditorValue(rteValue, setup.JsonSerializer, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, out var parsedRte);
        var gridContainerData = parsedRte!.Blocks!.ContentData.First(b => b.Key == setup.GridContainerKey);
        var blockGridVal = setup.JsonSerializer.Deserialize<BlockGridValue>(gridContainerData.Values.First(v => v.Alias == "blockGrid").Value!.ToString()!);
        var listContainerData = blockGridVal!.ContentData.First(b => b.Key == setup.ListContainerKey);
        var blockListVal = setup.JsonSerializer.Deserialize<BlockListValue>(listContainerData.Values.First(v => v.Alias == "blockList").Value!.ToString()!);

        // Verify 3 blocks in contentData
        Assert.AreEqual(3, blockListVal!.ContentData.Count);

        // Verify insertion order: original1, inserted, original2
        Assert.AreEqual(setup.TextBlock1Key, blockListVal.ContentData[0].Key);
        Assert.AreEqual(newBlockKey, blockListVal.ContentData[1].Key);
        Assert.AreEqual(setup.TextBlock2Key, blockListVal.ContentData[2].Key);

        // Verify inserted block values
        Assert.AreEqual("inserted block en", blockListVal.ContentData[1].Values.First(v => v.Alias == "text" && v.Culture == "en-US").Value?.ToString());

        // Verify layout order matches
        var layoutItems = blockListVal.Layout[Constants.PropertyEditors.Aliases.BlockList].ToList();
        Assert.AreEqual(3, layoutItems.Count);
    }
}
