using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class DocumentRepositoryOrderingTests
{
    // Deliberately gives the row with the HIGHER NodeId the earlier position in the source sequence,
    // decoupling "sequence order" from "NodeId order". A real SQLite integration test can't construct
    // this: there, NodeId == insertion order == the engine's incidental scan order for freshly-created
    // rows, so a missing tiebreak coincidentally still produces NodeId-ascending output and the bug
    // goes undetected. Here, in-memory sequence order is fully under the test's control.
    private static List<DocumentRepository.DocumentJoinRow> CreateTiedRows() =>
    [
        CreateRow(nodeId: 200, sortOrder: 0, path: "-1,999"),
        CreateRow(nodeId: 100, sortOrder: 0, path: "-1,999"),
    ];

    // Same Path for both rows (the thing being tied), but DIFFERENT SortOrder — unlike CreateTiedRows().
    // If the "path" switch case were missing and silently fell through to the sortOrderSelector default,
    // NodeId 200 (SortOrder 1) would sort before NodeId 100 (SortOrder 2), producing {200, 100} — visibly
    // different from the correct path-tiebreak result of {100, 200}. Reusing CreateTiedRows() here would
    // NOT be discriminating: both rows also share SortOrder there, so the fallback default ordering would
    // coincidentally tiebreak to the same {100, 200} the correct implementation produces.
    private static List<DocumentRepository.DocumentJoinRow> CreatePathTiedRowsWithDistinctSortOrder() =>
    [
        CreateRow(nodeId: 200, sortOrder: 1, path: "-1,999"),
        CreateRow(nodeId: 100, sortOrder: 2, path: "-1,999"),
    ];

    // The user with the LOWER id has the LATER name, so ordering by user id and ordering by user name give
    // opposite results. Creator and writer are swapped between the two rows, so "owner" and "updater" also
    // disagree with each other, and neither ascending result matches the source sequence order.
    private static List<DocumentRepository.DocumentJoinRow> CreateRowsWithDistinctCreatorAndWriter() =>
    [
        CreateRow(nodeId: 200, sortOrder: 0, path: "-1,999", creatorUserId: 5, writerUserId: 1),
        CreateRow(nodeId: 100, sortOrder: 0, path: "-1,999", creatorUserId: 1, writerUserId: 5),
    ];

    private static DocumentRepository.DocumentJoinRow CreateRow(int nodeId, int sortOrder, string path, int creatorUserId = -1, int? writerUserId = null) =>
        new()
        {
            Node = new NodeDto
            {
                NodeId = nodeId,
                SortOrder = sortOrder,
                Text = "Same",
                Path = path,
                CreateDate = DateTime.MinValue,
                UserId = creatorUserId,
            },
            Document = new DocumentDto { NodeId = nodeId },
            Content = new ContentDto { NodeId = nodeId },
            ContentVersion = new ContentVersionDto { NodeId = nodeId, VersionDate = DateTime.MinValue, UserId = writerUserId },
            DocumentVersion = new DocumentVersionDto { Published = false },
            ContentType = new ContentTypeDto { Alias = "alias" },
        };

    private static readonly List<UserDto> _users =
    [
        new() { Id = 5, UserName = "aaa" },
        new() { Id = 1, UserName = "zzz" },
    ];

    private static List<int> ApplyOrderingAndGetNodeIds(Ordering? ordering, List<DocumentRepository.DocumentJoinRow>? rows = null)
    {
        IOrderedQueryable<DocumentRepository.DocumentJoinRow> ordered = DocumentRepository.ApplyDocumentOrdering(
            (rows ?? CreateTiedRows()).AsQueryable(),
            _users.AsQueryable(),
            ordering);

        return ordered.Select(row => row.Node.NodeId).ToList();
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

    [Test]
    public void ApplyDocumentOrdering_UnknownField_IsRejected()
    {
        Assert.Throws<NotSupportedException>(
            () => ApplyOrderingAndGetNodeIds(Ordering.By("somethingNobodySupports")),
            "ordering by an unsupported field must be reported rather than quietly served in sort order");
    }

    [TestCase("owner", Direction.Ascending, new[] { 200, 100 })]
    [TestCase("owner", Direction.Descending, new[] { 100, 200 })]
    [TestCase("updater", Direction.Ascending, new[] { 100, 200 })]
    [TestCase("updater", Direction.Descending, new[] { 200, 100 })]
    public void ApplyDocumentOrdering_ByUser_OrdersByUserNameNotUserId(string orderBy, Direction direction, int[] expectedNodeIds)
    {
        List<int> nodeIds = ApplyOrderingAndGetNodeIds(Ordering.By(orderBy, direction), CreateRowsWithDistinctCreatorAndWriter());

        Assert.That(nodeIds, Is.EqualTo(expectedNodeIds),
            $"{orderBy} must order by the user's name resolved from the users sequence - ordering by the user id the row carries gives the reverse");
    }

    [Test]
    public void ApplyVariantNameOrdering_WithTiedNames_BreaksTieByAscendingNodeId()
    {
        // Every node whose culture has no variant name falls back to the same node.Text, so ties here are
        // ordinary rather than exotic - and a database-backed test cannot reliably reproduce the source order
        // that would expose a missing tiebreak.
        var rows = new[]
        {
            new { Name = (string?)"Same", NodeId = 200 },
            new { Name = (string?)"Same", NodeId = 100 },
        }.AsQueryable();

        var ascending = DocumentRepository
            .ApplyVariantNameOrdering(rows, row => row.Name, row => row.NodeId, descending: false)
            .Select(row => row.NodeId)
            .ToList();

        var descending = DocumentRepository
            .ApplyVariantNameOrdering(rows, row => row.Name, row => row.NodeId, descending: true)
            .Select(row => row.NodeId)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(ascending, Is.EqualTo(new[] { 100, 200 }));
            Assert.That(
                descending,
                Is.EqualTo(new[] { 100, 200 }),
                "the tiebreak stays ascending on node id even when the name ordering is descending");
        });
    }
}
