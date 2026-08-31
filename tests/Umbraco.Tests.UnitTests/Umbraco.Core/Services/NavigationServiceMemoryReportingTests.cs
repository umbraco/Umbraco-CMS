// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class NavigationServiceMemoryReportingTests
{
    [Test]
    public void Can_Report_Cache_Name_For_Documents()
        => Assert.That(AsReporter(CreateDocumentNavigationService()).CacheName, Is.EqualTo("Document navigation"));

    [Test]
    public void Can_Report_Cache_Name_For_Media()
        => Assert.That(AsReporter(CreateMediaNavigationService()).CacheName, Is.EqualTo("Media navigation"));

    [Test]
    public void Can_Report_Cache_Name_For_Elements()
        => Assert.That(AsReporter(CreateElementNavigationService()).CacheName, Is.EqualTo("Element navigation"));

    [Test]
    public void Can_Report_Node_Count_Including_Recycle_Bin_For_Documents()
        => AssertReportsNodeCountIncludingRecycleBin(CreateDocumentNavigationService());

    [Test]
    public void Can_Report_Node_Count_Including_Recycle_Bin_For_Media()
        => AssertReportsNodeCountIncludingRecycleBin(CreateMediaNavigationService());

    [Test]
    public void Can_Report_Node_Count_Including_Recycle_Bin_For_Elements()
        => AssertReportsNodeCountIncludingRecycleBin(CreateElementNavigationService());

    private static DocumentNavigationService CreateDocumentNavigationService()
        => new(Mock.Of<ICoreScopeProvider>(), Mock.Of<INavigationRepository>(), Mock.Of<IContentTypeService>());

    private static MediaNavigationService CreateMediaNavigationService()
        => new(Mock.Of<ICoreScopeProvider>(), Mock.Of<INavigationRepository>(), Mock.Of<IMediaTypeService>());

    private static ElementNavigationService CreateElementNavigationService()
        => new(Mock.Of<ICoreScopeProvider>(), Mock.Of<INavigationRepository>(), Mock.Of<IContentTypeService>());

    private static IMemoryCacheSizeReporter AsReporter(object service) => (IMemoryCacheSizeReporter)service;

    private static void AssertReportsNodeCountIncludingRecycleBin<TContentType, TContentTypeService>(
        ContentNavigationServiceBase<TContentType, TContentTypeService> sut)
        where TContentType : class, IContentTypeComposition
        where TContentTypeService : IContentTypeBaseService<TContentType>
    {
        var reporter = AsReporter(sut);

        Assert.That(reporter.GetApproximateCount(), Is.EqualTo(0), "Expected an empty navigation tree to report zero nodes.");

        var contentTypeKey = Guid.NewGuid();
        var root = Guid.NewGuid();
        var child = Guid.NewGuid();
        sut.Add(root, contentTypeKey);
        sut.Add(child, contentTypeKey, root);

        Assert.That(reporter.GetApproximateCount(), Is.EqualTo(2), "Expected both added nodes to be counted.");

        // Moving a node to the recycle bin must keep it counted — the reporter spans the active tree and the bin.
        sut.MoveToBin(child);

        Assert.That(reporter.GetApproximateCount(), Is.EqualTo(2), "Expected a binned node to still be counted (active tree + recycle bin).");
    }
}
