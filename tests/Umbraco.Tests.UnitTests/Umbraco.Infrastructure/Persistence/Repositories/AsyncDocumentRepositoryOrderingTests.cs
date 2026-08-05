using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class AsyncDocumentRepositoryOrderingTests
{
    private sealed record OrderingTestRow(
        int NodeId,
        int SortOrder,
        string? Text,
        DateTime CreateDate,
        DateTime VersionDate,
        int? OwnerId,
        bool Published,
        string? ContentTypeAlias);

    // Deliberately gives the row with the HIGHER NodeId the earlier position in the source sequence,
    // decoupling "sequence order" from "NodeId order". A real SQLite integration test can't construct
    // this: there, NodeId == insertion order == the engine's incidental scan order for freshly-created
    // rows, so a missing tiebreak coincidentally still produces NodeId-ascending output and the bug
    // goes undetected. Here, in-memory sequence order is fully under the test's control.
    private static List<OrderingTestRow> CreateTiedRows() =>
    [
        new(NodeId: 200, SortOrder: 0, Text: "Same", CreateDate: DateTime.MinValue, VersionDate: DateTime.MinValue, OwnerId: -1, Published: false, ContentTypeAlias: "alias"),
        new(NodeId: 100, SortOrder: 0, Text: "Same", CreateDate: DateTime.MinValue, VersionDate: DateTime.MinValue, OwnerId: -1, Published: false, ContentTypeAlias: "alias"),
    ];

    private static List<int> ApplyOrderingAndGetNodeIds(Ordering? ordering)
    {
        IOrderedQueryable<OrderingTestRow> ordered = AsyncDocumentRepository.ApplyDocumentOrdering(
            CreateTiedRows().AsQueryable(),
            ordering,
            sortOrderSelector: row => row.SortOrder,
            textSelector: row => row.Text,
            createDateSelector: row => row.CreateDate,
            versionDateSelector: row => row.VersionDate,
            idSelector: row => row.NodeId,
            ownerSelector: row => row.OwnerId,
            publishedSelector: row => row.Published,
            contentTypeAliasSelector: row => row.ContentTypeAlias);

        return ordered.Select(row => row.NodeId).ToList();
    }

    [Test]
    public void ApplyDocumentOrdering_DefaultSortOrderTied_BreaksTieByAscendingNodeId()
    {
        // No explicit ordering falls through to the sortOrderSelector branch — the branch every
        // GetChildrenCoreAsync/GetDescendantsCoreAsync/GetPagedRecycleBinAsync caller hits by default.
        List<int> nodeIds = ApplyOrderingAndGetNodeIds(ordering: null);

        Assert.That(nodeIds, Is.EqualTo(new[] { 100, 200 }),
            "tied SortOrder must break the tie by ascending NodeId, not preserve source sequence order (200 was listed first)");
    }

    [Test]
    public void ApplyDocumentOrdering_NameOrderingTied_BreaksTieByAscendingNodeId()
    {
        List<int> nodeIds = ApplyOrderingAndGetNodeIds(Ordering.By("name"));

        Assert.That(nodeIds, Is.EqualTo(new[] { 100, 200 }),
            "tied name (node.Text) must break the tie by ascending NodeId, not preserve source sequence order");
    }

    [Test]
    public void ApplyDocumentOrdering_OrderingById_DoesNotAddARedundantSecondTiebreak()
    {
        // Ordering directly by "id" already produces a unique order — ThenBy(idSelector) would be a
        // harmless no-op if applied, but the production code explicitly skips it for this case.
        List<int> nodeIds = ApplyOrderingAndGetNodeIds(Ordering.By("id"));

        Assert.That(nodeIds, Is.EqualTo(new[] { 100, 200 }));
    }
}
