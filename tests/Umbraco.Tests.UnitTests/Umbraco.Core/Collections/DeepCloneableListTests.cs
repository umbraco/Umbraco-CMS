// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class DeepCloneableListTests
{
    [Test]
    public void Deep_Clones_Each_Item_Once()
    {
        var list = new DeepCloneableList<TestClone>(ListCloneBehavior.CloneOnce) { new(), new(), new() };

        var cloned = list.DeepClone() as DeepCloneableList<TestClone>;

        // Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
        Assert.That(list.SequenceEqual(cloned), Is.True);

        // Test that each instance in the list is not the same one
        foreach (var item in list)
        {
            var clone = cloned.Single(x => x.Id == item.Id);
            Assert.That(clone, Is.Not.SameAs(item));
        }

        // Clone again from the clone - since it's clone once the items should be the same
        var cloned2 = cloned.DeepClone() as DeepCloneableList<TestClone>;

        // Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
        Assert.That(cloned.SequenceEqual(cloned2), Is.True);

        // Test that each instance in the list is the same one
        foreach (var item in cloned)
        {
            var clone = cloned2.Single(x => x.Id == item.Id);
            Assert.That(clone, Is.SameAs(item));
        }
    }

    [Test]
    public void Deep_Clones_All_Elements()
    {
        var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always) { new(), new(), new() };

        var cloned = list.DeepClone() as DeepCloneableList<TestClone>;

        Assert.That(cloned, Is.Not.Null);
        Assert.That(cloned, Is.Not.SameAs(list));
        Assert.That(cloned, Has.Count.EqualTo(list.Count));
    }

    [Test]
    public void Clones_Each_Item()
    {
        var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always) { new(), new(), new() };

        var cloned = (DeepCloneableList<TestClone>)list.DeepClone();

        foreach (var item in cloned)
        {
            Assert.That(item.IsClone, Is.True);
        }
    }

    [Test]
    public void Cloned_Sequence_Equals()
    {
        var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always) { new(), new(), new() };

        var cloned = (DeepCloneableList<TestClone>)list.DeepClone();

        // Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
        Assert.That(list.SequenceEqual(cloned), Is.True);

        // Test that each instance in the list is not the same one
        foreach (var item in list)
        {
            var clone = cloned.Single(x => x.Id == item.Id);
            Assert.That(clone, Is.Not.SameAs(item));
        }
    }
}
