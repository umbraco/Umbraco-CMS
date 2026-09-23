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

    private static IContentType CreateElementType()
        => new ContentTypeBuilder()
            .WithIsElement(true)
            .AddPropertyType()
                .WithAlias("message")
                .Done()
            .Build();
}
