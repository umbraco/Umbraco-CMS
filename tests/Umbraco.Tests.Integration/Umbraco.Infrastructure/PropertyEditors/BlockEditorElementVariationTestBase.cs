using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal abstract class BlockEditorElementVariationTestBase : UmbracoIntegrationTest
{
    protected ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    protected IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");

        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        builder.Services.AddUnique<IUmbracoContextAccessor, TestUmbracoContextAccessor>();
        builder.Services.AddUnique(mockHttpContextAccessor.Object);
        builder.AddUmbracoHybridCache();

        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [SetUp]
    public async Task SetUp() => await LanguageService.CreateAsync(
        new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

    protected void PublishContent(IContent content, string[] culturesToPublish)
    {
        var publishResult = ContentService.Publish(content, culturesToPublish);
        Assert.IsTrue(publishResult.Success);
        DocumentCacheService.RefreshContentAsync(content);
    }

    protected IContentType CreateElementType(ContentVariation variation, string alias = "myElementType")
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(alias)
            .WithName("My Element Type")
            .WithIsElement(true)
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("variantText")
            .WithName("Variant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(variation)
            .Done()
            .Build();
        ContentTypeService.Save(elementType);
        return elementType;
    }

    protected IContentType CreateContentType(ContentVariation contentTypeVariation, IDataType blocksEditorDataType, ContentVariation blocksPropertyVariation = ContentVariation.Nothing)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("myPage")
            .WithName("My Page")
            .WithContentVariation(contentTypeVariation)
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(blocksEditorDataType.Id)
            .WithVariations(blocksPropertyVariation)
            .Done()
            .Build();
        ContentTypeService.Save(contentType);
        return contentType;
    }

    protected IPublishedContent GetPublishedContent(Guid key)
    {
        UmbracoContextAccessor.Clear();
        var umbracoContext = UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
        var publishedContent = umbracoContext.Content?.GetById(key);
        Assert.IsNotNull(publishedContent);

        return publishedContent;
    }

    protected void SetVariationContext(string? culture, string? segment)
        => VariationContextAccessor.VariationContext = new VariationContext(culture: culture, segment: segment);

    protected async Task<IDataType> CreateBlockEditorDataType<T>(string propertyEditorAlias, T blocksConfiguration)
    {
        var dataType = new DataType(PropertyEditorCollection[propertyEditorAlias], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object> { { "blocks", blocksConfiguration } },
            Name = "My Block Editor",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    protected void RefreshContentTypeCache(params IContentType[] contentTypes)
    {
        // ContentTypeCacheRefresher.JsonPayload[] payloads = contentTypes
        //     .Select(contentType => new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), contentType.Id, ContentTypeChangeTypes.RefreshMain))
        //     .ToArray();

        DocumentCacheService.Rebuild(contentTypes.Select(x => x.Id).ToArray());
    }
}
