// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Search.Core.Extensions;

public class ContentIndexingDataCollectionServiceTests
{
    [Test]
    public async Task CollectAsync_WhenTwoIndexersContributeTheSameField_MergesTheirValues()
    {
        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.SetupGet(m => m.Variations).Returns(ContentVariation.Nothing);

        var contentMock = new Mock<IContent>();
        contentMock.SetupGet(m => m.Key).Returns(Guid.NewGuid());
        contentMock.SetupGet(m => m.ContentType).Returns(contentTypeMock.Object);

        var systemFieldsIndexerMock = new Mock<ISystemFieldsContentIndexer>();
        systemFieldsIndexerMock
            .Setup(m => m.GetIndexFieldsAsync(It.IsAny<IContentBase>(), It.IsAny<string?[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var keywordIndexerMock = new Mock<IContentIndexer>();
        keywordIndexerMock
            .Setup(m => m.GetIndexFieldsAsync(It.IsAny<IContentBase>(), It.IsAny<string?[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IndexField("inventoryNumber", new IndexValue { Keywords = ["ABC-123"] }, null, null)]);

        var textIndexerMock = new Mock<IContentIndexer>();
        textIndexerMock
            .Setup(m => m.GetIndexFieldsAsync(It.IsAny<IContentBase>(), It.IsAny<string?[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new IndexField("inventoryNumber", new IndexValue { Texts = ["ABC-123"] }, null, null)]);

        var indexDocumentServiceMock = new Mock<IIndexDocumentService>();
        indexDocumentServiceMock
            .Setup(m => m.GetAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((IndexDocument?)null);

        var service = new ContentIndexingDataCollectionService(
            [systemFieldsIndexerMock.Object, keywordIndexerMock.Object, textIndexerMock.Object],
            Mock.Of<ILogger<ContentIndexingDataCollectionService>>(),
            indexDocumentServiceMock.Object);

        IEnumerable<IndexField>? fields = await service.CollectAsync(contentMock.Object, false, CancellationToken.None);

        Assert.That(fields, Is.Not.Null);
        IndexField[] inventoryNumberFields = fields!.Where(f => f.FieldName == "inventoryNumber").ToArray();
        Assert.That(inventoryNumberFields, Has.Length.EqualTo(1));

        IndexValue value = inventoryNumberFields[0].Value;
        Assert.Multiple(() =>
        {
            Assert.That(value.Keywords, Is.Not.Null.And.Contains("ABC-123"));
            Assert.That(value.Texts, Is.Not.Null.And.Contains("ABC-123"));
        });
    }
}
