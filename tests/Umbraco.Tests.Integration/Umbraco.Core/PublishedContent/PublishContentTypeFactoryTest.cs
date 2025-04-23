using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.PublishedContent;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PublishContentTypeFactoryTest : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<DataTypeSavedNotification, DataTypeSavedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        base.CustomTestSetup(builder);
    }

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IPublishedContentTypeFactory PublishedContentTypeFactory => GetRequiredService<IPublishedContentTypeFactory>();

    [Test]
    public async Task Can_Update_Content_Type()
    {
        // Create a content type
        var template = TemplateBuilder.CreateTextPageTemplate("defaultTemplate");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateKey: template.Key);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeAttempt.Success);
        Assert.IsNotNull(contentTypeAttempt.Result);

        // Fetch the content type to cache data types
        var contentType = contentTypeAttempt.Result;
        PublishedContentTypeFactory.CreateContentType(contentType);

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .Build();
        var dataTypeCreateResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeCreateResult.Success);

        contentType.AddPropertyGroup("group", "Group");
        var propertyTypeAlias = "test";
        var propertyType = new PropertyTypeBuilder()
            .WithAlias(propertyTypeAlias)
            .WithDataTypeId(dataTypeCreateResult.Result.Id)
            .Build();
        propertyType.DataTypeKey = dataType.Key;

        contentType.AddPropertyType(propertyType, "group", "Group");

        // Update the content type
        var contentTypeUpdate = ContentTypeUpdateHelper.CreateContentTypeUpdateModel(contentType);
        var updateResult = await ContentTypeEditingService.UpdateAsync(contentType, contentTypeUpdate, Constants.Security.SuperUserKey);
        Assert.IsTrue(updateResult.Success);


        var publishedContentType = PublishedContentTypeFactory.CreateContentType(updateResult.Result);
        Assert.That(publishedContentType.PropertyTypes.Where(x => x.Alias == propertyTypeAlias), Is.Not.Empty);
    }

    [Test]
    public async Task Can_Get_Updated_Datatype()
    {
        var dataType = new DataTypeBuilder()
            .WithId(0)
            .Build();
        dataType.EditorUiAlias = "NotUpdated";
        var dataTypeCreateResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeCreateResult.Success);
        var createdDataType = dataTypeCreateResult.Result;
        PublishedDataType createdPublishedDataType = PublishedContentTypeFactory.GetDataType(createdDataType.Id);
        Assert.That(createdPublishedDataType.EditorUiAlias, Is.EqualTo("NotUpdated"));

        createdDataType.EditorUiAlias = "Updated";
        var dataTypeUpdateResult = await DataTypeService.UpdateAsync(createdDataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeUpdateResult.Success);
        var updatedPublishedDataType = PublishedContentTypeFactory.GetDataType(createdDataType.Id);
        Assert.That(updatedPublishedDataType.EditorUiAlias, Is.EqualTo("Updated"));
    }
}
