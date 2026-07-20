// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
public class ValueSetIndexerTests
{
    [Test]
    public void Can_Stream_To_Single_Index_Without_Materializing()
    {
        // Use a List (not a ValueSet[]) so this assertion also guards against a future regression: if the
        // single-index path ever materialised the sequence, the index would receive a different reference and
        // SameAs would fail. A ValueSet[] source couldn't catch that.
        var source = new List<ValueSet> { ValueSet.FromObject("1", "content", new { }), ValueSet.FromObject("2", "content", new { }) };
        IEnumerable<ValueSet>? received = null;
        var index = CreateIndex(v => received = v);

        ValueSetIndexer.IndexItems([index.Object], source);

        Assert.That(received, Is.SameAs(source));
    }

    [Test]
    public void Can_Materialize_Once_And_Reuse_For_Multiple_Indexes()
    {
        // A lazy sequence (not an array) so the helper has to materialise it.
        IEnumerable<ValueSet> source = Enumerable.Range(1, 3).Select(i => ValueSet.FromObject(i.ToString(), "content", new { }));
        IEnumerable<ValueSet>? receivedByFirst = null;
        IEnumerable<ValueSet>? receivedBySecond = null;
        var first = CreateIndex(v => receivedByFirst = v);
        var second = CreateIndex(v => receivedBySecond = v);

        ValueSetIndexer.IndexItems([first.Object, second.Object], source);

        Assert.Multiple(() =>
        {
            Assert.That(receivedByFirst, Is.InstanceOf<ValueSet[]>());
            Assert.That(receivedByFirst, Has.Exactly(3).Items);
            Assert.That(receivedBySecond, Is.SameAs(receivedByFirst)); // materialised once, reused
        });
    }

    [Test]
    public void Can_Handle_No_Indexes()
        => Assert.DoesNotThrow(() => ValueSetIndexer.IndexItems(Array.Empty<IIndex>(), new List<ValueSet>()));

    private static Mock<IIndex> CreateIndex(Action<IEnumerable<ValueSet>> onIndexItems)
    {
        var index = new Mock<IIndex>();
        index.Setup(x => x.IndexItems(It.IsAny<IEnumerable<ValueSet>>()))
            .Callback(onIndexItems);
        return index;
    }
}
