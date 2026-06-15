// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class VariationTests
{
    private readonly PropertyEditorCollection _propertyEditorCollection = new (new DataEditorCollection(() => []));

    [Test]
    public void ValidateVariationTests()
    {
        // All tests:
        // 1. if exact is set to true: culture cannot be null when the ContentVariation.Culture flag is set
        // 2. if wildcards is set to false: fail when "*" is passed in as either culture or segment.
        // 3. ContentVariation flag is ignored when wildcards are used.
        // 4. Empty string is considered the same as null
        AssertForNovariation();
        AssertForCultureVariation();
        AssertForSegmentVariation();
        AssertForCultureAndSegmentVariation();
    }

    private static void AssertForNovariation()
    {
        Assert4A(ContentVariation.Nothing, null, null, true);
        Assert4A(ContentVariation.Nothing, null, string.Empty, true);
        Assert4B(ContentVariation.Nothing, null, "*", true, false, false, true);
        Assert4A(ContentVariation.Nothing, null, "segment", false);
        Assert4A(ContentVariation.Nothing, string.Empty, null, true);
        Assert4A(ContentVariation.Nothing, string.Empty, string.Empty, true);
        Assert4B(ContentVariation.Nothing, string.Empty, "*", true, false, false, true);
        Assert4A(ContentVariation.Nothing, string.Empty, "segment", false);
        Assert4B(ContentVariation.Nothing, "*", null, true, false, false, true);
        Assert4B(ContentVariation.Nothing, "*", string.Empty, true, false, false, true);
        Assert4B(ContentVariation.Nothing, "*", "*", true, false, false, true);
        Assert4A(ContentVariation.Nothing, "*", "segment", false);
        Assert4A(ContentVariation.Nothing, "culture", null, false);
        Assert4A(ContentVariation.Nothing, "culture", string.Empty, false);
        Assert4A(ContentVariation.Nothing, "culture", "*", false);
        Assert4A(ContentVariation.Nothing, "culture", "segment", false);
    }

    private static void AssertForCultureVariation()
    {
        Assert4B(ContentVariation.Culture, null, null, false, true, false, true);
        Assert4B(ContentVariation.Culture, null, string.Empty, false, true, false, true);
        Assert4B(ContentVariation.Culture, null, "*", false, false, false, true);
        Assert4A(ContentVariation.Culture, null, "segment", false);
        Assert4B(ContentVariation.Culture, string.Empty, null, false, true, false, true);
        Assert4B(ContentVariation.Culture, string.Empty, string.Empty, false, true, false, true);
        Assert4B(ContentVariation.Culture, string.Empty, "*", false, false, false, true);
        Assert4A(ContentVariation.Culture, string.Empty, "segment", false);
        Assert4B(ContentVariation.Culture, "*", null, true, false, false, true);
        Assert4B(ContentVariation.Culture, "*", string.Empty, true, false, false, true);
        Assert4B(ContentVariation.Culture, "*", "*", true, false, false, true);
        Assert4A(ContentVariation.Culture, "*", "segment", false);
        Assert4A(ContentVariation.Culture, "culture", null, true);
        Assert4A(ContentVariation.Culture, "culture", string.Empty, true);
        Assert4B(ContentVariation.Culture, "culture", "*", true, false, false, true);
        Assert4A(ContentVariation.Culture, "culture", "segment", false);
    }

    private static void AssertForSegmentVariation()
    {
        Assert4B(ContentVariation.Segment, null, null, true, true, true, true);
        Assert4B(ContentVariation.Segment, null, string.Empty, true, true, true, true);
        Assert4B(ContentVariation.Segment, null, "*", true, false, false, true);
        Assert4A(ContentVariation.Segment, null, "segment", true);
        Assert4B(ContentVariation.Segment, string.Empty, null, true, true, true, true);
        Assert4B(ContentVariation.Segment, string.Empty, string.Empty, true, true, true, true);
        Assert4B(ContentVariation.Segment, string.Empty, "*", true, false, false, true);
        Assert4A(ContentVariation.Segment, string.Empty, "segment", true);
        Assert4B(ContentVariation.Segment, "*", null, true, false, false, true);
        Assert4B(ContentVariation.Segment, "*", string.Empty, true, false, false, true);
        Assert4B(ContentVariation.Segment, "*", "*", true, false, false, true);
        Assert4B(ContentVariation.Segment, "*", "segment", true, false, false, true);
        Assert4A(ContentVariation.Segment, "culture", null, false);
        Assert4A(ContentVariation.Segment, "culture", string.Empty, false);
        Assert4A(ContentVariation.Segment, "culture", "*", false);
        Assert4A(ContentVariation.Segment, "culture", "segment", false);
    }

    private static void AssertForCultureAndSegmentVariation()
    {
        Assert4B(ContentVariation.CultureAndSegment, null, null, false, true, false, true);
        Assert4B(ContentVariation.CultureAndSegment, null, string.Empty, false, true, false, true);
        Assert4B(ContentVariation.CultureAndSegment, null, "*", false, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, null, "segment", false, true, false, true);
        Assert4B(ContentVariation.CultureAndSegment, string.Empty, null, false, true, false, true);
        Assert4B(ContentVariation.CultureAndSegment, string.Empty, string.Empty, false, true, false, true);
        Assert4B(ContentVariation.CultureAndSegment, string.Empty, "*", false, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, string.Empty, "segment", false, true, false, true);
        Assert4B(ContentVariation.CultureAndSegment, "*", null, true, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, "*", string.Empty, true, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, "*", "*", true, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, "*", "segment", true, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, "culture", null, true, true, true, true);
        Assert4B(ContentVariation.CultureAndSegment, "culture", string.Empty, true, true, true, true);
        Assert4B(ContentVariation.CultureAndSegment, "culture", "*", true, false, false, true);
        Assert4B(ContentVariation.CultureAndSegment, "culture", "segment", true, true, true, true);
    }

    /// <summary>
    ///     Asserts the result of <see cref="ContentVariationExtensions.ValidateVariation" />
    /// </summary>
    /// <param name="variation">The variation to validate</param>
    /// <param name="culture">The culture to validate</param>
    /// <param name="segment">The segment to validate</param>
    /// <param name="exactAndWildcards">Validate using Exact + Wildcards flags</param>
    /// <param name="nonExactAndNoWildcards">Validate using non Exact + no Wildcard flags</param>
    /// <param name="exactAndNoWildcards">Validate using Exact + no Wildcard flags</param>
    /// <param name="nonExactAndWildcards">Validate using non Exact + Wildcard flags</param>
    private static void Assert4B(
        ContentVariation variation,
        string culture,
        string segment,
        bool exactAndWildcards,
        bool nonExactAndNoWildcards,
        bool exactAndNoWildcards,
        bool nonExactAndWildcards)
    {
        Assert.That(variation.ValidateVariation(culture, segment, true, true, false), Is.EqualTo(exactAndWildcards));
        Assert.That(variation.ValidateVariation(culture, segment, false, false, false), Is.EqualTo(nonExactAndNoWildcards));
        Assert.That(variation.ValidateVariation(culture, segment, true, false, false), Is.EqualTo(exactAndNoWildcards));
        Assert.That(variation.ValidateVariation(culture, segment, false, true, false), Is.EqualTo(nonExactAndWildcards));
    }

    /// <summary>
    ///     Asserts the result of
    ///     <see cref="ContentVariationExtensions.ValidateVariation(ContentVariation, string, string, bool, bool, bool)" />
    ///     where expectedResult matches all combinations of Exact + Wildcard
    /// </summary>
    private static void Assert4A(ContentVariation variation, string culture, string segment, bool expectedResult) =>
        Assert4B(variation, culture, segment, expectedResult, expectedResult, expectedResult, expectedResult);

    [Test]
    public void PropertyTests()
    {
        var propertyType =
            new PropertyType(TestHelper.ShortStringHelper, "editor", ValueStorageType.Nvarchar) { Alias = "prop" };
        var prop = new Property(propertyType);

        const string langFr = "fr-FR";

        // can set value
        // and get edited and published value
        // because non-publishing
        prop.SetValue("a");
        Assert.That(prop.GetValue(), Is.EqualTo("a"));
        Assert.That(prop.GetValue(published: true), Is.EqualTo("a"));

        // illegal, 'cos non-publishing
        Assert.Throws<NotSupportedException>(() => prop.PublishValues());

        // change
        propertyType.SupportsPublishing = true;

        // can get value
        // and now published value is null
        Assert.That(prop.GetValue(), Is.EqualTo("a"));
        Assert.That(prop.GetValue(published: true), Is.Null);

        // cannot set non-supported variation value
        Assert.Throws<NotSupportedException>(() => prop.SetValue("x", langFr));
        Assert.That(prop.GetValue(langFr), Is.Null);

        // can publish value
        // and get edited and published values
        prop.PublishValues();
        Assert.That(prop.GetValue(), Is.EqualTo("a"));
        Assert.That(prop.GetValue(published: true), Is.EqualTo("a"));

        // can set value
        // and get edited and published values
        prop.SetValue("b");
        Assert.That(prop.GetValue(), Is.EqualTo("b"));
        Assert.That(prop.GetValue(published: true), Is.EqualTo("a"));

        // can clear value
        prop.UnpublishValues();
        Assert.That(prop.GetValue(), Is.EqualTo("b"));
        Assert.That(prop.GetValue(published: true), Is.Null);

        // change - now we vary by culture
        propertyType.Variations |= ContentVariation.Culture;

        // can set value
        // and get values
        prop.SetValue("c", langFr);
        Assert.That(prop.GetValue(), Is.Null); // there is no invariant value anymore
        Assert.That(prop.GetValue(published: true), Is.Null);
        Assert.That(prop.GetValue(langFr), Is.EqualTo("c"));
        Assert.That(prop.GetValue(langFr, published: true), Is.Null);

        // can publish value
        // and get edited and published values
        prop.PublishValues(langFr);
        Assert.That(prop.GetValue(), Is.Null);
        Assert.That(prop.GetValue(published: true), Is.Null);
        Assert.That(prop.GetValue(langFr), Is.EqualTo("c"));
        Assert.That(prop.GetValue(langFr, published: true), Is.EqualTo("c"));

        // can clear all
        prop.UnpublishValues();
        Assert.That(prop.GetValue(), Is.Null);
        Assert.That(prop.GetValue(published: true), Is.Null);
        Assert.That(prop.GetValue(langFr), Is.EqualTo("c"));
        Assert.That(prop.GetValue(langFr, published: true), Is.Null);

        // can publish all
        prop.PublishValues();
        Assert.That(prop.GetValue(), Is.Null);
        Assert.That(prop.GetValue(published: true), Is.Null);
        Assert.That(prop.GetValue(langFr), Is.EqualTo("c"));
        Assert.That(prop.GetValue(langFr, published: true), Is.EqualTo("c"));

        // same for culture
        prop.UnpublishValues(langFr);
        Assert.That(prop.GetValue(langFr), Is.EqualTo("c"));
        Assert.That(prop.GetValue(langFr, published: true), Is.Null);
        prop.PublishValues(langFr);
        Assert.That(prop.GetValue(langFr), Is.EqualTo("c"));
        Assert.That(prop.GetValue(langFr, published: true), Is.EqualTo("c"));

        prop.UnpublishValues(); // does not throw, internal, content item throws
        Assert.That(prop.GetValue(), Is.Null);
        Assert.That(prop.GetValue(published: true), Is.Null);
        prop.PublishValues(); // does not throw, internal, content item throws
        Assert.That(prop.GetValue(), Is.Null);
        Assert.That(prop.GetValue(published: true), Is.Null);
    }

    [Test]
    public void ContentNames()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .Build();
        var content = CreateContent(contentType);

        const string langFr = "fr-FR";
        const string langUk = "en-UK";

        // throws if the content type does not support the variation
        Assert.Throws<NotSupportedException>(() => content.SetCultureName("name-fr", langFr));

        // now it will work
        contentType.Variations = ContentVariation.Culture;

        // recreate content to re-capture content type variations
        content = CreateContent(contentType);

        // invariant name works
        content.Name = "name";
        Assert.That(content.GetCultureName(null), Is.EqualTo("name"));
        content.SetCultureName("name2", null);
        Assert.That(content.Name, Is.EqualTo("name2"));
        Assert.That(content.GetCultureName(null), Is.EqualTo("name2"));

        // variant names work
        content.SetCultureName("name-fr", langFr);
        content.SetCultureName("name-uk", langUk);
        Assert.That(content.GetCultureName(langFr), Is.EqualTo("name-fr"));
        Assert.That(content.GetCultureName(langUk), Is.EqualTo("name-uk"));

        // variant dictionary of names work
        Assert.That(content.CultureInfos, Has.Count.EqualTo(2));
        Assert.That(content.CultureInfos.ContainsKey(langFr), Is.True);
        Assert.That(content.CultureInfos[langFr].Name, Is.EqualTo("name-fr"));
        Assert.That(content.CultureInfos.ContainsKey(langUk), Is.True);
        Assert.That(content.CultureInfos[langUk].Name, Is.EqualTo("name-uk"));
    }

    [Test]
    public void ContentPublishValues()
    {
        const string langFr = "fr-FR";

        var propertyType = new PropertyTypeBuilder()
            .WithAlias("prop")
            .Build();
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .Build();
        contentType.AddPropertyType(propertyType);

        var content = CreateContent(contentType);

        // can set value
        // and get edited value, published is null
        // because publishing
        content.SetValue("prop", "a");
        Assert.That(content.GetValue("prop"), Is.EqualTo("a"));
        Assert.That(content.GetValue("prop", published: true), Is.Null);

        // cannot set non-supported variation value
        Assert.Throws<NotSupportedException>(() => content.SetValue("prop", "x", langFr));
        Assert.That(content.GetValue("prop", langFr), Is.Null);

        // can publish value
        // and get edited and published values
        Assert.That(content.PublishCulture(CultureImpact.All, DateTime.UtcNow, _propertyEditorCollection), Is.True);
        Assert.That(content.GetValue("prop"), Is.EqualTo("a"));
        Assert.That(content.GetValue("prop", published: true), Is.EqualTo("a"));

        // can set value
        // and get edited and published values
        content.SetValue("prop", "b");
        Assert.That(content.GetValue("prop"), Is.EqualTo("b"));
        Assert.That(content.GetValue("prop", published: true), Is.EqualTo("a"));

        // can clear value
        content.UnpublishCulture();
        Assert.That(content.GetValue("prop"), Is.EqualTo("b"));
        Assert.That(content.GetValue("prop", published: true), Is.Null);

        // change - now we vary by culture
        contentType.Variations |= ContentVariation.Culture;
        propertyType.Variations |= ContentVariation.Culture;
        content.ChangeContentType(contentType);

        // can set value
        // and get values
        content.SetValue("prop", "c", langFr);
        Assert.That(content.GetValue("prop"), Is.Null); // there is no invariant value anymore
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.Null);

        // can publish value
        // and get edited and published values
        Assert.That(content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection), Is.False); // no name
        content.SetCultureName("name-fr", langFr);
        Assert.That(content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection), Is.True);
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.EqualTo("c"));

        // can clear all
        content.UnpublishCulture();
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.Null);

        // can publish all
        Assert.That(content.PublishCulture(CultureImpact.All, DateTime.UtcNow, _propertyEditorCollection), Is.True);
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.EqualTo("c"));

        // same for culture
        content.UnpublishCulture(langFr);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.Null);
        Assert.That(content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection), Is.True);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.EqualTo("c"));

        content.UnpublishCulture(); // clears invariant props if any
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.PublishCulture(CultureImpact.All, DateTime.UtcNow, _propertyEditorCollection), Is.True); // publishes invariant props if any
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);

        var other = CreateContent(contentType, 2, "other");

        Assert.Throws<NotSupportedException>(() => other.SetValue("prop", "o")); // don't even try
        other.SetValue("prop", "o1", langFr);

        // can copy other's edited value
        content.CopyFrom(other);
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("o1"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.EqualTo("c"));

        // can copy self's published value
        content.CopyFrom(content);
        Assert.That(content.GetValue("prop"), Is.Null);
        Assert.That(content.GetValue("prop", published: true), Is.Null);
        Assert.That(content.GetValue("prop", langFr), Is.EqualTo("c"));
        Assert.That(content.GetValue("prop", langFr, published: true), Is.EqualTo("c"));
    }

    [Test]
    public void ContentPublishValuesWithMixedPropertyTypeVariations()
    {
        var propertyValidationService = GetPropertyValidationService();
        const string langFr = "fr-FR";

        // content type varies by Culture
        // prop1 varies by Culture
        // prop2 is invariant
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .Build();
        contentType.Variations |= ContentVariation.Culture;

        var variantPropType = new PropertyTypeBuilder()
            .WithAlias("prop1")
            .WithVariations(ContentVariation.Culture)
            .WithMandatory(true)
            .Build();
        var invariantPropType = new PropertyTypeBuilder()
            .WithAlias("prop2")
            .WithVariations(ContentVariation.Nothing)
            .WithMandatory(true)
            .Build();
        contentType.AddPropertyType(variantPropType);
        contentType.AddPropertyType(invariantPropType);

        var content = CreateContent(contentType);

        content.SetCultureName("hello", langFr);

        // for this test we'll make the french culture the default one - this is needed for publishing invariant property values
        var langFrImpact = CultureImpact.Explicit(langFr, true);

        Assert.That(
            content.PublishCulture(langFrImpact, DateTime.UtcNow, _propertyEditorCollection), Is.True); // succeeds because names are ok (not validating properties here)
        Assert.That(
            propertyValidationService.IsPropertyDataValid(content, out _, langFrImpact), Is.False); // fails because prop1 is mandatory

        content.SetValue("prop1", "a", langFr);
        Assert.That(
            content.PublishCulture(langFrImpact, DateTime.UtcNow, _propertyEditorCollection), Is.True); // succeeds because names are ok (not validating properties here)

        // Fails because prop2 is mandatory and invariant and the item isn't published.
        // Invariant is validated against the default language except when there isn't a published version, in that case it's always validated.
        Assert.That(propertyValidationService.IsPropertyDataValid(content, out _, langFrImpact), Is.False);
        content.SetValue("prop2", "x");
        Assert.That(content.PublishCulture(langFrImpact, DateTime.UtcNow, _propertyEditorCollection), Is.True); // still ok...
        Assert.That(propertyValidationService.IsPropertyDataValid(content, out _, langFrImpact), Is.True); // now it's ok

        Assert.That(content.GetValue("prop1", langFr, published: true), Is.EqualTo("a"));
        Assert.That(content.GetValue("prop2", published: true), Is.EqualTo("x"));
    }

    [Test]
    public void ContentPublishVariations()
    {
        const string langFr = "fr-FR";
        const string langUk = "en-UK";
        const string langEs = "es-ES";

        var propertyType = new PropertyTypeBuilder()
            .WithAlias("prop")
            .Build();
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .Build();
        contentType.AddPropertyType(propertyType);

        var content = CreateContent(contentType);

        // change - now we vary by culture
        contentType.Variations |= ContentVariation.Culture;
        propertyType.Variations |= ContentVariation.Culture;

        content.ChangeContentType(contentType);

        Assert.Throws<NotSupportedException>(() => content.SetValue("prop", "a")); // invariant = no
        content.SetValue("prop", "a-fr", langFr);
        content.SetValue("prop", "a-uk", langUk);
        content.SetValue("prop", "a-es", langEs);

        // cannot publish without a name
        Assert.That(content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection), Is.False);

        // works with a name
        // and then FR is available, and published
        content.SetCultureName("name-fr", langFr);
        Assert.That(content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection), Is.True);

        // now UK is available too
        content.SetCultureName("name-uk", langUk);

        // test available, published
        Assert.That(content.IsCultureAvailable(langFr), Is.True);
        Assert.That(content.IsCulturePublished(langFr), Is.True);
        Assert.That(content.GetPublishName(langFr), Is.EqualTo("name-fr"));
        Assert.That(content.GetPublishDate(langFr), Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(content.IsCultureEdited(langFr), Is.False); // once published, edited is *wrong* until saved

        Assert.That(content.IsCultureAvailable(langUk), Is.True);
        Assert.That(content.IsCulturePublished(langUk), Is.False);
        Assert.That(content.GetPublishName(langUk), Is.Null);
        Assert.That(content.GetPublishDate(langUk), Is.Null); // not published

        Assert.That(content.IsCultureAvailable(langEs), Is.False);
        Assert.That(content.IsCultureEdited(langEs), Is.False); // not avail, so... not edited
        Assert.That(content.IsCulturePublished(langEs), Is.False);

        // not published!
        Assert.That(content.GetPublishName(langEs), Is.Null);
        Assert.That(content.GetPublishDate(langEs), Is.Null);

        // cannot test IsCultureEdited here - as that requires the content service and repository
        // see: ContentServiceTests.Can_SaveRead_Variations
    }

    [Test]
    public void IsDirtyTests()
    {
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("prop")
            .Build();
        var prop = new Property(propertyType);
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .Build();
        contentType.AddPropertyType(propertyType);

        var content = CreateContent(contentType);

        prop.SetValue("a");
        Assert.That(prop.GetValue(), Is.EqualTo("a"));
        Assert.That(prop.GetValue(published: true), Is.Null);

        Assert.That(prop.IsDirty(), Is.True);

        content.SetValue("prop", "a");
        Assert.That(content.GetValue("prop"), Is.EqualTo("a"));
        Assert.That(content.GetValue("prop", published: true), Is.Null);

        Assert.That(content.IsDirty(), Is.True);
        Assert.That(content.IsAnyUserPropertyDirty(), Is.True);
        //// how can we tell which variation was dirty?
    }

    [Test]
    public void ValidationTests()
    {
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("prop")
            .WithSupportsPublishing(true)
            .Build();

        var prop = new Property(propertyType);

        prop.SetValue("a");
        Assert.That(prop.GetValue(), Is.EqualTo("a"));
        Assert.That(prop.GetValue(published: true), Is.Null);
        var propertyValidationService = GetPropertyValidationService();

        Assert.That(propertyValidationService.IsPropertyValid(prop, PropertyValidationContext.Empty()), Is.True);

        propertyType.Mandatory = true;
        Assert.That(propertyValidationService.IsPropertyValid(prop, PropertyValidationContext.Empty()), Is.True);

        prop.SetValue(null);
        Assert.That(propertyValidationService.IsPropertyValid(prop, PropertyValidationContext.Empty()), Is.False);

        // can publish, even though invalid
        prop.PublishValues();
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void NoValueTests(bool variesByCulture, bool variesBySegment)
    {
        var variation = variesByCulture && variesBySegment
            ? ContentVariation.CultureAndSegment
            : variesByCulture
                ? ContentVariation.Culture
                : variesBySegment
                    ? ContentVariation.Segment
                    : ContentVariation.Nothing;

        var culture = variesByCulture ? "en-US" : null;
        var segment = variesBySegment ? "my-segment" : null;

        var propertyType = new PropertyTypeBuilder()
            .WithAlias("prop")
            .WithSupportsPublishing(true)
            .WithVariations(variation)
            .Build();

        var prop = new Property(propertyType);
        var propertyValidationService = GetPropertyValidationService();

        // "no value" is valid for non-mandatory properties
        Assert.That(propertyValidationService.IsPropertyValid(prop, PropertyValidationContext.CultureAndSegment(culture, segment)), Is.True);

        propertyType.Mandatory = true;

        // "no value" is NOT valid for mandatory properties
        Assert.That(propertyValidationService.IsPropertyValid(prop, PropertyValidationContext.CultureAndSegment(culture, segment)), Is.False);

        // can publish, even though invalid
        prop.PublishValues();
    }

    private static Content CreateContent(IContentType contentType, int id = 1, string name = "content") =>
        new ContentBuilder()
            .WithId(id)
            .WithVersionId(1)
            .WithName(name)
            .WithContentType(contentType)
            .Build();

    private static PropertyValidationService GetPropertyValidationService()
    {
        var ioHelper = Mock.Of<IIOHelper>();
        var dataTypeService = Mock.Of<IDataTypeService>();

        var attribute = new DataEditorAttribute("a");
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(x
            => x.Create<TextOnlyValueEditor>(It.IsAny<DataEditorAttribute>()) == new TextOnlyValueEditor(
                attribute,
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
                Mock.Of<IIOHelper>()));

        var textBoxEditor = new TextboxPropertyEditor(
            dataValueEditorFactory,
            ioHelper);

        var serializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var textboxKey = Guid.NewGuid();
        Mock.Get(dataTypeService).Setup(x => x.GetAsync(textboxKey))
            .ReturnsAsync(new DataType(textBoxEditor, serializer));

        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap.Setup(x => x.GetKeyForId(Constants.DataTypes.Textbox, UmbracoObjectTypes.DataType))
            .Returns(Attempt.Succeed(textboxKey));

        var propertyEditorCollection =
            new PropertyEditorCollection(new DataEditorCollection(() => new[] { textBoxEditor }));
        return new PropertyValidationService(
            propertyEditorCollection,
            dataTypeService,
            Mock.Of<ILocalizedTextService>(),
            new ValueEditorCache(),
            Mock.Of<ICultureDictionary>(),
            Mock.Of<ILanguageService>(),
            Mock.Of<IOptions<ContentSettings>>(),
            idKeyMap.Object);
    }
}
