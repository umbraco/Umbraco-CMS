// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class ContentTypeChangeExtensionsTests
{
    [TestCase(ContentTypeChangeTypes.RefreshMain, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.Create, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Remove, false)]
    public void IsStructuralChange(ContentTypeChangeTypes change, bool expected) =>
        Assert.AreEqual(expected, change.IsStructuralChange());

    [TestCase(ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Create, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Remove, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain, false)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    public void IsNonStructuralChange(ContentTypeChangeTypes change, bool expected) =>
        Assert.AreEqual(expected, change.IsNonStructuralChange());

    [TestCase(ContentTypeChangeTypes.RefreshMain, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.VariationChanged, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RawDataUnaffected, false)]
    [TestCase(ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.RawDataUnaffected, false)] // never set without RefreshMain, but guard anyway
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    public void RequiresRawDataRebuild(ContentTypeChangeTypes change, bool expected) =>
        Assert.AreEqual(expected, change.RequiresRawDataRebuild());

    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RawDataUnaffected, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain, false)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.VariationChanged, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    public void RequiresConvertedCacheClearOnly(ContentTypeChangeTypes change, bool expected) =>
        Assert.AreEqual(expected, change.RequiresConvertedCacheClearOnly());
}
