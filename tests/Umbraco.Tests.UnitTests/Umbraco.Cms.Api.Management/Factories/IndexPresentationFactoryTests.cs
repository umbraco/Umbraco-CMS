using System.Collections.ObjectModel;
using Examine;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class IndexPresentationFactoryTests
{
    [Test]
    public async Task Create_Should_Set_HealthStatusMessage_On_Diagnostics_Failure()
    {
        var indexDiagnosticsFailureMessage = "something is wrong";
        // arrange
        var indexMock = new Mock<IIndex>();
        indexMock
            .SetupGet(index => index.Name)
            .Returns("testIndex");
        indexMock
            .SetupGet(index => index.Searcher)
            .Returns(Mock.Of<ISearcher>());

        var indexDiagnosticsMock = new Mock<IIndexDiagnostics>();
        indexDiagnosticsMock
            .Setup(diagnostic => diagnostic.IsHealthy())
            .Returns(Attempt<string?>.Fail(indexDiagnosticsFailureMessage));
        indexDiagnosticsMock
            .SetupGet(diagnostic => diagnostic.Metadata)
            .Returns(ReadOnlyDictionary<string, object?>.Empty);
        indexDiagnosticsMock
            .Setup(diagnostic => diagnostic.GetFieldNames())
            .Returns(Enumerable.Empty<string>());

        var indexDiagnosticsFactoryMock = new Mock<IIndexDiagnosticsFactory>();
        indexDiagnosticsFactoryMock
            .Setup(f => f.Create(It.IsAny<IIndex>()))
            .Returns(indexDiagnosticsMock.Object);

        var indexRebuilderMock = new Mock<IIndexRebuilder>();

        var indexRebuilderServiceMock = new Mock<IIndexingRebuilderService>();
        indexRebuilderServiceMock
            .Setup(rebuilder => rebuilder.IsRebuildingAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var factory = new IndexPresentationFactory(
            indexDiagnosticsFactoryMock.Object,
            indexRebuilderMock.Object,
            indexRebuilderServiceMock.Object,
            Mock.Of<ILogger<IndexPresentationFactory>>());


        // act
        var responseModel = await factory.CreateAsync(indexMock.Object);

        // assert
        Assert.AreEqual(indexDiagnosticsFailureMessage, responseModel.HealthStatus.Message);
    }

    [Test]
    public async Task Create_Should_Set_UniqueKeyFieldName_For_UmbracoIndex()
    {
        var expectedFieldName = "__Key";

        // arrange
        var indexMock = new Mock<IUmbracoIndex>();
        indexMock
            .SetupGet(index => index.Name)
            .Returns("testIndex");
        indexMock
            .As<IIndex>()
            .SetupGet(index => index.Searcher)
            .Returns(Mock.Of<ISearcher>());
        indexMock
            .SetupGet(index => index.UniqueKeyFieldName)
            .Returns(expectedFieldName);

        var indexDiagnosticsMock = new Mock<IIndexDiagnostics>();
        indexDiagnosticsMock
            .Setup(diagnostic => diagnostic.IsHealthy())
            .Returns(Attempt<string?>.Succeed());
        indexDiagnosticsMock
            .SetupGet(diagnostic => diagnostic.Metadata)
            .Returns(ReadOnlyDictionary<string, object?>.Empty);
        indexDiagnosticsMock
            .Setup(diagnostic => diagnostic.GetFieldNames())
            .Returns(Enumerable.Empty<string>());

        var indexDiagnosticsFactoryMock = new Mock<IIndexDiagnosticsFactory>();
        indexDiagnosticsFactoryMock
            .Setup(f => f.Create(It.IsAny<IIndex>()))
            .Returns(indexDiagnosticsMock.Object);

        var indexRebuilderServiceMock = new Mock<IIndexingRebuilderService>();
        indexRebuilderServiceMock
            .Setup(rebuilder => rebuilder.IsRebuildingAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var factory = new IndexPresentationFactory(
            indexDiagnosticsFactoryMock.Object,
            new Mock<IIndexRebuilder>().Object,
            indexRebuilderServiceMock.Object,
            Mock.Of<ILogger<IndexPresentationFactory>>());

        // act
        var responseModel = await factory.CreateAsync(indexMock.Object);

        // assert
        Assert.AreEqual(expectedFieldName, responseModel.UniqueKeyFieldName);
    }

    [Test]
    public async Task Create_Should_Set_UniqueKeyFieldName_Null_For_NonUmbracoIndex()
    {
        // arrange
        var indexMock = new Mock<IIndex>();
        indexMock
            .SetupGet(index => index.Name)
            .Returns("testIndex");
        indexMock
            .SetupGet(index => index.Searcher)
            .Returns(Mock.Of<ISearcher>());

        var indexDiagnosticsMock = new Mock<IIndexDiagnostics>();
        indexDiagnosticsMock
            .Setup(diagnostic => diagnostic.IsHealthy())
            .Returns(Attempt<string?>.Succeed());
        indexDiagnosticsMock
            .SetupGet(diagnostic => diagnostic.Metadata)
            .Returns(ReadOnlyDictionary<string, object?>.Empty);
        indexDiagnosticsMock
            .Setup(diagnostic => diagnostic.GetFieldNames())
            .Returns(Enumerable.Empty<string>());

        var indexDiagnosticsFactoryMock = new Mock<IIndexDiagnosticsFactory>();
        indexDiagnosticsFactoryMock
            .Setup(f => f.Create(It.IsAny<IIndex>()))
            .Returns(indexDiagnosticsMock.Object);

        var indexRebuilderServiceMock = new Mock<IIndexingRebuilderService>();
        indexRebuilderServiceMock
            .Setup(rebuilder => rebuilder.IsRebuildingAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var factory = new IndexPresentationFactory(
            indexDiagnosticsFactoryMock.Object,
            new Mock<IIndexRebuilder>().Object,
            indexRebuilderServiceMock.Object,
            Mock.Of<ILogger<IndexPresentationFactory>>());

        // act
        var responseModel = await factory.CreateAsync(indexMock.Object);

        // assert
        Assert.IsNull(responseModel.UniqueKeyFieldName);
    }

    [Test]
    public async Task Create_Should_Set_UniqueKeyFieldName_When_Rebuilding()
    {
        var expectedFieldName = "itemId";

        // arrange
        var indexMock = new Mock<IUmbracoIndex>();
        indexMock
            .SetupGet(index => index.Name)
            .Returns("testIndex");
        indexMock
            .As<IIndex>()
            .SetupGet(index => index.Searcher)
            .Returns(Mock.Of<ISearcher>());
        indexMock
            .SetupGet(index => index.UniqueKeyFieldName)
            .Returns(expectedFieldName);

        var indexRebuilderServiceMock = new Mock<IIndexingRebuilderService>();
        indexRebuilderServiceMock
            .Setup(rebuilder => rebuilder.IsRebuildingAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var factory = new IndexPresentationFactory(
            new Mock<IIndexDiagnosticsFactory>().Object,
            new Mock<IIndexRebuilder>().Object,
            indexRebuilderServiceMock.Object,
            Mock.Of<ILogger<IndexPresentationFactory>>());

        // act
        var responseModel = await factory.CreateAsync(indexMock.Object);

        // assert
        Assert.AreEqual(expectedFieldName, responseModel.UniqueKeyFieldName);
    }
}
