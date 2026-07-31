// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

[TestFixture]
public class CompositeStringStringKeyTests
{
    [Test]
    public void Equal_Keys_Are_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("a", "b");

        Assert.IsTrue(key1.Equals(key2));
        Assert.IsTrue(key1 == key2);
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Different_FirstKeys_Are_Not_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("c", "b");

        Assert.IsFalse(key1.Equals(key2));
        Assert.IsTrue(key1 != key2);
    }

    [Test]
    public void Different_SecondKeys_Are_Not_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("a", "c");

        Assert.IsFalse(key1.Equals(key2));
        Assert.IsTrue(key1 != key2);
    }

    [Test]
    public void Keys_Are_Case_Insensitive()
    {
        var key1 = new CompositeStringStringKey("Hello", "World");
        var key2 = new CompositeStringStringKey("hello", "world");

        Assert.IsTrue(key1.Equals(key2));
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Null_Part_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeStringStringKey("a", null!));
    }
}
