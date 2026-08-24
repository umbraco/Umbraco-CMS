// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Covers the routing decision in <see cref="IDataEditor.CanMergePartialPropertyValues"/> that
/// <c>ContentRepositoryExtensions.PublishPropertyValues</c> relies on to decide whether a property on a
/// culture-varying content type should go through partial (element-level) publishing.
/// </summary>
/// <remarks>
/// Partial publishing only ever touches the invariant value slot, so a property that varies by segment must
/// never opt in to it - its segment values live in the variant value slots and would be silently dropped on
/// publish otherwise (see issue #23553).
/// </remarks>
[TestFixture]
public class BlockEditorPartialPublishingTests
{
    [TestCase(ContentVariation.Nothing, true)]
    [TestCase(ContentVariation.Culture, false)]
    [TestCase(ContentVariation.Segment, false)]
    [TestCase(ContentVariation.CultureAndSegment, false)]
    public void BlockListPropertyEditor_CanMergePartialPropertyValues_Respects_Segment_Variation(ContentVariation variation, bool expected)
    {
        var editor = new BlockListPropertyEditor(
            Mock.Of<IDataValueEditorFactory>(),
            Mock.Of<IIOHelper>(),
            Mock.Of<IBlockValuePropertyIndexValueFactory>(),
            Mock.Of<IJsonSerializer>());

        IPropertyType propertyType = new PropertyTypeBuilder().WithVariations(variation).Build();

        Assert.AreEqual(expected, editor.CanMergePartialPropertyValues(propertyType));
    }

    [TestCase(ContentVariation.Nothing, true)]
    [TestCase(ContentVariation.Culture, false)]
    [TestCase(ContentVariation.Segment, false)]
    [TestCase(ContentVariation.CultureAndSegment, false)]
    public void BlockGridPropertyEditor_CanMergePartialPropertyValues_Respects_Segment_Variation(ContentVariation variation, bool expected)
    {
        var editor = new BlockGridPropertyEditor(
            Mock.Of<IDataValueEditorFactory>(),
            Mock.Of<IIOHelper>(),
            Mock.Of<IBlockValuePropertyIndexValueFactory>());

        IPropertyType propertyType = new PropertyTypeBuilder().WithVariations(variation).Build();

        Assert.AreEqual(expected, editor.CanMergePartialPropertyValues(propertyType));
    }

    [TestCase(ContentVariation.Nothing, true)]
    [TestCase(ContentVariation.Culture, false)]
    [TestCase(ContentVariation.Segment, false)]
    [TestCase(ContentVariation.CultureAndSegment, false)]
    public void SingleBlockPropertyEditor_CanMergePartialPropertyValues_Respects_Segment_Variation(ContentVariation variation, bool expected)
    {
        var editor = new SingleBlockPropertyEditor(
            Mock.Of<IDataValueEditorFactory>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            Mock.Of<IBlockValuePropertyIndexValueFactory>());

        IPropertyType propertyType = new PropertyTypeBuilder().WithVariations(variation).Build();

        Assert.AreEqual(expected, editor.CanMergePartialPropertyValues(propertyType));
    }

    [TestCase(ContentVariation.Nothing, true)]
    [TestCase(ContentVariation.Culture, false)]
    [TestCase(ContentVariation.Segment, false)]
    [TestCase(ContentVariation.CultureAndSegment, false)]
    public void RichTextPropertyEditor_CanMergePartialPropertyValues_Respects_Segment_Variation(ContentVariation variation, bool expected)
    {
        var editor = new RichTextPropertyEditor(
            Mock.Of<IDataValueEditorFactory>(),
            Mock.Of<IIOHelper>(),
            Mock.Of<IRichTextPropertyIndexValueFactory>());

        IPropertyType propertyType = new PropertyTypeBuilder().WithVariations(variation).Build();

        Assert.AreEqual(expected, editor.CanMergePartialPropertyValues(propertyType));
    }
}
