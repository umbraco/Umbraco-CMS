using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DateTimePropertyEditorTests : UmbracoIntegrationTest
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private static readonly object[] _sourceList1 =
    [
        new object[] { Constants.PropertyEditors.Aliases.DateOnly, false, new DateOnly(2025, 6, 22) },
        new object[] { Constants.PropertyEditors.Aliases.TimeOnly, false, new TimeOnly(18, 33, 1) },
        new object[] { Constants.PropertyEditors.Aliases.DateTimeUnspecified, false, new DateTime(2025, 6, 22, 18, 33, 1) },
        new object[] { Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, true, new DateTimeOffset(2025, 6, 22, 18, 33, 1, TimeSpan.FromHours(2)) },
    ];

    [TestCaseSource(nameof(_sourceList1))]
    public async Task Returns_Correct_Type_Based_On_Configuration(
        string editorAlias,
        bool timeZone,
        object expectedValue)
    {
        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Ntext)
            .AddEditor()
            .WithAlias(editorAlias)
            .WithConfigurationEditor(
                new DateTimeConfigurationEditor(IOHelper)
                {
                    DefaultConfiguration = new Dictionary<string, object>
                    {
                        ["timeFormat"] = "HH:mm",
                        ["timeZones"] = timeZone ? new { mode = "all" } : null,
                    },
                })
            .WithDefaultConfiguration(
                new Dictionary<string, object>
                {
                    ["timeFormat"] = "HH:mm",
                    ["timeZones"] = timeZone ? new { mode = "all" } : null,
                })
            .Done()
            .Build();

        var dataTypeCreateResult = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeCreateResult.Success);

        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .WithName("Content Type")
            .WithAllowAsRoot(true)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithAlias("dateTime")
                    .WithName("Date Time")
                    .WithDataTypeId(dataTypeCreateResult.Result.Id)
                    .Done()
                .Done()
            .Build();

        var contentTypeCreateResult = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(contentTypeCreateResult.Success);

        var content = new ContentEditingBuilder()
            .WithContentTypeKey(contentType.Key)
            .AddVariant()
                .WithName("My Content")
                .Done()
            .AddProperty()
                .WithAlias("dateTime")
                .WithValue(
                    new JsonObject
                    {
                        ["date"] = "2025-06-22T18:33:01.0000000+02:00",
                        ["timeZone"] = "Europe/Copenhagen",
                    })
                .Done()
            .Build();
        var createContentResult = await ContentEditingService.CreateAsync(content, Constants.Security.SuperUserKey);
        Assert.IsTrue(createContentResult.Success);
        Assert.IsNotNull(createContentResult.Result.Content);
        var dateTimeProperty = createContentResult.Result.Content.Properties["dateTime"];
        Assert.IsNotNull(dateTimeProperty, "After content creation, the property should exist");
        Assert.IsNotNull(dateTimeProperty.GetValue(), "After content creation, the property value should not be null");

        var publishResult = await ContentPublishingService.PublishBranchAsync(
            createContentResult.Result.Content.Key,
            [],
            PublishBranchFilter.IncludeUnpublished,
            Constants.Security.SuperUserKey,
            false);

        Assert.IsTrue(publishResult.Success);

        var publishedContent = await PublishedContentCache.GetByIdAsync(createContentResult.Result.Content.Key, false);
        Assert.IsNotNull(publishedContent);

        var value = publishedContent.GetProperty("dateTime")?.GetValue();
        Assert.IsNotNull(value);
        Assert.AreEqual(expectedValue, value);
    }
}
