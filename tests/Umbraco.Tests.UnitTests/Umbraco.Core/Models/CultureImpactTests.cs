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
        Assert.That(result, Is.EqualTo("en-US")); // default culture is being saved so use it

        result = BasicImpactFactory.GetCultureForInvariantErrors(
            Mock.Of<IContent>(x => x.Published == false),
            new[] { "fr-FR" },
            "en-US");
        Assert.That(result, Is.EqualTo("fr-FR")); // default culture not being saved with not published version,
                                          // use the first culture being saved

        result = BasicImpactFactory.GetCultureForInvariantErrors(
            Mock.Of<IContent>(x => x.Published == true),
            new[] { "fr-FR" },
            "en-US");
        Assert.That(result, Is.EqualTo(null)); // default culture not being saved with published version, use null
    }

    [Test]
    public void All_Cultures()
    {
        var impact = BasicImpactFactory.ImpactAll();

        Assert.That(impact.Culture, Is.EqualTo("*"));

        Assert.That(impact.ImpactsInvariantProperties, Is.True);
        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.False);
        Assert.That(impact.ImpactsOnlyInvariantCulture, Is.False);
        Assert.That(impact.ImpactsExplicitCulture, Is.False);
        Assert.That(impact.ImpactsAllCultures, Is.True);
        Assert.That(impact.ImpactsOnlyDefaultCulture, Is.False);
    }

    [Test]
    public void Invariant_Culture()
    {
        var impact = BasicImpactFactory.ImpactInvariant();

        Assert.That(impact.Culture, Is.EqualTo(null));

        Assert.That(impact.ImpactsInvariantProperties, Is.True);
        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.False);
        Assert.That(impact.ImpactsOnlyInvariantCulture, Is.True);
        Assert.That(impact.ImpactsExplicitCulture, Is.False);
        Assert.That(impact.ImpactsAllCultures, Is.False);
        Assert.That(impact.ImpactsOnlyDefaultCulture, Is.False);
    }

    [Test]
    public void Explicit_Default_Culture()
    {
        var impact = BasicImpactFactory.ImpactExplicit("en-US", true);

        Assert.That(impact.Culture, Is.EqualTo("en-US"));

        Assert.That(impact.ImpactsInvariantProperties, Is.True);
        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.True);
        Assert.That(impact.ImpactsOnlyInvariantCulture, Is.False);
        Assert.That(impact.ImpactsExplicitCulture, Is.True);
        Assert.That(impact.ImpactsAllCultures, Is.False);
        Assert.That(impact.ImpactsOnlyDefaultCulture, Is.True);
    }

    [Test]
    public void Explicit_NonDefault_Culture()
    {
        var impact = BasicImpactFactory.ImpactExplicit("en-US", false);

        Assert.That(impact.Culture, Is.EqualTo("en-US"));

        Assert.That(impact.ImpactsInvariantProperties, Is.False);
        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.False);
        Assert.That(impact.ImpactsOnlyInvariantCulture, Is.False);
        Assert.That(impact.ImpactsExplicitCulture, Is.True);
        Assert.That(impact.ImpactsAllCultures, Is.False);
        Assert.That(impact.ImpactsOnlyDefaultCulture, Is.False);
    }

    [Test]
    public void TryCreate_Explicit_Default_Culture()
    {
        var success =
            BasicImpactFactory.TryCreate("en-US", true, ContentVariation.Culture, false, false, out var impact);
        Assert.That(success, Is.True);

        Assert.That(impact, Is.Not.Null);
        Assert.That(impact.Culture, Is.EqualTo("en-US"));

        Assert.That(impact.ImpactsInvariantProperties, Is.True);
        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.True);
        Assert.That(impact.ImpactsOnlyInvariantCulture, Is.False);
        Assert.That(impact.ImpactsExplicitCulture, Is.True);
        Assert.That(impact.ImpactsAllCultures, Is.False);
        Assert.That(impact.ImpactsOnlyDefaultCulture, Is.True);
    }

    [Test]
    public void TryCreate_Explicit_NonDefault_Culture()
    {
        var success =
            BasicImpactFactory.TryCreate("en-US", false, ContentVariation.Culture, false, false, out var impact);
        Assert.That(success, Is.True);

        Assert.That(impact, Is.Not.Null);
        Assert.That(impact.Culture, Is.EqualTo("en-US"));

        Assert.That(impact.ImpactsInvariantProperties, Is.False);
        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.False);
        Assert.That(impact.ImpactsOnlyInvariantCulture, Is.False);
        Assert.That(impact.ImpactsExplicitCulture, Is.True);
        Assert.That(impact.ImpactsAllCultures, Is.False);
        Assert.That(impact.ImpactsOnlyDefaultCulture, Is.False);
    }

    [Test]
    public void TryCreate_AllCultures_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate("*", false, ContentVariation.Nothing, false, false, out var impact);
        Assert.That(success, Is.True);

        Assert.That(impact, Is.Not.Null);
        Assert.That(impact.Culture, Is.EqualTo(null));

        Assert.That(impact, Is.SameAs(BasicImpactFactory.ImpactInvariant()));
    }

    [Test]
    public void TryCreate_AllCultures_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate("*", false, ContentVariation.Culture, false, false, out var impact);
        Assert.That(success, Is.True);

        Assert.That(impact, Is.Not.Null);
        Assert.That(impact.Culture, Is.EqualTo("*"));

        Assert.That(impact, Is.SameAs(BasicImpactFactory.ImpactAll()));
    }

    [Test]
    public void TryCreate_Invariant_For_Variant()
    {
        var success = BasicImpactFactory.TryCreate(null, false, ContentVariation.Culture, false, false, out var impact);
        Assert.That(success, Is.False);
    }

    [Test]
    public void TryCreate_Invariant_For_Invariant()
    {
        var success = BasicImpactFactory.TryCreate(null, false, ContentVariation.Nothing, false, false, out var impact);
        Assert.That(success, Is.True);

        Assert.That(impact, Is.SameAs(BasicImpactFactory.ImpactInvariant()));
    }

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

        Assert.That(impact.ImpactsAlsoInvariantProperties, Is.EqualTo(allowEditInvariantFromNonDefault));
    }

    private CultureImpactFactory CreateCultureImpactService(ContentSettings contentSettings = null)
    {
        contentSettings ??= new ContentSettings { AllowEditInvariantFromNonDefault = false, };

        return new CultureImpactFactory(new TestOptionsMonitor<ContentSettings>(contentSettings));
    }
}
