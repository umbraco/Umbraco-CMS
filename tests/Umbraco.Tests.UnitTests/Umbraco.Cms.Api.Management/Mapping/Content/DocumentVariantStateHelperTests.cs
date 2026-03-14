using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Mapping.Content;

/// <summary>
/// Contains unit tests for the <see cref="DocumentVariantStateHelper"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class DocumentVariantStateHelperTests
{
    /// <summary>
    /// Tests the state of culture-invariant content based on edited, published, and trashed flags.
    /// </summary>
    /// <param name="edited">Indicates whether the content has been edited.</param>
    /// <param name="published">Indicates whether the content is published.</param>
    /// <param name="trashed">Indicates whether the content is trashed.</param>
    /// <param name="expectedResult">The expected DocumentVariantState result.</param>
    [TestCase(false, false, false, DocumentVariantState.Draft)]
    [TestCase(false, true, false, DocumentVariantState.Published)]
    [TestCase(true, false, false, DocumentVariantState.Draft)]
    [TestCase(true, true, false, DocumentVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, DocumentVariantState.Trashed)]
    public void Culture_Invariant_Content_State(bool edited, bool published, bool trashed, DocumentVariantState expectedResult)
    {
        var content = Mock.Of<IContent>(c => c.Id == 1 && c.Published == published && c.Edited == edited && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, DocumentVariantStateHelper.GetState(content, culture: null));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Culture_Invariant_Content_Not_Created_State(bool edited, bool published)
    {
        var content = Mock.Of<IContent>(c => c.Id == 0 && c.Published == published && c.Edited == edited);
        Assert.AreEqual(DocumentVariantState.NotCreated, DocumentVariantStateHelper.GetState(content, culture: null));
    }

    /// <summary>
    /// Tests the state of a culture variant content based on its edited, published, and trashed flags.
    /// </summary>
    /// <param name="edited">Indicates whether the content is edited.</param>
    /// <param name="published">Indicates whether the content is published.</param>
    /// <param name="trashed">Indicates whether the content is trashed.</param>
    /// <param name="expectedResult">The expected <see cref="DocumentVariantState"/> result.</param>
    [TestCase(false, false, false, DocumentVariantState.Draft)]
    [TestCase(false, true, false, DocumentVariantState.Published)]
    [TestCase(true, false, false, DocumentVariantState.Draft)]
    [TestCase(true, true, false, DocumentVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, DocumentVariantState.Trashed)]
    public void Culture_Variant_Content_Existing_Culture_State(bool edited, bool published, bool trashed, DocumentVariantState expectedResult)
    {
        const string culture = "en";
        var content = Mock.Of<IContent>(c =>
            c.Id == 1
            && c.AvailableCultures == new[] { culture }
            && c.EditedCultures == (edited ? new[] { culture } : Enumerable.Empty<string>())
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>())
            && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, DocumentVariantStateHelper.GetState(content, culture));
    }

    /// <summary>
    /// Verifies that the <see cref="DocumentVariantStateHelper.GetState"/> method returns <see cref="DocumentVariantState.NotCreated"/> when requesting the state for a culture that is not present on a culture-variant content item.
    /// </summary>
    /// <param name="edited">If set to <c>true</c>, the culture is marked as edited; otherwise, it is not.</param>
    /// <param name="published">If set to <c>true</c>, the culture is marked as published; otherwise, it is not.</param>
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Culture_Variant_Content_Missing_Culture_State(bool edited, bool published)
    {
        const string culture = "en";
        var content = Mock.Of<IContent>(c =>
            c.Id == 1
            && c.AvailableCultures == new[] { culture }
            && c.EditedCultures == (edited ? new[] { culture } : Enumerable.Empty<string>())
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>()));
        Assert.AreEqual(DocumentVariantState.NotCreated, DocumentVariantStateHelper.GetState(content, "dk"));
    }

    /// <summary>
    /// Verifies that the <see cref="DocumentVariantStateHelper.GetState"/> method returns the correct <see cref="DocumentVariantState"/> for a culture-invariant <see cref="IDocumentEntitySlim"/>
    /// based on the edited, published, and trashed flags.
    /// </summary>
    /// <param name="edited">True if the document has been edited; otherwise, false.</param>
    /// <param name="published">True if the document is published; otherwise, false.</param>
    /// <param name="trashed">True if the document is trashed; otherwise, false.</param>
    /// <param name="expectedResult">The expected <see cref="DocumentVariantState"/> result for the given flags.</param>
    [TestCase(false, false, false, DocumentVariantState.Draft)]
    [TestCase(false, true, false, DocumentVariantState.Published)]
    [TestCase(true, false, false, DocumentVariantState.Draft)]
    [TestCase(true, true, false, DocumentVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, DocumentVariantState.Trashed)]
    public void Culture_Invariant_DocumentEntitySlim_State(bool edited, bool published, bool trashed, DocumentVariantState expectedResult)
    {
        var entity = Mock.Of<IDocumentEntitySlim>(c => c.Id == 1 && c.Published == published && c.Edited == edited && c.CultureNames == new Dictionary<string, string>() && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, DocumentVariantStateHelper.GetState(entity, culture: null));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Culture_Invariant_DocumentEntitySlim_Not_Created_State(bool edited, bool published)
    {
        var entity = Mock.Of<IDocumentEntitySlim>(c => c.Id == 0 && c.Published == published && c.Edited == edited && c.CultureNames == new Dictionary<string, string>());
        Assert.AreEqual(DocumentVariantState.NotCreated, DocumentVariantStateHelper.GetState(entity, culture: null));
    }

    /// <summary>
    /// Verifies that the <see cref="DocumentVariantStateHelper.GetState"/> method returns the correct <see cref="DocumentVariantState"/>
    /// for a culture variant document entity slim, given specific edited, published, and trashed states for a culture.
    /// </summary>
    /// <param name="edited">True if the culture is marked as edited; otherwise, false.</param>
    /// <param name="published">True if the culture is published; otherwise, false.</param>
    /// <param name="trashed">True if the document is trashed; otherwise, false.</param>
    /// <param name="expectedResult">The expected <see cref="DocumentVariantState"/> result for the given input.</param>
    [TestCase(false, false, false, DocumentVariantState.Draft)]
    [TestCase(false, true, false, DocumentVariantState.Published)]
    [TestCase(true, false, false, DocumentVariantState.Draft)]
    [TestCase(true, true, false, DocumentVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, DocumentVariantState.Trashed)]
    public void Culture_Variant_DocumentEntitySlim_Existing_Culture_State(bool edited, bool published, bool trashed, DocumentVariantState expectedResult)
    {
        const string culture = "en";
        var entity = Mock.Of<IDocumentEntitySlim>(c =>
            c.Id == 1
            && c.CultureNames == new Dictionary<string, string> { { culture, "value does not matter" } }
            && c.EditedCultures == (edited ? new[] { culture } : Enumerable.Empty<string>())
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>())
            && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, DocumentVariantStateHelper.GetState(entity, culture));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Culture_Variant_DocumentEntitySlim_Missing_Culture_State(bool edited, bool published)
    {
        const string culture = "en";
        var entity = Mock.Of<IDocumentEntitySlim>(c =>
            c.Id == 1
            && c.CultureNames == new Dictionary<string, string> { { culture, "value does not matter" } }
            && c.EditedCultures == (edited ? new[] { culture } : Enumerable.Empty<string>())
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>()));
        Assert.AreEqual(DocumentVariantState.NotCreated, DocumentVariantStateHelper.GetState(entity, "dk"));
    }
}
