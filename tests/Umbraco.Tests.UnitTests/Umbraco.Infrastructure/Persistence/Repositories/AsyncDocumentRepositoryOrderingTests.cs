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
        string? ContentTypeAlias,
        string? Path);

    // Deliberately gives the row with the HIGHER NodeId the earlier position in the source sequence,
    // decoupling "sequence order" from "NodeId order". A real SQLite integration test can't construct
    // this: there, NodeId == insertion order == the engine's incidental scan order for freshly-created
    // rows, so a missing tiebreak coincidentally still produces NodeId-ascending output and the bug
    // goes undetected. Here, in-memory sequence order is fully under the test's control.
    private static List<OrderingTestRow> CreateTiedRows() =>
    [
        new(NodeId: 200, SortOrder: 0, Text: "Same", CreateDate: DateTime.MinValue, VersionDate: DateTime.MinValue, OwnerId: -1, Published: false, ContentTypeAlias: "alias", Path: "-1,999"),
        new(NodeId: 100, SortOrder: 0, Text: "Same", CreateDate: DateTime.MinValue, VersionDate: DateTime.MinValue, OwnerId: -1, Published: false, ContentTypeAlias: "alias", Path: "-1,999"),
    ];

    // Same Path for both rows (the thing being tied), but DIFFERENT SortOrder — unlike CreateTiedRows().
    // If the "path" switch case were missing and silently fell through to the sortOrderSelector default,
    // NodeId 200 (SortOrder 1) would sort before NodeId 100 (SortOrder 2), producing {200, 100} — visibly
    // different from the correct path-tiebreak result of {100, 200}. Reusing CreateTiedRows() here would
    // NOT be discriminating: both rows also share SortOrder there, so the fallback default ordering would
    // coincidentally tiebreak to the same {100, 200} the correct implementation produces.
    private static List<OrderingTestRow> CreatePathTiedRowsWithDistinctSortOrder() =>
    [
        new(NodeId: 200, SortOrder: 1, Text: "Same", CreateDate: DateTime.MinValue, VersionDate: DateTime.MinValue, OwnerId: -1, Published: false, ContentTypeAlias: "alias", Path: "-1,999"),
        new(NodeId: 100, SortOrder: 2, Text: "Same", CreateDate: DateTime.MinValue, VersionDate: DateTime.MinValue, OwnerId: -1, Published: false, ContentTypeAlias: "alias", Path: "-1,999"),
    ];

    private static List<int> ApplyOrderingAndGetNodeIds(Ordering? ordering, List<OrderingTestRow>? rows = null)
    {
        IOrderedQueryable<OrderingTestRow> ordered = AsyncDocumentRepository.ApplyDocumentOrdering(
            (rows ?? CreateTiedRows()).AsQueryable(),
            ordering,
            sortOrderSelector: row => row.SortOrder,
            textSelector: row => row.Text,
            createDateSelector: row => row.CreateDate,
            versionDateSelector: row => row.VersionDate,
            idSelector: row => row.NodeId,
            ownerSelector: row => row.OwnerId,
            publishedSelector: row => row.Published,
            contentTypeAliasSelector: row => row.ContentTypeAlias,
            pathSelector: row => row.Path);

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

    [Test]
    public void ApplyDocumentOrdering_PathOrderingTied_BreaksTieByAscendingNodeId()
    {
        // A real node's Path always includes its own NodeId, so two real rows can never share a Path —
        // this tie is only constructible here, against a synthetic in-memory sequence.
        List<int> nodeIds = ApplyOrderingAndGetNodeIds(Ordering.By("path"), CreatePathTiedRowsWithDistinctSortOrder());

        Assert.That(nodeIds, Is.EqualTo(new[] { 100, 200 }),
            "tied Path must break the tie by ascending NodeId — a missing \"path\" case would instead fall " +
            "through to the SortOrder default and produce {200, 100}");
    }
}
