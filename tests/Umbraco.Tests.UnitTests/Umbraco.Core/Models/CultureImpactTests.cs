// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class CultureImpactTests
{
    private CultureImpactFactory BasicImpactFactory => CreateCultureImpactService();

    [Test]
    public void Get_Culture_For_Invariant_Errors()
    {
        var result = BasicImpactFactory.GetCultureForInvariantErrors(
            Mock.Of<IContent>(x => x.Published == true),
            new[] { "en-US", "fr-FR" },
            "en-US");
        Assert.AreEqual("en-US", result); // default culture is being saved so use it

        result = BasicImpactFactory.GetCultureForInvariantErrors(
            Mock.Of<IContent>(x => x.Published == false),
            new[] { "fr-FR" },
            "en-US");
        Assert.AreEqual("fr-FR", result); // default culture not being saved with not published version,
                                          // use the first culture being saved

        result = BasicImpactFactory.GetCultureForInvariantErrors(
            Mock.Of<IContent>(x => x.Published == true),
            new[] { "fr-FR" },
            "en-US");
        Assert.AreEqual(null, result); // default culture not being saved with published version, use null
    }

    [Test]
    public void All_Cultures()
    {
        var impact = BasicImpactFactory.ImpactAll();

        Assert.AreEqual(impact.Culture, "*");

        Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsFalse(impact.ImpactsExplicitCulture);
        Assert.IsTrue(impact.ImpactsAllCultures);
    }

    [Test]
    public void Invariant_Culture()
    {
        var impact = BasicImpactFactory.ImpactInvariant();

        Assert.AreEqual(impact.Culture, null);

        Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
        Assert.IsTrue(impact.ImpactsOnlyInvariantCulture);
        Assert.IsFalse(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Explicit_Culture(bool allowEditInvariantForVariant)
    {
        var impact = BasicImpactFactory.ImpactExplicit("en-US", allowEditInvariantForVariant);

        Assert.AreEqual(impact.Culture, "en-US");

        Assert.AreEqual(allowEditInvariantForVariant, impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsTrue(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TryCreate_Explicit_Culture(bool allowEditInvariantForVariant)
    {
        var success =
            BasicImpactFactory.TryCreate("en-US", ContentVariation.Culture, false, allowEditInvariantForVariant, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, "en-US");

        Assert.AreEqual(allowEditInvariantForVariant, impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsTrue(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
    }

    [Test]
    public void TryCreate_AllCultures_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate("*", ContentVariation.Nothing, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, null);

        Assert.AreSame(BasicImpactFactory.ImpactInvariant(), impact);
    }

    [Test]
    public void TryCreate_AllCultures_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate("*", ContentVariation.Culture, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, "*");

        Assert.AreSame(BasicImpactFactory.ImpactAll(), impact);
    }

    [Test]
    public void TryCreate_Invariant_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate(null, ContentVariation.Culture, false, false, out var impact);
        Assert.IsFalse(success);
    }

    [Test]
    public void TryCreate_Invariant_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate(null,  ContentVariation.Nothing, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.AreSame(BasicImpactFactory.ImpactInvariant(), impact);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Edit_Invariant_From_Variant_Impacts_Invariant_Properties(bool allowEditInvariantFromVariant)
    {
        var sut = CreateCultureImpactService();
        var impact = sut.ImpactExplicit("da", allowEditInvariantFromVariant);

        Assert.AreEqual(allowEditInvariantFromVariant, impact.ImpactsAlsoInvariantProperties);
    }

    private CultureImpactFactory CreateCultureImpactService() => new CultureImpactFactory();
}
