// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="CultureImpact"/> class.
/// </summary>
[TestFixture]
public class CultureImpactTests
{
    private CultureImpactFactory BasicImpactFactory => CreateCultureImpactService();

    /// <summary>
    /// Unit test for <see cref="BasicImpactFactory.GetCultureForInvariantErrors"/>.
    /// Verifies that the correct culture is returned based on the content's published state,
    /// the provided cultures array, and the default culture.
    /// Scenarios tested include:
    /// - Published content with the default culture present
    /// - Unpublished content with only non-default cultures
    /// - Published content with only non-default cultures
    /// </summary>
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

    /// <summary>
    /// Tests that the impact object correctly represents the case where all cultures are affected.
    /// Verifies that the culture is set to '*', invariant properties are impacted, and the appropriate flags are set.
    /// </summary>
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

    /// <summary>
    /// Verifies that the <c>ImpactInvariant</c> method produces a culture impact
    /// with the expected invariant properties, ensuring that only invariant culture
    /// properties are impacted and no explicit or default cultures are affected.
    /// </summary>
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

    /// <summary>
    /// Verifies that when an explicit default culture ("en-US") is set, the resulting impact object
    /// correctly identifies its culture and the properties related to invariant and explicit culture impacts.
    /// </summary>
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

    /// <summary>
    /// Verifies that an explicit non-default culture impact is correctly identified.
    /// Ensures that the impact is set for the specified culture (e.g., "en-US"),
    /// does not affect invariant or default cultures, and only impacts the explicit culture.
    /// </summary>
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

    /// <summary>
    /// Tests the creation of a CultureImpact with an explicit default culture.
    /// Verifies that the impact properties are set correctly for the "en-US" culture.
    /// </summary>
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

    /// <summary>
    /// Verifies that creating a <see cref="CultureImpact"/> instance with an explicit non-default culture (e.g., "en-US")
    /// succeeds and that the resulting instance correctly reflects the expected culture-specific impact flags.
    /// </summary>
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

    /// <summary>
    /// Tests that the TryCreate method successfully creates an invariant culture impact.
    /// </summary>
    [Test]
    public void TryCreate_AllCultures_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate("*", false, ContentVariation.Nothing, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, null);

        Assert.AreSame(BasicImpactFactory.ImpactInvariant(), impact);
    }

    /// <summary>
    /// Tests that the TryCreate method successfully creates an impact for all cultures when the variant is culture.
    /// </summary>
    [Test]
    public void TryCreate_AllCultures_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate("*", false, ContentVariation.Culture, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.IsNotNull(impact);
        Assert.AreEqual(impact.Culture, "*");

        Assert.AreSame(BasicImpactFactory.ImpactAll(), impact);
    }

    /// <summary>
    /// Tests that TryCreate returns false when attempting to create an invariant impact for a variant content variation.
    /// </summary>
    [Test]
    public void TryCreate_Invariant_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate(null, false, ContentVariation.Culture, false, false, out var impact);
        Assert.IsFalse(success);
    }

    /// <summary>
    /// Tests that TryCreate returns the invariant impact when called with invariant parameters.
    /// </summary>
    [Test]
    public void TryCreate_Invariant_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate(null, false, ContentVariation.Nothing, false, false, out var impact);
        Assert.IsTrue(success);

        Assert.AreSame(BasicImpactFactory.ImpactInvariant(), impact);
    }

    /// <summary>
    /// Verifies that editing invariant properties from a non-default culture impacts invariant properties
    /// according to the <paramref name="allowEditInvariantFromNonDefault"/> setting in <see cref="ContentSettings"/>.
    /// </summary>
    /// <param name="allowEditInvariantFromNonDefault">If <c>true</c>, editing invariant properties from a non-default culture is allowed and should impact invariant properties; otherwise, it should not.</param>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Edit_Invariant_From_Non_Default_Impacts_Invariant_Properties(bool allowEditInvariantFromNonDefault)
    {
        var sut = CreateCultureImpactService(new ContentSettings
        {
            AllowEditInvariantFromNonDefault = allowEditInvariantFromNonDefault
        });
        var impact = sut.ImpactExplicit("da", false);

        Assert.AreEqual(allowEditInvariantFromNonDefault, impact.ImpactsAlsoInvariantProperties);
    }

    private CultureImpactFactory CreateCultureImpactService(ContentSettings contentSettings = null)
    {
        contentSettings ??= new ContentSettings { AllowEditInvariantFromNonDefault = false, };

        return new CultureImpactFactory(new TestOptionsMonitor<ContentSettings>(contentSettings));
    }
}
