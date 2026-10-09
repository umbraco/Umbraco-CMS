using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Mapping.Content;

[TestFixture]
public class PublishableVariantStateHelperTests
{
    [TestCase(false, false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, false, PublishableVariantState.Published)]
    [TestCase(true, false, false, PublishableVariantState.Draft)]
    [TestCase(true, true, false, PublishableVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, PublishableVariantState.Trashed)]
    public void Culture_Invariant_Content_State(bool edited, bool published, bool trashed, PublishableVariantState expectedResult)
    {
        var content = Mock.Of<IContent>(c => c.Id == 1 && c.Published == published && c.Edited == edited && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(content, culture: null));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Culture_Invariant_Content_Not_Created_State(bool edited, bool published)
    {
        var content = Mock.Of<IContent>(c => c.Id == 0 && c.Published == published && c.Edited == edited);
        Assert.AreEqual(PublishableVariantState.NotCreated, PublishableVariantStateHelper.GetState(content, culture: null));
    }

    [TestCase(false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, PublishableVariantState.Published)]
    [TestCase(true, false, PublishableVariantState.Draft)]
    [TestCase(true, true, PublishableVariantState.PublishedPendingChanges)]
    public void Culture_Invariant_Content_Uses_Edited_Not_InvariantEdited(bool edited, bool published, PublishableVariantState expectedResult)
    {
        // AvailableCultures is empty (invariant content), so the invariant slot's state must come from
        // Edited - InvariantEdited is set to the opposite value to prove it is not consulted here.
        var content = Mock.Of<IContent>(c => c.Id == 1 && c.Published == published && c.Edited == edited && c.InvariantEdited == !edited);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(content, culture: null));
    }

    [TestCase(false, false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, false, PublishableVariantState.Published)]
    [TestCase(true, false, false, PublishableVariantState.Draft)]
    [TestCase(true, true, false, PublishableVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, PublishableVariantState.Trashed)]
    public void Culture_Variant_Content_Invariant_State(bool invariantEdited, bool published, bool trashed, PublishableVariantState expectedResult)
    {
        // querying the invariant slot (culture: null) of a culture-varying document must be driven by
        // InvariantEdited, not Edited - Edited is set to the opposite value to prove the distinction.
        const string culture = "en";
        var content = Mock.Of<IContent>(c =>
            c.Id == 1
            && c.AvailableCultures == new[] { culture }
            && c.InvariantEdited == invariantEdited
            && c.Edited == !invariantEdited
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>())
            && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(content, culture: null));
    }

    [TestCase(false, false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, false, PublishableVariantState.Published)]
    [TestCase(true, false, false, PublishableVariantState.Draft)]
    [TestCase(true, true, false, PublishableVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, PublishableVariantState.Trashed)]
    public void Culture_Variant_Content_Existing_Culture_State(bool edited, bool published, bool trashed, PublishableVariantState expectedResult)
    {
        const string culture = "en";
        var content = Mock.Of<IContent>(c =>
            c.Id == 1
            && c.AvailableCultures == new[] { culture }
            && c.EditedCultures == (edited ? new[] { culture } : Enumerable.Empty<string>())
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>())
            && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(content, culture));
    }

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
        Assert.AreEqual(PublishableVariantState.NotCreated, PublishableVariantStateHelper.GetState(content, "dk"));
    }

    [TestCase(false, false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, false, PublishableVariantState.Published)]
    [TestCase(true, false, false, PublishableVariantState.Draft)]
    [TestCase(true, true, false, PublishableVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, PublishableVariantState.Trashed)]
    public void Culture_Invariant_DocumentEntitySlim_State(bool edited, bool published, bool trashed, PublishableVariantState expectedResult)
    {
        var entity = Mock.Of<IDocumentEntitySlim>(c => c.Id == 1 && c.Published == published && c.Edited == edited && c.CultureNames == new Dictionary<string, string>() && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(entity, culture: null));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Culture_Invariant_DocumentEntitySlim_Not_Created_State(bool edited, bool published)
    {
        var entity = Mock.Of<IDocumentEntitySlim>(c => c.Id == 0 && c.Published == published && c.Edited == edited && c.CultureNames == new Dictionary<string, string>());
        Assert.AreEqual(PublishableVariantState.NotCreated, PublishableVariantStateHelper.GetState(entity, culture: null));
    }

    [TestCase(false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, PublishableVariantState.Published)]
    [TestCase(true, false, PublishableVariantState.Draft)]
    [TestCase(true, true, PublishableVariantState.PublishedPendingChanges)]
    public void Culture_Invariant_DocumentEntitySlim_Uses_Edited_Not_InvariantEdited(bool edited, bool published, PublishableVariantState expectedResult)
    {
        // Variations does not vary by culture, so the invariant slot's state must come from Edited -
        // InvariantEdited is set to the opposite value to prove it is not consulted here.
        var entity = Mock.Of<IDocumentEntitySlim>(c => c.Id == 1 && c.Published == published && c.Edited == edited && c.InvariantEdited == !edited && c.CultureNames == new Dictionary<string, string>());
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(entity, culture: null));
    }

    [TestCase(false, false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, false, PublishableVariantState.Published)]
    [TestCase(true, false, false, PublishableVariantState.Draft)]
    [TestCase(true, true, false, PublishableVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, PublishableVariantState.Trashed)]
    public void Culture_Variant_DocumentEntitySlim_Invariant_State(bool invariantEdited, bool published, bool trashed, PublishableVariantState expectedResult)
    {
        // querying the invariant slot (culture: null) of a culture-varying entity must be driven by
        // InvariantEdited, not Edited - Edited is set to the opposite value to prove the distinction.
        const string culture = "en";
        var entity = Mock.Of<IDocumentEntitySlim>(c =>
            c.Id == 1
            && c.Variations == ContentVariation.Culture
            && c.CultureNames == new Dictionary<string, string> { { culture, "value does not matter" } }
            && c.InvariantEdited == invariantEdited
            && c.Edited == !invariantEdited
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>())
            && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(entity, culture: null));
    }

    [TestCase(false, false, false, PublishableVariantState.Draft)]
    [TestCase(false, true, false, PublishableVariantState.Published)]
    [TestCase(true, false, false, PublishableVariantState.Draft)]
    [TestCase(true, true, false, PublishableVariantState.PublishedPendingChanges)]
    [TestCase(true, false, true, PublishableVariantState.Trashed)]
    public void Culture_Variant_DocumentEntitySlim_Existing_Culture_State(bool edited, bool published, bool trashed, PublishableVariantState expectedResult)
    {
        const string culture = "en";
        var entity = Mock.Of<IDocumentEntitySlim>(c =>
            c.Id == 1
            && c.CultureNames == new Dictionary<string, string> { { culture, "value does not matter" } }
            && c.EditedCultures == (edited ? new[] { culture } : Enumerable.Empty<string>())
            && c.Published == published
            && c.PublishedCultures == (published ? new[] { culture } : Enumerable.Empty<string>())
            && c.Trashed == trashed);
        Assert.AreEqual(expectedResult, PublishableVariantStateHelper.GetState(entity, culture));
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
        Assert.AreEqual(PublishableVariantState.NotCreated, PublishableVariantStateHelper.GetState(entity, "dk"));
    }
}
