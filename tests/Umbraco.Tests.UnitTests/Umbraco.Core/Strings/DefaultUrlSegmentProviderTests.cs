using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

[TestFixture]
public class DefaultUrlSegmentProviderTests
{
    private DefaultUrlSegmentProvider _provider = null!;

    [SetUp]
    public void SetUp() =>
        _provider = new DefaultUrlSegmentProvider(new MockShortStringHelper());

    /// <summary>
    /// Verifies that requesting the draft segment (<c>published: false</c>) for invariant content
    /// that has unpublished edits returns a segment based on the current (draft) name, not the
    /// published name. This is critical for redirect tracking: when a node is renamed and published,
    /// the draft name represents what the segment <em>will</em> be after publishing.
    /// </summary>
    [Test]
    public void GetUrlSegment_Draft_InvariantContent_WithEdits_Returns_Draft_Name()
    {
        var content = CreateInvariantContent(
            currentName: "New Name",
            publishedName: "Old Name",
            edited: true);

        var result = _provider.GetUrlSegment(content, published: false, culture: null);

        Assert.AreEqual("URL-SEGMENT-CULTURE::New Name", result);
    }

    /// <summary>
    /// Verifies that requesting the published segment (<c>published: true</c>) for invariant content
    /// that has unpublished edits returns a segment based on the published name, preserving the
    /// existing behavior for displaying the current live URL.
    /// </summary>
    [Test]
    public void GetUrlSegment_Published_InvariantContent_WithEdits_Returns_Published_Name()
    {
        var content = CreateInvariantContent(
            currentName: "New Name",
            publishedName: "Old Name",
            edited: true);

        var result = _provider.GetUrlSegment(content, published: true, culture: null);

        Assert.AreEqual("URL-SEGMENT-CULTURE::Old Name", result);
    }

    /// <summary>
    /// Verifies that when content has never been published (<c>GetPublishName</c> returns null),
    /// both draft and published segments use the current name regardless of the <c>published</c>
    /// parameter.
    /// </summary>
    [TestCase(true)]
    [TestCase(false)]
    public void GetUrlSegment_NeverPublishedContent_Returns_Current_Name(bool published)
    {
        var content = CreateInvariantContent(
            currentName: "Draft Only Page",
            publishedName: null,
            edited: true);

        var result = _provider.GetUrlSegment(content, published, culture: null);

        Assert.AreEqual("URL-SEGMENT-CULTURE::Draft Only Page", result);
    }

    /// <summary>
    /// Verifies that when content is not edited (published values match draft values),
    /// both draft and published segments return the same value based on the current name.
    /// </summary>
    [TestCase(true)]
    [TestCase(false)]
    public void GetUrlSegment_ContentNotEdited_Returns_Current_Name(bool published)
    {
        var content = CreateInvariantContent(
            currentName: "Same Name",
            publishedName: "Same Name",
            edited: false);

        var result = _provider.GetUrlSegment(content, published, culture: null);

        Assert.AreEqual("URL-SEGMENT-CULTURE::Same Name", result);
    }

    /// <summary>
    /// Verifies that when the <c>umbracoUrlName</c> property is set, the draft segment uses
    /// the draft property value, taking precedence over the content name.
    /// </summary>
    [Test]
    public void GetUrlSegment_WithUrlNameProperty_Draft_Returns_Draft_PropertyValue()
    {
        var content = CreateInvariantContent(
            currentName: "Page Name",
            publishedName: "Page Name",
            edited: true,
            draftUrlName: "custom-draft-slug",
            publishedUrlName: "custom-published-slug");

        var result = _provider.GetUrlSegment(content, published: false, culture: null);

        Assert.AreEqual("URL-SEGMENT-CULTURE::custom-draft-slug", result);
    }

    /// <summary>
    /// Verifies that when the <c>umbracoUrlName</c> property is set, the published segment uses
    /// the published property value.
    /// </summary>
    [Test]
    public void GetUrlSegment_WithUrlNameProperty_Published_Returns_Published_PropertyValue()
    {
        var content = CreateInvariantContent(
            currentName: "Page Name",
            publishedName: "Page Name",
            edited: true,
            draftUrlName: "custom-draft-slug",
            publishedUrlName: "custom-published-slug");

        var result = _provider.GetUrlSegment(content, published: true, culture: null);

        Assert.AreEqual("URL-SEGMENT-CULTURE::custom-published-slug", result);
    }

    private static IContent CreateInvariantContent(
        string currentName,
        string? publishedName,
        bool edited,
        string? draftUrlName = null,
        string? publishedUrlName = null)
    {
        var hasUrlNameProperty = draftUrlName is not null || publishedUrlName is not null;

        var content = new Mock<IContent>();
        content.Setup(c => c.GetCultureName(null)).Returns(currentName);
        content.Setup(c => c.GetCultureName(string.Empty)).Returns(currentName);
        content.SetupGet(c => c.Edited).Returns(edited);
        content.Setup(c => c.GetPublishName(null)).Returns(publishedName);
        content.Setup(c => c.GetPublishName(string.Empty)).Returns(publishedName);
        content.Setup(c => c.HasProperty(Constants.Conventions.Content.UrlName)).Returns(hasUrlNameProperty);

        if (hasUrlNameProperty)
        {
            content.Setup(c => c.GetValue<string>(Constants.Conventions.Content.UrlName, null, null, false))
                .Returns(draftUrlName);
            content.Setup(c => c.GetValue<string>(Constants.Conventions.Content.UrlName, null, null, true))
                .Returns(publishedUrlName);
        }

        return content.Object;
    }
}
