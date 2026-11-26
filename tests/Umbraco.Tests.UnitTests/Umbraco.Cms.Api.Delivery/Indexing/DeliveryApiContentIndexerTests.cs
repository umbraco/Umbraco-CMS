using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Indexing;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using IndexField = Umbraco.Cms.Core.Search.Indexing.IndexField;
using SystemIndexField = Umbraco.Cms.Core.DeliveryApi.IndexField;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Indexing;

[TestFixture]
public class DeliveryApiContentIndexerTests
{
    [Test]
    public async Task GetIndexFieldsAsync_Skips_System_Content_Index_Handlers()
    {
        Mock<ISystemContentIndexHandler> systemHandler = CreateHandlerMock<ISystemContentIndexHandler>("systemField", "system-value");
        Mock<IContentIndexHandler> customHandler = CreateHandlerMock<IContentIndexHandler>("customField", "custom-value");

        var collection = new ContentIndexHandlerCollection(() => new IContentIndexHandler[] { systemHandler.Object, customHandler.Object });

        var indexer = new DeliveryApiContentIndexer(
            collection,
            Mock.Of<IDateTimeOffsetConverter>(),
            Mock.Of<ILogger<DeliveryApiContentIndexer>>());

        IEnumerable<IndexField> result = await indexer.GetIndexFieldsAsync(
            Mock.Of<IContent>(),
            [null],
            published: true,
            CancellationToken.None);

        var fieldNames = result.Select(f => f.FieldName).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(fieldNames, Does.Contain("customField"));
            Assert.That(fieldNames, Does.Not.Contain("systemField"));
        });
    }

    private static Mock<THandler> CreateHandlerMock<THandler>(string fieldName, string value)
        where THandler : class, IContentIndexHandler
    {
        var handler = new Mock<THandler>();
        handler.Setup(h => h.GetFields()).Returns(new[]
        {
            new SystemIndexField { FieldName = fieldName, FieldType = FieldType.StringRaw, VariesByCulture = false },
        });
        handler.Setup(h => h.GetFieldValues(It.IsAny<IContent>(), It.IsAny<string>())).Returns(new[]
        {
            new IndexFieldValue { FieldName = fieldName, Values = new object[] { value } },
        });
        return handler;
    }
}
