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

/// <summary>
/// Contains unit tests for verifying the behavior of the <see cref="IndexPresentationFactory"/> class in the Umbraco CMS API Management context.
/// </summary>
[TestFixture]
public class IndexPresentationFactoryTests
{
    /// <summary>
    /// Tests that the CreateAsync method sets the HealthStatus message correctly when diagnostics report a failure.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
}
