using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public class DocumentPublishStatusServiceAncestorPathTests
{
    private static readonly string?[] Cultures =
    {
        null, string.Empty, Constants.System.InvariantCulture, "en-US", "da-DK", "fr-FR",
    };

    [Test]
    public void Batch_Matches_Per_Key_For_Unpublished_Intermediate_Ancestor()
    {
        DocumentNavigationService nav = NewNavigationService();
        var contentType = Guid.NewGuid();
        Guid root = Add(nav, contentType, null);
        Guid a = Add(nav, contentType, root);
        Guid b = Add(nav, contentType, a);
        Guid c = Add(nav, contentType, b);

        // root, a and c are published in en-US; b is not - which breaks the path for its descendant c.
        var svc = SeededService(
            nav,
            new Dictionary<Guid, ISet<string>>
            {
                [root] = Set("en-US"),
                [a] = Set("en-US"),
                [c] = Set("en-US"),
            });

        // Documented per-key expectations (the ancestor-path check looks at ancestors only, not self).
        Assert.Multiple(() =>
        {
            Assert.IsTrue(svc.HasPublishedAncestorPath(root, "en-US"), "root has no ancestors");
            Assert.IsTrue(svc.HasPublishedAncestorPath(a, "en-US"), "ancestor root is published");
            Assert.IsTrue(svc.HasPublishedAncestorPath(b, "en-US"), "ancestors root and a are published (b's own status is irrelevant)");
            Assert.IsFalse(svc.HasPublishedAncestorPath(c, "en-US"), "ancestor b is unpublished");
        });

        AssertBatchMatchesPerKey(svc, new[] { root, a, b, c }, "unpublished-intermediate");
    }

    [Test]
    public void Batch_Matches_Per_Key_For_Variant_Cultures()
    {
        DocumentNavigationService nav = NewNavigationService();
        var contentType = Guid.NewGuid();
        Guid root = Add(nav, contentType, null);
        Guid child = Add(nav, contentType, root);
        Guid grandchild = Add(nav, contentType, child);

        // root published in both cultures, child only in da-DK, grandchild in both.
        var svc = SeededService(
            nav,
            new Dictionary<Guid, ISet<string>>
            {
                [root] = Set("en-US", "da-DK"),
                [child] = Set("da-DK"),
                [grandchild] = Set("en-US", "da-DK"),
            });

        AssertBatchMatchesPerKey(svc, new[] { root, child, grandchild }, "variant");
    }

    [Test]
    public void Batch_Matches_Per_Key_Across_Random_Connected_Trees()
    {
        var rng = new Random(20250811);
        var allCultures = new[] { "en-US", "da-DK", "fr-FR" };

        for (var iteration = 0; iteration < 300; iteration++)
        {
            DocumentNavigationService nav = NewNavigationService();
            var contentType = Guid.NewGuid();
            var publish = new Dictionary<Guid, ISet<string>>();
            var nodes = new List<Guid>();

            var nodeCount = rng.Next(1, 40);
            for (var i = 0; i < nodeCount; i++)
            {
                // First node is always a root; afterwards ~1 in 5 is a new root, the rest hang off an
                // existing node so the tree stays connected (Add rejects a missing parent anyway).
                Guid? parent = nodes.Count == 0 || rng.Next(5) == 0
                    ? null
                    : nodes[rng.Next(nodes.Count)];
                Guid key = Add(nav, contentType, parent);
                nodes.Add(key);

                // Random published cultures, sometimes none (fully unpublished -> absent from the map).
                var published = allCultures.Where(_ => rng.Next(2) == 0).ToArray();
                if (published.Length > 0)
                {
                    publish[key] = Set(published);
                }
            }

            DocumentPublishStatusService svc = SeededService(nav, publish);

            foreach (var culture in Cultures)
            {
                AssertBatchMatchesPerKey(svc, nodes, $"iteration {iteration}");
            }
        }
    }

    [Test]
    public void Dangling_Ancestor_Excludes_The_Node()
    {
        var child = Guid.NewGuid();
        var missingParent = Guid.NewGuid();

        var nav = new Mock<IDocumentNavigationQueryService>();
        nav.Setup(x => x.TryGetParentKey(child, out It.Ref<Guid?>.IsAny))
            .Returns(new TryGetParentKeyCallback((Guid _, out Guid? parentKey) =>
            {
                parentKey = missingParent;
                return true;
            }));
        nav.Setup(x => x.TryGetParentKey(missingParent, out It.Ref<Guid?>.IsAny))
            .Returns(new TryGetParentKeyCallback((Guid _, out Guid? parentKey) =>
            {
                parentKey = null;
                return false; // parent not in navigation
            }));

        DocumentPublishStatusService svc = SeededService(
            nav.Object,
            new Dictionary<Guid, ISet<string>> { [child] = Set("en-US") });

        Assert.Multiple(() =>
        {
            Assert.IsFalse(
                svc.HasPublishedAncestorPath(child, "en-US"),
                "single-key: an unconfirmable missing ancestor excludes the node");
            Assert.IsFalse(
                svc.WhereAncestorPathPublished(new[] { child }, "en-US").Any(),
                "batch: an unconfirmable missing ancestor excludes the node");
        });
    }

    private static void AssertBatchMatchesPerKey(DocumentPublishStatusService svc, IReadOnlyList<Guid> nodes, string context)
    {
        foreach (var culture in Cultures)
        {
            var batch = svc.WhereAncestorPathPublished(nodes, culture).ToHashSet();
            foreach (Guid node in nodes)
            {
                bool expected = culture is null
                    ? svc.HasPublishedAncestorPath(node)
                    : svc.HasPublishedAncestorPath(node, culture);

                Assert.AreEqual(
                    expected,
                    batch.Contains(node),
                    $"{context}: culture='{culture ?? "<null>"}', node={node}");
            }
        }
    }

    private static DocumentNavigationService NewNavigationService()
        => new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

    private static Guid Add(DocumentNavigationService nav, Guid contentType, Guid? parent)
    {
        var key = Guid.NewGuid();
        Assert.IsTrue(nav.Add(key, contentType, parent), "navigation Add should succeed for a valid parent");
        return key;
    }

    private static DocumentPublishStatusService SeededService(
        IDocumentNavigationQueryService nav,
        IDictionary<Guid, ISet<string>> publishStatus,
        string defaultCulture = "en-US")
    {
        var svc = new TestableDocumentPublishStatusService(nav);
        svc.Seed(publishStatus, defaultCulture);
        return svc;
    }

    private static ISet<string> Set(params string[] cultures)
        => new HashSet<string>(cultures, StringComparer.InvariantCultureIgnoreCase);

    private delegate bool TryGetParentKeyCallback(Guid childKey, out Guid? parentKey);

    // Exposes the protected cache-seeding hooks so tests can set publish status without booting a
    // scope/repository around InitializeAsync.
    private sealed class TestableDocumentPublishStatusService : DocumentPublishStatusService
    {
        public TestableDocumentPublishStatusService(IDocumentNavigationQueryService navigation)
            : base(
                NullLogger<DocumentPublishStatusService>.Instance,
                Mock.Of<IPublishStatusRepository>(),
                Mock.Of<ICoreScopeProvider>(),
                Mock.Of<ILanguageService>(),
                navigation)
        {
        }

        public void Seed(IDictionary<Guid, ISet<string>> publishStatus, string defaultCulture)
        {
            PopulateCache(publishStatus);
            DefaultCulture = defaultCulture;
        }
    }
}
