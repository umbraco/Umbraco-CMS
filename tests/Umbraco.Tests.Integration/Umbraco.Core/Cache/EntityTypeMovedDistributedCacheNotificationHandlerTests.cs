using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

/// <summary>
/// Tests for the handlers that invalidate moved entity types across servers.
/// </summary>
/// <remarks>
/// A move only changes an entity's parent, path and level. The repository cache policies clear the local cache
/// on save but queue no cache instruction, so before these handlers existed other servers kept serving the
/// entity with a stale path and level.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class EntityTypeMovedDistributedCacheNotificationHandlerTests : UmbracoIntegrationTest
{
    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    [SetUp]
    public void ResetCapturedPayloads() => CacheRefreshCapturingHandler.Payloads.Clear();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Integration tests use a no-op server messenger and do not register the distributed cache notification
        // handlers by default, so opt in to the handlers under test and a messenger that delivers locally.
        builder.AddNotificationHandler<ContentTypeMovedNotification, ContentTypeMovedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<DataTypeMovedNotification, DataTypeMovedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<EntityTypeMovedCacheRefresherNotification, CacheRefreshCapturingHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public async Task Moving_Content_Type_Refreshes_Its_Cache()
    {
        EntityContainer container = (await ContentTypeContainerService.CreateAsync(null, "Container", null, Constants.Security.SuperUserKey)).Result!;

        ContentType contentType = ContentTypeBuilder.CreateBasicContentType("test", "Test");
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var moveResult = await ContentTypeService.MoveAsync(contentType.Key, container.Key);
        Assert.IsTrue(moveResult.Success, $"Failed to move content type: {moveResult.Status}");

        AssertRefreshedExactly(nameof(IContentType), contentType.Id, contentType.Key);
    }

    [Test]
    public async Task Moving_Data_Type_Refreshes_Its_Cache()
    {
        EntityContainer container = (await DataTypeContainerService.CreateAsync(null, "Container", null, Constants.Security.SuperUserKey)).Result!;

        IDataType dataType = await CreateDataTypeAsync();

        var moveResult = await DataTypeService.MoveAsync(dataType, container.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(moveResult.Success, $"Failed to move data type: {moveResult.Status}");

        AssertRefreshedExactly(nameof(IDataType), dataType.Id, dataType.Key);
    }

    private async Task<IDataType> CreateDataTypeAsync()
    {
        IDataType dataType = new DataType(
            GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.TextBox],
            GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            Name = "Test Data Type",
        };

        Attempt<IDataType, DataTypeOperationStatus> result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create data type: {result.Status}");
        return result.Result;
    }

    private static void AssertRefreshedExactly(string expectedItemType, int expectedId, Guid expectedKey)
    {
        EntityTypeMovedCacheRefresher.JsonPayload[] payloads = CacheRefreshCapturingHandler.Payloads.ToArray();

        Assert.AreEqual(1, payloads.Length, "Expected exactly one moved entity to be refreshed.");
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedItemType, payloads[0].ItemType);
            Assert.AreEqual(expectedId, payloads[0].Id);
            Assert.AreEqual(expectedKey, payloads[0].Key);
        });
    }

    /// <summary>
    /// Captures the payloads the <see cref="EntityTypeMovedCacheRefresher"/> was invoked with. The refresher
    /// itself clears caches that the local server has already cleared, so the dispatch is what is observable.
    /// </summary>
    private sealed class CacheRefreshCapturingHandler : INotificationHandler<EntityTypeMovedCacheRefresherNotification>
    {
        public static List<EntityTypeMovedCacheRefresher.JsonPayload> Payloads { get; } = [];

        public void Handle(EntityTypeMovedCacheRefresherNotification notification)
        {
            if (notification.MessageObject is EntityTypeMovedCacheRefresher.JsonPayload[] payloads)
            {
                Payloads.AddRange(payloads);
            }
        }
    }
}
