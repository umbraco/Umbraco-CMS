using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

public partial class PublishedContentStatusFilteringServiceTests
{
    // NOTE: these tests are duplicates of FilterChildren, because the behavior is the same
    //       for children and siblings at the time of writing.

    [Test]
    public void FilterSiblings_Invariant_ForNonPreview_YieldsPublishedItems()
    {
        var (sut, items) = SetupForChildrenInvariant(false);

        var children = sut.FilterSiblings(items.Keys, null).ToArray();
        Assert.AreEqual(5, children.Length);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(2, children[1].Id);
            Assert.AreEqual(4, children[2].Id);
            Assert.AreEqual(6, children[3].Id);
            Assert.AreEqual(8, children[4].Id);
        });
    }

    [Test]
    public void FilterSiblings_Invariant_ForPreview_YieldsUnpublishedItems()
    {
        var (sut, items) = SetupForChildrenInvariant(true);

        var children = sut.FilterSiblings(items.Keys, null).ToArray();
        Assert.AreEqual(10, children.Length);
        for (var i = 0; i < 10; i++)
        {
            Assert.AreEqual(i, children[i].Id);
        }
    }

    [TestCase("da-DK", 3)]
    [TestCase("en-US", 4)]
    public void FilterSiblings_Variant_ForNonPreview_YieldsPublishedItemsInCulture(string culture, int expectedNumberOfChildren)
    {
        var (sut, items) = SetupForChildrenVariant(false, culture);

        var children = sut.FilterSiblings(items.Keys, culture).ToArray();
        Assert.AreEqual(expectedNumberOfChildren, children.Length);

        // IDs 0 through 3 exist in both en-US and da-DK - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(2, children[1].Id);
        });

        // IDs 4 through 6 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.AreEqual(4, children[2].Id);
            Assert.AreEqual(6, children[3].Id);
        }

        // IDs 7 through 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.AreEqual(8, children[2].Id);
        }
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void FilterSiblings_Variant_ForPreview_YieldsUnpublishedItemsInCulture(string culture)
    {
        var (sut, items) = SetupForChildrenVariant(true, culture);

        var children = sut.FilterSiblings(items.Keys, culture).ToArray();
        Assert.AreEqual(7, children.Length);

        // IDs 0 through 3 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(1, children[1].Id);
            Assert.AreEqual(2, children[2].Id);
            Assert.AreEqual(3, children[3].Id);
        });

        // IDs 4 through 6 exist only in en-US
        if (culture == "en-US")
        {
            Assert.AreEqual(4, children[4].Id);
            Assert.AreEqual(5, children[5].Id);
            Assert.AreEqual(6, children[6].Id);
        }

        // IDs 7 through 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.AreEqual(7, children[4].Id);
            Assert.AreEqual(8, children[5].Id);
            Assert.AreEqual(9, children[6].Id);
        }
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void FilterSiblings_MixedVariance_ForNonPreview_YieldsPublishedItemsInCultureOrInvariant(string culture)
    {
        var (sut, items) = SetupForChildrenMixedVariance(false, culture);

        var children = sut.FilterSiblings(items.Keys, culture).ToArray();
        Assert.AreEqual(4, children.Length);

        // IDs 0 through 2 are invariant - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(2, children[1].Id);
        });

        // IDs 3 through 5 exist in both en-US and da-DK - only even IDs are published
        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, children[2].Id);
        });

        // IDs 6 and 7 exist only in en-US - only even IDs are published
        if (culture == "en-US")
        {
            Assert.AreEqual(6, children[3].Id);
        }

        // IDs 8 and 9 exist only in da-DK - only even IDs are published
        if (culture == "da-DK")
        {
            Assert.AreEqual(8, children[3].Id);
        }
    }

    [TestCase("da-DK")]
    [TestCase("en-US")]
    public void FilterSiblings_MixedVariance_FoPreview_YieldsPublishedItemsInCultureOrInvariant(string culture)
    {
        var (sut, items) = SetupForChildrenMixedVariance(true, culture);

        var children = sut.FilterSiblings(items.Keys, culture).ToArray();
        Assert.AreEqual(8, children.Length);

        // IDs 0 through 2 are invariant
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, children[0].Id);
            Assert.AreEqual(1, children[1].Id);
            Assert.AreEqual(2, children[2].Id);
        });

        // IDs 3 through 5 exist in both en-US and da-DK
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, children[3].Id);
            Assert.AreEqual(4, children[4].Id);
            Assert.AreEqual(5, children[5].Id);
        });

        // IDs 6 and 7 exist only in en-US
        if (culture == "en-US")
        {
            Assert.AreEqual(6, children[6].Id);
            Assert.AreEqual(7, children[7].Id);
        }

        // IDs 8 and 9 exist only in da-DK
        if (culture == "da-DK")
        {
            Assert.AreEqual(8, children[6].Id);
            Assert.AreEqual(9, children[7].Id);
        }
    }
}
