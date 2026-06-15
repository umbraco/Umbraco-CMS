// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class ContentTypeChangeExtensionsTests
{
    // "Main" base flags
    [TestCase(ContentTypeChangeTypes.RefreshMain, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.Create, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Remove, false)]
    // Granular structural flags (all include RefreshMain)
    [TestCase(ContentTypeChangeTypes.AliasChanged, true)]
    [TestCase(ContentTypeChangeTypes.PropertyAliasChanged, true)]
    [TestCase(ContentTypeChangeTypes.PropertyRemoved, true)]
    [TestCase(ContentTypeChangeTypes.CompositionRemoved, true)]
    [TestCase(ContentTypeChangeTypes.PropertyVariationChanged, true)]
    // Granular non-structural flags (all include RefreshOther, but NOT RefreshMain)
    [TestCase(ContentTypeChangeTypes.PropertyAdded, false)]
    [TestCase(ContentTypeChangeTypes.CompositionAdded, false)]
    public void IsStructuralChange(ContentTypeChangeTypes change, bool expected) =>
        Assert.That(change.IsStructuralChange(), Is.EqualTo(expected));

    // "Main" base flags
    [TestCase(ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Create, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Remove, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain, false)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    // Granular non-structural flags (include RefreshOther but not RefreshMain)
    [TestCase(ContentTypeChangeTypes.PropertyAdded, true)]
    [TestCase(ContentTypeChangeTypes.CompositionAdded, true)]
    // Granular structural flags (include RefreshMain, so NOT non-structural)
    [TestCase(ContentTypeChangeTypes.AliasChanged, false)]
    [TestCase(ContentTypeChangeTypes.PropertyRemoved, false)]
    [TestCase(ContentTypeChangeTypes.CompositionRemoved, false)]
    [TestCase(ContentTypeChangeTypes.PropertyVariationChanged, false)]
    // Mixed: structural + non-structural granular flags together (RefreshMain is set, so NOT non-structural)
    [TestCase(ContentTypeChangeTypes.PropertyRemoved | ContentTypeChangeTypes.PropertyAdded, false)]
    public void IsNonStructuralChange(ContentTypeChangeTypes change, bool expected) =>
        Assert.That(change.IsNonStructuralChange(), Is.EqualTo(expected));

    [Test]
    public void Granular_Structural_Flags_Include_RefreshMain_Bit()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ContentTypeChangeTypes.AliasChanged.HasType(ContentTypeChangeTypes.RefreshMain), Is.True);
            Assert.That(ContentTypeChangeTypes.PropertyAliasChanged.HasType(ContentTypeChangeTypes.RefreshMain), Is.True);
            Assert.That(ContentTypeChangeTypes.PropertyRemoved.HasType(ContentTypeChangeTypes.RefreshMain), Is.True);
            Assert.That(ContentTypeChangeTypes.CompositionRemoved.HasType(ContentTypeChangeTypes.RefreshMain), Is.True);
            Assert.That(ContentTypeChangeTypes.PropertyVariationChanged.HasType(ContentTypeChangeTypes.RefreshMain), Is.True);
        });
    }

    [Test]
    public void Granular_NonStructural_Flags_Include_RefreshOther_Bit()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ContentTypeChangeTypes.PropertyAdded.HasType(ContentTypeChangeTypes.RefreshOther), Is.True);
            Assert.That(ContentTypeChangeTypes.CompositionAdded.HasType(ContentTypeChangeTypes.RefreshOther), Is.True);
        });
    }

    [Test]
    public void Granular_NonStructural_Flags_Do_Not_Include_RefreshMain_Bit()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ContentTypeChangeTypes.PropertyAdded.HasType(ContentTypeChangeTypes.RefreshMain), Is.False);
            Assert.That(ContentTypeChangeTypes.CompositionAdded.HasType(ContentTypeChangeTypes.RefreshMain), Is.False);
        });
    }

    [Test]
    public void Combined_Granular_Flags_Are_Distinguishable()
    {
        var combined = ContentTypeChangeTypes.PropertyRemoved | ContentTypeChangeTypes.AliasChanged;

        Assert.Multiple(() =>
        {
            Assert.That(combined.HasType(ContentTypeChangeTypes.PropertyRemoved), Is.True);
            Assert.That(combined.HasType(ContentTypeChangeTypes.AliasChanged), Is.True);
            Assert.That(combined.HasType(ContentTypeChangeTypes.RefreshMain), Is.True);
            // HasTypesAll checks that ALL bits of the target are set, which is the correct way
            // to test for composite flags that share the RefreshMain bit
            Assert.That(combined.HasTypesAll(ContentTypeChangeTypes.CompositionRemoved), Is.False);
            Assert.That(combined.HasTypesAll(ContentTypeChangeTypes.PropertyVariationChanged), Is.False);
            Assert.That(combined.HasType(ContentTypeChangeTypes.RefreshOther), Is.False);
        });
    }

    [TestCase(ContentTypeChangeTypes.AliasChanged, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyAliasChanged, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyRemoved, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.CompositionRemoved, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyVariationChanged, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyAdded, ContentTypeChangeTypes.RefreshOther)]
    [TestCase(ContentTypeChangeTypes.CompositionAdded, ContentTypeChangeTypes.RefreshOther)]
    public void Sub_Flag_Yields_Positive_For_Main_Flag(ContentTypeChangeTypes subFlag, ContentTypeChangeTypes mainFlag)
        => Assert.That(subFlag.HasFlag(mainFlag), Is.True);

    [TestCase(ContentTypeChangeTypes.AliasChanged, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyAliasChanged, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyRemoved, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.CompositionRemoved, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyVariationChanged, ContentTypeChangeTypes.RefreshMain)]
    [TestCase(ContentTypeChangeTypes.PropertyAdded, ContentTypeChangeTypes.RefreshOther)]
    [TestCase(ContentTypeChangeTypes.CompositionAdded, ContentTypeChangeTypes.RefreshOther)]
    public void Main_Flag_Yields_Negative_For_Sub_Flag(ContentTypeChangeTypes subFlag, ContentTypeChangeTypes mainFlag)
        => Assert.That(mainFlag.HasFlag(subFlag), Is.False);

    [TestCase(ContentTypeChangeTypes.AliasChanged, ContentTypeChangeTypes.PropertyAliasChanged)]
    [TestCase(ContentTypeChangeTypes.PropertyAliasChanged, ContentTypeChangeTypes.PropertyRemoved)]
    [TestCase(ContentTypeChangeTypes.PropertyRemoved, ContentTypeChangeTypes.CompositionRemoved)]
    [TestCase(ContentTypeChangeTypes.CompositionRemoved, ContentTypeChangeTypes.PropertyVariationChanged)]
    [TestCase(ContentTypeChangeTypes.PropertyVariationChanged, ContentTypeChangeTypes.AliasChanged)]
    [TestCase(ContentTypeChangeTypes.PropertyAdded, ContentTypeChangeTypes.CompositionAdded)]
    [TestCase(ContentTypeChangeTypes.CompositionAdded, ContentTypeChangeTypes.PropertyAdded)]
    public void Sub_Flag_Yields_Negative_For_Other_Sub_Flag(ContentTypeChangeTypes subFlag1, ContentTypeChangeTypes subFlag2)
        => Assert.That(subFlag1.HasFlag(subFlag2), Is.False);
}
