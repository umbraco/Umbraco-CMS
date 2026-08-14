// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class BlockExposeFallbackHelperTests
{
    private static readonly Guid _elementKey = Guid.NewGuid();

    [Test]
    public void IsBlockExposed_Returns_True_For_Direct_Match()
    {
        var expose = CreateExpose(("nl-BE", null));
        var languages = BuildLanguageMap(CreateLanguage("nl-BE"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, default, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.AreEqual("nl-BE", resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_True_Via_Language_Fallback()
    {
        var expose = CreateExpose(("en-US", null));
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("nl-BE", fallbackIsoCode: "en-US"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, Fallback.ToLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.AreEqual("en-US", resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_True_Via_Multi_Level_Language_Fallback()
    {
        var expose = CreateExpose(("en-US", null));
        // nl-BE -> fr-FR -> en-US
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("fr-FR", fallbackIsoCode: "en-US"),
            CreateLanguage("nl-BE", fallbackIsoCode: "fr-FR"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, Fallback.ToLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.AreEqual("en-US", resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Resolves_To_Nearest_Fallback_Culture()
    {
        // Block is exposed for fr-FR (the intermediate language).
        var expose = CreateExpose(("fr-FR", null));
        // nl-BE -> fr-FR -> en-US
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("fr-FR", fallbackIsoCode: "en-US"),
            CreateLanguage("nl-BE", fallbackIsoCode: "fr-FR"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, Fallback.ToLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.AreEqual("fr-FR", resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_False_When_Fallback_Is_None()
    {
        var expose = CreateExpose(("en-US", null));
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("nl-BE", fallbackIsoCode: "en-US"));

        // Default Fallback (None) — should NOT walk fallback chain.
        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, default, languages, "en-US", out var resolvedCulture);

        Assert.IsFalse(result);
        Assert.IsNull(resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_True_Via_DefaultLanguage_Fallback()
    {
        var expose = CreateExpose(("en-US", null));
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("nl-BE"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, Fallback.ToDefaultLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.AreEqual("en-US", resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Handles_Circular_Fallback_Without_Infinite_Loop()
    {
        var expose = CreateExpose(("de-DE", null));

        // Circular: en-US -> nl-BE -> en-US (neither resolves to de-DE).
        var languages = BuildLanguageMap(
            CreateLanguage("en-US", fallbackIsoCode: "nl-BE"),
            CreateLanguage("nl-BE", fallbackIsoCode: "en-US"),
            CreateLanguage("de-DE"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "en-US", null, Fallback.ToLanguage, languages, "de-DE", out var resolvedCulture);

        Assert.IsFalse(result);
        Assert.IsNull(resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_True_For_Invariant_Block_Without_Fallback()
    {
        var expose = CreateExpose((null, null));
        var languages = BuildLanguageMap(CreateLanguage("en-US"));

        // expectedCulture is null for invariant blocks — direct match should succeed.
        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, null, null, default, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.IsNull(resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Preserves_Segment_In_Language_Fallback()
    {
        var expose = CreateExpose(("en-US", "my-segment"));
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("nl-BE", fallbackIsoCode: "en-US"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", "my-segment", Fallback.ToLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsTrue(result);
        Assert.AreEqual("en-US", resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_False_When_Segment_Does_Not_Match_In_Fallback()
    {
        var expose = CreateExpose(("en-US", "segment-a"));
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("nl-BE", fallbackIsoCode: "en-US"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", "segment-b", Fallback.ToLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsFalse(result);
        Assert.IsNull(resolvedCulture);
    }

    [Test]
    public void IsBlockExposed_Returns_False_When_No_Fallback_Configured_For_Language()
    {
        var expose = CreateExpose(("en-US", null));

        // nl-BE has no fallback configured.
        var languages = BuildLanguageMap(
            CreateLanguage("en-US"),
            CreateLanguage("nl-BE"));

        var result = BlockExposeFallbackHelper.IsBlockExposed(expose, _elementKey, "nl-BE", null, Fallback.ToLanguage, languages, "en-US", out var resolvedCulture);

        Assert.IsFalse(result);
        Assert.IsNull(resolvedCulture);
    }

    private static List<BlockItemVariation> CreateExpose(params (string? Culture, string? Segment)[] entries)
        => entries.Select(e => new BlockItemVariation { ContentKey = _elementKey, Culture = e.Culture, Segment = e.Segment }).ToList();

    private static Dictionary<string, ILanguage> BuildLanguageMap(params ILanguage[] languages)
        => languages.ToDictionary(l => l.IsoCode, StringComparer.OrdinalIgnoreCase);

    private static ILanguage CreateLanguage(string isoCode, string? fallbackIsoCode = null)
    {
        var builder = new LanguageBuilder()
            .WithCultureInfo(isoCode);

        if (fallbackIsoCode is not null)
        {
            builder.WithFallbackLanguageIsoCode(fallbackIsoCode);
        }

        return builder.Build();
    }
}
