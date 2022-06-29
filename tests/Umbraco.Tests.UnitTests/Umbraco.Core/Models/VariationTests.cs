// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
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
        Assert.AreEqual(exactAndWildcards, variation.ValidateVariation(culture, segment, true, true, false));
        Assert.AreEqual(nonExactAndNoWildcards, variation.ValidateVariation(culture, segment, false, false, false));
        Assert.AreEqual(exactAndNoWildcards, variation.ValidateVariation(culture, segment, true, false, false));
        Assert.AreEqual(nonExactAndWildcards, variation.ValidateVariation(culture, segment, false, true, false));
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
        Assert.AreEqual("a", prop.GetValue());
        Assert.AreEqual("a", prop.GetValue(published: true));

        // illegal, 'cos non-publishing
        Assert.Throws<NotSupportedException>(() => prop.PublishValues());

        // change
        propertyType.SupportsPublishing = true;

        // can get value
        // and now published value is null
        Assert.AreEqual("a", prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));

        // cannot set non-supported variation value
        Assert.Throws<NotSupportedException>(() => prop.SetValue("x", langFr));
        Assert.IsNull(prop.GetValue(langFr));

        // can publish value
        // and get edited and published values
        prop.PublishValues();
        Assert.AreEqual("a", prop.GetValue());
        Assert.AreEqual("a", prop.GetValue(published: true));

        // can set value
        // and get edited and published values
        prop.SetValue("b");
        Assert.AreEqual("b", prop.GetValue());
        Assert.AreEqual("a", prop.GetValue(published: true));

        // can clear value
        prop.UnpublishValues();
        Assert.AreEqual("b", prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));

        // change - now we vary by culture
        propertyType.Variations |= ContentVariation.Culture;

        // can set value
        // and get values
        prop.SetValue("c", langFr);
        Assert.IsNull(prop.GetValue()); // there is no invariant value anymore
        Assert.IsNull(prop.GetValue(published: true));
        Assert.AreEqual("c", prop.GetValue(langFr));
        Assert.IsNull(prop.GetValue(langFr, published: true));

        // can publish value
        // and get edited and published values
        prop.PublishValues(langFr);
        Assert.IsNull(prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));
        Assert.AreEqual("c", prop.GetValue(langFr));
        Assert.AreEqual("c", prop.GetValue(langFr, published: true));

        // can clear all
        prop.UnpublishValues();
        Assert.IsNull(prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));
        Assert.AreEqual("c", prop.GetValue(langFr));
        Assert.IsNull(prop.GetValue(langFr, published: true));

        // can publish all
        prop.PublishValues();
        Assert.IsNull(prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));
        Assert.AreEqual("c", prop.GetValue(langFr));
        Assert.AreEqual("c", prop.GetValue(langFr, published: true));

        // same for culture
        prop.UnpublishValues(langFr);
        Assert.AreEqual("c", prop.GetValue(langFr));
        Assert.IsNull(prop.GetValue(langFr, published: true));
        prop.PublishValues(langFr);
        Assert.AreEqual("c", prop.GetValue(langFr));
        Assert.AreEqual("c", prop.GetValue(langFr, published: true));

        prop.UnpublishValues(); // does not throw, internal, content item throws
        Assert.IsNull(prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));
        prop.PublishValues(); // does not throw, internal, content item throws
        Assert.IsNull(prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));
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
        Assert.AreEqual("name", content.GetCultureName(null));
        content.SetCultureName("name2", null);
        Assert.AreEqual("name2", content.Name);
        Assert.AreEqual("name2", content.GetCultureName(null));

        // variant names work
        content.SetCultureName("name-fr", langFr);
        content.SetCultureName("name-uk", langUk);
        Assert.AreEqual("name-fr", content.GetCultureName(langFr));
        Assert.AreEqual("name-uk", content.GetCultureName(langUk));

        // variant dictionary of names work
        Assert.AreEqual(2, content.CultureInfos.Count);
        Assert.IsTrue(content.CultureInfos.ContainsKey(langFr));
        Assert.AreEqual("name-fr", content.CultureInfos[langFr].Name);
        Assert.IsTrue(content.CultureInfos.ContainsKey(langUk));
        Assert.AreEqual("name-uk", content.CultureInfos[langUk].Name);
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
        Assert.AreEqual("a", content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));

        // cannot set non-supported variation value
        Assert.Throws<NotSupportedException>(() => content.SetValue("prop", "x", langFr));
        Assert.IsNull(content.GetValue("prop", langFr));

        // can publish value
        // and get edited and published values
        Assert.IsTrue(content.PublishCulture(CultureImpact.All));
        Assert.AreEqual("a", content.GetValue("prop"));
        Assert.AreEqual("a", content.GetValue("prop", published: true));

        // can set value
        // and get edited and published values
        content.SetValue("prop", "b");
        Assert.AreEqual("b", content.GetValue("prop"));
        Assert.AreEqual("a", content.GetValue("prop", published: true));

        // can clear value
        content.UnpublishCulture();
        Assert.AreEqual("b", content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));

        // change - now we vary by culture
        contentType.Variations |= ContentVariation.Culture;
        propertyType.Variations |= ContentVariation.Culture;
        content.ChangeContentType(contentType);

        // can set value
        // and get values
        content.SetValue("prop", "c", langFr);
        Assert.IsNull(content.GetValue("prop")); // there is no invariant value anymore
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.IsNull(content.GetValue("prop", langFr, published: true));

        // can publish value
        // and get edited and published values
        Assert.IsFalse(content.PublishCulture(CultureImpact.Explicit(langFr, false))); // no name
        content.SetCultureName("name-fr", langFr);
        Assert.IsTrue(content.PublishCulture(CultureImpact.Explicit(langFr, false)));
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

        // can clear all
        content.UnpublishCulture();
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.IsNull(content.GetValue("prop", langFr, published: true));

        // can publish all
        Assert.IsTrue(content.PublishCulture(CultureImpact.All));
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

        // same for culture
        content.UnpublishCulture(langFr);
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.IsNull(content.GetValue("prop", langFr, published: true));
        Assert.IsTrue(content.PublishCulture(CultureImpact.Explicit(langFr, false)));
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

        content.UnpublishCulture(); // clears invariant props if any
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.IsTrue(content.PublishCulture(CultureImpact.All)); // publishes invariant props if any
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));

        var other = CreateContent(contentType, 2, "other");

        Assert.Throws<NotSupportedException>(() => other.SetValue("prop", "o")); // don't even try
        other.SetValue("prop", "o1", langFr);

        // can copy other's edited value
        content.CopyFrom(other);
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.AreEqual("o1", content.GetValue("prop", langFr));
        Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

        // can copy self's published value
        content.CopyFrom(content);
        Assert.IsNull(content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));
        Assert.AreEqual("c", content.GetValue("prop", langFr));
        Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));
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

        Assert.IsTrue(
            content.PublishCulture(langFrImpact)); // succeeds because names are ok (not validating properties here)
        Assert.IsFalse(
            propertyValidationService.IsPropertyDataValid(content, out _, langFrImpact)); // fails because prop1 is mandatory

        content.SetValue("prop1", "a", langFr);
        Assert.IsTrue(
            content.PublishCulture(langFrImpact)); // succeeds because names are ok (not validating properties here)

        // Fails because prop2 is mandatory and invariant and the item isn't published.
        // Invariant is validated against the default language except when there isn't a published version, in that case it's always validated.
        Assert.IsFalse(propertyValidationService.IsPropertyDataValid(content, out _, langFrImpact));
        content.SetValue("prop2", "x");
        Assert.IsTrue(content.PublishCulture(langFrImpact)); // still ok...
        Assert.IsTrue(propertyValidationService.IsPropertyDataValid(content, out _, langFrImpact)); // now it's ok

        Assert.AreEqual("a", content.GetValue("prop1", langFr, published: true));
        Assert.AreEqual("x", content.GetValue("prop2", published: true));
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
        Assert.IsFalse(content.PublishCulture(CultureImpact.Explicit(langFr, false)));

        // works with a name
        // and then FR is available, and published
        content.SetCultureName("name-fr", langFr);
        Assert.IsTrue(content.PublishCulture(CultureImpact.Explicit(langFr, false)));

        // now UK is available too
        content.SetCultureName("name-uk", langUk);

        // test available, published
        Assert.IsTrue(content.IsCultureAvailable(langFr));
        Assert.IsTrue(content.IsCulturePublished(langFr));
        Assert.AreEqual("name-fr", content.GetPublishName(langFr));
        Assert.AreNotEqual(DateTime.MinValue, content.GetPublishDate(langFr));
        Assert.IsFalse(content.IsCultureEdited(langFr)); // once published, edited is *wrong* until saved

        Assert.IsTrue(content.IsCultureAvailable(langUk));
        Assert.IsFalse(content.IsCulturePublished(langUk));
        Assert.IsNull(content.GetPublishName(langUk));
        Assert.IsNull(content.GetPublishDate(langUk)); // not published

        Assert.IsFalse(content.IsCultureAvailable(langEs));
        Assert.IsFalse(content.IsCultureEdited(langEs)); // not avail, so... not edited
        Assert.IsFalse(content.IsCulturePublished(langEs));

        // not published!
        Assert.IsNull(content.GetPublishName(langEs));
        Assert.IsNull(content.GetPublishDate(langEs));

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
        Assert.AreEqual("a", prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));

        Assert.IsTrue(prop.IsDirty());

        content.SetValue("prop", "a");
        Assert.AreEqual("a", content.GetValue("prop"));
        Assert.IsNull(content.GetValue("prop", published: true));

        Assert.IsTrue(content.IsDirty());
        Assert.IsTrue(content.IsAnyUserPropertyDirty());
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
        Assert.AreEqual("a", prop.GetValue());
        Assert.IsNull(prop.GetValue(published: true));
        var propertyValidationService = GetPropertyValidationService();

        Assert.IsTrue(propertyValidationService.IsPropertyValid(prop));

        propertyType.Mandatory = true;
        Assert.IsTrue(propertyValidationService.IsPropertyValid(prop));

        prop.SetValue(null);
        Assert.IsFalse(propertyValidationService.IsPropertyValid(prop));

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
        var localizedTextService = Mock.Of<ILocalizedTextService>();
        var editorConfigurationParser = Mock.Of<IEditorConfigurationParser>();

        var attribute = new DataEditorAttribute("a", "a", "a");
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(x
            => x.Create<TextOnlyValueEditor>(It.IsAny<DataEditorAttribute>()) == new TextOnlyValueEditor(
                attribute,
                localizedTextService,
                Mock.Of<IShortStringHelper>(),
                new JsonNetSerializer(),
                Mock.Of<IIOHelper>()));

        var textBoxEditor = new TextboxPropertyEditor(
            dataValueEditorFactory,
            ioHelper,
            editorConfigurationParser);

        var serializer = new ConfigurationEditorJsonSerializer();

        var mockDataTypeService = new Mock<IDataTypeService>();
        Mock.Get(dataTypeService).Setup(x => x.GetDataType(It.Is<int>(y => y == Constants.DataTypes.Textbox)))
            .Returns(new DataType(textBoxEditor, serializer));

        var propertyEditorCollection =
            new PropertyEditorCollection(new DataEditorCollection(() => new[] { textBoxEditor }));
        return new PropertyValidationService(
            propertyEditorCollection,
            dataTypeService,
            localizedTextService,
            new ValueEditorCache());
    }
}
