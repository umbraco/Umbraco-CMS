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

        // Invariant properties now travel with any explicit culture impact. Whether the editing user
        // is actually allowed to mutate invariant property values is gated upstream at the service
        // boundary via the HasAccessToInvariantForVariant user-group permission.
        Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
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

        // Invariant properties now travel with any explicit culture impact — permission gating lives upstream.
        Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
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
    public void AllowEditInvariantFromNonDefault_Config_No_Longer_Affects_ImpactsAlsoInvariantProperties(bool allowEditInvariantFromNonDefault)
    {
        var sut = CreateCultureImpactService(new ContentSettings
        {
            AllowEditInvariantFromNonDefault = allowEditInvariantFromNonDefault
        });
        var impact = sut.ImpactExplicit("da", false);

        // Regardless of the legacy config flag, an explicit non-default culture impact always
        // carries invariant properties. The upstream permission check decides whether the user
        // is allowed to mutate those values.
        Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
    }

    private CultureImpactFactory CreateCultureImpactService(ContentSettings contentSettings = null)
    {
        contentSettings ??= new ContentSettings { AllowEditInvariantFromNonDefault = false, };

        return new CultureImpactFactory(new TestOptionsMonitor<ContentSettings>(contentSettings));
    }
}
