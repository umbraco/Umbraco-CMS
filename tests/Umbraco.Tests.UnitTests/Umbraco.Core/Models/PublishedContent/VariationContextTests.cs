// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.PublishedContent;

[TestFixture]
public class VariationContextTests
{
    [Test]
    public void Can_Resolve_Segment_From_Subclass_Overriding_Only_The_Obsolete_Overload()
    {
        var context = new LegacyVariationContext();

        var segment = context.GetSegment(1, "title", () => null);

        Assert.AreEqual("legacy-segment", segment);
    }

    private class LegacyVariationContext : VariationContext
    {
#pragma warning disable CS0672 // Member overrides obsolete member
        public override string GetSegment(int contentId, string propertyAlias) => "legacy-segment";
#pragma warning restore CS0672 // Member overrides obsolete member
    }
}
