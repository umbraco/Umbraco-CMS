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
