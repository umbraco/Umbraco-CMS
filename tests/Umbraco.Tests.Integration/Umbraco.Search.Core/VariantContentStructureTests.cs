using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class VariantContentStructureTests : VariantContentTestBase
{
    [Test]
    public void PublishedStructureInAllCultures_YieldsAllPublishedDocumentsInAllCultures()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));

            Assert.That(documents.All(d => d.ObjectType is UmbracoObjectTypes.Document), Is.True);
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentVariance(documents[0], "en-US", "da-DK");
            VerifyDocumentVariance(documents[1], "en-US", "da-DK");
            VerifyDocumentVariance(documents[2], "en-US", "da-DK");
            VerifyDocumentVariance(documents[3], "en-US", "da-DK");
        });
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public void PublishedStructureSingleCulture_YieldsAllPublishedDocumentsInOneCultures(string culture)
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, [culture]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentVariance(documents[0], culture);
            VerifyDocumentVariance(documents[1], culture);
            VerifyDocumentVariance(documents[2], culture);
            VerifyDocumentVariance(documents[3], culture);
        });
    }

    [Test]
    public void PublishedRootInAllCultures_YieldsOnlyRootDocumentInAllCultures()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.Default, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
        VerifyDocumentVariance(documents[0], "en-US", "da-DK");
    }

    [Test]
    public void PublishedStructureInAllCultures_WithUnpublishedRoot_YieldsNoDocuments()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Root());
        Assert.That(result.Success, Is.True);
        Assert.That(Child().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Is.Empty);
    }

    [TestCase("en-US", "da-DK")]
    [TestCase("da-DK", "en-US")]
    public void PublishedStructureInAllCultures_WithUnpublishedRootInSingleCulture_YieldsAllDocumentInPublishedRootCulture(string cultureToUnpublish, string expectedCulture)
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Root(), cultureToUnpublish);
        Assert.That(result.Success, Is.True);

        IContent root = Root();
        Assert.Multiple(() =>
        {
            Assert.That(root.Published, Is.True);
            Assert.That(
                root.PublishedCultures.Select(c => c.ToLowerInvariant()),
                Is.EquivalentTo(new[] { expectedCulture.ToLowerInvariant() }));
        });

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentVariance(documents[0], expectedCulture);
            VerifyDocumentVariance(documents[1], expectedCulture);
            VerifyDocumentVariance(documents[2], expectedCulture);
            VerifyDocumentVariance(documents[3], expectedCulture);
        });
    }

    [Test]
    public void PublishedStructureInAllCultures_WithUnpublishedGrandchildInAllCultures_YieldsNothingBelowChild()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Grandchild());
        Assert.That(result.Success, Is.True);
        Assert.That(GreatGrandchild().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentVariance(documents[0], "en-US", "da-DK");
            VerifyDocumentVariance(documents[1], "en-US", "da-DK");
        });
    }

    [Test]
    public void PublishedStructureInAllCultures_UnpublishAllCulturesForGrandchildOneAtATime_YieldsNothingBelowChild()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Grandchild(), "en-US");
        Assert.That(result.Success, Is.True);
        Assert.That(Grandchild().Published, Is.True);
        result = ContentService.Unpublish(Grandchild(), "da-DK");
        Assert.That(result.Success, Is.True);
        Assert.That(Grandchild().Published, Is.False);
        Assert.That(GreatGrandchild().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentVariance(documents[0], "en-US", "da-DK");
            VerifyDocumentVariance(documents[1], "en-US", "da-DK");
        });
    }

    [TestCase("en-US", "da-DK")]
    [TestCase("da-DK", "en-US")]
    public void PublishedStructureInAllCultures_WithUnpublishedGrandchildInSingleCulture_YieldsSingleCultureBelowChild(string cultureToUnpublish, string expectedCulture)
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Grandchild(), cultureToUnpublish);
        Assert.That(result.Success, Is.True);

        // grandchild should now be unpublished in a single culture
        IContent grandchild = Grandchild();
        Assert.Multiple(() =>
        {
            Assert.That(grandchild.Published, Is.True);
            Assert.That(
                grandchild.PublishedCultures.Select(c => c.ToLowerInvariant()),
                Is.EquivalentTo(new[] { expectedCulture.ToLowerInvariant() }));
        });

        Assert.That(GreatGrandchild().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentVariance(documents[0], "en-US", "da-DK");
            VerifyDocumentVariance(documents[1], "en-US", "da-DK");
            VerifyDocumentVariance(documents[2], expectedCulture);
            VerifyDocumentVariance(documents[3], expectedCulture);
        });
    }

    private void VerifyDocumentVariance(TestIndexDocument document, params string[] cultures)
    {
        Variation[] variations = document.Variations.ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(cultures, Is.Not.Empty);
            Assert.That(variations, Has.Length.EqualTo(cultures.Length * 3));
            foreach (var culture in cultures)
            {
                Assert.That(variations.Any(v => v.Culture.InvariantEquals(culture) && v.Segment is null), Is.True);
                Assert.That(variations.Any(v => v.Culture.InvariantEquals(culture) && v.Segment == "segment-1"), Is.True);
                Assert.That(variations.Any(v => v.Culture.InvariantEquals(culture) && v.Segment == "segment-2"), Is.True);
            }
        });
    }
}
