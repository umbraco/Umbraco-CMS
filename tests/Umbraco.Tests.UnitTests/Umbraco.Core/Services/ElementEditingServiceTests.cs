// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ElementEditingServiceTests
{
    [Test]
    public void Re_Applying_The_Same_Stored_Values_Leaves_The_Element_Unchanged()
    {
        // the transfer skips its second save when the owner's draft matches what was published, and this is
        // what it relies on to tell: setting a value it already holds must not mark it dirty.
        IContentType elementType = CreateElementType();
        IElement element = new Element("element", elementType);
        BlockPropertyValue[] values = [new() { Alias = "message", Value = "same" }];

        ElementEditingService.ApplyStoredValues(element, elementType, values);
        element.ResetDirtyProperties();

        ElementEditingService.ApplyStoredValues(element, elementType, values);

        Assert.IsFalse(element.IsDirty());
    }

    [Test]
    public void Applying_Different_Stored_Values_Marks_The_Element_Changed()
    {
        IContentType elementType = CreateElementType();
        IElement element = new Element("element", elementType);

        ElementEditingService.ApplyStoredValues(element, elementType, [new BlockPropertyValue { Alias = "message", Value = "published" }]);
        element.ResetDirtyProperties();

        ElementEditingService.ApplyStoredValues(element, elementType, [new BlockPropertyValue { Alias = "message", Value = "draft" }]);

        Assert.IsTrue(element.IsDirty());
    }

    [Test]
    public void Stored_Values_Are_Applied_As_They_Are()
    {
        IContentType elementType = CreateElementType();
        IElement element = new Element("element", elementType);

        ElementEditingService.ApplyStoredValues(
            element,
            elementType,
            [new BlockPropertyValue { Alias = "message", Value = "already stored" }]);

        Assert.AreEqual("already stored", element.GetValue("message"));
    }

    [Test]
    public void Values_For_Aliases_The_Content_Type_Does_Not_Have_Are_Ignored()
    {
        IContentType elementType = CreateElementType();
        IElement element = new Element("element", elementType);

        Assert.DoesNotThrow(() => ElementEditingService.ApplyStoredValues(
            element,
            elementType,
            [
                new BlockPropertyValue { Alias = "noSuchProperty", Value = "value" },
                new BlockPropertyValue { Alias = "message", Value = "value" },
            ]));

        Assert.AreEqual("value", element.GetValue("message"));
    }

    [Test]
    public void Values_The_Content_Type_Cannot_Hold_In_Their_Variation_Are_Ignored()
    {
        // block values carry the variation they were stored under, which is the block's own variance
        // intersected with whatever contained it. Lifted out, that intersection no longer applies, and
        // SetValue throws on a variation the property type does not support - a 500 over one stale value.
        IContentType elementType = CreateElementType();
        IElement element = new Element("element", elementType);

        Assert.DoesNotThrow(() => ElementEditingService.ApplyStoredValues(
            element,
            elementType,
            [
                new BlockPropertyValue { Alias = "message", Value = "cultured", Culture = "en-US" },
                new BlockPropertyValue { Alias = "message", Value = "segmented", Segment = "mobile" },
                new BlockPropertyValue { Alias = "message", Value = "invariant" },
            ]));

        Assert.AreEqual("invariant", element.GetValue("message"));
    }

    [Test]
    public void A_Variant_Element_Is_Named_In_Every_Culture_The_Block_Was_Created_For()
    {
        // which cultures a block exists in is what it is exposed in, not what it happens to hold values for -
        // a block created for a culture but left empty is still created for it.
        IContentType elementType = CreateVariantElementType();
        IElement element = new Element(string.Empty, elementType);
        Guid blockKey = Guid.NewGuid();

        ElementEditingService.ApplyNames(
            element,
            elementType,
            "Shared block",
            [new BlockItemVariation(blockKey, "en-US", null), new BlockItemVariation(blockKey, "pt-PT", null)],
            defaultIsoCode: "en-US");

        Assert.AreEqual("Shared block", element.GetCultureName("en-US"));
        Assert.AreEqual("Shared block", element.GetCultureName("pt-PT"));
    }

    [Test]
    public void A_Variant_Element_Is_Named_In_The_Default_Culture_When_The_Block_Is_Exposed_Nowhere()
    {
        // nothing to derive a culture from, and an unnamed element cannot be saved at all - the default
        // culture keeps that from turning into an opaque failure.
        IContentType elementType = CreateVariantElementType();
        IElement element = new Element(string.Empty, elementType);

        ElementEditingService.ApplyNames(element, elementType, "Shared block", [], defaultIsoCode: "en-US");

        Assert.AreEqual("Shared block", element.GetCultureName("en-US"));
    }

    private static IContentType CreateElementType()
        => new ContentTypeBuilder()
            .WithIsElement(true)
            .AddPropertyType()
                .WithAlias("message")
                .Done()
            .Build();

    private static IContentType CreateVariantElementType()
        => new ContentTypeBuilder()
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
                .WithAlias("message")
                .WithVariations(ContentVariation.Culture)
                .Done()
            .Build();
}
