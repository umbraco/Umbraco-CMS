// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class CultureImpactTests
{
    private CultureImpactFactory BasicImpactFactory => createCultureImpactService();

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
        Assert.AreEqual("fr-FR",
            result); // default culture not being saved with not published version, use the first culture being saved

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

        Assert.IsTrue(impact.ImpactsInvariantProperties);
        Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsFalse(impact.ImpactsExplicitCulture);
        Assert.IsTrue(impact.ImpactsAllCultures);
        Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
    }

    [Test]
    public void Invariant_Culture()
    {
        var impact = BasicImpactFactory.ImpactInvariant();

        Assert.AreEqual(impact.Culture, null);

        Assert.IsTrue(impact.ImpactsInvariantProperties);
        Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
        Assert.IsTrue(impact.ImpactsOnlyInvariantCulture);
        Assert.IsFalse(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
        Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
    }

    [Test]
    public void Explicit_Default_Culture()
    {
        var impact = BasicImpactFactory.ImpactExplicit("en-US", true);

        Assert.AreEqual(impact.Culture, "en-US");

        Assert.IsTrue(impact.ImpactsInvariantProperties);
        Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsTrue(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
        Assert.IsTrue(impact.ImpactsOnlyDefaultCulture);
    }

    [Test]
    public void Explicit_NonDefault_Culture()
    {
        var impact = BasicImpactFactory.ImpactExplicit("en-US", false);

        Assert.AreEqual(impact.Culture, "en-US");

        Assert.IsFalse(impact.ImpactsInvariantProperties);
        Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsTrue(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
        Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
    }

    [Test]
    public void TryCreate_Explicit_Default_Culture()
    {
        var success =
            BasicImpactFactory.TryCreate("en-US", true, ContentVariation.Culture, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, "en-US");

        Assert.IsTrue(impact.ImpactsInvariantProperties);
        Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsTrue(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
        Assert.IsTrue(impact.ImpactsOnlyDefaultCulture);
    }

    [Test]
    public void TryCreate_Explicit_NonDefault_Culture()
    {
        var success =
            BasicImpactFactory.TryCreate("en-US", false, ContentVariation.Culture, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, "en-US");

        Assert.IsFalse(impact.ImpactsInvariantProperties);
        Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
        Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
        Assert.IsTrue(impact.ImpactsExplicitCulture);
        Assert.IsFalse(impact.ImpactsAllCultures);
        Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
    }

    [Test]
    public void TryCreate_AllCultures_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate("*", false, ContentVariation.Nothing, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, null);

        Assert.AreSame(BasicImpactFactory.ImpactInvariant(), impact);
    }

    [Test]
    public void TryCreate_AllCultures_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate("*", false, ContentVariation.Culture, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, "*");

        Assert.AreSame(BasicImpactFactory.ImpactAll(), impact);
    }

    [Test]
    public void TryCreate_Invariant_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate(null, false, ContentVariation.Culture, false, false, out var impact);
        Assert.IsFalse(success);
    }

    [Test]
    public void TryCreate_Invariant_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate(null, false, ContentVariation.Nothing, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.AreSame(BasicImpactFactory.ImpactInvariant(), impact);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Edit_Invariant_From_Non_Default_Impacts_Invariant_Properties(bool allowEditInvariantFromNonDefault)
    {
        var sut = createCultureImpactService(new ContentSettings
        {
            AllowEditInvariantFromNonDefault = allowEditInvariantFromNonDefault
        });
        var impact = sut.ImpactExplicit("da", false);

        Assert.AreEqual(allowEditInvariantFromNonDefault, impact.ImpactsAlsoInvariantProperties);
    }

    private CultureImpactFactory createCultureImpactService(ContentSettings contentSettings = null)
    {
        contentSettings ??= new ContentSettings { AllowEditInvariantFromNonDefault = false, };

        return new CultureImpactFactory(new TestOptionsMonitor<ContentSettings>(contentSettings));
    }
}
